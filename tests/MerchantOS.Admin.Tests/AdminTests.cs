using System.Net;
using System.Net.Http.Json;
using MerchantOS.Admin.Controllers;
using MerchantOS.Admin.Models;
using MerchantOS.Admin.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;
namespace MerchantOS.Admin.Tests;
public sealed class FakePos : IPosClient {
    public bool Fail { get; set; }
    public List<ProductDto> Catalog { get; set; } = [new("a", "SKU-A", "Laptop", "حاسوب", "Computers", 12345, 5, "", "", null), new("b", "SKU-B", "Mouse", "فأرة", "Accessories", 1000, 0, "", "", null)];
    public List<OrderDto> Sales { get; set; } = [new(Guid.NewGuid(), "MOS-TEST", DateTime.UtcNow, "SAR", "completed", 1000, 0, 150, 1150, "cash", 0, [])];
    public Task<List<ProductDto>> Products(CancellationToken ct) => Fail ? throw new PosUnavailableException(new Exception()) : Task.FromResult(Catalog);
    public Task<ProductDto?> Product(string id, CancellationToken ct) => Task.FromResult(Catalog.Find(p=>p.Id==id));
    public Task<List<OrderDto>> Orders(CancellationToken ct) => Fail ? throw new PosUnavailableException(new Exception()) : Task.FromResult(Sales);
    public Task<OrderDto?> Order(Guid id, CancellationToken ct) => Task.FromResult(Sales.Find(p=>p.Id==id));
}
public sealed class AdminTests {
    [Fact] public async Task AggregatesCatalogAndLatestSales() {
        var model = await new AdminService(new FakePos()).Dashboard(default);
        Assert.Equal(2, model.Catalog.TotalProducts); Assert.Equal(5, model.Catalog.TotalUnits);
        Assert.Equal(1, model.Catalog.LowStock); Assert.Equal(1, model.Catalog.OutOfStock);
        Assert.Equal(1, model.CompletedOrders); Assert.Equal(1150, model.SalesHalalas);
    }
    [Fact] public async Task SalesExcludeOtherCurrenciesAndNonCompletedOrders() {
        var pos = new FakePos(); var sale = pos.Sales[0];
        pos.Sales.Add(sale with {Currency="USD"}); pos.Sales.Add(sale with {Status="cancelled"});
        Assert.Equal(1150, (await new AdminService(pos).Dashboard(default)).SalesHalalas);
    }
    [Theory] [InlineData("laptop",null,null,1)] [InlineData("sku-b",null,null,1)]
    [InlineData(null,"Accessories",null,1)] [InlineData(null,null,"low",1)]
    [InlineData(null,null,"out",1)] [InlineData(null,null,"available",0)] [InlineData("missing",null,null,0)]
    public async Task FiltersCatalog(string? search,string? category,string? stock,int count) =>
        Assert.Equal(count,(await new AdminService(new FakePos()).Catalog(search,category,stock,false,default)).Products.Count);
    [Fact] public async Task EmptyDataRemainsZeroAndNotAnError() {
        var model=await new AdminService(new FakePos {Catalog=[],Sales=[]}).Dashboard(default);
        Assert.Null(model.Catalog.Error); Assert.Equal(0,model.Catalog.TotalProducts); Assert.Equal(0,model.CompletedOrders);
    }
    [Fact] public async Task FailuresHaveExplicitUnavailableStates() {
        var model=await new AdminService(new FakePos {Fail=true}).Dashboard(default);
        Assert.NotNull(model.Catalog.Error); Assert.NotNull(model.Orders.Error);
    }
    [Fact] public void MappingUsesHalalasAndStockThreshold() {
        var row=ProductRow.From(new FakePos().Catalog[0]);
        Assert.Equal("SAR 123.45",row.Price); Assert.Equal("Low stock",row.StockStatus);
    }
    [Fact] public async Task ControllerReturnsTypedCatalog() {
        var pos=new FakePos(); var controller=new AdminController(new AdminService(pos),pos);
        var view=Assert.IsType<ViewResult>(await controller.Products(null,null,null,default));
        Assert.IsType<CatalogVm>(view.Model); Assert.Equal("Catalog",view.ViewName);
    }
    [Fact] public async Task ControllerRejectsInvalidFilters() {
        var pos=new FakePos(); var controller=new AdminController(new AdminService(pos),pos);
        controller.ModelState.AddModelError("search","Too long");
        Assert.IsType<BadRequestObjectResult>(await controller.Products(null,null,null,default));
    }
    [Fact] public async Task MissingProductReturns404() {
        var pos=new FakePos(); var controller=new AdminController(new AdminService(pos),pos);
        Assert.IsType<NotFoundResult>(await controller.Product("unknown",default));
    }
    [Fact] public async Task OrdersSearchUsesWindow() => Assert.Empty((await new AdminService(new FakePos()).Orders("missing",default)).Orders);
}
public sealed class StubHandler(Func<HttpRequestMessage,HttpResponseMessage> respond) : HttpMessageHandler {
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct) => Task.FromResult(respond(request));
}
public sealed class PosClientTests {
    private static PosClient Client(Func<HttpRequestMessage,HttpResponseMessage> respond) => new(new HttpClient(new StubHandler(respond)) {BaseAddress=new Uri("http://localhost:5080/")});
    [Fact] public async Task ReadsCatalogContract() {
        var client=Client(request=> {Assert.Equal("/api/products",request.RequestUri!.AbsolutePath); return new(HttpStatusCode.OK) {Content=JsonContent.Create(new FakePos().Catalog)};});
        Assert.Equal(2,(await client.Products(default)).Count);
    }
    [Fact] public async Task ReadsOrderDetailContract() {
        var sale=new FakePos().Sales[0]; var client=Client(request=> {Assert.Equal($"/api/orders/{sale.Id}",request.RequestUri!.AbsolutePath); return new(HttpStatusCode.OK) {Content=JsonContent.Create(sale)};});
        Assert.Equal(sale.OrderNumber,(await client.Order(sale.Id,default))!.OrderNumber);
    }
    [Fact] public async Task MissingDetailIsNull() => Assert.Null(await Client(_=>new(HttpStatusCode.NotFound)).Product("missing",default));
    [Fact] public async Task HttpFailureIsWrapped() => await Assert.ThrowsAsync<PosUnavailableException>(()=>Client(_=>new(HttpStatusCode.ServiceUnavailable)).Products(default));
    [Fact] public async Task MalformedJsonIsWrapped() => await Assert.ThrowsAsync<PosUnavailableException>(()=>Client(_=>new(HttpStatusCode.OK) {Content=new StringContent("invalid")}).Orders(default));
}
public sealed class RenderingTests {
    [Theory] [InlineData("/","Overview")] [InlineData("/Admin/Products","Products")]
    [InlineData("/Admin/Inventory","Inventory")] [InlineData("/Admin/Orders","Orders")]
    public async Task RazorRenders(string path,string heading) {
        using var app=new WebApplicationFactory<Program>().WithWebHostBuilder(builder=>builder.ConfigureServices(services=> {
            services.RemoveAll<IPosClient>(); services.AddSingleton<IPosClient>(new FakePos());
        }));
        var response=await app.CreateClient().GetAsync(path);
        Assert.Equal(HttpStatusCode.OK,response.StatusCode); Assert.Contains(heading,await response.Content.ReadAsStringAsync());
        Assert.Contains("frame-ancestors 'none'",response.Headers.GetValues("Content-Security-Policy").Single());
    }
    [Fact] public async Task RazorEncodesProductNames() {
        var pos=new FakePos(); pos.Catalog=[pos.Catalog[0] with {Name="<script>alert(1)</script>"}];
        using var app=new WebApplicationFactory<Program>().WithWebHostBuilder(builder=>builder.ConfigureServices(services=> {
            services.RemoveAll<IPosClient>(); services.AddSingleton<IPosClient>(pos);
        }));
        var html=await app.CreateClient().GetStringAsync("/Admin/Products");
        Assert.Contains("&lt;script&gt;",html); Assert.DoesNotContain("<script>alert(1)</script>",html);
    }
}
