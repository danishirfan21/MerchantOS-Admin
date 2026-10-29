using MerchantOS.Admin.Models;
namespace MerchantOS.Admin.Services;
public sealed class AdminService(IPosClient client) {
    public async Task<CatalogVm> Catalog(string? search, string? category, string? stock, bool inventory, CancellationToken ct) {
        try {
            var all = await client.Products(ct);
            var filtered = all.Where(p => string.IsNullOrWhiteSpace(search) ||
                p.Name.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase) ||
                p.NameAr.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase) ||
                p.Sku.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase))
                .Where(p => string.IsNullOrWhiteSpace(category) || p.Category == category)
                .Where(p => stock switch { "low" => p.Stock is > 0 and <= 5, "out" => p.Stock == 0,
                    "available" => p.Stock > 5, _ => true });
            return new(filtered.OrderBy(p => p.Name).Select(ProductRow.From).ToList(),
                all.Select(p => p.Category).Distinct().Order().ToList(), search, category, stock, inventory, null,
                all.Count, all.Sum(p => (long)p.Stock), all.Count(p => p.Stock is > 0 and <= 5), all.Count(p => p.Stock == 0));
        }
        catch (PosUnavailableException ex) { return new([], [], search, category, stock, inventory, ex.Message, 0, 0, 0, 0); }
    }
    public async Task<OrdersVm> Orders(string? search, CancellationToken ct) {
        try {
            var orders = await client.Orders(ct);
            return new(orders.Where(o => string.IsNullOrWhiteSpace(search) ||
                o.OrderNumber.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(o => o.CreatedAt).ToList(), search, null);
        }
        catch (PosUnavailableException ex) { return new([], search, ex.Message); }
    }
    public async Task<DashboardVm> Dashboard(CancellationToken ct) {
        var catalog = Catalog(null, null, null, false, ct);
        var orders = Orders(null, ct);
        await Task.WhenAll(catalog, orders);
        return new(await catalog, await orders);
    }
}
