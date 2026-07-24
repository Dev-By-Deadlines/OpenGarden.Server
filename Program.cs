using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using StarterKit.Api.Constants;
using StarterKit.Api.Data;
using StarterKit.Api.Endpoints;

// Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
            "/var/log/starter.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7)
    .CreateLogger();

try
{
    Log.Information("Starting web application");
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddSerilog();

    builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
                policy.WithOrigins("http://localhost:3000")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()));

    builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = 429;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));

            //     Named policy for specific endpoints that need stricter limits
            // options.AddFixedWindowLimiter("FixedPolicy", opt =>
            // {
            //     opt.Window = TimeSpan.FromMinutes(1);
            //     opt.PermitLimit = 60;
            // });
        });

    builder.Services.AddDbContextPool<AppDbContext>(opt =>
            opt.UseNpgsql(builder.Configuration.GetConnectionString("Default"),
                o => o
                .UseNodaTime()));
    // o.MapEnum<Mood>("mood") later for using postgres enums.

    builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>();

    builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
    .AddJwtBearer(options =>
            {
                options.TokenValidationParameters.ValidIssuer = builder.Configuration["Jwt:Issuer"];
                options.TokenValidationParameters.ValidAudience = builder.Configuration["Jwt:Audience"];
                options.TokenValidationParameters.IssuerSigningKey =
                    new SymmetricSecurityKey(
                            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!));
            });

    builder.Services.AddAuthorization();
    builder.Services.AddOpenApi();

    var app = builder.Build();
    app.UseSerilogRequestLogging();
    app.UseCors();
    app.UseRateLimiter();
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference("/docs", options =>
                options
                .WithTitle("Template API")
                .WithTheme(ScalarTheme.Saturn)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.Fetch)
                );
    }

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync(Roles.Admin))
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
        }
        if (!await roleManager.RoleExistsAsync(Roles.Member))
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.Member));
        }
    }

    app.MapEndpoints();
    app.UseAuthentication();
    app.UseAuthorization();
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
