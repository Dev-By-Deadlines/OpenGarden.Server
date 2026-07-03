using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StarterKit.Api.Models;

namespace StarterKit.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // builder.Entity<ApplicationUser>(entity =>
        //         entity.Property(e => e.EnableNotifications).HasDefaultValue(true));
        builder.HasDefaultSchema("identity");
        builder.Entity<TestModel>().ToTable("TestModels", "public");
    }

    public DbSet<TestModel> TestModels { get; set; }
}
