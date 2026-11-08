using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenGarden.Server.Models;

namespace OpenGarden.Server.Data.Configuration;

public sealed class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.ToTable("Players", "public");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.DisplayName).HasMaxLength(50);

        builder.HasOne<ApplicationUser>()
               .WithOne()
               .HasForeignKey<Player>(p => p.Id)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
