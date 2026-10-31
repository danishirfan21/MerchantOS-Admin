using System.Net.Http.Json;
using MerchantOS.Admin.Models;
namespace MerchantOS.Admin.Services;
// PosSessionHandler supplies this user's backend cookie. No shared cookie jar or service identity.
public sealed class ManagementClient(HttpClient client) {
 public async Task<ManagedProduct?> Product(string id,CancellationToken ct) {
  using var r=await client.GetAsync("api/admin/products/"+Uri.EscapeDataString(id),ct);if(r.StatusCode==System.Net.HttpStatusCode.NotFound)return null;r.EnsureSuccessStatusCode();return await r.Content.ReadFromJsonAsync<ManagedProduct>(cancellationToken:ct);
 }
 public async Task<AuditPage> History(string id,int page,CancellationToken ct) {
  using var r=await client.GetAsync($"api/admin/audit?entityId={Uri.EscapeDataString(id)}&page={page}&pageSize=5",ct);r.EnsureSuccessStatusCode();return (await r.Content.ReadFromJsonAsync<AuditPage>(cancellationToken:ct))!;
 }
 public async Task<HttpResponseMessage> Write(string path,object value,CancellationToken ct) {
  // Fetch a token bound to the authenticated backend user on every write; never expose it to the browser.
  using var csrf=await client.GetAsync("api/auth/csrf",ct);csrf.EnsureSuccessStatusCode();var token=await csrf.Content.ReadFromJsonAsync<Csrf>(cancellationToken:ct);
  using var request=new HttpRequestMessage(HttpMethod.Post,path){Content=JsonContent.Create(value)};request.Headers.Add("X-CSRF-TOKEN",token!.Token);
  return await client.SendAsync(request,ct);
 }
 private sealed record Csrf(string Token);
}
