namespace TranHuuPhuoc_2123110236.Services
{
    public interface IVietQRService
    {
        string GenerateQRUrl(string orderId, decimal amount, string description);
    }

    public class VietQRService : IVietQRService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<VietQRService> _logger;

        public VietQRService(IConfiguration configuration, ILogger<VietQRService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // Tạo chuỗi VietQR theo chuẩn NAPAS/QR Pay
        public string GenerateQRUrl(string orderId, decimal amount, string description)
        {
            try
            {
                var bankCode = _configuration["VietQR:BankCode"] ?? "970422";
                var accountNumber = _configuration["VietQR:AccountNumber"] ?? "808080190705";
                var accountName = _configuration["VietQR:AccountName"] ?? "Tran Huu Phuoc";

                // Format: https://img.vietqr.io/image/{bankcode}-{accountno}-compact2.png
                // Params: amount, addInfo (description), accountName
                var cleanDescription = description.Length > 25 
                    ? description.Substring(0, 25) 
                    : description;

                var qrUrl = $"https://img.vietqr.io/image/{bankCode}-{accountNumber}-compact2.png?amount={amount}&addInfo={Uri.EscapeDataString(cleanDescription)}&accountName={Uri.EscapeDataString(accountName)}";

                _logger.LogInformation($"Generated VietQR URL for order {orderId}: amount={amount}");

                return qrUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating VietQR URL: {ex.Message}");
                throw new Exception("Lỗi khi tạo URL VietQR: " + ex.Message);
            }
        }
    }
}
