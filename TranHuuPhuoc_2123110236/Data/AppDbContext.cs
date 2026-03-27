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
    }
}