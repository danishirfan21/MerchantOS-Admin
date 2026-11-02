using MerchantOS.Admin.Services;
using MerchantOS.Operations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
if(args.Contains("--healthcheck")){Environment.ExitCode=await OperationalSecurity.HealthProbe("/health/ready");return;}
var builder = WebApplication.CreateBuilder(args);
OperationalSecurity.Configure(builder,"Admin");
builder.Services.AddHostedService<AdminSessionMaintenance>();
builder.WebHost.UseUrls(builder.Configuration["Urls"] ?? "http://127.0.0.1:5090");
builder.Services.AddControllersWithViews(options => options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()));
var localHttp=builder.Environment.IsDevelopment() && builder.Configuration.GetValue<bool>("Security:AllowLoopbackHttp");
if(builder.Configuration.GetValue<bool>("Security:AllowLoopbackHttp")&&!builder.Environment.IsDevelopment())throw new InvalidOperationException("Loopback HTTP is development-only.");
builder.Services.AddMemoryCache();builder.Services.AddHttpContextAccessor();builder.Services.AddSingleton<AdminTicketStore>();builder.Services.AddScoped<BackendAuth>();builder.Services.AddTransient<PosSessionHandler>();
builder.Services.AddAuthentication("Admin").AddCookie("Admin",options=>{
 options.Cookie.Name="MerchantOS.Admin.Session";options.Cookie.HttpOnly=true;options.Cookie.SameSite=SameSiteMode.Strict;options.Cookie.SecurePolicy=localHttp?CookieSecurePolicy.SameAsRequest:CookieSecurePolicy.Always;
 options.LoginPath="/Account/Login";options.AccessDeniedPath="/Account/Denied";options.ExpireTimeSpan=TimeSpan.FromMinutes(30);options.SlidingExpiration=false;
 options.Events.OnRedirectToLogin=context=>{if(context.Request.Path=="/Admin/ProductRows" || context.Request.Path.StartsWithSegments("/Management"))context.Response.StatusCode=401;else context.Response.Redirect(context.RedirectUri);return Task.CompletedTask;};
 options.Events.OnRedirectToAccessDenied=context=>{context.Response.StatusCode=403;return Task.CompletedTask;};
 options.Events.OnValidatePrincipal=async context=>{
  context.Properties.Items.TryGetValue("backend",out var cookie);BackendUser? user=null;
  try{if(cookie is not null)user=await context.HttpContext.RequestServices.GetRequiredService<BackendAuth>().Me(cookie,context.HttpContext.RequestAborted);}catch(Exception ex)when(ex is HttpRequestException or TaskCanceledException){throw new HttpRequestException("Identity authority temporarily unavailable.");}
  if(user?.Role!="Administrator"||!user.MfaVerified){context.RejectPrincipal();await context.HttpContext.SignOutAsync("Admin");}
 };
}).AddCookie("Pending",options=>{
 options.Cookie.Name="MerchantOS.Admin.Pending";options.Cookie.HttpOnly=true;options.Cookie.SameSite=SameSiteMode.Strict;options.Cookie.SecurePolicy=localHttp?CookieSecurePolicy.SameAsRequest:CookieSecurePolicy.Always;options.ExpireTimeSpan=TimeSpan.FromMinutes(5);options.SlidingExpiration=false;
});
builder.Services.AddOptions<CookieAuthenticationOptions>("Pending").Configure<AdminTicketStore>((o,s)=>o.SessionStore=s);
builder.Services.AddOptions<CookieAuthenticationOptions>("Admin").Configure<AdminTicketStore>((o,s)=>o.SessionStore=s);
builder.Services.AddAuthorization(options=>{options.FallbackPolicy=new AuthorizationPolicyBuilder().RequireRole("Administrator").Build();options.AddPolicy("OperationsRead",p=>p.RequireRole("Administrator"));});
builder.Services.AddAntiforgery(options=>{options.Cookie.Name="MerchantOS.Admin.Csrf";options.Cookie.HttpOnly=true;options.Cookie.SameSite=SameSiteMode.Strict;options.Cookie.SecurePolicy=localHttp?CookieSecurePolicy.SameAsRequest:CookieSecurePolicy.Always;});
void ConfigurePos(HttpClient client) {
    var url = new Uri(builder.Configuration["PosApi:BaseUrl"] ?? "http://127.0.0.1:5080/");
    if ((!OperationalSecurity.Enabled(builder.Configuration)&&!url.IsLoopback) || (url.Scheme != "http" && url.Scheme != "https") || (OperationalSecurity.Enabled(builder.Configuration)&&(url.Scheme!="https"||url.Host!=builder.Configuration["Operations:BackendHost"])))
        throw new InvalidOperationException("Milestone 1 requires a loopback POS API URL.");
    client.BaseAddress = url;
    client.Timeout = TimeSpan.FromSeconds(8);
}
builder.Services.AddHttpClient<IPosClient, PosClient>(ConfigurePos).ConfigurePrimaryHttpMessageHandler(()=>OperationalSecurity.HttpHandler(builder.Configuration)).AddHttpMessageHandler<CorrelationHandler>().AddHttpMessageHandler<PosSessionHandler>();
builder.Services.AddHttpClient("auth",ConfigurePos).ConfigurePrimaryHttpMessageHandler(()=>OperationalSecurity.HttpHandler(builder.Configuration)).AddHttpMessageHandler<CorrelationHandler>();
builder.Services.AddHttpClient<ManagementClient>(ConfigurePos).ConfigurePrimaryHttpMessageHandler(()=>OperationalSecurity.HttpHandler(builder.Configuration)).AddHttpMessageHandler<CorrelationHandler>().AddHttpMessageHandler<PosSessionHandler>();
builder.Services.AddScoped<AdminService>();
var app = builder.Build();
OperationalSecurity.InitializeKeys(app,args);
if(args.Contains("--rotate-keys")||args.Contains("--revoke-keys"))return;
app.UseOperationalBoundary();
app.UseExceptionHandler("/Admin/Error");
app.Use(async (context, next) => {
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "same-origin";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data:; object-src 'none'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";
    context.Response.Headers.CacheControl = "no-store";
    if(!context.Request.IsHttps && (!localHttp || context.Connection.RemoteIpAddress is not {} ip || !System.Net.IPAddress.IsLoopback(ip) || !Uri.TryCreate("http://"+context.Request.Host,UriKind.Absolute,out var host) || !host.IsLoopback)) {context.Response.StatusCode=400;return;}
    await next();
});
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();app.UseAuthorization();
app.MapGet("/health/live",()=>Results.Ok(new{status="alive"})).AllowAnonymous();
app.MapGet("/health/ready",async(AdminTicketStore tickets,IHttpClientFactory factory,CancellationToken ct)=>{
 try{if(!await tickets.Ready(ct))return Results.StatusCode(503);using var client=factory.CreateClient("auth");using var response=await client.GetAsync("api/health/ready",ct);return response.IsSuccessStatusCode?Results.Ok(new{status="ready"}):Results.StatusCode(503);}catch{return Results.StatusCode(503);}
}).AllowAnonymous();
app.MapGet("/ops/metrics",(OperationalMetrics metrics)=>Results.Ok(metrics.Snapshot())).RequireAuthorization("OperationsRead");
app.MapControllerRoute("default", "{controller=Admin}/{action=Index}/{id?}");
app.MapMonitor("/ops/monitor");
app.Run();
public partial class Program { }
