using MerchantOS.Admin.Services;
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(builder.Configuration["Urls"] ?? "http://127.0.0.1:5090");
builder.Services.AddControllersWithViews(options => options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddHttpClient<IPosClient, PosClient>(client => {
    var url = new Uri(builder.Configuration["PosApi:BaseUrl"] ?? "http://127.0.0.1:5080/");
    if (!url.IsLoopback || (url.Scheme != "http" && url.Scheme != "https"))
        throw new InvalidOperationException("Milestone 1 requires a loopback POS API URL.");
    client.BaseAddress = url;
    client.Timeout = TimeSpan.FromSeconds(8);
});
builder.Services.AddScoped<AdminService>();
var app = builder.Build();
app.UseExceptionHandler("/Admin/Error");
app.Use(async (context, next) => {
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "same-origin";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data:; object-src 'none'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";
    context.Response.Headers.CacheControl = "no-store";
    await next();
});
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute("default", "{controller=Admin}/{action=Index}/{id?}");
app.Run();
public partial class Program { }
