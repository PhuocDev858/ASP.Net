using Microsoft.EntityFrameworkCore;
using TranHuuPhuoc_2123110236.Models;

namespace TranHuuPhuoc_2123110236.Data
{
    public class AppDbContext : DbContext
    {
        // Constructor này bắt buộc phải có để nhận Connection String từ Program.cs
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Product { get; set; }
        public DbSet<Category> Category { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.CategoryId);
                entity.Property(e => e.CategoryId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.CategoryName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.ToTable("Category");
            });

            // Cấu hình Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.ProductId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.CategoryId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.ToTable("Product");

                // Foreign key constraint
                entity.HasOne(p => p.Category)
                        .WithMany(c => c.Products)  // ← thêm c => c.Products
                        .HasForeignKey(p => p.CategoryId)
                        .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}