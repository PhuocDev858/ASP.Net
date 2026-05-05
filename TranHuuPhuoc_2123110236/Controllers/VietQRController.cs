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

        public VietQRController(IVietQRService vietQRService, AppDbContext context, ILogger<VietQRController> logger)
        {
            _vietQRService = vietQRService;
            _context = context;
            _logger = logger;
        }

        // POST: api/vietqr/create-qr
        [HttpPost("create-qr")]
        public async Task<IActionResult> CreateVietQRPayment([FromBody] VietQRPaymentRequest request)
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

                // Tạo mô tả (max 25 chars)
                var description = request.Description ?? $"Don hang {request.OrderId}";
                if (description.Length > 25)
                    description = description.Substring(0, 25);

                // Tạo URL QR từ VietQR API
                var qrImageUrl = _vietQRService.GenerateQRUrl(
                    request.OrderId,
                    request.Amount,
                    description
                );

                // Tạo mã QR dạng ảnh (base64)
                var qrCodeBytes = _vietQRService.GenerateQRCode(qrImageUrl);
                var qrCodeBase64 = Convert.ToBase64String(qrCodeBytes);

                // Lấy config VietQR
                var bankCode = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build()["VietQR:BankCode"] ?? "970422";
                var accountNumber = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build()["VietQR:AccountNumber"] ?? "808080190705";
                var accountName = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build()["VietQR:AccountName"] ?? "Tran Huu Phuoc";

                _logger.LogInformation($"VietQR payment created for order {request.OrderId}, amount {request.Amount}");

                return Ok(new VietQRPaymentResponse
                {
                    QRImageUrl = qrImageUrl,
                    QRCodeBase64 = qrCodeBase64,
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
                _logger.LogError($"Error creating VietQR payment: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/vietqr/webhook (Webhook từ ngân hàng khi khách thanh toán)
        [HttpPost("webhook")]
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
