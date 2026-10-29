using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenGarden.Server.Models;

namespace OpenGarden.Server.Data.Configuration;

public sealed class PlantsConfiguration : IEntityTypeConfiguration<Plant>
{
    public void Configure(EntityTypeBuilder<Plant> builder)
    {
        builder.HasKey(p => new { p.X, p.Y });
        builder.ToTable("Plants", "public");
    }
}
