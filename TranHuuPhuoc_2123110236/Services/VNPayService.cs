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
                var createDate = DateTime.Now.ToString("yyyyMMddHHmmss");
                
                // Minimal parameters set (only essential ones)
                var vnPayData = new Dictionary<string, string>
                {
                    { "vnp_Amount", ((long)(amount * 100)).ToString() },
                    { "vnp_Command", "pay" },
                    { "vnp_CreateDate", createDate },
                    { "vnp_CurrCode", "VND" },
                    { "vnp_Locale", "vn" },
                    { "vnp_OrderInfo", orderInfo },
                    { "vnp_OrderType", "other" },
                    { "vnp_ReturnUrl", _returnUrl },
                    { "vnp_TmnCode", _tmnCode },
                    { "vnp_TxnRef", orderId },
                    { "vnp_Version", "2.1.0" },
                    { "vnp_IpAddr", ipAddress }
                };

                _logger.LogInformation($"===== VNPay Payment URL Creation Start =====");
                _logger.LogInformation($"TmnCode: {_tmnCode}");
                _logger.LogInformation($"HashSecret: {_hashSecret}");
                _logger.LogInformation($"OrderId: {orderId}");
                _logger.LogInformation($"Amount: {amount} VND (x100 = {(long)(amount * 100)})");
                _logger.LogInformation($"CreateDate: {createDate}");

                // Sort parameters alphabetically
                var sortedData = vnPayData.OrderBy(kv => kv.Key).ToList();

                _logger.LogInformation($"Sorted parameters for hash (count={sortedData.Count}):");
                foreach (var item in sortedData)
                {
                    _logger.LogInformation($"  {item.Key}={item.Value}");
                }

                // Create hash input string with URL encoding
                // IMPORTANT: URL-encode both key and value before hashing!
                var hashInputParts = new List<string>();
                foreach (var item in sortedData)
                {
                    var encodedKey = Uri.EscapeDataString(item.Key);
                    var encodedValue = Uri.EscapeDataString(item.Value);
                    hashInputParts.Add($"{encodedKey}={encodedValue}");
                }
                var hashInput = string.Join("&", hashInputParts);
                _logger.LogInformation($"Hash input (URL-encoded): {hashInput}");

                // Calculate HMAC-SHA512 (NOT SHA256!)
                var hash = ComputeHmacSHA512(hashInput, _hashSecret);
                _logger.LogInformation($"SecureHash (SHA256): {hash}");
                _logger.LogInformation($"Hash length: {hash.Length}");

                // Build payment URL with URL encoding
                var paymentUrlBuilder = new StringBuilder(_paymentUrl + "?");
                foreach (var item in sortedData)
                {
                    paymentUrlBuilder.Append($"{item.Key}={Uri.EscapeDataString(item.Value)}&");
                }
                paymentUrlBuilder.Append($"vnp_SecureHash={hash}");

                var finalUrl = paymentUrlBuilder.ToString();
                
                _logger.LogInformation($"Final URL: {finalUrl}");
                _logger.LogInformation($"===== VNPay Payment URL Creation SUCCESS =====");

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

                var secureHash = vnpayData["vnp_SecureHash"].ToLower();
                _logger.LogInformation($"Received SecureHash: {secureHash}");
                _logger.LogInformation($"TmnCode: {_tmnCode}");
                _logger.LogInformation($"HashSecret: {_hashSecret}");
                _logger.LogInformation($"HashSecret length: {_hashSecret?.Length ?? 0}");

                // Create a copy for verification (remove SecureHash and SecureHashType)
                var verifyData = new Dictionary<string, string>(vnpayData);
                verifyData.Remove("vnp_SecureHash");
                verifyData.Remove("vnp_SecureHashType");

                // Sort and create hash input with URL encoding
                var sortedData = verifyData
                    .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
                    .OrderBy(kv => kv.Key)
                    .ToList();

                var hashInputParts = new List<string>();
                foreach (var item in sortedData)
                {
                    var encodedKey = Uri.EscapeDataString(item.Key);
                    var encodedValue = Uri.EscapeDataString(item.Value);
                    hashInputParts.Add($"{encodedKey}={encodedValue}");
                }
                var hashInput = string.Join("&", hashInputParts);

                _logger.LogInformation($"Parameters for hash (count={sortedData.Count}):");
                foreach (var item in sortedData)
                {
                    _logger.LogInformation($"  {item.Key}={item.Value}");
                }
                _logger.LogInformation($"Hash input (URL-encoded): {hashInput}");

                // Calculate hash using HMAC-SHA512
                var hash = ComputeHmacSHA512(hashInput, _hashSecret).ToLower();

                _logger.LogInformation($"Calculated hash: {hash}");
                _logger.LogInformation($"Expected hash:   {secureHash}");
                _logger.LogInformation($"Hash match: {hash.Equals(secureHash, StringComparison.OrdinalIgnoreCase)}");

                var isValid = hash.Equals(secureHash, StringComparison.OrdinalIgnoreCase);

                if (isValid)
                {
                    _logger.LogInformation($"✓ VNPay callback verified for order {verifyData.GetValueOrDefault("vnp_TxnRef")}");
                    _logger.LogInformation($"✓ Response code: {verifyData.GetValueOrDefault("vnp_ResponseCode")}");
                    _logger.LogInformation($"✓ Transaction no: {verifyData.GetValueOrDefault("vnp_TransactionNo")}");
                    _logger.LogInformation($"===== VNPay Callback Verification SUCCESS =====");
                }
                else
                {
                    _logger.LogWarning($"✗ VNPay callback verification FAILED");
                    _logger.LogWarning($"Response code: {verifyData.GetValueOrDefault("vnp_ResponseCode")}");
                    _logger.LogWarning($"Transaction no: {verifyData.GetValueOrDefault("vnp_TransactionNo")}");
                    _logger.LogWarning($"Order ID: {verifyData.GetValueOrDefault("vnp_TxnRef")}");
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

        private string ComputeHmacSHA256(string input, string key)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(key)))
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
