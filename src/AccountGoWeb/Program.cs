using AccountGoWeb.Components;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// The API URL is provided through an environment variable.
// In Docker Compose this will point to the API container.
// In Azure Container Apps this will point to the deployed API URL.
string apiurl = Environment.GetEnvironmentVariable("APIURL") ?? "http://localhost:8001/api/";
builder.Configuration["ApiUrl"] = apiurl;

Console.WriteLine($"[ASPNETCORE SERVER] API URL {builder.Configuration["ApiUrl"]}");

builder.Services.AddHttpClient();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o => o.LoginPath = new PathString("/account/signin"));

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(options => options.DetailedErrors = true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// HTTPS is handled outside the container by Azure Container Apps.
// The container itself runs HTTP internally.
// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAntiforgery();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();