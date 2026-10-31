using MerchantOS.Admin.Models;
using MerchantOS.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace MerchantOS.Admin.Controllers;
[Authorize(Roles="Administrator")]
public sealed class ManagementController(ManagementClient client):Controller {
 private static bool ValidId(string id)=>System.Text.RegularExpressions.Regex.IsMatch(id??"","^[a-zA-Z0-9_-]{1,32}$");
 [HttpGet]public async Task<IActionResult> Editor(string id,CancellationToken ct)=>await Form(id,"_ProductEditor",ct);
 [HttpGet]public async Task<IActionResult> Adjustment(string id,CancellationToken ct)=>await Form(id,"_InventoryEditor",ct);
 private async Task<IActionResult> Form(string id,string view,CancellationToken ct) {
  if(!ValidId(id))return BadRequest();
  try{var p=await client.Product(id,ct);return p is null?NotFound():PartialView(view,p);}catch(HttpRequestException){return StatusCode(503,"Unable to load current data. Try again.");}
 }
 [HttpPost]public async Task<IActionResult> SaveProduct(ProductEdit input,CancellationToken ct) {
  var price=input.Halalas();if(price is null)ModelState.AddModelError("Price","Enter a price from 0.01 to 1000000.00 SAR, with at most two decimals.");
  if(!ModelState.IsValid)return ValidationProblem(ModelState);
  return await Write("api/admin/products/"+Uri.EscapeDataString(input.Id),new{input.Name,input.NameAr,input.Category,priceHalalas=price!.Value,input.Version},ct);
 }
 [HttpPost]public async Task<IActionResult> AdjustInventory(InventoryEdit input,CancellationToken ct) {
  if(!ModelState.IsValid)return ValidationProblem(ModelState);
  return await Write("api/admin/inventory/"+Uri.EscapeDataString(input.Id)+"/adjustments",new{input.Operation,input.Quantity,input.Reason,input.StockVersion,input.IdempotencyKey},ct);
 }
 private async Task<IActionResult> Write(string path,object value,CancellationToken ct) {
  try{using var r=await client.Write(path,value,ct);var body=await r.Content.ReadAsStringAsync(ct);return new ContentResult{Content=body,ContentType="application/json",StatusCode=(int)r.StatusCode};}
  catch(Exception ex)when(ex is HttpRequestException or TaskCanceledException){return StatusCode(503,new{message="Unable to confirm the change. Retry this unchanged request; stock adjustments retain the same request key."});}
 }
 [HttpGet]public async Task<IActionResult> History(string id,int page=1,CancellationToken ct=default) {
  if(!ValidId(id)||page is <1 or >10000)return BadRequest();
  try{return PartialView("_AuditHistory",new HistoryVm(id,await client.History(id,page,ct)));}catch(HttpRequestException){return StatusCode(503,"Unable to load history. Try again.");}
 }
}
