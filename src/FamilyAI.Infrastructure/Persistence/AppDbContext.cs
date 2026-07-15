using FamilyAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyAI.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Family> Families => Set<Family>();
    public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();
    public DbSet<AiPromptTemplate> AiPromptTemplates => Set<AiPromptTemplate>();
    public DbSet<ConversationSession> ConversationSessions => Set<ConversationSession>();
    public DbSet<ConversationMessage> ConversationMessages => Set<ConversationMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Enable pgvector extension
        modelBuilder.HasPostgresExtension("vector");

        // Entity Configurations
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<FamilyMember>(entity =>
        {
            entity.HasOne(m => m.Family)
                  .WithMany(f => f.Members)
                  .HasForeignKey(m => m.FamilyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ConversationSession>(entity =>
        {
            entity.HasIndex(s => s.FamilyId);
            entity.HasIndex(s => s.MemberId);
        });

        modelBuilder.Entity<ConversationMessage>(entity =>
        {
            entity.HasOne(m => m.Session)
                  .WithMany(s => s.Messages)
                  .HasForeignKey(m => m.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
