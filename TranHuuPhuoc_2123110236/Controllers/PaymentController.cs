using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TranHuuPhuoc_2123110236.Data;
using TranHuuPhuoc_2123110236.DTOs;
using TranHuuPhuoc_2123110236.Models;
using TranHuuPhuoc_2123110236.Services;

namespace TranHuuPhuoc_2123110236.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IVNPayService _vnPayService;
        private readonly IPaymentManagementService _paymentManagementService;
        private readonly AppDbContext _context;
        private readonly ILogger<PaymentController> _logger;
        private readonly IConfiguration _configuration;
        public PaymentController(IVNPayService vnPayService, IPaymentManagementService paymentManagementService, AppDbContext context, ILogger<PaymentController> logger, IConfiguration configuration)
        {
            _vnPayService = vnPayService;
            _paymentManagementService = paymentManagementService;
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        // POST: api/payment/create-vnpay-payment
        [AllowAnonymous]
        [HttpPost("create-vnpay-payment")]
        public async Task<IActionResult> CreateVNPayPayment([FromBody] VNPayPaymentRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.OrderId))
                    return BadRequest(new { message = "OrderId không được để trống" });

                if (request.Amount <= 0)
                    return BadRequest(new { message = "Số tiền phải lớn hơn 0" });

                // Kiểm tra order tồn tại
                var order = await _context.Orders.FindAsync(request.OrderId);
                if (order == null)
                    return NotFound(new { message = "Đơn hàng không tồn tại" });

                if (order.TotalAmount != request.Amount)
                    return BadRequest(new { message = "Số tiền không khớp với đơn hàng" });

                // Lấy IP address
                var ipAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                ?? HttpContext.Connection.RemoteIpAddress?.ToString()
                ?? "127.0.0.1";

                // Nếu có nhiều IP trong X-Forwarded-For, lấy cái đầu tiên
                if (ipAddress.Contains(","))
                    ipAddress = ipAddress.Split(",")[0].Trim();

                // Tạo payment URL
                var safeOrderInfo = RemoveVietnamese(
                    request.OrderInfo ?? $"Thanh toan don hang {request.OrderId}"
                );

                var paymentUrl = _vnPayService.CreatePaymentUrl(
                    request.OrderId,
                    request.Amount,
                    safeOrderInfo,
                    ipAddress
                );

                // Tạo hoặc cập nhật Payment record
                var existingPayment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == request.OrderId);
                
                if (existingPayment == null)
                {
                    var payment = new Payment
                    {
                        PaymentId = Guid.NewGuid().ToString(),
                        OrderId = request.OrderId,
                        CustomerId = order.CustomerId,
                        Amount = request.Amount,
                        PaymentMethod = "VNPay",
                        Status = "Pending",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        Notes = "VNPay payment URL created"
                    };
                    _context.Payments.Add(payment);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Payment record created: {payment.PaymentId}");
                }
                else
                {
                    existingPayment.Status = "Pending";
                    existingPayment.UpdatedAt = DateTime.Now;
                    existingPayment.Notes = "VNPay payment URL recreated";
                    _context.Payments.Update(existingPayment);
                    await _context.SaveChangesAsync();
                }

                // Generate QR Code
                var qrCodeBytes = _vnPayService.GenerateQRCode(paymentUrl);
                var qrCodeBase64 = Convert.ToBase64String(qrCodeBytes);

                _logger.LogInformation($"VNPay payment created for order {request.OrderId}");

                return Ok(new VNPayPaymentResponse
                {
                    PaymentUrl = paymentUrl,
                    QRCodeBase64 = qrCodeBase64,
                    OrderId = request.OrderId,
                    Amount = request.Amount,
                    Message = "Tạo liên kết thanh toán thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating VNPay payment: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/payment/vnpay-return (VNPay callback)
        [HttpGet("vnpay-return")]
        public async Task<IActionResult> VNPayReturn()
        {
            try
            {
                // Lấy tất cả query parameters
                var vnpayData = new Dictionary<string, string>();
                foreach (var param in Request.Query)
                {
                    vnpayData[param.Key] = param.Value.ToString();
                }

                // Verify callback
                var isValid = _vnPayService.VerifyCallback(vnpayData);

                if (!isValid)
                {
                    _logger.LogWarning("Invalid VNPay callback signature");
                    return BadRequest(new { message = "Chữ ký không hợp lệ" });
                }

                // Lấy response code
                vnpayData.TryGetValue("vnp_ResponseCode", out var responseCode);
                vnpayData.TryGetValue("vnp_TxnRef", out var orderId);
                vnpayData.TryGetValue("vnp_TransactionNo", out var transactionNo);
                vnpayData.TryGetValue("vnp_Amount", out var amountStr);

                // Update Payment record
                if (!string.IsNullOrWhiteSpace(orderId))
                {
                    var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
                    if (payment != null)
                    {
                        payment.TransactionId = transactionNo;
                        payment.ConfirmationCode = transactionNo; // Set ConfirmationCode
                        payment.Status = responseCode == "00" ? "Success" : "Failed";
                        payment.UpdatedAt = DateTime.Now;
                        if (responseCode == "00")
                        {
                            payment.CompletedAt = DateTime.Now;
                        }

                        _context.Payments.Update(payment);
                        
                        // Update Order status if payment is successful
                        if (responseCode == "00")
                        {
                            var order = await _context.Orders.FindAsync(orderId);
                            if (order != null)
                            {
                                order.Status = "Paid";
                                order.UpdatedAt = DateTime.Now;
                                _context.Orders.Update(order);
                                _logger.LogInformation($"Order {orderId} status updated to Paid");
                            }
                        }
                        
                        await _context.SaveChangesAsync();

                        _logger.LogInformation($"Payment updated for order {orderId}, status: {payment.Status}");
                    }
                }

                var callbackResponse = new VNPayCallbackResponse
                {
                    OrderId = orderId,
                    TransactionNo = transactionNo,
                    TransactionStatus = responseCode == "00" ? "Success" : "Failed",
                    Amount = string.IsNullOrEmpty(amountStr) ? 0 : long.Parse(amountStr) / 100m,
                    Message = responseCode == "00" ? "Thanh toán thành công" : "Thanh toán thất bại"
                };

                // Redirect to frontend success/failure page
                var baseUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000"; // Frontend URL
                var redirectUrl = responseCode == "00"
                    ? $"{baseUrl}/payment-success?orderId={orderId}"
                    : $"{baseUrl}/payment-failed?orderId={orderId}";

                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing VNPay callback: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/payment/check-status
        [Authorize]
        [HttpPost("check-status")]
        public async Task<IActionResult> CheckPaymentStatus([FromBody] VNPayTransactionStatusRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.OrderId))
                    return BadRequest(new { message = "OrderId không được để trống" });

                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == request.OrderId);
                if (payment == null)
                    return NotFound(new { message = "Không tìm thấy thông tin thanh toán" });

                return Ok(new
                {
                    orderId = payment.OrderId,
                    status = payment.Status,
                    amount = payment.Amount,
                    transactionId = payment.TransactionId,
                    paymentDate = payment.PaymentDate,
                    completedAt = payment.CompletedAt,
                    message = "Kiểm tra trạng thái thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking payment status: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/payment/refund (hoàn tiền)
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost("refund")]
        public async Task<IActionResult> RefundPayment([FromBody] VNPayRefundRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.OrderId))
                    return BadRequest(new { message = "OrderId không được để trống" });

                if (request.Amount <= 0)
                    return BadRequest(new { message = "Số tiền hoàn lại phải lớn hơn 0" });

                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == request.OrderId);
                if (payment == null)
                    return NotFound(new { message = "Không tìm thấy thông tin thanh toán" });

                if (payment.Status != "Success")
                    return BadRequest(new { message = "Chỉ có thể hoàn tiền cho giao dịch thành công" });

                // Update payment status to Refunded
                payment.Status = "Refunded";
                payment.Notes = request.Reason ?? "Hoàn tiền";
                payment.UpdatedAt = DateTime.Now;

                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Payment refunded for order {request.OrderId}, amount: {request.Amount}");

                return Ok(new
                {
                    message = "Hoàn tiền thành công",
                    orderId = request.OrderId,
                    amount = request.Amount,
                    status = "Refunded"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error refunding payment: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/payment/{orderId}
        [Authorize]
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetPaymentByOrderId(string orderId)
        {
            try
            {
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
                if (payment == null)
                    return NotFound(new { message = "Không tìm thấy thông tin thanh toán" });

                return Ok(new
                {
                    paymentId = payment.PaymentId,
                    orderId = payment.OrderId,
                    amount = payment.Amount,
                    paymentMethod = payment.PaymentMethod,
                    status = payment.Status,
                    transactionId = payment.TransactionId,
                    paymentDate = payment.PaymentDate,
                    completedAt = payment.CompletedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting payment: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // ========== PAYMENT MANAGEMENT ENDPOINTS ==========

        // GET: api/payment/management/all
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("management/all")]
        public async Task<ActionResult<List<PaymentDetailResponse>>> GetAllPayments()
        {
            try
            {
                var payments = await _paymentManagementService.GetAllPayments();
                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all payments: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/payment/management/{paymentId}
        [Authorize(Roles = "Admin,Staff,Customer")]
        [HttpGet("management/{paymentId}")]
        public async Task<ActionResult<PaymentDetailResponse>> GetPaymentById(string paymentId)
        {
            try
            {
                var payment = await _paymentManagementService.GetPaymentById(paymentId);
                return Ok(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting payment: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/payment/management/order/{orderId}
        [Authorize(Roles = "Admin,Staff,Customer")]
        [HttpGet("management/order/{orderId}")]
        public async Task<ActionResult<PaymentDetailResponse>> GetPaymentOrderDetails(string orderId)
        {
            try
            {
                var payment = await _paymentManagementService.GetPaymentByOrderId(orderId);
                return Ok(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting payment by order: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/payment/management/customer/{customerId}
        [Authorize(Roles = "Admin,Staff,Customer")]
        [HttpGet("management/customer/{customerId}")]
        public async Task<ActionResult<List<PaymentDetailResponse>>> GetPaymentsByCustomer(string customerId)
        {
            try
            {
                var payments = await _paymentManagementService.GetPaymentsByCustomer(customerId);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting customer payments: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/payment/management/status/{status}
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("management/status/{status}")]
        public async Task<ActionResult<List<PaymentDetailResponse>>> GetPaymentsByStatus(string status)
        {
            try
            {
                var payments = await _paymentManagementService.GetPaymentsByStatus(status);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting payments by status: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/payment/management/method/{method}
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("management/method/{method}")]
        public async Task<ActionResult<List<PaymentDetailResponse>>> GetPaymentsByMethod(string method)
        {
            try
            {
                var payments = await _paymentManagementService.GetPaymentsByMethod(method);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting payments by method: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/payment/management/search
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost("management/search")]
        public async Task<ActionResult<List<PaymentDetailResponse>>> SearchPayments([FromBody] PaymentSearchRequest request)
        {
            try
            {
                var payments = await _paymentManagementService.SearchPayments(request);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching payments: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/payment/management/statistics
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("management/statistics")]
        public async Task<ActionResult<PaymentStatisticsResponse>> GetPaymentStatistics()
        {
            try
            {
                var stats = await _paymentManagementService.GetPaymentStatistics();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting statistics: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/payment/management/daily-revenue
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("management/daily-revenue")]
        public async Task<ActionResult<List<DailyRevenueResponse>>> GetDailyRevenue([FromQuery] int days = 30)
        {
            try
            {
                var revenue = await _paymentManagementService.GetDailyRevenue(days);
                return Ok(revenue);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting daily revenue: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/payment/management/revenue-by-method
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("management/revenue-by-method")]
        public async Task<ActionResult<List<PaymentMethodRevenueResponse>>> GetRevenueByPaymentMethod()
        {
            try
            {
                var revenue = await _paymentManagementService.GetRevenueByPaymentMethod();
                return Ok(revenue);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting revenue by method: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
        private string RemoveVietnamese(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            string[] from = {
        "àáạảãâầấậẩẫăằắặẳẵ", "ÀÁẠẢÃÂẦẤẬẨẪĂẰẮẶẲẴ",
        "èéẹẻẽêềếệểễ", "ÈÉẸẺẼÊỀẾỆỂỄ",
        "ìíịỉĩ", "ÌÍỊỈĨ",
        "òóọỏõôồốộổỗơờớợởỡ", "ÒÓỌỎÕÔỒỐỘỔỖƠỜỚỢỞỠ",
        "ùúụủũưừứựửữ", "ÙÚỤỦŨƯỪỨỰỬỮ",
        "ỳýỵỷỹ", "ỲÝỴỶỸ",
        "đ", "Đ"
    };
            string[] to = {
        "a", "A", "e", "E", "i", "I",
        "o", "O", "u", "U", "y", "Y", "d", "D"
    };

            for (int i = 0; i < from.Length; i++)
                foreach (char c in from[i])
                    text = text.Replace(c.ToString(), to[i]);

            return text;
        }
    }
}
