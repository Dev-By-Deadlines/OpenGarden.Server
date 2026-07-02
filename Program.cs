using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using StarterKit.Api.Data;
using StarterKit.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

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
