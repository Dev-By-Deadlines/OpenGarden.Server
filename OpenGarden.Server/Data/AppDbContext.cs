using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenGarden.Server.Models;

namespace OpenGarden.Server.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // builder.Entity<ApplicationUser>(entity =>
        //         entity.Property(e => e.EnableNotifications).HasDefaultValue(true));
        builder.HasDefaultSchema("identity");
        builder.Entity<RefreshToken>().ToTable("RefreshTokens", "identity");

        builder.Entity<Plant>().HasKey(p => new { p.X, p.Y });
        builder.Entity<PlantType>().Property(t => t.Id).ValueGeneratedNever();
    }

    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Plant> Plants => Set<Plant>();
    public DbSet<PlantType> PlantTypes => Set<PlantType>();
}
