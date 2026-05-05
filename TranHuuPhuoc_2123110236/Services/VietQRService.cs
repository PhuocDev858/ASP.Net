using QRCoder;
using TranHuuPhuoc_2123110236.DTOs;

namespace TranHuuPhuoc_2123110236.Services
{
    public interface IVietQRService
    {
        string GenerateQRUrl(string orderId, decimal amount, string description);
        byte[] GenerateQRCode(string qrUrl);
        string GenerateVietQRString(string orderId, decimal amount, string description);
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
        public string GenerateVietQRString(string orderId, decimal amount, string description)
        {
            try
            {
                var bankCode = _configuration["VietQR:BankCode"] ?? "970422";
                var accountNumber = _configuration["VietQR:AccountNumber"] ?? "808080190705";
                var accountName = _configuration["VietQR:AccountName"] ?? "Tran Huu Phuoc";

                // Format: https://img.vietqr.io/image/{bankcode}-{accountno}-compact2.png
                // Params: amount, addInfo (description), accountName
                var qrString = $"https://img.vietqr.io/image/{bankCode}-{accountNumber}-compact2.png?amount={amount}&addInfo={Uri.EscapeDataString(description)}&accountName={Uri.EscapeDataString(accountName)}";
                
                _logger.LogInformation($"Generated VietQR string for order {orderId}: amount={amount}");
                
                return qrString;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating VietQR string: {ex.Message}");
                throw new Exception("Lỗi khi tạo chuỗi VietQR: " + ex.Message);
            }
        }

        // Tạo URL QR từ VietQR API
        public string GenerateQRUrl(string orderId, decimal amount, string description)
        {
            try
            {
                var bankCode = _configuration["VietQR:BankCode"] ?? "970422";
                var accountNumber = _configuration["VietQR:AccountNumber"] ?? "808080190705";
                var accountName = _configuration["VietQR:AccountName"] ?? "Tran Huu Phuoc";

                // VietQR API endpoint
                // Format: amount in VND (whole number), description limited to 25 chars
                var cleanDescription = description.Length > 25 
                    ? description.Substring(0, 25) 
                    : description;

                var qrUrl = $"https://img.vietqr.io/image/{bankCode}-{accountNumber}-compact2.png?amount={amount}&addInfo={Uri.EscapeDataString(cleanDescription)}&accountName={Uri.EscapeDataString(accountName)}";

                _logger.LogInformation($"Generated VietQR URL for order {orderId}: {qrUrl}");

                return qrUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating VietQR URL: {ex.Message}");
                throw new Exception("Lỗi khi tạo URL VietQR: " + ex.Message);
            }
        }

        // Tạo mã QR (PNG bytes) từ URL
        public byte[] GenerateQRCode(string qrUrl)
        {
            try
            {
                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                {
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrUrl, QRCodeGenerator.ECCLevel.Q);
                    
                    using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                    {
                        byte[] qrCodeImage = qrCode.GetGraphic(10); // 10 pixels per module
                        
                        _logger.LogInformation("Generated QR code image successfully");
                        
                        return qrCodeImage;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating QR code: {ex.Message}");
                throw new Exception("Lỗi khi tạo mã QR: " + ex.Message);
            }
        }

        // Helper: Tạo mô tả từ orderId và amount
        public string GenerateDescription(string orderId, decimal amount)
        {
            // VietQR description limit: 25 characters
            var desc = $"Don hang {orderId}";
            if (desc.Length > 25)
                desc = desc.Substring(0, 25);
            
            return desc;
        }
    }
}
