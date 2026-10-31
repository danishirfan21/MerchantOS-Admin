using System.ComponentModel.DataAnnotations;
using System.Text.Json;
namespace MerchantOS.Admin.Models;
public sealed record ManagedProduct(string Id,string Sku,string Name,string NameAr,string Category,long PriceHalalas,long Version,int Stock,long StockVersion);
public sealed class ProductEdit {
 [Required,StringLength(32),RegularExpression("^[a-zA-Z0-9_-]+$")] public string Id {get;set;}="";
 [Required,StringLength(240)]public string Name {get;set;}="";
 [Required,StringLength(240)]public string NameAr {get;set;}="";
 [Required,RegularExpression("^(Phones|Audio|Wearables|Gaming|Power|Accessories)$")]public string Category {get;set;}="";
 [Required,RegularExpression(@"^\d{1,7}(\.\d{1,2})?$")]public string Price {get;set;}="";
 [Range(1,long.MaxValue)]public long Version {get;set;}
 public long? Halalas()=>decimal.TryParse(Price,System.Globalization.NumberStyles.AllowDecimalPoint,System.Globalization.CultureInfo.InvariantCulture,out var n)&&n>0&&n<=1000000?(long)(n*100):null;
}
public sealed class InventoryEdit {
 [Required,StringLength(32),RegularExpression("^[a-zA-Z0-9_-]+$")]public string Id {get;set;}="";
 [Required,RegularExpression("^(restock|damage|correction)$")]public string Operation {get;set;}="restock";
 [Range(0,int.MaxValue)]public int Quantity {get;set;}
 [Required,StringLength(500)]public string Reason {get;set;}="";
 [Range(1,long.MaxValue)]public long StockVersion {get;set;}
 [Required,StringLength(128)]public string IdempotencyKey {get;set;}="";
}
public sealed record AuditItem(Guid Id,DateTime Timestamp,Guid ActorId,string Administrator,string EntityType,string EntityId,string Action,JsonElement PreviousValues,JsonElement NewValues,string? Reason,Guid RequestId);
public sealed record AuditPage(List<AuditItem> Items,int Page,int PageSize,bool HasMore);
public sealed record HistoryVm(string ProductId,AuditPage History);
