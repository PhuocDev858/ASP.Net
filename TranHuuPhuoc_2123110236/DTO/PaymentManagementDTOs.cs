namespace TranHuuPhuoc_2123110236.DTOs
{
    // Payment Detail Response
    public class PaymentDetailResponse
    {
        public string PaymentId { get; set; }
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public string? TransactionId { get; set; }
        public string? ConfirmationCode { get; set; }
        public string? Notes { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string OrderStatus { get; set; }
    }

    // Payment Search Request
    public class PaymentSearchRequest
    {
        public string PaymentId { get; set; }
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    // Payment Statistics Response
    public class PaymentStatisticsResponse
    {
        public int TotalPayments { get; set; }
        public int SuccessPayments { get; set; }
        public int FailedPayments { get; set; }
        public int PendingPayments { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageAmount { get; set; }
        public double SuccessRate { get; set; }
        public List<PaymentMethodStatistic> PaymentMethodStatistics { get; set; }
    }

    // Payment Method Statistic
    public class PaymentMethodStatistic
    {
        public string PaymentMethod { get; set; }
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
        public int SuccessCount { get; set; }
    }

    // Daily Revenue Response
    public class DailyRevenueResponse
    {
        public DateTime Date { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TransactionCount { get; set; }
        public decimal AverageAmount { get; set; }
    }

    // Payment Method Revenue Response
    public class PaymentMethodRevenueResponse
    {
        public string PaymentMethod { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TransactionCount { get; set; }
        public decimal AverageAmount { get; set; }
        public double PercentageOfTotal { get; set; }
    }
}