using Microsoft.EntityFrameworkCore;
using Domain.Models;

namespace Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<UserFavorite> UserFavorites { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.Rating).HasColumnType("decimal(3,2)");
                entity.Property(e => e.Brand).HasMaxLength(100);
                entity.Property(e => e.Thumbnail).HasMaxLength(500);

                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.Brand);
            });

            modelBuilder.Entity<UserFavorite>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.ProductId).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();

                entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();

                // No foreign key constraint since products come from external API
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(256);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();

                entity.HasIndex(e => e.Email).IsUnique();
            });
        }
    }
}