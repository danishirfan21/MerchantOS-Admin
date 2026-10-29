namespace MerchantOS.Admin.Models;
public sealed record ProductDto(string Id, string Sku, string Name, string NameAr, string Category,
    long PriceHalalas, int Stock, string Color, string Icon, string? Badge);
public sealed record ReceiptLine(string ProductId, string Sku, string Name, int Quantity, long UnitPriceHalalas);
public sealed record OrderDto(Guid Id, string OrderNumber, DateTime CreatedAt, string Currency, string Status,
    long SubtotalHalalas, long DiscountHalalas, long VatHalalas, long PayableHalalas,
    string PaymentMethod, long ChangeHalalas, List<ReceiptLine>? Items);
public static class Display {
    public static string Money(long halalas, string currency = "SAR") => $"{currency} {halalas / 100m:N2}";
    public static string StockStatus(int stock) => stock == 0 ? "Out of stock" : stock <= 5 ? "Low stock" : "In stock";
}
public sealed record ProductRow(string Id, string Sku, string Name, string Category, string Price,
    int Stock, string StockStatus, string StatusClass) {
    public static ProductRow From(ProductDto product) => new(product.Id, product.Sku, product.Name,
        product.Category, Display.Money(product.PriceHalalas), product.Stock, Display.StockStatus(product.Stock),
        product.Stock == 0 ? "danger" : product.Stock <= 5 ? "warning" : "success");
}
public sealed record CatalogVm(List<ProductRow> Products, List<string> Categories, string? Search,
    string? Category, string? Stock, bool Inventory, string? Error, int TotalProducts, long TotalUnits,
    int LowStock, int OutOfStock);
public sealed record OrdersVm(List<OrderDto> Orders, string? Search, string? Error);
public sealed record DashboardVm(CatalogVm Catalog, OrdersVm Orders) {
    public int CompletedOrders => Orders.Orders.Count(o => o.Status == "completed");
    public long SalesHalalas => Orders.Orders.Where(o => o.Status == "completed" && o.Currency == "SAR").Sum(o => o.PayableHalalas);
}
