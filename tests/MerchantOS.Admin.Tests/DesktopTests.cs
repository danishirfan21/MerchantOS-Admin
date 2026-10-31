using MerchantOS.Admin.Models;
using MerchantOS.Admin.Services;
using Xunit;
namespace MerchantOS.Admin.Tests;
public sealed class DesktopTests {
 [Fact] public async Task CatalogPaginationKeepsGlobalMetricsAndClampsPage() {
  var pos=new FakePos();pos.Catalog=Enumerable.Range(1,16).Select(i=>pos.Catalog[0] with {Id=$"p{i}",Sku=$"SKU-{i}",Name=$"Product {i:D2}",Stock=i}).ToList();
  var vm=await new AdminService(pos).Catalog(null,null,null,false,default,2);
  Assert.Equal(7,vm.Products.Count);Assert.Equal("Product 08",vm.Products[0].Name);Assert.Equal(16,vm.TotalProducts);Assert.Equal(16,vm.FilteredCount);Assert.Equal(8,vm.Pager.Start);Assert.Equal(14,vm.Pager.End);
  var last=await new AdminService(pos).Catalog(null,null,null,false,default,999);
  Assert.Equal(3,last.Page);Assert.Equal(2,last.Products.Count);
 }
 [Fact] public async Task SortsPricesUsingIntegerHalalas() {
  var vm=await new AdminService(new FakePos()).Catalog(null,null,null,false,default,sort:"price-asc");
  Assert.Equal("Mouse",vm.Products[0].Name);
 }
 [Fact] public async Task AttentionIncludesLowAndZeroWithoutChangingThreshold() {
  var vm=await new AdminService(new FakePos()).Catalog(null,null,"attention",true,default);
  Assert.Equal(2,vm.Products.Count);Assert.Equal(0,vm.HealthyStock);
 }
 [Fact] public async Task OrdersFilterPaymentAndStoreDateOnServer() {
  var pos=new FakePos();var first=pos.Sales[0];pos.Sales.Add(first with {Id=Guid.NewGuid(),PaymentMethod="card",CreatedAt=DateTime.UtcNow.Date.AddDays(-1)});
  var vm=await new AdminService(pos).Orders(null,default,payment:"card",range:"yesterday");
  Assert.Single(vm.Orders);Assert.Equal(1,vm.WindowCount);Assert.Equal(1,vm.FilteredCount);
 }
 [Fact] public async Task DashboardAggregationUsesAllOrdersRatherThanFirstPage() {
  var pos=new FakePos();var sale=pos.Sales[0];pos.Sales=Enumerable.Range(0,12).Select(i=>sale with {Id=Guid.NewGuid()}).ToList();
  var vm=await new AdminService(pos).Dashboard(default);
  Assert.Equal(12,vm.CompletedOrders);Assert.Equal(13800,vm.SalesHalalas);Assert.Equal(12,vm.Rhythm.Sum(b=>b.Count));
 }
 [Fact] public void PagerPreservesFiltersAndEscapesInput() {
  var pager=new PagerVm(1,7,30,"Products",new(){["search"]="a&b",["category"]="Audio"});
  Assert.Equal(5,pager.TotalPages);Assert.Contains("search=a%26b",pager.Links[2].Url);Assert.True(pager.Links[0].Disabled);
 }
 [Fact] public async Task EmptyRhythmDoesNotInventCounts() {
  var vm=await new AdminService(new FakePos {Sales=[]}).Dashboard(default);Assert.Empty(vm.Rhythm);
 }
}
