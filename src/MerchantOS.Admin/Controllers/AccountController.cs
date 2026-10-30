using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using MerchantOS.Admin.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace MerchantOS.Admin.Controllers;
public sealed class LoginModel {
    [Required,StringLength(80)] public string Username {get;set;}="";
    [Required,StringLength(128)] public string Password {get;set;}="";
    public string? ReturnUrl {get;set;}
}
public sealed class AccountController(BackendAuth backend) : Controller {
    [AllowAnonymous,HttpGet] public async Task<IActionResult> Login(string? returnUrl=null) {
        if(User.Identity?.IsAuthenticated==true)return LocalRedirect("/");
        if(Request.Cookies.ContainsKey("MerchantOS.Admin.Session"))await HttpContext.SignOutAsync("Admin");
        ModelState.Remove(nameof(LoginModel.ReturnUrl)); return View(new LoginModel {ReturnUrl=Url.IsLocalUrl(returnUrl)?returnUrl:"/"});
    }
    [AllowAnonymous,HttpPost] public async Task<IActionResult> Login(LoginModel model,CancellationToken ct) {
        if(User.Identity?.IsAuthenticated==true) return LocalRedirect("/");
        if(!Url.IsLocalUrl(model.ReturnUrl))model.ReturnUrl="/"; ModelState.Remove(nameof(LoginModel.ReturnUrl));
        BackendSession? session=null;
        if(ModelState.IsValid){try{session=await backend.Login(model.Username,model.Password,ct);}catch(Exception ex)when(ex is HttpRequestException or TaskCanceledException){}}
        ModelState.Remove(nameof(model.Password));model.Password="";
        if(session is null){ModelState.AddModelError("","Unable to sign in. Check your credentials or try again later.");Response.StatusCode=401;return View(model);}
        var principal=new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,session.User.Id.ToString()),new(ClaimTypes.Name,session.User.Username),new(ClaimTypes.Role,session.User.Role)],"Admin"));
        await HttpContext.SignInAsync("Admin",principal,new AuthenticationProperties {ExpiresUtc=DateTimeOffset.UtcNow.AddMinutes(30),AllowRefresh=false,Items={{"backend",session.Cookie}}});
        return LocalRedirect(model.ReturnUrl ?? "/");
    }
    [Authorize, HttpPost] public async Task<IActionResult> Logout(CancellationToken ct) {
        var session=await HttpContext.AuthenticateAsync("Admin");
        try{if(session.Properties?.Items.TryGetValue("backend",out var cookie)==true&&cookie is not null)await backend.Logout(cookie,ct);}
        catch(Exception ex)when(ex is HttpRequestException or TaskCanceledException){Response.StatusCode=503;return Content("Unable to revoke the backend session. Retry sign out.");}
        await HttpContext.SignOutAsync("Admin");return RedirectToAction("Login");
    }
    [AllowAnonymous] public IActionResult Denied(){Response.StatusCode=403;return Content("Access denied.");}
}
