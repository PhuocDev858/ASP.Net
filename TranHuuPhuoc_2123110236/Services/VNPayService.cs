using System.Security.Cryptography;
using System.Text;
using QRCoder;

namespace TranHuuPhuoc_2123110236.Services
{
    public interface IVNPayService
    {
        /// <summary>
        /// Tạo payment URL cho VNPay
        /// </summary>
        string CreatePaymentUrl(string orderId, decimal amount, string orderInfo, string ipAddress);

        /// <summary>
        /// Verify callback từ VNPay
        /// </summary>
        bool VerifyCallback(Dictionary<string, string> vnpayData);

        /// <summary>
        /// Generate QR Code từ payment URL
        /// </summary>
        byte[] GenerateQRCode(string paymentUrl);

        /// <summary>
        /// Lấy transaction status từ VNPay
        /// </summary>
        Task<Dictionary<string, string>> GetTransactionStatus(string orderId, DateTime transactionDate);
    }

    public class VNPayService : IVNPayService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<VNPayService> _logger;
        private readonly string _tmnCode;
        private readonly string _hashSecret;
        private readonly string _paymentUrl;
        private readonly string _returnUrl;

        public VNPayService(IConfiguration configuration, ILogger<VNPayService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _tmnCode = _configuration["VNPay:TmnCode"] ?? "";
            _hashSecret = _configuration["VNPay:HashSecret"] ?? "";
            _paymentUrl = _configuration["VNPay:PaymentUrl"] ?? "https://sandbox.vnpayment.vn/paygate";
            _returnUrl = _configuration["VNPay:ReturnUrl"] ?? "";

            if (string.IsNullOrWhiteSpace(_tmnCode) || string.IsNullOrWhiteSpace(_hashSecret))
            {
                _logger.LogWarning("VNPay configuration incomplete. Payment may not work.");
            }
        }

