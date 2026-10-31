using MerchantOS.Admin.Models;
using Xunit;
public sealed class ManagementTests {
 [Theory][InlineData("0")][InlineData("-1")][InlineData("1000000.01")][InlineData("NaN")][InlineData("1e3")]public void RejectUnsafePrice(string value)=>Assert.Null(new ProductEdit{Price=value}.Halalas());
 [Theory][InlineData("0.01",1)][InlineData("123.45",12345)][InlineData("1000000.00",100000000)]public void ConvertDecimalPriceExactly(string value,long expected)=>Assert.Equal(expected,new ProductEdit{Price=value}.Halalas());
}
public sealed class ManagementHttpTests {
 [Theory][InlineData("/Management/Editor/p01")][InlineData("/Management/Adjustment/p01")][InlineData("/Management/History/p01")]
 public async Task AnonymousManagementIs401(string path){using var app=new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>();using var client=app.CreateClient(new(){BaseAddress=new Uri("https://localhost"),AllowAutoRedirect=false});Assert.Equal(System.Net.HttpStatusCode.Unauthorized,(await client.GetAsync(path)).StatusCode);}
 [Theory][InlineData("/Management/SaveProduct")][InlineData("/Management/AdjustInventory")]
 public async Task ManagementWritesRequireMvcCsrf(string path){using var app=new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>().WithWebHostBuilder(b=>b.ConfigureServices(MerchantOS.Admin.Tests.TestAuthentication.Add));using var client=app.CreateClient(new(){BaseAddress=new Uri("https://localhost"),AllowAutoRedirect=false});Assert.Equal(System.Net.HttpStatusCode.BadRequest,(await client.PostAsync(path,new FormUrlEncodedContent(new Dictionary<string,string>()))).StatusCode);}
}
