using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
namespace MerchantOS.Admin.Services;
// The per-reference protection purpose prevents ciphertext substitution between database rows.
// Native ASP.NET ticket serialization protected by the application's persistent DP key ring.
// Development-only memory fallback keeps isolated rendering tests independent of a database.
public sealed class AdminTicketStore:ITicketStore,IDisposable {
 private readonly NpgsqlDataSource? db;private readonly IMemoryCache cache;private readonly IDataProtector protector;
 public AdminTicketStore(IMemoryCache cache,IDataProtectionProvider protection,IConfiguration config,IHostEnvironment env){
  this.cache=cache;protector=protection.CreateProtector("MerchantOS.Admin.ServerTickets.v1");
  var connection=config.GetConnectionString("AdminSessions");
  if(!string.IsNullOrEmpty(connection))db=NpgsqlDataSource.Create(connection);
  else if(!env.IsDevelopment()&&!env.IsEnvironment("Testing"))throw new InvalidOperationException("ConnectionStrings:AdminSessions is required for durable sessions.");
 }
 private static string Hash(string key)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key)));
 public async Task<string> StoreAsync(AuthenticationTicket ticket){
  var key=Convert.ToHexString(RandomNumberGenerator.GetBytes(32));var expires=ticket.Properties.ExpiresUtc??throw new InvalidOperationException("Absolute ticket expiry required.");
  if(expires>DateTimeOffset.UtcNow.AddMinutes(30))throw new InvalidOperationException("Ticket lifetime exceeds policy.");
  if(db is null){cache.Set(key,ticket,expires);return key;}
  await using var cmd=db.CreateCommand("INSERT INTO admin_ticket_sessions(id_hash,user_id,protected_ticket,expires_at) VALUES(@id,@user,@ticket,@expiry)");
  cmd.Parameters.AddWithValue("id",Hash(key));cmd.Parameters.AddWithValue("user",Guid.Parse(ticket.Principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value));cmd.Parameters.AddWithValue("ticket",protector.CreateProtector(Hash(key)).Protect(TicketSerializer.Default.Serialize(ticket)));cmd.Parameters.AddWithValue("expiry",expires);await cmd.ExecuteNonQueryAsync();return key;
 }
 public Task RenewAsync(string key,AuthenticationTicket ticket)=>Task.CompletedTask; // Never slide or extend absolute lifetime.
 public async Task<AuthenticationTicket?> RetrieveAsync(string key){
  if(db is null)return cache.TryGetValue<AuthenticationTicket>(key,out var t)?t:null;
  await using var cmd=db.CreateCommand("SELECT protected_ticket FROM admin_ticket_sessions WHERE id_hash=@id AND expires_at>now()");cmd.Parameters.AddWithValue("id",Hash(key));var blob=await cmd.ExecuteScalarAsync();if(blob is not byte[] bytes)return null;
  try{var ticket=TicketSerializer.Default.Deserialize(protector.CreateProtector(Hash(key)).Unprotect(bytes));return ticket?.Properties.ExpiresUtc>DateTimeOffset.UtcNow?ticket:null;}catch(CryptographicException){return null;}
 }
 public async Task RemoveAsync(string key){if(db is null){cache.Remove(key);return;}await using var cmd=db.CreateCommand("DELETE FROM admin_ticket_sessions WHERE id_hash=@id");cmd.Parameters.AddWithValue("id",Hash(key));await cmd.ExecuteNonQueryAsync();}
 public async Task<bool> Ready(CancellationToken ct){if(db is null)return true;await using var cmd=db.CreateCommand("SELECT to_regclass('admin_ticket_sessions') IS NOT NULL");cmd.CommandTimeout=2;return (bool)(await cmd.ExecuteScalarAsync(ct))!;}
 public async Task<int> Cleanup(CancellationToken ct){if(db is null)return 0;await using var cmd=db.CreateCommand("DELETE FROM admin_ticket_sessions WHERE id_hash IN (SELECT id_hash FROM admin_ticket_sessions WHERE expires_at<=now() ORDER BY expires_at LIMIT 1000)");return await cmd.ExecuteNonQueryAsync(ct);}
 public void Dispose()=>db?.Dispose();
}
public sealed class AdminSessionMaintenance(AdminTicketStore store,ILogger<AdminSessionMaintenance> log):BackgroundService {
 protected override async Task ExecuteAsync(CancellationToken ct){using var timer=new PeriodicTimer(TimeSpan.FromMinutes(1));while(await timer.WaitForNextTickAsync(ct))try{log.LogInformation("Session cleanup removed {Count} expired records",await store.Cleanup(ct));}catch(Exception e)when(e is not OperationCanceledException){log.LogWarning("Session cleanup unavailable; category {Category}",e.GetType().Name);}}
}
