using Microsoft.AspNetCore.WebUtilities;
namespace MerchantOS.Admin.Models;
public sealed record ReportPeriod(string Range,DateOnly? From,DateOnly? To,DateTime? StartUtc,DateTime? EndUtc,string TimeZone);
public sealed record ReportFilters(string? Range=null,string? From=null,string? To=null,string? Search=null,string? Payment=null,string? Status=null) {
 public Dictionary<string,string?> Values()=>new(){["range"]=Range,["from"]=Range=="custom"?From:null,["to"]=Range=="custom"?To:null,["search"]=Search,["paymentMethod"]=Payment,["status"]=Status};
 public string Query(string path,int? page=null,int? pageSize=null){var values=Values();if(page is not null)values["page"]=page.ToString();if(pageSize is not null)values["pageSize"]=pageSize.ToString();return QueryHelpers.AddQueryString(path,values.Where(x=>!string.IsNullOrEmpty(x.Value)));}
 public string ExportUrl()=>QueryHelpers.AddQueryString("/Admin/ExportOrders",new Dictionary<string,string?>{["range"]=Range,["from"]=Range=="custom"?From:null,["to"]=Range=="custom"?To:null,["search"]=Search,["payment"]=Payment,["status"]=Status}.Where(x=>!string.IsNullOrEmpty(x.Value)));
}
public sealed record OrderPage(List<OrderDto> Items,int Page,int PageSize,long Total,bool HasMore);
public sealed record SalesSummary(long GrossHalalas,long DiscountsHalalas,long NetBeforeVatHalalas,long VatHalalas,long RevenueHalalas,long CompletedOrders,long AverageOrderHalalas);
public sealed record SalesBucket(string Label,long RevenueHalalas,long CompletedOrders);
public sealed record PaymentSales(string Method,long CompletedOrders,long RevenueHalalas,decimal RevenueShare);
public sealed record TopSale(string ProductId,string Name,long UnitsSold,long GrossHalalas);
public sealed record SalesReport(ReportPeriod Period,SalesSummary Summary,string Granularity,List<SalesBucket> Trend,List<PaymentSales> PaymentMethods,List<TopSale> TopProducts);
public sealed class ReportRejectedException(int status):Exception(status==413?"Export exceeds 10,000 orders. Narrow the filters.":"Choose a valid reporting period and filters. Custom ranges must be 1–366 past/current store days.") {public int Status {get;}=status;}
