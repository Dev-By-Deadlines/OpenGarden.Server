using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenGarden.Server.Models;

public sealed class SeedInventoryConfiguration : IEntityTypeConfiguration<SeedInventory>
{
    public void Configure(EntityTypeBuilder<SeedInventory> builder)
    {
        builder.ToTable("SeedInventory", "public",
            t => t.HasCheckConstraint("CK_SeedInventory_Quantity", "\"Quantity\" >= 0"));

        builder.HasKey(s => new { s.PlayerId, s.PlantTypeId });

        builder.HasOne<Player>().WithMany()
            .HasForeignKey(s => s.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<PlantType>().WithMany()
            .HasForeignKey(s => s.PlantTypeId);
    }
}
