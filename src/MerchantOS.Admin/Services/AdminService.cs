using MerchantOS.Admin.Models;
namespace MerchantOS.Admin.Services;
public sealed class AdminService(IPosClient client) {
    public async Task<CatalogVm> Catalog(string? search, string? category, string? stock, bool inventory,
        CancellationToken ct, int page = 1, string sort = "price-desc", bool paginate = true) {
        try {
            var all = await client.Products(ct);
            var filtered = all.Where(p => string.IsNullOrWhiteSpace(search) ||
                p.Name.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase) ||
                p.NameAr.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase) ||
                p.Sku.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase))
                .Where(p => string.IsNullOrWhiteSpace(category) || p.Category == category)
                .Where(p => stock switch { "low" => p.Stock is > 0 and <= 5, "out" => p.Stock == 0,
                    "attention" => p.Stock <= 5, "available" => p.Stock > 5, _ => true });
            filtered = sort switch {
                "price-desc" => filtered.OrderByDescending(p => p.PriceHalalas).ThenBy(p => p.Name),
                "price-asc" => filtered.OrderBy(p => p.PriceHalalas).ThenBy(p => p.Name),
                "stock-desc" => filtered.OrderByDescending(p => p.Stock).ThenBy(p => p.Name),
                _ => filtered.OrderBy(p => p.Name) };
            var rows = filtered.Select(ProductRow.From).ToList();
            page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(rows.Count / 7d)));
            return new(paginate ? rows.Skip((page - 1) * 7).Take(7).ToList() : rows,
                all.Select(p => p.Category).Distinct().Order().ToList(), search, category, stock, inventory, null,
                all.Count, all.Sum(p => (long)p.Stock), all.Count(p => p.Stock is > 0 and <= 5), all.Count(p => p.Stock == 0)) {
                AllProducts = all.Select(ProductRow.From).ToList(), Page = page, FilteredCount = rows.Count, Sort = sort };
        }
        catch (PosUnavailableException ex) { return new([], [], search, category, stock, inventory, ex.Message, 0, 0, 0, 0); }
    }
    public async Task<OrdersVm> Orders(string? search, CancellationToken ct, int page = 1,
        string? payment = null, string? range = null, bool paginate = true) {
        try {
            var all = await client.Orders(ct);
            var today = DateTime.UtcNow.Date;
            var filtered = all.Where(o => string.IsNullOrWhiteSpace(search) ||
                o.OrderNumber.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase))
                .Where(o => string.IsNullOrEmpty(payment) || o.PaymentMethod == payment)
                .Where(o => range switch { "today" => o.CreatedAt.ToUniversalTime().Date == today,
                    "yesterday" => o.CreatedAt.ToUniversalTime().Date == today.AddDays(-1),
                    "week" => o.CreatedAt.ToUniversalTime() >= today.AddDays(-6) && o.CreatedAt.ToUniversalTime() < today.AddDays(1), _ => true })
                .OrderByDescending(o => o.CreatedAt).ToList();
            page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(filtered.Count / 7d)));
            return new(paginate ? filtered.Skip((page - 1) * 7).Take(7).ToList() : filtered, search, null) {
                Page = page, FilteredCount = filtered.Count, WindowCount = all.Count, Payment = payment, Range = range };
        }
        catch (PosUnavailableException ex) { return new([], search, ex.Message); }
    }
    public async Task<DashboardVm> Dashboard(CancellationToken ct) {
        var catalog = Catalog(null, null, null, false, ct, paginate: false);
        var orders = Orders(null, ct, paginate: false);
        await Task.WhenAll(catalog, orders);
        return new(await catalog, await orders);
    }
}
