namespace TranHuuPhuoc_2123110236.Models
{
    public class OrderDetail
    {
        public string OrderDetailId { get; set; }
        public string OrderId { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        // Navigation
        public Order Order { get; set; }
        public Product Product { get; set; }
    }
}