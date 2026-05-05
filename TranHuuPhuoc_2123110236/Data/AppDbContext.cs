using Microsoft.EntityFrameworkCore;
using TranHuuPhuoc_2123110236.Models;

namespace TranHuuPhuoc_2123110236.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Product { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Employee> Employee { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.CategoryId);
                entity.Property(e => e.CategoryId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.CategoryName).IsRequired().HasMaxLength(255).HasColumnType("nvarchar(255)");
                entity.Property(e => e.Description).HasMaxLength(1000).HasColumnType("nvarchar(1000)");
                entity.ToTable("Categories");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.ProductId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(255).HasColumnType("nvarchar(255)");
                entity.Property(e => e.CategoryId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.Description).HasMaxLength(1000).HasColumnType("nvarchar(1000)");
                entity.ToTable("Products");

                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.EmployeeId);
                entity.Property(e => e.EmployeeId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(255).HasColumnType("nvarchar(255)");
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(500).HasColumnType("nvarchar(500)");
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Role).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Salary).HasPrecision(18, 2);
                entity.ToTable("Employees");

                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerId);
                entity.Property(e => e.CustomerId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(255).HasColumnType("nvarchar(255)");
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(500).HasColumnType("nvarchar(500)");
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.TotalSpent).HasPrecision(18, 2);
                entity.ToTable("Customers");

                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.OrderId);
                entity.Property(e => e.OrderId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.CustomerId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.EmployeeId).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
                entity.Property(e => e.ShippingAddress).HasMaxLength(500).HasColumnType("nvarchar(500)");
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.ToTable("Orders");

                entity.HasOne(o => o.Customer)
                    .WithMany(c => c.Orders)
                    .HasForeignKey(o => o.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.Employee)
                    .WithMany()
                    .HasForeignKey(o => o.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(e => e.OrderDetailId);
                entity.Property(e => e.OrderDetailId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.OrderId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.ProductId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
                entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
                entity.ToTable("OrderDetails");

                entity.HasOne(od => od.Order)
                    .WithMany(o => o.OrderDetails)
                    .HasForeignKey(od => od.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(od => od.Product)
                    .WithMany()
                    .HasForeignKey(od => od.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.PaymentId);
                entity.Property(e => e.PaymentId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.OrderId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.CustomerId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
                entity.Property(e => e.TransactionId).HasMaxLength(100);
                entity.Property(e => e.ConfirmationCode).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.ToTable("Payments");

                entity.HasOne(p => p.Order)
                    .WithMany()
                    .HasForeignKey(p => p.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Customer)
                    .WithMany()
                    .HasForeignKey(p => p.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}