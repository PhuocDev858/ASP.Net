namespace TranHuuPhuoc_2123110236.DTOs
{
    // Add to Cart Request
    public class AddToCartRequest
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }

    // Update Cart Item Request
    public class UpdateCartItemRequest
    {
        public int Quantity { get; set; }
    }

    // Cart Item Response
    public class CartItemResponse
    {
        public string CartId { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string ImageUrl { get; set; }
        public DateTime AddedAt { get; set; }
    }

    // Get Cart Response
    public class GetCartResponse
    {
        public List<CartItemResponse> Items { get; set; }
        public int TotalItems { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; } = 0;
        public decimal ShippingFee { get; set; } = 0;
        public decimal GrandTotal { get; set; }
    }

    // Checkout Request
    public class CheckoutRequest
    {
        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; }  // COD, CreditCard, BankTransfer, EWallet
    }

    // Checkout Response
    public class CheckoutResponse
    {
        public string OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}