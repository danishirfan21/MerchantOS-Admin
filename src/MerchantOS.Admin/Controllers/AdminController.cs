using System.ComponentModel.DataAnnotations;
using MerchantOS.Admin.Services;
using Microsoft.AspNetCore.Mvc;
namespace MerchantOS.Admin.Controllers;
public sealed class AdminController(AdminService service, IPosClient client) : Controller {
    public async Task<IActionResult> Index(CancellationToken ct) => View(await service.Dashboard(ct));
    public async Task<IActionResult> Products([StringLength(120)] string? search, [StringLength(80)] string? category,
        [RegularExpression("^(low|out|available)?$")] string? stock, CancellationToken ct) {
        if (!ModelState.IsValid) return BadRequest("Invalid filters.");
        return View("Catalog", await service.Catalog(search, category, stock, false, ct));
    }
    public async Task<IActionResult> Inventory([StringLength(120)] string? search, [StringLength(80)] string? category,
        [RegularExpression("^(low|out|available)?$")] string? stock, CancellationToken ct) {
        if (!ModelState.IsValid) return BadRequest("Invalid filters.");
        return View("Catalog", await service.Catalog(search, category, stock, true, ct));
    }
    public async Task<IActionResult> ProductRows([StringLength(120)] string? search, [StringLength(80)] string? category,
        [RegularExpression("^(low|out|available)?$")] string? stock, CancellationToken ct) {
        if (!ModelState.IsValid) return BadRequest("Invalid filters.");
        var model = await service.Catalog(search, category, stock, false, ct);
        return model.Error is null ? Json(model.Products) : StatusCode(503, new { message = model.Error });
    }
    public async Task<IActionResult> Product([Required, StringLength(32), RegularExpression("^[a-zA-Z0-9_-]+$")] string id, CancellationToken ct) {
        if (!ModelState.IsValid) return BadRequest();
        try { var product = await client.Product(id, ct); return product is null ? NotFound() : View(product); }
        catch (PosUnavailableException) { Response.StatusCode = 503; return View("Error"); }
    }
    public async Task<IActionResult> Orders([StringLength(120)] string? search, CancellationToken ct) {
        if (!ModelState.IsValid) return BadRequest("Invalid search.");
        return View(await service.Orders(search, ct));
    }
    public async Task<IActionResult> Order(Guid id, CancellationToken ct) {
        try { var order = await client.Order(id, ct); return order is null ? NotFound() : View(order); }
        catch (PosUnavailableException) { Response.StatusCode = 503; return View("Error"); }
    }
    public IActionResult Error() { Response.StatusCode = 503; return View(); }
}
