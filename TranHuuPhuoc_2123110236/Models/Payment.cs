namespace TranHuuPhuoc_2123110236.Models
{
    public class Payment
    {
        public string PaymentId { get; set; }
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }  // CreditCard, BankTransfer, COD, EWallet
        public string Status { get; set; } = "Pending";  // Pending, Success, Failed, Refunded
        public string TransactionId { get; set; }
        public string ConfirmationCode { get; set; }
        public string Notes { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation
        public Order Order { get; set; }
        public Customer Customer { get; set; }
    }
}