using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
namespace MerchantOS.Admin.Tests;
public sealed class AuthenticationTests {
 [Theory][InlineData("/")][InlineData("/Admin/Orders")][InlineData("/Admin/Inventory?stock=low")]
 public async Task AnonymousMvcRequiresLogin(string path){using var app=new WebApplicationFactory<Program>();using var client=app.CreateClient(new(){BaseAddress=new Uri("https://localhost"),AllowAutoRedirect=false});var response=await client.GetAsync(path);Assert.Equal(HttpStatusCode.Redirect,response.StatusCode);Assert.Contains("/Account/Login",response.Headers.Location!.OriginalString);}
 [Fact]public async Task AnonymousJsonIs401(){using var app=new WebApplicationFactory<Program>();using var client=app.CreateClient(new(){BaseAddress=new Uri("https://localhost"),AllowAutoRedirect=false});Assert.Equal(HttpStatusCode.Unauthorized,(await client.GetAsync("/Admin/ProductRows")).StatusCode);}
 [Fact]public async Task LoginRequiresCsrf(){using var app=new WebApplicationFactory<Program>();using var client=app.CreateClient(new(){BaseAddress=new Uri("https://localhost"),AllowAutoRedirect=false});Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsync("/Account/Login",new FormUrlEncodedContent(new Dictionary<string,string>{{"Username","invalid"},{"Password","invalid"}}))).StatusCode);}
 [Fact]public async Task UnsafeReturnIsNotRendered(){using var app=new WebApplicationFactory<Program>();using var client=app.CreateClient(new(){BaseAddress=new Uri("https://localhost")});var html=await client.GetStringAsync("/Account/Login?returnUrl=https://example.com");Assert.DoesNotContain("https://example.com",html);}
}
