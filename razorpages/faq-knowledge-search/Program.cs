using FaqKnowledgeSearch.Infrastructure;
using FaqKnowledgeSearch.Infrastructure.Identity;
using FaqKnowledgeSearch.Infrastructure.Persistence;
using FaqKnowledgeSearch.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");

builder.Services.AddInfrastructure(connectionString);

builder.Services.AddAiInfrastructure(
    builder.Configuration);

builder.Services.AddDataProtection();

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 10;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);

        // 現段階ではメール送信機能がないためfalse
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "FaqKnowledgeSearch.Auth";

    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";

    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "AdminOnly",
        policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole(AppRoles.Admin);
        });
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder(
        "/Admin",
        "AdminOnly");
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter =
        PartitionedRateLimiter.Create<HttpContext, string>(
            httpContext =>
            {
                // AI回答生成以外はレート制限しない
                if (!IsAiSearchGenerationRequest(httpContext))
                {
                    return RateLimitPartition.GetNoLimiter(
                        "not-ai-search");
                }

                // IPアドレス単位で個別にカウント
                var ipAddress =
                    httpContext.Connection.RemoteIpAddress?
                        .MapToIPv4()
                        .ToString()
                    ?? "unknown";

                return RateLimitPartition
                    .GetFixedWindowLimiter(
                        partitionKey:
                            $"ai-search:{ipAddress}",
                        factory: _ =>
                            new FixedWindowRateLimiterOptions
                            {
                                AutoReplenishment = true,

                                // 1分間に5回
                                PermitLimit = 5,

                                Window =
                                    TimeSpan.FromMinutes(1),

                                // 待機させず、6回目を即座に拒否
                                QueueLimit = 0,

                                QueueProcessingOrder =
                                    QueueProcessingOrder
                                        .OldestFirst
                            });
            });

    options.OnRejected =
        (context, cancellationToken) =>
        {
            var retryAfterSeconds = 60;

            if (context.Lease.TryGetMetadata(
                    MetadataName.RetryAfter,
                    out var retryAfter))
            {
                retryAfterSeconds =
                    Math.Max(
                        1,
                        (int)Math.Ceiling(
                            retryAfter.TotalSeconds));
            }

            context.HttpContext.Response.Headers.RetryAfter =
                retryAfterSeconds.ToString(
                    CultureInfo.InvariantCulture);

            var location =
                $"/Ai?rateLimited=true"
                + $"&retryAfter={retryAfterSeconds}";

            // POST後にGET /Aiへ移動
            context.HttpContext.Response.StatusCode =
                StatusCodes.Status303SeeOther;

            context.HttpContext.Response.Headers.Location =
                location;

            return ValueTask.CompletedTask;
        };
});

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    await DatabaseInitializer.InitializeAsync(
        app.Services,
        seedDemoData: app.Environment.IsDevelopment());

    var adminEmail =
        app.Configuration["SeedAdmin:Email"];

    var adminPassword =
        app.Configuration["SeedAdmin:Password"];

    var seedDemoUsers =
        app.Environment.IsDevelopment()
        && app.Configuration.GetValue<bool>(
            "Seed:DemoUsers");

    var demoUserPassword =
        app.Configuration["Seed:DemoUserPassword"];

    if (!string.IsNullOrWhiteSpace(adminEmail)
        && !string.IsNullOrWhiteSpace(adminPassword))
    {
        await IdentitySeeder.SeedAsync(
            app.Services,
            adminEmail,
            adminPassword,
            seedDemoUsers,
            demoUserPassword);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// 404などのHTTPステータスコード用画面
app.UseStatusCodePagesWithReExecute(
    "/StatusCode/{0}");

app.UseHttpsRedirection();
app.UseRouting();

// AI検索の回数制限
app.UseRateLimiter();

// 順序が重要
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

app.Run();

static bool IsAiSearchGenerationRequest(
    HttpContext httpContext)
{
    // AI検索はPOSTのみ
    if (!HttpMethods.IsPost(
            httpContext.Request.Method))
    {
        return false;
    }

    var path =
        httpContext.Request.Path.Value;

    var isAiSearchPage =
        string.Equals(
            path,
            "/Ai",
            StringComparison.OrdinalIgnoreCase)
        || string.Equals(
            path,
            "/Ai/Index",
            StringComparison.OrdinalIgnoreCase);

    if (!isAiSearchPage)
    {
        return false;
    }

    // ?handler=Feedback はフィードバック登録なので除外
    var handler =
        httpContext.Request.Query["handler"]
            .ToString();

    return string.IsNullOrWhiteSpace(handler);
}