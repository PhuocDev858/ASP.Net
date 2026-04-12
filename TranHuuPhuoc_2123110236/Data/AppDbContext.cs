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
        public DbSet<Employee> Employee { get; set; }
        public DbSet<Customer> Customer { get; set; }

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
            // Cấu hình Employee
            modelBuilder.Entity<Employee>(entity =>
                {
                    entity.HasKey(e => e.EmployeeId);
                    entity.Property(e => e.EmployeeId).HasMaxLength(50).IsRequired();
                    entity.Property(e => e.FullName).IsRequired().HasMaxLength(255);
                    entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                    entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                    entity.Property(e => e.Address).HasMaxLength(500);
                    entity.Property(e => e.PasswordHash).IsRequired();
                    entity.Property(e => e.Role).HasMaxLength(50).IsRequired();
                    entity.Property(e => e.Salary).HasPrecision(18, 2);
                    entity.ToTable("Employee");

                    entity.HasIndex(e => e.Email).IsUnique();
                });

                // Cấu hình Customer
                modelBuilder.Entity<Customer>(entity =>
                {
                    entity.HasKey(e => e.CustomerId);
                    entity.Property(e => e.CustomerId).HasMaxLength(50).IsRequired();
                    entity.Property(e => e.FullName).IsRequired().HasMaxLength(255);
                    entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                    entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                    entity.Property(e => e.Address).HasMaxLength(500);
                    entity.Property(e => e.PasswordHash).IsRequired();
                    entity.Property(e => e.TotalSpent).HasPrecision(18, 2);
                    entity.ToTable("Customer");

                    entity.HasIndex(e => e.Email).IsUnique();
                });
            }
    }
}