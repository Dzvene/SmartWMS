using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using SmartWMS.API.Common.Middleware;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Infrastructure.Identity;
using SmartWMS.API.Modules.Auth.Services;
using SmartWMS.API.Modules.Users.Services;
using SmartWMS.API.Modules.Sites.Services;
using SmartWMS.API.Modules.Warehouse.Services;
using SmartWMS.API.Modules.Inventory.Services;
using SmartWMS.API.Modules.Equipment.Services;
using SmartWMS.API.Modules.Orders.Services;
using SmartWMS.API.Modules.Fulfillment.Services;
using SmartWMS.API.Modules.Receiving.Services;
using SmartWMS.API.Modules.Reports.Services;
using SmartWMS.API.Modules.Putaway.Services;
using SmartWMS.API.Modules.Packing.Services;
using SmartWMS.API.Modules.Returns.Services;
using SmartWMS.API.Modules.Carriers.Services;
using SmartWMS.API.Modules.CycleCount.Services;
using SmartWMS.API.Modules.Configuration.Services;
using SmartWMS.API.Modules.Adjustments.Services;
using SmartWMS.API.Modules.Transfers.Services;
using SmartWMS.API.Modules.Shipping.Services;
using SmartWMS.API.Modules.Audit.Services;
using SmartWMS.API.Modules.Roles.Services;
using SmartWMS.API.Modules.Notifications.Services;
using SmartWMS.API.Modules.Integrations.Services;
using SmartWMS.API.Modules.Sessions.Services;
using SmartWMS.API.Modules.Dashboard.Services;
using SmartWMS.API.Modules.OperationHub.Services;
using SmartWMS.API.Modules.Automation.Services;
using SmartWMS.API.Hubs;
using SmartWMS.API.Infrastructure.Services.Email;

// ======================
// Serilog Configuration
// ======================
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/smartwms-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting SmartWMS API");

var builder = WebApplication.CreateBuilder(args);

// Use Serilog
builder.Host.UseSerilog();

// ======================
// Configuration
// ======================
var configuration = builder.Configuration;

// JWT Settings - Environment variables take precedence over appsettings
var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? configuration["JwtSettings:SecretKey"]
    ?? throw new InvalidOperationException("JWT secret key not configured. Set JWT_SECRET_KEY environment variable.");

var jwtSettings = new JwtSettings
{
    SecretKey = jwtSecretKey,
    Issuer = configuration["JwtSettings:Issuer"] ?? "SmartWMS",
    Audience = configuration["JwtSettings:Audience"] ?? "SmartWMS.Client",
    AccessTokenExpirationMinutes = int.TryParse(configuration["JwtSettings:AccessTokenExpirationMinutes"], out var accessExp) ? accessExp : 60,
    RefreshTokenExpirationDays = int.TryParse(configuration["JwtSettings:RefreshTokenExpirationDays"], out var refreshExp) ? refreshExp : 7
};

builder.Services.AddSingleton(jwtSettings);
builder.Services.Configure<JwtSettings>(opts =>
{
    opts.SecretKey = jwtSettings.SecretKey;
    opts.Issuer = jwtSettings.Issuer;
    opts.Audience = jwtSettings.Audience;
    opts.AccessTokenExpirationMinutes = jwtSettings.AccessTokenExpirationMinutes;
    opts.RefreshTokenExpirationDays = jwtSettings.RefreshTokenExpirationDays;
});

// Log warning if using default secret (development only)
if (jwtSettings.SecretKey.Contains("CHANGE_THIS") && !builder.Environment.IsDevelopment())
{
    Log.Warning("JWT Secret Key appears to be the default value. This is a security risk in production!");
}

// SMTP Settings for Email Service
builder.Services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

// ======================
// Database
// ======================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
    ));

// ======================
// Identity
// ======================
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ======================
// Authentication (JWT)
// ======================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ======================
// Services
// ======================
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<ISitesService, SitesService>();
builder.Services.AddScoped<IZonesService, ZonesService>();
builder.Services.AddScoped<IWarehousesService, WarehousesService>();
builder.Services.AddScoped<ILocationsService, LocationsService>();
builder.Services.AddScoped<IProductsService, ProductsService>();
builder.Services.AddScoped<IProductCategoriesService, ProductCategoriesService>();
builder.Services.AddScoped<IEquipmentService, EquipmentService>();
builder.Services.AddScoped<ICustomersService, CustomersService>();
builder.Services.AddScoped<ISuppliersService, SuppliersService>();
builder.Services.AddScoped<ISalesOrdersService, SalesOrdersService>();
builder.Services.AddScoped<IPurchaseOrdersService, PurchaseOrdersService>();
builder.Services.AddScoped<IFulfillmentBatchesService, FulfillmentBatchesService>();
builder.Services.AddScoped<IPickTasksService, PickTasksService>();
builder.Services.AddScoped<IShipmentsService, ShipmentsService>();
builder.Services.AddScoped<IGoodsReceiptService, GoodsReceiptService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IReportsService, ReportsService>();
builder.Services.AddScoped<IPutawayService, PutawayService>();
builder.Services.AddScoped<IPackingService, PackingService>();
builder.Services.AddScoped<IReturnsService, ReturnsService>();
builder.Services.AddScoped<ICarriersService, CarriersService>();
builder.Services.AddScoped<ICycleCountService, CycleCountService>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<IAdjustmentsService, AdjustmentsService>();
builder.Services.AddScoped<ITransfersService, TransfersService>();
builder.Services.AddScoped<IShippingService, ShippingService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IRolesService, RolesService>();
builder.Services.AddScoped<INotificationsService, NotificationsService>();
builder.Services.AddScoped<IIntegrationsService, IntegrationsService>();
builder.Services.AddScoped<ISessionsService, SessionsService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IOperationHubService, OperationHubService>();
builder.Services.AddScoped<IRuleEngine, RuleEngine>();
builder.Services.AddScoped<IActionExecutor, ActionExecutor>();
builder.Services.AddScoped<IAutomationService, AutomationService>();
builder.Services.AddScoped<IAutomationEventPublisher, AutomationEventPublisher>();
builder.Services.AddHostedService<AutomationSchedulerService>();
builder.Services.AddHttpClient("AutomationWebhook"); // For webhook actions
builder.Services.AddScoped<DatabaseSeeder>();

// ======================
// SignalR
// ======================
builder.Services.AddSignalR();
builder.Services.AddScoped<IWarehouseNotifier, WarehouseNotifier>();

// ======================
// Controllers
// ======================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Позволяет использовать строки для enum (например "Picking" вместо 1)
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// ======================
// CORS
// ======================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });

    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins(configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ======================
// Swagger
// ======================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SmartWMS API",
        Version = "v1",
        Description = "Warehouse Management System API - Built with .NET 8"
    });

    // Use full type names to avoid schema conflicts between modules
    c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));

    // JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ======================
// Database Seeding
// ======================
// Skip seeding for Testing environment (handled by test infrastructure)
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

// ======================
// Middleware Pipeline
// ======================

// Global exception handler - MUST be first
app.UseGlobalExceptionHandler();

// Request logging
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
});

// Development-only middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartWMS API v1");
        c.RoutePrefix = string.Empty; // Swagger at root
    });
}

app.UseHttpsRedirection();

// CORS - before auth
app.UseCors(app.Environment.IsDevelopment() ? "AllowAll" : "Production");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Map SignalR hub
app.MapHub<WarehouseHub>("/hubs/warehouse");

// ======================
// Run
// ======================
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for WebApplicationFactory
public partial class Program { }
