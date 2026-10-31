using MerchantOS.Admin.Models;
using MerchantOS.Admin.Services;
using System.Net;
using System.Net.Http.Json;
using Xunit;
namespace MerchantOS.Admin.Tests;
public sealed class ReportingTests {
 [Fact]public async Task HistoricalPagesUseTypedServerQueryWithEscapedFilters(){var client=new PosClient(new HttpClient(new StubHandler(r=>{Assert.Equal("/api/admin/reports/orders",r.RequestUri!.AbsolutePath);Assert.Contains("search=a%26b",r.RequestUri.Query);Assert.Contains("page=16",r.RequestUri.Query);Assert.Contains("paymentMethod=cash",r.RequestUri.Query);return new(HttpStatusCode.OK){Content=JsonContent.Create(new OrderPage([],16,7,10000,true))};})){BaseAddress=new("https://localhost/")});var result=await client.QueryOrders(new("all",Search:"a&b",Payment:"cash"),16,7,default);Assert.Equal(10000,result.Total);Assert.True(result.HasMore);}
 [Fact]public async Task OversizedExportRemainsExplicitFailure(){var client=new PosClient(new HttpClient(new StubHandler(_=>new(HttpStatusCode.RequestEntityTooLarge))){BaseAddress=new("https://localhost/")});var error=await Assert.ThrowsAsync<ReportRejectedException>(()=>client.Export(new(),default));Assert.Equal(413,error.Status);}
 [Fact]public void PresetsDoNotForwardStaleCustomDates(){var url=new ReportFilters("today","2020-01-01","2020-01-02").Query("api/admin/reports/sales");Assert.DoesNotContain("from=",url);Assert.DoesNotContain("to=",url);Assert.Contains("range=today",url);}
 [Fact]public async Task ReportingFailureIsUnavailableRatherThanZero(){var vm=await new AdminService(new FakePos{Fail=true}).Dashboard(default);Assert.Null(vm.Report);Assert.NotNull(vm.Orders.Error);}
}
public sealed class ReportingRenderingTests {
 [Theory][InlineData("/")][InlineData("/Admin/Orders")]
 public async Task UnavailableReportDoesNotRenderZeroSalesOrEmptyPagination(string path){
  using var app=new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>().WithWebHostBuilder(b=>b.ConfigureServices(services=>{
   TestAuthentication.Add(services);Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions.RemoveAll<IPosClient>(services);Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddSingleton<IPosClient>(services,new FakePos{Fail=true});
  }));
  var html=await app.CreateClient(new(){BaseAddress=new Uri("https://localhost")}).GetStringAsync(path);Assert.Contains("unavailable",html,StringComparison.OrdinalIgnoreCase);Assert.DoesNotContain("SAR 0.00",html);Assert.DoesNotContain("id=\"result-pagination\"",html);
 }
}
