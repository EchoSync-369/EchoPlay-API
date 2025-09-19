using Microsoft.EntityFrameworkCore;
using EchoPlayAPI.Models;

namespace EchoPlayAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserSearchHistory> UserSearchHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<UserSearchHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Query).IsRequired().HasMaxLength(512);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                entity.HasIndex(e => e.UserId);
            });
        }
    }
}