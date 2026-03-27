namespace TranHuuPhuoc_2123110236.Models
{
    public class Product
    {
        public string ProductId { get; set; }  // Đổi từ int sang string
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}