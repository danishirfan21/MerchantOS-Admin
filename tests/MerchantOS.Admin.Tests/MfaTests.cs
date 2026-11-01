using System.Net;
using System.Net.Http.Json;
using MerchantOS.Admin.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
namespace MerchantOS.Admin.Tests;
public sealed class MfaTests {
 [Theory][InlineData("/Account/Verify")][InlineData("/Account/Cancel")][InlineData("/Account/Password")][InlineData("/Account/Regenerate")]
 public async Task MfaAccountPostsRequireCsrf(string path){using var app=new WebApplicationFactory<Program>();using var client=app.CreateClient(new(){BaseAddress=new Uri("https://localhost"),AllowAutoRedirect=false});var response=await client.PostAsync(path,new FormUrlEncodedContent(new Dictionary<string,string>()));Assert.Contains(response.StatusCode,new[]{HttpStatusCode.BadRequest,HttpStatusCode.Redirect});if(path is "/Account/Verify" or "/Account/Cancel")Assert.Equal(HttpStatusCode.BadRequest,response.StatusCode);}
 [Fact]public void PendingCookieIsSeparateRestrictedAndAbsolute(){using var app=new WebApplicationFactory<Program>();var options=app.Services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>();var pending=options.Get("Pending");Assert.Equal("MerchantOS.Admin.Pending",pending.Cookie.Name);Assert.True(pending.Cookie.HttpOnly);Assert.False(pending.SlidingExpiration);Assert.Equal(TimeSpan.FromMinutes(5),pending.ExpireTimeSpan);Assert.NotEqual(options.Get("Admin").Cookie.Name,pending.Cookie.Name);Assert.NotNull(pending.SessionStore);}
 sealed class Handler(Func<HttpRequestMessage,HttpResponseMessage> answer):HttpMessageHandler{protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r,CancellationToken ct)=>Task.FromResult(answer(r));}
 sealed class Factory(Func<HttpRequestMessage,HttpResponseMessage> answer):IHttpClientFactory{public HttpClient CreateClient(string name)=>new(new Handler(answer)){BaseAddress=new Uri("https://localhost/")};}
 [Fact]public async Task PasswordOnlyAdministratorResponseIsRejected(){bool revoked=false;var auth=new BackendAuth(new Factory(r=>{if(r.RequestUri!.AbsolutePath.EndsWith("csrf"))return new(HttpStatusCode.OK){Content=JsonContent.Create(new{token="test"})};if(r.RequestUri.AbsolutePath.EndsWith("logout")){revoked=true;return new(HttpStatusCode.NoContent);}var response=new HttpResponseMessage(HttpStatusCode.OK){Content=JsonContent.Create(new BackendUser(Guid.NewGuid(),"test-admin","Administrator",false))};response.Headers.Add("Set-Cookie","MerchantOS.Pos.Session=test-only; HttpOnly");return response;}));Assert.Null(await auth.Login("test-admin","test-password",default));Assert.True(revoked);}
 [Fact]public async Task RestrictedPrimaryResponseCannotBecomeFullSession(){var id=Guid.NewGuid();var auth=new BackendAuth(new Factory(r=>{if(r.RequestUri!.AbsolutePath.EndsWith("csrf"))return new(HttpStatusCode.OK){Content=JsonContent.Create(new{token="test"})};var response=new HttpResponseMessage(HttpStatusCode.Accepted){Content=JsonContent.Create(new BackendChallenge(id,"test-admin","verify",DateTimeOffset.UtcNow.AddMinutes(5),false))};response.Headers.Add("Set-Cookie","MerchantOS.Pos.Challenge=test-only; HttpOnly");return response;}));var result=await auth.Login("test-admin","test-password",default);Assert.NotNull(result);Assert.Null(result.Session);Assert.Equal(id,result.Challenge!.Id);}
}
