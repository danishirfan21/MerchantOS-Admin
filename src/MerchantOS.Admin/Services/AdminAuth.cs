using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;
namespace MerchantOS.Admin.Services;
public sealed record BackendUser(Guid Id, string Username, string Role);
public sealed record BackendSession(BackendUser User, string Cookie);
public sealed class BackendAuth(IHttpClientFactory factory) {
    public async Task<BackendSession?> Login(string username, string password, CancellationToken ct) {
        using var client = factory.CreateClient("auth");
        using var csrf = await client.GetAsync("api/auth/csrf",ct);
        if(!csrf.IsSuccessStatusCode)return null;
        var token = await csrf.Content.ReadFromJsonAsync<CsrfToken>(cancellationToken:ct);
        var cookies = Cookies(csrf);
        using var message = new HttpRequestMessage(HttpMethod.Post,"api/auth/login") {Content=JsonContent.Create(new {username,password})};
        message.Headers.Add("Cookie",cookies);message.Headers.Add("X-CSRF-TOKEN",token!.Token);
        using var response = await client.SendAsync(message,ct);
        if(!response.IsSuccessStatusCode)return null;
        var user = await response.Content.ReadFromJsonAsync<BackendUser>(cancellationToken:ct);
        var cookie = cookies + "; " + Cookies(response);
        if(user?.Role != "Administrator") { await Logout(cookie,ct); return null; }
        return new(user,cookie);
    }
    public async Task<BackendUser?> Me(string cookie,CancellationToken ct) {
        using var client=factory.CreateClient("auth");using var request=new HttpRequestMessage(HttpMethod.Get,"api/auth/me");request.Headers.Add("Cookie",cookie);
        using var response=await client.SendAsync(request,ct);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<BackendUser>(cancellationToken:ct) : null;
    }
    public async Task Logout(string cookie,CancellationToken ct) {
        using var client=factory.CreateClient("auth");using var get=new HttpRequestMessage(HttpMethod.Get,"api/auth/csrf");get.Headers.Add("Cookie",cookie);
        using var csrf=await client.SendAsync(get,ct);if(!csrf.IsSuccessStatusCode)return;
        var token=await csrf.Content.ReadFromJsonAsync<CsrfToken>(cancellationToken:ct);
        using var request=new HttpRequestMessage(HttpMethod.Post,"api/auth/logout") {Content=JsonContent.Create(new {})};
        request.Headers.Add("Cookie",cookie);request.Headers.Add("X-CSRF-TOKEN",token!.Token);using var response=await client.SendAsync(request,ct);
        response.EnsureSuccessStatusCode();
    }
    private static string Cookies(HttpResponseMessage response) => response.Headers.TryGetValues("Set-Cookie",out var values) ? string.Join("; ",values.Select(v=>v.Split(';')[0]).Where(v=>!v.EndsWith('='))) : "";
    private sealed record CsrfToken(string Token);
}
public sealed class AdminTicketStore(IMemoryCache cache) : ITicketStore {
    public Task<string> StoreAsync(AuthenticationTicket ticket) {var key=Guid.NewGuid().ToString("N");cache.Set(key,ticket,ticket.Properties.ExpiresUtc!.Value);return Task.FromResult(key);}
    public Task RenewAsync(string key,AuthenticationTicket ticket)=>Task.CompletedTask;
    public Task<AuthenticationTicket?> RetrieveAsync(string key)=>Task.FromResult(cache.TryGetValue<AuthenticationTicket>(key,out var ticket)?ticket:null);
    public Task RemoveAsync(string key){cache.Remove(key);return Task.CompletedTask;}
}
public sealed class PosSessionHandler(IHttpContextAccessor accessor) : DelegatingHandler {
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct) {
        var context=accessor.HttpContext;
        if(context is not null){var auth=await context.AuthenticateAsync("Admin");if(auth.Properties?.Items.TryGetValue("backend",out var cookie)==true&&!string.IsNullOrEmpty(cookie))request.Headers.Add("Cookie",cookie);}
        return await base.SendAsync(request,ct);
    }
}
