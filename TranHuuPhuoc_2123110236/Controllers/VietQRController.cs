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
    public class VietQRController : ControllerBase
    {
        private readonly IVietQRService _vietQRService;
        private readonly AppDbContext _context;
        private readonly ILogger<VietQRController> _logger;
        private readonly IConfiguration _configuration;

        public VietQRController(IVietQRService vietQRService, AppDbContext context, ILogger<VietQRController> logger, IConfiguration configuration)
        {
            _vietQRService = vietQRService;
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        // GET: api/vietqr/test/{orderId} - Test để kiểm tra order tồn tại
        [HttpGet("test/{orderId}")]
        [AllowAnonymous]
        public async Task<IActionResult> TestOrderExists(string orderId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return Ok(new
                    {
                        exists = false,
                        message = $"Order {orderId} không tồn tại",
                        orderId = orderId
                    });
                }

                return Ok(new
                {
                    exists = true,
                    message = "Order tồn tại",
                    orderId = order.OrderId,
                    totalAmount = order.TotalAmount,
                    status = order.Status,
                    createdAt = order.OrderDate
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/vietqr/create-qr
        [HttpPost("create-qr")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateVietQRPayment([FromBody] VietQRPaymentRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.OrderId))
                    return BadRequest(new { message = "OrderId không được để trống" });

                if (request.Amount <= 0)
                    return BadRequest(new { message = "Số tiền phải lớn hơn 0" });

                _logger.LogInformation($"Creating VietQR payment for orderId: {request.OrderId}, amount: {request.Amount}");

                // Kiểm tra order tồn tại
                var order = await _context.Orders.FindAsync(request.OrderId);
                if (order == null)
                {
                    _logger.LogWarning($"Order not found: {request.OrderId}");
                    return NotFound(new { message = $"Không tìm thấy đơn hàng có ID: {request.OrderId}" });
                }

                _logger.LogInformation($"Order found. Order.TotalAmount: {order.TotalAmount}, Request.Amount: {request.Amount}");

                if (order.TotalAmount != request.Amount)
                {
                    _logger.LogWarning($"Amount mismatch for order {request.OrderId}. Expected: {order.TotalAmount}, Got: {request.Amount}");
                    return BadRequest(new { message = $"Số tiền không khớp. Đơn hàng: {order.TotalAmount}đ, Request: {request.Amount}đ" });
                }

                // Tạo mô tả (max 25 chars)
                var description = request.Description ?? $"Don hang {request.OrderId}";
                if (description.Length > 25)
                    description = description.Substring(0, 25);

                // Tạo hoặc cập nhật Payment record với status = Pending
                var existingPayment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == request.OrderId);
                
                if (existingPayment == null)
                {
                    var payment = new Payment
                    {
                        PaymentId = Guid.NewGuid().ToString(),
                        OrderId = request.OrderId,
                        CustomerId = order.CustomerId,  // Lấy từ Order
                        Amount = request.Amount,
                        PaymentMethod = "VietQR",
                        Status = "Pending",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        Notes = "QR code generated, waiting for payment"
                    };
                    _context.Payments.Add(payment);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Payment record created: {payment.PaymentId}");
                }

                // Tạo URL QR từ VietQR API
                var qrImageUrl = _vietQRService.GenerateQRUrl(
                    request.OrderId,
                    request.Amount,
                    description
                );

                // Lấy config VietQR
                var bankCode = _configuration["VietQR:BankCode"] ?? "970422";
                var accountNumber = _configuration["VietQR:AccountNumber"] ?? "808080190705";
                var accountName = _configuration["VietQR:AccountName"] ?? "Tran Huu Phuoc";

                _logger.LogInformation($"VietQR payment QR created for order {request.OrderId}, amount {request.Amount}");

                return Ok(new VietQRPaymentResponse
                {
                    QRImageUrl = qrImageUrl,
                    QRCodeBase64 = "",  // Không dùng base64, chỉ dùng URL
                    OrderId = request.OrderId,
                    Amount = request.Amount,
                    AccountNumber = accountNumber,
                    AccountName = accountName,
                    BankCode = bankCode,
                    Message = "Tạo mã QR VietQR thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating VietQR payment: {ex.Message}\n{ex.StackTrace}");
                return BadRequest(new { message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        // POST: api/vietqr/webhook (Webhook từ ngân hàng khi khách thanh toán)
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleVietQRWebhook([FromBody] VietQRWebhookNotification notification)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(notification.OrderId))
                    return BadRequest(new { message = "OrderId không được để trống" });

                _logger.LogInformation($"VietQR webhook received for order {notification.OrderId}, status: {notification.Status}");

                // Kiểm tra order
                var order = await _context.Orders.FindAsync(notification.OrderId);
                if (order == null)
                    return NotFound(new { message = "Đơn hàng không tồn tại" });

                // Tìm hoặc tạo payment record
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == notification.OrderId);
                
                if (payment == null)
                {
                    payment = new Payment
                    {
                        PaymentId = Guid.NewGuid().ToString(),
                        OrderId = notification.OrderId,
                        CustomerId = order.CustomerId,  // Lấy từ Order
                        Amount = notification.Amount,
                        PaymentMethod = "VietQR",
                        Status = "Pending",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _context.Payments.Add(payment);
                }

                // Update status dựa trên webhook
                if (notification.Status == "Success")
                {
                    payment.Status = "Success";
                    payment.TransactionId = notification.TransactionId;
                    payment.CompletedAt = notification.TransactionDate;
                    payment.Notes = $"VietQR thanh toán từ {notification.FromAccountName}";
                    payment.UpdatedAt = DateTime.Now;

                    // Update order
                    order.Status = "Paid";
                    order.UpdatedAt = DateTime.Now;
                    _context.Orders.Update(order);
                }
                else if (notification.Status == "Failed")
                {
                    payment.Status = "Failed";
                    payment.Notes = "VietQR thanh toán thất bại";
                    payment.UpdatedAt = DateTime.Now;
                }

                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Payment {payment.PaymentId} updated to {payment.Status}");

                return Ok(new { message = "Webhook processed successfully", orderId = notification.OrderId, status = payment.Status });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing VietQR webhook: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/vietqr/{orderId}
        [HttpGet("{orderId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVietQRStatus(string orderId)
        {
            try
            {
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
                if (payment == null)
                    return NotFound(new { message = "Không tìm thấy thông tin thanh toán" });

                return Ok(new
                {
                    orderId = payment.OrderId,
                    amount = payment.Amount,
                    status = payment.Status,
                    method = payment.PaymentMethod,
                    transactionId = payment.TransactionId,
                    transactionDate = payment.CompletedAt,
                    createdAt = payment.CreatedAt,
                    updatedAt = payment.UpdatedAt,
                    notes = payment.Notes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting VietQR status: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
