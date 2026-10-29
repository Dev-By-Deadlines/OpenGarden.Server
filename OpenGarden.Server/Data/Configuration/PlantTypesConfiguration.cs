using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenGarden.Server.Models;

namespace OpenGarden.Server.Data.Configuration;

public sealed class PlantTypesConfiguration : IEntityTypeConfiguration<PlantType>
{
    public void Configure(EntityTypeBuilder<PlantType> builder)
    {
        builder.Property(t => t.Id).ValueGeneratedNever();
        builder.ToTable("PlantTypes", "public");
        builder.HasData(
            new PlantType { Id = 1, Name = "Red" },
            new PlantType { Id = 2, Name = "Orange" },
            new PlantType { Id = 3, Name = "Yellow" },
            new PlantType { Id = 4, Name = "Light Green" },
            new PlantType { Id = 5, Name = "Dark Green" },
            new PlantType { Id = 6, Name = "Teal" },
            new PlantType { Id = 7, Name = "Light Blue" },
            new PlantType { Id = 8, Name = "Blue" },
            new PlantType { Id = 9, Name = "Dark Blue" },
            new PlantType { Id = 10, Name = "Purple" },
            new PlantType { Id = 11, Name = "Pink" },
            new PlantType { Id = 12, Name = "Brown" },
            new PlantType { Id = 13, Name = "White" },
            new PlantType { Id = 14, Name = "Light Gray" },
            new PlantType { Id = 15, Name = "Dark Gray" },
            new PlantType { Id = 16, Name = "Black" }
        );
    }
}
