namespace TranHuuPhuoc_2123110236.DTOs
{
    // Create Order Request
    public class CreateOrderRequest
    {
        public string CustomerId { get; set; }
        public string ShippingAddress { get; set; }
        public List<OrderItemRequest> Products { get; set; } = new List<OrderItemRequest>();
    }

    // Order Item Request
    public class OrderItemRequest
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }

    // Order Response
    public class OrderResponse
    {
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public string EmployeeId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string ShippingAddress { get; set; }
        public List<OrderDetailResponse> OrderDetails { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // Order Detail Response
    public class OrderDetailResponse
    {
        public string OrderDetailId { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    // Update Order Status Request
    public class UpdateOrderStatusRequest
    {
        public string NewStatus { get; set; }  // Pending, Shipped, Delivered, Cancelled
    }
}