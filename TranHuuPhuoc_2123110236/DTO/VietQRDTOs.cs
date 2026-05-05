namespace TranHuuPhuoc_2123110236.DTOs
{
    // VietQR Payment Request
    public class VietQRPaymentRequest
    {
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }

    // VietQR Payment Response
    public class VietQRPaymentResponse
    {
        public string QRImageUrl { get; set; }  // URL ảnh QR từ VietQR API
        public string QRCodeBase64 { get; set; }  // QR code dạng base64
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string BankCode { get; set; }
        public string Message { get; set; }
    }

    // VietQR Webhook Notification (khi khách thanh toán)
    public class VietQRWebhookNotification
    {
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionId { get; set; }
        public string ConfirmationCode { get; set; }  // Mã xác nhận từ ngân hàng
        public DateTime TransactionDate { get; set; }
        public string FromAccountNumber { get; set; }
        public string FromAccountName { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }  // Success, Failed, Pending
    }
}
