using Microsoft.EntityFrameworkCore;
using StarterKit.Api.Data;
using StarterKit.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextPool<AppDbContext>(opt =>
        opt.UseNpgsql(builder.Configuration.GetConnectionString("Default"),
            o => o
            .UseNodaTime()));
// o.MapEnum<Mood>("mood") later for using postgres enums.

var app = builder.Build();

// For production, run "dotnet ef database update" separately instead.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.MapEndpoints();
app.Run();
