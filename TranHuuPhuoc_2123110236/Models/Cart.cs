namespace TranHuuPhuoc_2123110236.Models
{
    public class Cart
    {
        public string CartId { get; set; }
        public string CustomerId { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation
        public Customer Customer { get; set; }
        public Product Product { get; set; }
    }
}