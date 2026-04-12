namespace TranHuuPhuoc_2123110236.Models
{
    public class Customer
    {
        public string CustomerId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public decimal TotalSpent { get; set; } = 0;  // Tổng tiền đã chi
        public int TotalOrders { get; set; } = 0;  // Tổng số đơn hàng
    }
}