using Microsoft.EntityFrameworkCore;

namespace FamilyAI.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Enable pgvector extension
        modelBuilder.HasPostgresExtension("vector");
    }
}
