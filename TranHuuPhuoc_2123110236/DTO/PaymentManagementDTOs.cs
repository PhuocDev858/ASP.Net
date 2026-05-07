public class PaymentDetailResponse
{
    public string PaymentId { get; set; }
    public string OrderId { get; set; }
    public string CustomerId { get; set; }
    public string CustomerName { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; }
    public string Status { get; set; }
    public string? TransactionId { get; set; }      // ← thêm ?
    public string? ConfirmationCode { get; set; }   // ← thêm ?
    public string? Notes { get; set; }              // ← thêm ?
    public DateTime? PaymentDate { get; set; }      // ← đổi thành DateTime?
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string OrderStatus { get; set; }
}