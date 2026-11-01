using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using MerchantOS.Admin.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
namespace MerchantOS.Admin.Controllers;
public sealed class LoginModel {
 [Required,StringLength(80)]public string Username{get;set;}="";
 [Required,StringLength(128)]public string Password{get;set;}="";
 public string? ReturnUrl{get;set;}
}
public sealed class FactorModel {
 [RegularExpression("^[0-9]{6}$")]public string? Code{get;set;}
 [StringLength(68,MinimumLength=68)]public string? RecoveryCode{get;set;}
 [StringLength(128,MinimumLength=14)]public string? NewPassword{get;set;}
 [Compare(nameof(NewPassword))]public string? ConfirmPassword{get;set;}
 public bool Enrollment{get;set;}
 public bool MustChangePassword{get;set;}
 public string? Secret{get;set;}
 public string? QrData{get;set;}
 public DateTimeOffset ExpiresAt{get;set;}
}
public sealed class PasswordModel {
 [Required,StringLength(128)]public string CurrentPassword{get;set;}="";
 [Required,StringLength(128,MinimumLength=14)]public string NewPassword{get;set;}="";
 [Compare(nameof(NewPassword))]public string ConfirmPassword{get;set;}="";
}
public sealed record CodesModel(string[] Codes,string ReturnUrl);
public sealed class AccountController(BackendAuth backend):Controller {
 async Task<AuthenticateResult?> Pending(){var result=await HttpContext.AuthenticateAsync("Pending");return result.Succeeded&&result.Properties?.ExpiresUtc>DateTimeOffset.UtcNow?result:null;}
 async Task<BackendSession?> Current(){var auth=await HttpContext.AuthenticateAsync("Admin");if(auth.Succeeded&&auth.Properties?.Items.TryGetValue("backend",out var cookie)==true&&cookie is not null)return new(new(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),User.Identity!.Name!,"Administrator",true),cookie,auth.Properties.ExpiresUtc);return null;}
 static ClaimsPrincipal Principal(BackendUser user)=>new(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,user.Id.ToString()),new(ClaimTypes.Name,user.Username),new(ClaimTypes.Role,user.Role)],"Admin"));
 async Task Establish(BackendSession session){if(!session.User.MfaVerified)throw new InvalidOperationException("Backend MFA proof is mandatory.");await HttpContext.SignInAsync("Admin",Principal(session.User),new AuthenticationProperties{ExpiresUtc=session.ExpiresAt??DateTimeOffset.UtcNow.AddMinutes(30),AllowRefresh=false,Items={{"backend",session.Cookie}}});}
 string LocalReturn(string? value)=>Url.IsLocalUrl(value)?value!:"/";
 [AllowAnonymous,HttpGet]public async Task<IActionResult> Login(string? returnUrl=null){if(User.Identity?.IsAuthenticated==true)return LocalRedirect("/");if(Request.Cookies.ContainsKey("MerchantOS.Admin.Session"))await HttpContext.SignOutAsync("Admin");ModelState.Remove(nameof(LoginModel.ReturnUrl));return View(new LoginModel{ReturnUrl=LocalReturn(returnUrl)});}
 [AllowAnonymous,HttpPost]public async Task<IActionResult> Login(LoginModel model,CancellationToken ct){
  if(User.Identity?.IsAuthenticated==true)return LocalRedirect("/");model.ReturnUrl=LocalReturn(model.ReturnUrl);ModelState.Remove(nameof(LoginModel.ReturnUrl));BackendStart? start=null;
  if(ModelState.IsValid)try{start=await backend.Login(model.Username,model.Password,ct);}catch(Exception ex)when(ex is HttpRequestException or TaskCanceledException){}
  ModelState.Remove(nameof(model.Password));model.Password="";
  if(start is null){ModelState.AddModelError("","Unable to sign in. Check your credentials or try again later.");Response.StatusCode=401;return View(model);}
  await HttpContext.SignOutAsync("Pending");
  if(start.Challenge is {} challenge){var principal=new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,challenge.Id.ToString()),new(ClaimTypes.Role,"Pending")],"Pending"));await HttpContext.SignInAsync("Pending",principal,new AuthenticationProperties{ExpiresUtc=challenge.ExpiresAt,AllowRefresh=false,Items={{"backend",start.Cookie},{"challenge",JsonSerializer.Serialize(challenge)},{"return",model.ReturnUrl}}});return RedirectToAction("Mfa");}
  if(start.Session is null)return Unauthorized();await Establish(start.Session);return LocalRedirect(model.ReturnUrl);
 }
 [AllowAnonymous,HttpGet]public async Task<IActionResult> Mfa(CancellationToken ct){
  var pending=await Pending();if(pending is null)return RedirectToAction("Login");var challenge=JsonSerializer.Deserialize<BackendChallenge>(pending.Properties!.Items["challenge"]!)!;var model=new FactorModel{Enrollment=challenge.Step=="enroll",MustChangePassword=challenge.MustChangePassword,ExpiresAt=challenge.ExpiresAt};
  if(model.Enrollment){try{var info=await backend.Enrollment(pending.Properties.Items["backend"]!,ct);if(info is not null){model.Secret=info.Secret;using var generator=new QRCodeGenerator();using var data=generator.CreateQrCode(info.OtpAuthUri,QRCodeGenerator.ECCLevel.Q);using var svg=new SvgQRCode(data);model.QrData="data:image/svg+xml;base64,"+Convert.ToBase64String(Encoding.UTF8.GetBytes(svg.GetGraphic(4)));}else ModelState.AddModelError("","Setup information was already shown or expired. Use your saved authenticator, or cancel and start again.");}catch(Exception ex)when(ex is HttpRequestException or TaskCanceledException){ModelState.AddModelError("","Identity service unavailable. Cancel and retry when it recovers.");}}
  return View(model);
 }
 [AllowAnonymous,HttpPost]public async Task<IActionResult> Verify(FactorModel model,CancellationToken ct){
  var pending=await Pending();if(pending is null)return RedirectToAction("Login");var challenge=JsonSerializer.Deserialize<BackendChallenge>(pending.Properties!.Items["challenge"]!)!;var destination=LocalReturn(pending.Properties.Items["return"]);(BackendSession Session,string[] Codes)? complete=null;
  if(ModelState.IsValid&&(!string.IsNullOrEmpty(model.Code)||!string.IsNullOrEmpty(model.RecoveryCode)))try{complete=await backend.Complete(pending.Properties.Items["backend"]!,model.Code,model.RecoveryCode,model.NewPassword,ct);}catch(Exception ex)when(ex is HttpRequestException or TaskCanceledException){}
  if(complete is {} success){await Establish(success.Session);await HttpContext.SignOutAsync("Pending");return success.Codes.Length>0?View("RecoveryCodes",new CodesModel(success.Codes,destination)):LocalRedirect(destination);}
  foreach(var field in new[]{nameof(model.Code),nameof(model.RecoveryCode),nameof(model.NewPassword),nameof(model.ConfirmPassword)})ModelState.Remove(field);
  model.Code=model.RecoveryCode=model.NewPassword=model.ConfirmPassword=null;model.Secret=model.QrData=null;model.Enrollment=challenge.Step=="enroll";model.MustChangePassword=challenge.MustChangePassword;model.ExpiresAt=challenge.ExpiresAt;
  ModelState.AddModelError("","Unable to verify. Check your authenticator/recovery code and password policy, or cancel an expired challenge and sign in again.");Response.StatusCode=401;return View("Mfa",model);
 }
 [AllowAnonymous,HttpPost]public async Task<IActionResult> Cancel(CancellationToken ct){var pending=await Pending();try{if(pending?.Properties?.Items["backend"] is {} cookie)await backend.Cancel(cookie,ct);}catch(Exception ex)when(ex is HttpRequestException or TaskCanceledException){}await HttpContext.SignOutAsync("Pending");return RedirectToAction("Login");}
 [Authorize,HttpGet]public IActionResult Password()=>View(new PasswordModel());
 [Authorize,HttpPost]public async Task<IActionResult> Password(PasswordModel model,CancellationToken ct){
  var session=await Current();int status=400;if(ModelState.IsValid&&session is not null)try{status=await backend.ChangePassword(session.Cookie,model.CurrentPassword,model.NewPassword,ct);}catch(Exception ex)when(ex is HttpRequestException or TaskCanceledException){status=503;}
  if(status==204){await HttpContext.SignOutAsync("Admin");return RedirectToAction("Login");}
  ModelState.Clear();ModelState.AddModelError("","Password change failed. Verify the current password, use 14–128 characters, and sign in with MFA again if verification is older than five minutes.");Response.StatusCode=status;return View(new PasswordModel());
 }
 [Authorize,HttpPost]public async Task<IActionResult> Regenerate([StringLength(128)]string currentPassword,[RegularExpression("^[0-9]{6}$")]string code,CancellationToken ct){var session=await Current();string[]? codes=null;if(ModelState.IsValid&&session is not null)try{codes=await backend.Regenerate(session.Cookie,currentPassword,code,ct);}catch(Exception ex)when(ex is HttpRequestException or TaskCanceledException){}if(codes is null){ModelState.Clear();ModelState.AddModelError("","Recovery regeneration requires your current password and a fresh, unused authenticator code.");Response.StatusCode=401;return View("Password",new PasswordModel());}await HttpContext.SignOutAsync("Admin");return View("RecoveryCodes",new CodesModel(codes,"/Account/Login"));}
 [Authorize,HttpPost]public async Task<IActionResult> Logout(CancellationToken ct){var session=await HttpContext.AuthenticateAsync("Admin");try{if(session.Properties?.Items.TryGetValue("backend",out var cookie)==true&&cookie is not null)await backend.Logout(cookie,ct);}catch(Exception ex)when(ex is HttpRequestException or TaskCanceledException){Response.StatusCode=503;return Content("Unable to revoke the backend session. Retry sign out.");}await HttpContext.SignOutAsync("Admin");await HttpContext.SignOutAsync("Pending");return RedirectToAction("Login");}
 [AllowAnonymous]public IActionResult Denied(){Response.StatusCode=403;return Content("Access denied.");}
}
