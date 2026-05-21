using System.Text;
using FaqApp.Api.Data;
using FaqApp.Api.Services;
using FaqApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FaqApp.Api.Settings;
using FaqApp.Api.Data.Seed;
using FaqApp.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection Ç™ê›íËÇ≥ÇÍÇƒÇ¢Ç‹ÇπÇÒÅB");

    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 4, 0)));
});

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

builder.Services.AddScoped<IFaqService, FaqService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddScoped<IAiSearchHistoryService, AiSearchHistoryService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.Configure<AiSettings>(
    builder.Configuration.GetSection("AiSettings"));

builder.Services.AddHttpClient<IAiApiClient, AiApiClient>();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var secret = builder.Configuration["JwtSettings:Secret"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secret!))
        };
    });

builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

var enableSwagger = builder.Configuration.GetValue<bool>("ENABLE_SWAGGER");

if (app.Environment.IsDevelopment() || enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Text(
@"FAQ Knowledge Search API

- Swagger UI: /swagger
- Health: /health
", "text/plain"));

app.MapControllers();

var enableMigration = builder.Configuration.GetValue<bool>("ENABLE_DB_MIGRATION");
var enableSeed = builder.Configuration.GetValue<bool>("ENABLE_DB_SEED");

if (enableMigration || enableSeed)
{
    using var scope = app.Services.CreateScope();

    var logger = scope.ServiceProvider
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("StartupDbInit");

    try
    {
        if (enableMigration)
        {
            logger.LogInformation("Database migration started.");

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();

            logger.LogInformation("Database migration completed.");
        }

        if (enableSeed)
        {
            logger.LogInformation("Database seed started.");

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            await RoleSeeder.SeedAsync(roleManager);
            await AdminUserSeeder.SeedAsync(userManager, configuration);

            logger.LogInformation("Database seed completed.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database initialization failed during startup.");
        throw;
    }
}

app.Run();

public partial class Program
{
}