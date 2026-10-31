using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace MerchantOS.Admin.Tests;
// Isolated rendering-test identity only. Production and live CI always use real authentication.
public sealed class TestAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> o,ILoggerFactory l,UrlEncoder e) : AuthenticationHandler<AuthenticationSchemeOptions>(o,l,e) {
 protected override Task<AuthenticateResult> HandleAuthenticateAsync()=>Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Name,"render-test"),new(ClaimTypes.Role,"Administrator")],"Test")),"Test")));
 public static void Add(IServiceCollection services){services.AddAuthentication(o=>{o.DefaultAuthenticateScheme="Test";o.DefaultChallengeScheme="Test";}).AddScheme<AuthenticationSchemeOptions,TestAuthentication>("Test",_=>{});}
}
