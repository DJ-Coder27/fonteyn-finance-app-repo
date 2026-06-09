using AccountGoWeb.Components;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

string apiurl = System.Environment.GetEnvironmentVariable("APIURL") ?? "http://localhost:8001/api/";
builder.Configuration["ApiUrl"] = apiurl;
System.Console.WriteLine($"[ASPNETCORE SERVER] API URL {builder.Configuration["ApiUrl"]}");

// Microsoft Entra ID SSO authentication.
// AzureAd values will be provided through Azure Container Apps environment variables/secrets.
builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services.Configure<CookieAuthenticationOptions>(
    CookieAuthenticationDefaults.AuthenticationScheme,
    options =>
    {
        options.LoginPath = "/account/signin";
        options.LogoutPath = "/account/signout";
        options.AccessDeniedPath = "/account/accessdenied";
    });

builder.Services.AddAuthorization();

builder.Services.AddHttpClient();

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(options => options.DetailedErrors = true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// HTTPS is handled by Azure Container Apps.
// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

// Custom gate: show the custom login page first before users enter the Finance App.
app.Use(async (context, next) =>
{
    var path = context.Request.Path;

    var isAllowedWithoutLogin =
        path.StartsWithSegments("/account") ||
        path.StartsWithSegments("/signin-oidc") ||
        path.StartsWithSegments("/signout-callback-oidc") ||
        path.StartsWithSegments("/css") ||
        path.StartsWithSegments("/js") ||
        path.StartsWithSegments("/lib") ||
        path.StartsWithSegments("/images") ||
        path.StartsWithSegments("/plugins") ||
        path.StartsWithSegments("/_framework") ||
        path == "/favicon.ico";

    if (context.User?.Identity?.IsAuthenticated != true && !isAllowedWithoutLogin)
    {
        context.Response.Redirect("/account/signin");
        return;
    }

    await next();
});

app.UseAntiforgery();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();