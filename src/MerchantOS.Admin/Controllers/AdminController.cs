using System.ComponentModel.DataAnnotations;
using MerchantOS.Admin.Services;
using Microsoft.AspNetCore.Mvc;
namespace MerchantOS.Admin.Controllers;
public sealed class AdminController(AdminService service, IPosClient client) : Controller {
    public async Task<IActionResult> Index(CancellationToken ct) => View(await service.Dashboard(ct));
    public async Task<IActionResult> Products([StringLength(120)] string? search, [StringLength(80)] string? category,
        [RegularExpression("^(low|out|available|attention)?$")] string? stock, CancellationToken ct, [Range(1, 10000)] int page = 1,
        [RegularExpression("^(name|price-desc|price-asc|stock-desc)$")] string sort = "price-desc") {
        if (!ModelState.IsValid) return BadRequest("Invalid filters.");
        return View("Catalog", await service.Catalog(search, category, stock, false, ct, page, sort));
    }
    public async Task<IActionResult> Inventory([StringLength(120)] string? search, [StringLength(80)] string? category,
        [RegularExpression("^(low|out|available|attention)?$")] string? stock, CancellationToken ct, [Range(1, 10000)] int page = 1,
        [RegularExpression("^(name|price-desc|price-asc|stock-desc)$")] string sort = "price-desc") {
        if (!ModelState.IsValid) return BadRequest("Invalid filters.");
        return View("Catalog", await service.Catalog(search, category, stock, true, ct, page, sort));
    }
    public async Task<IActionResult> CatalogSummary(bool inventory,CancellationToken ct) => PartialView("_CatalogSummary",await service.Catalog(null,null,null,inventory,ct));
    public async Task<IActionResult> ProductRows([StringLength(120)] string? search, [StringLength(80)] string? category,
        [RegularExpression("^(low|out|available|attention)?$")] string? stock, CancellationToken ct, [Range(1, 10000)] int page = 1,
        [RegularExpression("^(name|price-desc|price-asc|stock-desc)$")] string sort = "price-desc", bool inventory = false) {
        if (!ModelState.IsValid) return BadRequest("Invalid filters.");
        var model = await service.Catalog(search, category, stock, inventory, ct, page, sort);
        return model.Error is null ? Json(new { products = model.Products, pager = model.Pager, links = model.Pager.Links }) : StatusCode(503, new { message = model.Error });
    }
    public async Task<IActionResult> Product([Required, StringLength(32), RegularExpression("^[a-zA-Z0-9_-]+$")] string id, CancellationToken ct) {
        if (!ModelState.IsValid) return BadRequest();
        try { var product = await client.Product(id, ct); return product is null ? NotFound() : Request.Query["partial"] == "true" ? PartialView("_ProductDetails", product) : View(product); }
        catch (PosUnavailableException) { Response.StatusCode = 503; return View("Error"); }
    }
    public async Task<IActionResult> Orders([StringLength(120)] string? search, CancellationToken ct,
        [Range(1, 10000)] int page = 1, [RegularExpression("^(cash|card)?$")] string? payment = null,
        [RegularExpression("^(today|yesterday|week)?$")] string? range = null) {
        if (!ModelState.IsValid) return BadRequest("Invalid search.");
        return View(await service.Orders(search, ct, page, payment, range));
    }
    public async Task<IActionResult> Order(Guid id, CancellationToken ct) {
        try { var order = await client.Order(id, ct); return order is null ? NotFound() : View(order); }
        catch (PosUnavailableException) { Response.StatusCode = 503; return View("Error"); }
    }
    public IActionResult Error() { Response.StatusCode = 503; return View(); }
}
