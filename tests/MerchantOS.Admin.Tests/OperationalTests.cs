using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
namespace MerchantOS.Admin.Tests;
public sealed class OperationalTests {
 [Fact]public async Task LivenessIsAnonymousAndDependencyIndependent(){using var app=new WebApplicationFactory<Program>();using var c=app.CreateClient(new(){BaseAddress=new("https://localhost")});Assert.Equal(System.Net.HttpStatusCode.OK,(await c.GetAsync("/health/live")).StatusCode);}
 [Fact]public void StagingCannotFallBackToEphemeralKeys(){using var app=new WebApplicationFactory<Program>().WithWebHostBuilder(b=>b.UseEnvironment("Staging"));Assert.ThrowsAny<Exception>(()=>app.CreateClient());}
 [Fact]public async Task OperationalMetricsRemainAdministratorOnly(){using var app=new WebApplicationFactory<Program>();using var c=app.CreateClient(new(){BaseAddress=new("https://localhost"),AllowAutoRedirect=false});Assert.Equal(System.Net.HttpStatusCode.Redirect,(await c.GetAsync("/ops/metrics")).StatusCode);}
}
