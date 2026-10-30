namespace MerchantOS.Admin.Models;
public sealed record ProductDto(string Id, string Sku, string Name, string NameAr, string Category,
    long PriceHalalas, int Stock, string Color, string Icon, string? Badge);
public sealed record ReceiptLine(string ProductId, string Sku, string Name, int Quantity, long UnitPriceHalalas);
public sealed record OrderDto(Guid Id, string OrderNumber, DateTime CreatedAt, string Currency, string Status,
    long SubtotalHalalas, long DiscountHalalas, long VatHalalas, long PayableHalalas,
    string PaymentMethod, long ChangeHalalas, List<ReceiptLine>? Items);
public static class Display {
    public static string Money(long halalas, string currency = "SAR") => $"{currency} {halalas / 100m:N2}";
    public static string Payment(string method) => method == "card" ? "Simulated Card" : method == "cash" ? "Cash" : method;
    public static string Thumbnail(string id, string category = "") => id switch {
        "p01" => "/products/iphone15pro.jpg", "p02" => "/products/galaxy-s24.jpg",
        "p06" => "/products/airpods-pro.jpg", "p08" => "/products/sony-xm5.jpg",
        "p13" => "/products/watch6.jpg", _ => category switch {
            "Phones" => "/products/phones.svg", "Audio" => "/products/audio.svg",
            "Wearables" => "/products/wearables.svg", "Gaming" => "/products/gaming.svg",
            "Power" => "/products/power.svg", _ => "/products/accessories.svg" } };
    public static string StockStatus(int stock) => stock == 0 ? "Out of stock" : stock <= 5 ? "Low stock" : "In stock";
}
public sealed record ProductRow(string Id, string Sku, string Name, string Category, string Price,
    int Stock, string StockStatus, string StatusClass) {
    public string ImageUrl { get; init; } = "/products/accessories.svg";
    public long PriceHalalas { get; init; }
    public static ProductRow From(ProductDto product) => new(product.Id, product.Sku, product.Name,
        product.Category, Display.Money(product.PriceHalalas), product.Stock, Display.StockStatus(product.Stock),
        product.Stock == 0 ? "danger" : product.Stock <= 5 ? "warning" : "success") { ImageUrl = Display.Thumbnail(product.Id, product.Category), PriceHalalas = product.PriceHalalas };
}
public sealed record CatalogVm(List<ProductRow> Products, List<string> Categories, string? Search,
    string? Category, string? Stock, bool Inventory, string? Error, int TotalProducts, long TotalUnits,
    int LowStock, int OutOfStock) {
    public List<ProductRow> AllProducts { get; init; } = [];
    public int Page { get; init; } = 1;
    public int FilteredCount { get; init; }
    public string Sort { get; init; } = "price-desc";
    public int HealthyStock => TotalProducts - LowStock - OutOfStock;
    public PagerVm Pager => new(Page, 7, FilteredCount, Inventory ? "Inventory" : "Products",
        new() { ["search"] = Search, ["category"] = Category, ["stock"] = Stock, ["sort"] = Sort });
}
public sealed record OrdersVm(List<OrderDto> Orders, string? Search, string? Error) {
    public int Page { get; init; } = 1;
    public int FilteredCount { get; init; }
    public int WindowCount { get; init; }
    public string? Payment { get; init; }
    public string? Range { get; init; }
    public PagerVm Pager => new(Page, 7, FilteredCount, "Orders", new() { ["search"] = Search, ["payment"] = Payment, ["range"] = Range });
}
public sealed record DashboardVm(CatalogVm Catalog, OrdersVm Orders) {
    public List<HourBucket> Rhythm {
        get {
            var completed = Orders.Orders.Where(o => o.Status == "completed").ToList();
            if (completed.Count == 0) return [];
            var latest = completed.Max(o => o.CreatedAt.ToUniversalTime());
            var start = latest.Date.AddHours(Math.Clamp(latest.Hour - 8, 0, 15));
            return Enumerable.Range(0, 9).Select(i => new HourBucket(start.AddHours(i),
                completed.Count(o => o.CreatedAt.ToUniversalTime() >= start.AddHours(i) && o.CreatedAt.ToUniversalTime() < start.AddHours(i + 1)))).ToList();
        }
    }
    public int CompletedOrders => Orders.Orders.Count(o => o.Status == "completed");
    public long SalesHalalas => Orders.Orders.Where(o => o.Status == "completed" && o.Currency == "SAR").Sum(o => o.PayableHalalas);
}

public sealed record HourBucket(DateTime Hour, int Count);
public sealed record PageLink(string Label, string? Url, bool Current, bool Disabled);
public sealed record PagerVm(int Page, int PageSize, int Total, string Action, Dictionary<string, string?> Filters) {
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(Total / (double)PageSize));
    public int Start => Total == 0 ? 0 : (Page - 1) * PageSize + 1;
    public int End => Math.Min(Page * PageSize, Total);
    private string Url(int page) => Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(
        "/Admin/" + Action, Filters.Concat(new[] { new KeyValuePair<string, string?>("page", page.ToString()) }));
    public List<PageLink> Links => new[] { new PageLink("Previous", Page > 1 ? Url(Page - 1) : null, false, Page <= 1) }
        .Concat(Enumerable.Range(1, TotalPages).Select(p => new PageLink(p.ToString(), Url(p), p == Page, false)))
        .Append(new PageLink("Next", Page < TotalPages ? Url(Page + 1) : null, false, Page >= TotalPages)).ToList();
}
