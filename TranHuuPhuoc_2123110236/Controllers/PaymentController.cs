using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly AppDbContext _context;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IVNPayService vnPayService, AppDbContext context, ILogger<PaymentController> logger)
        {
            _vnPayService = vnPayService;
            _context = context;
            _logger = logger;
        }

        // POST: api/payment/create-vnpay-payment
        [Authorize(Roles = "Customer")]
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
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

                // Tạo payment URL
                var paymentUrl = _vnPayService.CreatePaymentUrl(
                    request.OrderId,
                    request.Amount,
                    request.OrderInfo ?? $"Thanh toán đơn hàng {request.OrderId}",
                    ipAddress
                );

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
                        payment.Status = responseCode == "00" ? "Success" : "Failed";
                        payment.UpdatedAt = DateTime.Now;
                        if (responseCode == "00")
                        {
                            payment.CompletedAt = DateTime.Now;
                        }

                        _context.Payments.Update(payment);
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
                var redirectUrl = responseCode == "00"
                    ? $"https://yourdomain.com/payment-success?orderId={orderId}"
                    : $"https://yourdomain.com/payment-failed?orderId={orderId}";

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

                var payment = await _context.Payment.FirstOrDefaultAsync(p => p.OrderId == request.OrderId);
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

                var payment = await _context.Payment.FirstOrDefaultAsync(p => p.OrderId == request.OrderId);
                if (payment == null)
                    return NotFound(new { message = "Không tìm thấy thông tin thanh toán" });

                if (payment.Status != "Success")
                    return BadRequest(new { message = "Chỉ có thể hoàn tiền cho giao dịch thành công" });

                // Update payment status to Refunded
                payment.Status = "Refunded";
                payment.Notes = request.Reason ?? "Hoàn tiền";
                payment.UpdatedAt = DateTime.Now;

                _context.Payment.Update(payment);
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
                var payment = await _context.Payment.FirstOrDefaultAsync(p => p.OrderId == orderId);
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
    }
}
