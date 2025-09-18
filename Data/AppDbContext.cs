using Microsoft.EntityFrameworkCore;
using EchoPlayAPI.Models;

namespace EchoPlayAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<FavoriteCategory> FavoriteCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<Favorite>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EntityType).HasConversion<string>();
                
                // Create unique constraint to prevent duplicates
                entity.HasIndex(e => new { e.UserId, e.EntityType, e.SpotifyId })
                      .IsUnique()
                      .HasDatabaseName("IX_Favorites_User_EntityType_SpotifyId");
                
                // Foreign key relationships
                entity.HasOne(f => f.User)
                      .WithMany()
                      .HasForeignKey(f => f.UserId)
                      .OnDelete(DeleteBehavior.NoAction); // Changed to NoAction to avoid cascade conflicts
                
                entity.HasOne(f => f.Category)
                      .WithMany(c => c.Favorites)
                      .HasForeignKey(f => f.CategoryId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<FavoriteCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Unique constraint for category names per user
                entity.HasIndex(e => new { e.UserId, e.Name })
                      .IsUnique()
                      .HasDatabaseName("IX_FavoriteCategories_User_Name");
                
                // Foreign key relationship
                entity.HasOne(c => c.User)
                      .WithMany()
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}