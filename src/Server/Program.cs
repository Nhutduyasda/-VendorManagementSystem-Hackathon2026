using System.Security.Claims;
using System.Text;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VendorManagementSystem.Server.Configuration;
using VendorManagementSystem.Server.Data;
using VendorManagementSystem.Server.Hubs;
using VendorManagementSystem.Server.Services;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// ──────────────────────────────────────────────
// Configuration binding
// ──────────────────────────────────────────────
var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? new JwtSettings();
builder.Services.AddSingleton(jwtSettings);

var cloudinarySettings = configuration.GetSection(CloudinarySettings.SectionName).Get<CloudinarySettings>()
    ?? new CloudinarySettings();
builder.Services.AddSingleton(cloudinarySettings);

// ──────────────────────────────────────────────
// EF Core + SQL Server
// ──────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

// ──────────────────────────────────────────────
// ASP.NET Core Identity
// ──────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ──────────────────────────────────────────────
// Authentication: JWT + Cookie (dual scheme)
// ──────────────────────────────────────────────
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };

        // Allow SignalR to receive token from query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    })
    .AddCookie("Cookies", options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// ──────────────────────────────────────────────
// CORS
// ──────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorWasmPolicy", policy =>
    {
        policy.WithOrigins(
                configuration.GetSection("AllowedOrigins").Get<string[]>()
                ?? ["https://localhost:5001", "https://localhost:7250"])
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// ──────────────────────────────────────────────
// Controllers + OpenAPI (native .NET 10)
// ──────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// ──────────────────────────────────────────────
// SignalR
// ──────────────────────────────────────────────
builder.Services.AddSignalR();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes
        .Concat(["application/octet-stream"]);
});

// ──────────────────────────────────────────────
// Cloudinary
// ──────────────────────────────────────────────
builder.Services.AddSingleton(_ =>
{
    var account = new Account(
        cloudinarySettings.CloudName,
        cloudinarySettings.ApiKey,
        cloudinarySettings.ApiSecret);
    return new Cloudinary(account);
});

// ──────────────────────────────────────────────
// Application Services (DI)
// ──────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

// ──────────────────────────────────────────────
// Blazor WASM hosting
// ──────────────────────────────────────────────
builder.Services.AddRazorComponents();

var app = builder.Build();

// ──────────────────────────────────────────────
// Middleware pipeline
// ──────────────────────────────────────────────
app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/openapi/v1.json", "VMS API v1"));
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("BlazorWasmPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapFallbackToFile("index.html");

// ──────────────────────────────────────────────
// Seed data on startup
// ──────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    await DataSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();