        public string CreatePaymentUrl(string orderId, decimal amount, string orderInfo, string ipAddress)
        {
            try
            {
                var vnPayData = new Dictionary<string, string>
                {
                    { "vnp_Version", "2.1.0" },
                    { "vnp_Command", "pay" },
                    { "vnp_TmnCode", _tmnCode },
                    { "vnp_Amount", ((long)(amount * 100)).ToString() },
                    { "vnp_CurrCode", "VND" },
                    { "vnp_TxnRef", orderId },
                    { "vnp_OrderInfo", orderInfo },
                    { "vnp_OrderType", "other" },
                    { "vnp_Locale", "vn" },
                    { "vnp_ReturnUrl", _returnUrl },
                    { "vnp_IpAddr", ipAddress },
                    { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") }
                };

                // Sắp xếp theo thứ tự bảng chữ cái
                var sortedVnPayData = vnPayData.OrderBy(kv => kv.Key).ToList();

                // Tạo hash input
                var hashInput = string.Join("&", sortedVnPayData.Select(kv => $"{kv.Key}={kv.Value}"));

                // Tính HMAC-SHA512 hash (VNPay requirement)
                var hash = ComputeHmacSHA512(hashInput, _hashSecret);

                // Build payment URL
                var paymentUrlBuilder = new StringBuilder(_paymentUrl + "?");
                foreach (var item in sortedVnPayData)
                {
                    paymentUrlBuilder.Append($"{item.Key}={Uri.EscapeDataString(item.Value)}&");
                }
                paymentUrlBuilder.Append($"vnp_SecureHash={hash}");

                var finalUrl = paymentUrlBuilder.ToString();
                _logger.LogInformation($"VNPay payment URL created for order {orderId}");

                return finalUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating VNPay payment URL: {ex.Message}");
                throw new Exception($"Lỗi khi tạo URL thanh toán VNPay: {ex.Message}");
            }
        }

        public bool VerifyCallback(Dictionary<string, string> vnpayData)
        {
            try
            {
                _logger.LogInformation($"===== VNPay Callback Verification Start =====");
                _logger.LogInformation($"Total parameters received: {vnpayData.Count}");

                // Log all received parameters
                foreach (var param in vnpayData.OrderBy(x => x.Key))
                {
                    _logger.LogInformation($"  {param.Key}: {param.Value}");
                }

                if (vnpayData == null || !vnpayData.ContainsKey("vnp_SecureHash"))
                {
                    _logger.LogWarning("VNPay callback missing vnp_SecureHash");
                    return false;
                }

                var secureHash = vnpayData["vnp_SecureHash"];
                _logger.LogInformation($"Received SecureHash: {secureHash}");
                _logger.LogInformation($"TmnCode: {_tmnCode}");
                _logger.LogInformation($"HashSecret length: {_hashSecret?.Length ?? 0}");

                // Remove vnp_SecureHash from data
                vnpayData.Remove("vnp_SecureHash");
                vnpayData.Remove("vnp_SecureHashType");

                // Sort and create hash input
                var sortedData = vnpayData.OrderBy(kv => kv.Key).ToList();
                var hashInput = string.Join("&", sortedData.Select(kv => $"{kv.Key}={kv.Value}"));

                _logger.LogInformation($"Hash input string: {hashInput}");
                _logger.LogInformation($"HashSecret: {_hashSecret}");

                // Calculate hash using HMAC-SHA512 (VNPay requirement)
                var hash = ComputeHmacSHA512(hashInput, _hashSecret);

                _logger.LogInformation($"Calculated hash: {hash}");
                _logger.LogInformation($"Expected hash:   {secureHash}");
                _logger.LogInformation($"Hash match: {hash.Equals(secureHash, StringComparison.OrdinalIgnoreCase)}");

                var isValid = hash.Equals(secureHash, StringComparison.OrdinalIgnoreCase);

                if (isValid)
                {
                    _logger.LogInformation($"✓ VNPay callback verified for order {vnpayData.GetValueOrDefault("vnp_TxnRef")}");
                    _logger.LogInformation($"===== VNPay Callback Verification SUCCESS =====");
                }
                else
                {
                    _logger.LogWarning($"✗ VNPay callback verification FAILED");
                    _logger.LogWarning($"Response code: {vnpayData.GetValueOrDefault("vnp_ResponseCode")}");
                    _logger.LogWarning($"Transaction no: {vnpayData.GetValueOrDefault("vnp_TransactionNo")}");
                    _logger.LogWarning($"===== VNPay Callback Verification FAILED =====");
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error verifying VNPay callback: {ex.Message}\n{ex.StackTrace}");
                _logger.LogError($"===== VNPay Callback Verification ERROR =====");
                return false;
            }
        }

        public byte[] GenerateQRCode(string paymentUrl)
        {
            try
            {
                using (var qrGenerator = new QRCodeGenerator())
                {
                    var qrCodeData = qrGenerator.CreateQrCode(paymentUrl, QRCodeGenerator.ECCLevel.Q);
                    using (var qrCode = new PngByteQRCode(qrCodeData))
                    {
                        var qrCodeBytes = qrCode.GetGraphic(10);
                        _logger.LogInformation("QR Code generated successfully");
                        return qrCodeBytes;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating QR Code: {ex.Message}");
                throw new Exception($"Lỗi khi tạo QR Code: {ex.Message}");
            }
        }

        public async Task<Dictionary<string, string>> GetTransactionStatus(string orderId, DateTime transactionDate)
        {
            try
            {
                // Tính toán request hash using HMAC-SHA512
                var data = $"{_tmnCode}|{orderId}|{transactionDate:yyyyMMdd}";
                var hash = ComputeHmacSHA512(data, _hashSecret);

                var requestData = new Dictionary<string, string>
                {
                    { "TmnCode", _tmnCode },
                    { "TxnRef", orderId },
                    { "TransactionDate", transactionDate.ToString("yyyyMMdd") },
                    { "SecureHash", hash }
                };

                // Note: Thực tế cần gọi VNPay API, đây là placeholder
                _logger.LogInformation($"Transaction status request for order {orderId}");

                return await Task.FromResult(requestData);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting transaction status: {ex.Message}");
                throw new Exception($"Lỗi khi lấy trạng thái giao dịch: {ex.Message}");
            }
        }

        private string ComputeSHA256Hash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                var hash = new StringBuilder();
                foreach (var b in hashedBytes)
                {
                    hash.Append(b.ToString("x2"));
                }
                return hash.ToString();
            }
        }

        private string ComputeHmacSHA512(string input, string key)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(Encoding.UTF8.GetBytes(key)))
            {
                var hashedBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
                var hash = new StringBuilder();
                foreach (var b in hashedBytes)
                {
                    hash.Append(b.ToString("x2"));
                }
                return hash.ToString();
            }
        }
    }
}
