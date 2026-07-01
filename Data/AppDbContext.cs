using Microsoft.EntityFrameworkCore;
using StarterKit.Api.Models;

namespace StarterKit.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TestModel> TestModels { get; set; }
}
