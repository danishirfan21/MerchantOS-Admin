using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
namespace MerchantOS.Admin.Services;
public sealed record BackendUser(Guid Id,string Username,string Role,bool MfaVerified=false,DateTimeOffset? MfaVerifiedAt=null);
public sealed record BackendSession(BackendUser User,string Cookie,DateTimeOffset? ExpiresAt=null);
public sealed record BackendChallenge(Guid Id,string Username,string Step,DateTimeOffset ExpiresAt,bool MustChangePassword);
public sealed record BackendStart(BackendSession? Session,BackendChallenge? Challenge,string Cookie);
public sealed record EnrollmentInfo(string Secret,string OtpAuthUri,DateTimeOffset ExpiresAt);
public sealed record BackendCompletion(BackendUser User,string[] RecoveryCodes,DateTimeOffset ExpiresAt);
public sealed record RecoveryBatch(string[] RecoveryCodes);
public sealed class BackendAuth(IHttpClientFactory factory) {
 const string SessionName="MerchantOS.Pos.Session";
 static string Merge(string existing,HttpResponseMessage response,bool full=false){
  var pairs=new Dictionary<string,string>(StringComparer.Ordinal);
  foreach(var part in existing.Split(';',StringSplitOptions.RemoveEmptyEntries)){var bits=part.Trim().Split('=',2);if(bits.Length==2)pairs[bits[0]]=bits[1];}
  if(response.Headers.TryGetValues("Set-Cookie",out var values))foreach(var value in values){var bits=value.Split(';')[0].Split('=',2);if(bits.Length==2){if(bits[1].Length==0)pairs.Remove(bits[0]);else pairs[bits[0]]=bits[1];}}
  return string.Join("; ",pairs.Where(p=>!full||p.Key is "MerchantOS.Pos.Session" or "MerchantOS.Pos.Csrf").Select(p=>p.Key+"="+p.Value));
 }
 async Task<HttpResponseMessage> Post(string cookie,string path,object body,CancellationToken ct){
  using var client=factory.CreateClient("auth");using var get=new HttpRequestMessage(HttpMethod.Get,"api/auth/csrf");if(cookie.Length>0)get.Headers.Add("Cookie",cookie);
  using var csrf=await client.SendAsync(get,ct);csrf.EnsureSuccessStatusCode();var token=await csrf.Content.ReadFromJsonAsync<CsrfToken>(cancellationToken:ct);
  using var message=new HttpRequestMessage(HttpMethod.Post,path){Content=JsonContent.Create(body)};message.Headers.Add("Cookie",Merge(cookie,csrf));message.Headers.Add("X-CSRF-TOKEN",token!.Token);var response=await client.SendAsync(message,ct);
  // Carry the cookie-token pair internally; never expose it in a browser DTO.
  response.OptionsSetCookie(Merge(cookie,csrf));return response;
 }
 public async Task<BackendStart?> Login(string username,string password,CancellationToken ct){
  using var response=await Post("","api/auth/login",new{username,password},ct);if(!response.IsSuccessStatusCode)return null;var cookie=Merge(response.OptionsGetCookie(),response);
  if(response.StatusCode==HttpStatusCode.Accepted){var challenge=await response.Content.ReadFromJsonAsync<BackendChallenge>(cancellationToken:ct);if(challenge is not null&&challenge.Id!=Guid.Empty&&challenge.Step is "enroll" or "verify"&&challenge.ExpiresAt>DateTimeOffset.UtcNow)return new(null,challenge,cookie);await Cancel(cookie,ct);return null;}
  var user=await response.Content.ReadFromJsonAsync<BackendUser>(cancellationToken:ct);if(user?.Role!="Administrator"||!user.MfaVerified){if(cookie.Contains(SessionName+"=",StringComparison.Ordinal))await Logout(cookie,ct);return null;}return new(new(user,cookie),null,cookie);
 }
 public async Task<EnrollmentInfo?> Enrollment(string cookie,CancellationToken ct){using var response=await Post(cookie,"api/auth/mfa/enrollment",new{},ct);return response.IsSuccessStatusCode?await response.Content.ReadFromJsonAsync<EnrollmentInfo>(cancellationToken:ct):null;}
 public async Task<(BackendSession Session,string[] Codes)?> Complete(string cookie,string? code,string? recovery,string? password,CancellationToken ct){
  using var response=await Post(cookie,"api/auth/mfa/complete",new{code,recoveryCode=recovery,newPassword=password},ct);if(!response.IsSuccessStatusCode)return null;var result=await response.Content.ReadFromJsonAsync<BackendCompletion>(cancellationToken:ct);if(result?.User.Role!="Administrator"||!result.User.MfaVerified)return null;return(new(result.User,Merge(response.OptionsGetCookie(),response,true),result.ExpiresAt),result.RecoveryCodes);
 }
 public async Task Cancel(string cookie,CancellationToken ct){using var response=await Post(cookie,"api/auth/mfa/cancel",new{},ct);response.EnsureSuccessStatusCode();}
 public async Task<BackendUser?> Me(string cookie,CancellationToken ct){using var client=factory.CreateClient("auth");using var request=new HttpRequestMessage(HttpMethod.Get,"api/auth/me");request.Headers.Add("Cookie",cookie);using var response=await client.SendAsync(request,ct);if(response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)return null;response.EnsureSuccessStatusCode();return await response.Content.ReadFromJsonAsync<BackendUser>(cancellationToken:ct);}
 public async Task Logout(string cookie,CancellationToken ct){using var response=await Post(cookie,"api/auth/logout",new{},ct);response.EnsureSuccessStatusCode();}
 public async Task<int> ChangePassword(string cookie,string current,string replacement,CancellationToken ct){using var response=await Post(cookie,"api/auth/password",new{currentPassword=current,newPassword=replacement},ct);return (int)response.StatusCode;}
 public async Task<string[]?> Regenerate(string cookie,string password,string code,CancellationToken ct){using var response=await Post(cookie,"api/auth/recovery/regenerate",new{currentPassword=password,code},ct);return response.IsSuccessStatusCode?(await response.Content.ReadFromJsonAsync<RecoveryBatch>(cancellationToken:ct))?.RecoveryCodes:null;}
 sealed record CsrfToken(string Token);
}
// Per-message state has no cross-user shared handler/container. It never becomes a response header.
static class BackendMessageState {
 static readonly System.Runtime.CompilerServices.ConditionalWeakTable<HttpResponseMessage,State> Values=new();
 sealed record State(string Cookie);
 public static void OptionsSetCookie(this HttpResponseMessage response,string cookie)=>Values.Add(response,new(cookie));
 public static string OptionsGetCookie(this HttpResponseMessage response)=>Values.TryGetValue(response,out var value)?value.Cookie:"";
}
public sealed class PosSessionHandler(IHttpContextAccessor accessor):DelegatingHandler {
 protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct){if(accessor.HttpContext is {} context){var auth=await context.AuthenticateAsync("Admin");if(auth.Properties?.Items.TryGetValue("backend",out var cookie)==true&&!string.IsNullOrEmpty(cookie))request.Headers.Add("Cookie",cookie);}return await base.SendAsync(request,ct);}
}
