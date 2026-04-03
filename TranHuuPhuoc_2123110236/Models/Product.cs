namespace TranHuuPhuoc_2123110236.Models
{
    public class Product
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string CategoryId { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;  // ← thêm
        public DateTime UpdatedAt { get; set; } = DateTime.Now;  // ← thêm

        public Category? Category { get; set; } = null!;
    }
}