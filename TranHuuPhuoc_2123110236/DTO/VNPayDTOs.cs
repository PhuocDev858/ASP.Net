namespace TranHuuPhuoc_2123110236.DTOs
{
    // VNPay Payment Request
    public class VNPayPaymentRequest
    {
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string OrderInfo { get; set; }
    }

    // VNPay Payment Response
    public class VNPayPaymentResponse
    {
        public string PaymentUrl { get; set; }
        public string QRCodeBase64 { get; set; }
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Message { get; set; }
    }

    // VNPay Callback Response
    public class VNPayCallbackResponse
    {
        public string OrderId { get; set; }
        public string TransactionNo { get; set; }
        public string TransactionStatus { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string BankCode { get; set; }
        public string Message { get; set; }
    }

    // VNPay Refund Request
    public class VNPayRefundRequest
    {
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionNo { get; set; }
        public string Reason { get; set; }
    }

    // VNPay Transaction Status Request
    public class VNPayTransactionStatusRequest
    {
        public string OrderId { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
