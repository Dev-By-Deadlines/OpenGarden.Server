using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
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

    builder.Services.AddDbContextPool<AppDbContext>(opt =>
            opt.UseNpgsql(builder.Configuration.GetConnectionString("Default"),
                o => o
                .UseNodaTime()));
    // o.MapEnum<Mood>("mood") later for using postgres enums.

    builder.Services.AddOpenApi();

    var app = builder.Build();

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

    // For production, run "dotnet ef database update" separately instead.
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }

    app.MapEndpoints();
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
