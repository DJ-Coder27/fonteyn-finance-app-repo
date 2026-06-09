using System;
using Api.Data;
using Api.Data.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver =
            new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();

        options.SerializerSettings.ReferenceLoopHandling =
            Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Read database connection string from configuration/environment variable.
// In Azure Container Apps this will come from a secret/environment variable.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
}

// Database contexts
builder.Services
    .AddDbContext<ApiDbContext>(options => options.UseSqlServer(connectionString))
    .AddDbContext<ApplicationIdentityDbContext>(options => options.UseSqlServer(connectionString));

// Identity
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
    .AddDefaultTokenProviders();

// CORS
var AllowAllOrigins = "AllowAll";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowAllOrigins, policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Generic repository
builder.Services.AddScoped(typeof(Core.Data.IRepository<>), typeof(EfRepository<>));

// Custom repositories
builder.Services.AddScoped(typeof(Core.Data.ISalesOrderRepository), typeof(SalesOrderRepository));
builder.Services.AddScoped(typeof(Core.Data.IPurchaseOrderRepository), typeof(PurchaseOrderRepository));
builder.Services.AddScoped(typeof(Core.Data.ISecurityRepository), typeof(SecurityRepository));

// Domain services
builder.Services.AddScoped(typeof(Services.Sales.ISalesService), typeof(Services.Sales.SalesService));
builder.Services.AddScoped(typeof(Services.Financial.IFinancialService), typeof(Services.Financial.FinancialService));
builder.Services.AddScoped(typeof(Services.Inventory.IInventoryService), typeof(Services.Inventory.InventoryService));
builder.Services.AddScoped(typeof(Services.Purchasing.IPurchasingService), typeof(Services.Purchasing.PurchasingService));
builder.Services.AddScoped(typeof(Services.Administration.IAdministrationService), typeof(Services.Administration.AdministrationService));
builder.Services.AddScoped(typeof(Services.Security.ISecurityService), typeof(Services.Security.SecurityService));
builder.Services.AddScoped(typeof(Services.TaxSystem.ITaxService), typeof(Services.TaxSystem.TaxService));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS is handled outside the container by Azure Container Apps.
// app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(AllowAllOrigins);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Apply pending EF migrations when the API starts.
// This allows the empty Azure SQL database to receive the required tables.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var apiDbContext = services.GetRequiredService<ApiDbContext>();
    apiDbContext.Database.Migrate();

    var identityDbContext = services.GetRequiredService<ApplicationIdentityDbContext>();
    identityDbContext.Database.Migrate();
}

app.Run();