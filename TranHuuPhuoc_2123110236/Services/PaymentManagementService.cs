using Microsoft.EntityFrameworkCore;
using TranHuuPhuoc_2123110236.Data;
using TranHuuPhuoc_2123110236.DTOs;
using TranHuuPhuoc_2123110236.Models;

namespace TranHuuPhuoc_2123110236.Services
{
    public class PaymentManagementService : IPaymentManagementService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PaymentManagementService> _logger;

        public PaymentManagementService(AppDbContext context, ILogger<PaymentManagementService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Lấy tất cả thanh toán
        public async Task<List<PaymentDetailResponse>> GetAllPayments()
        {
            try
            {
                var payments = await _context.Payments
                    .Include(p => p.Order)
                    .Include(p => p.Customer)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                return payments.Select(MapToPaymentDetailResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all payments: {ex.Message}");
                throw new Exception("Lỗi khi lấy danh sách thanh toán: " + ex.Message);
            }
        }

        // Lấy thanh toán theo ID
        public async Task<PaymentDetailResponse> GetPaymentById(string paymentId)
        {
            try
            {
                var payment = await _context.Payments
                    .Include(p => p.Order)
                    .Include(p => p.Customer)
                    .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

                if (payment == null)
                    throw new Exception("Thanh toán không tồn tại");

                return MapToPaymentDetailResponse(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting payment by id: {ex.Message}");
                throw new Exception("Lỗi khi lấy thanh toán: " + ex.Message);
            }
        }

        // Lấy thanh toán theo Order ID
        public async Task<PaymentDetailResponse> GetPaymentByOrderId(string orderId)
        {
            try
            {
                var payment = await _context.Payments
                    .Include(p => p.Order)
                    .Include(p => p.Customer)
                    .FirstOrDefaultAsync(p => p.OrderId == orderId);

                if (payment == null)
                    throw new Exception("Không tìm thấy thanh toán cho đơn hàng này");

                return MapToPaymentDetailResponse(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting payment by order id: {ex.Message}");
                throw new Exception("Lỗi khi lấy thanh toán: " + ex.Message);
            }
        }

        // Lấy thanh toán của khách hàng
        public async Task<List<PaymentDetailResponse>> GetPaymentsByCustomer(string customerId)
        {
            try
            {
                var payments = await _context.Payments
                    .Where(p => p.CustomerId == customerId)
                    .Include(p => p.Order)
                    .Include(p => p.Customer)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                return payments.Select(MapToPaymentDetailResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting payments by customer: {ex.Message}");
                throw new Exception("Lỗi khi lấy thanh toán: " + ex.Message);
            }
        }

        // Lấy thanh toán theo trạng thái
        public async Task<List<PaymentDetailResponse>> GetPaymentsByStatus(string status)
        {
            try
            {
                var payments = await _context.Payments
                    .Where(p => p.Status == status)
                    .Include(p => p.Order)
                    .Include(p => p.Customer)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                return payments.Select(MapToPaymentDetailResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting payments by status: {ex.Message}");
                throw new Exception("Lỗi khi lấy thanh toán: " + ex.Message);
            }
        }

        // Lấy thanh toán theo phương thức thanh toán
        public async Task<List<PaymentDetailResponse>> GetPaymentsByMethod(string method)
        {
            try
            {
                var payments = await _context.Payments
                    .Where(p => p.PaymentMethod == method)
                    .Include(p => p.Order)
                    .Include(p => p.Customer)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                return payments.Select(MapToPaymentDetailResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting payments by method: {ex.Message}");
                throw new Exception("Lỗi khi lấy thanh toán: " + ex.Message);
            }
        }

        // Tìm kiếm thanh toán
        public async Task<List<PaymentDetailResponse>> SearchPayments(PaymentSearchRequest request)
        {
            try
            {
                var query = _context.Payments
                    .Include(p => p.Order)
                    .Include(p => p.Customer)
                    .AsQueryable();

                // Tìm theo ID
                if (!string.IsNullOrWhiteSpace(request.PaymentId))
                    query = query.Where(p => p.PaymentId.Contains(request.PaymentId));

                // Tìm theo Order ID
                if (!string.IsNullOrWhiteSpace(request.OrderId))
                    query = query.Where(p => p.OrderId.Contains(request.OrderId));

                // Tìm theo Customer ID
                if (!string.IsNullOrWhiteSpace(request.CustomerId))
                    query = query.Where(p => p.CustomerId == request.CustomerId);

                // Lọc theo trạng thái
                if (!string.IsNullOrWhiteSpace(request.Status))
                    query = query.Where(p => p.Status == request.Status);

                // Lọc theo phương thức
                if (!string.IsNullOrWhiteSpace(request.PaymentMethod))
                    query = query.Where(p => p.PaymentMethod == request.PaymentMethod);

                // Lọc theo khoảng số tiền
                if (request.MinAmount.HasValue)
                    query = query.Where(p => p.Amount >= request.MinAmount);

                if (request.MaxAmount.HasValue)
                    query = query.Where(p => p.Amount <= request.MaxAmount);

                // Lọc theo ngày
                if (request.FromDate.HasValue)
                    query = query.Where(p => p.CreatedAt >= request.FromDate);

                if (request.ToDate.HasValue)
                    query = query.Where(p => p.CreatedAt <= request.ToDate);

                var payments = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

                return payments.Select(MapToPaymentDetailResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching payments: {ex.Message}");
                throw new Exception("Lỗi khi tìm kiếm thanh toán: " + ex.Message);
            }
        }

        // Lấy thống kê thanh toán
        public async Task<PaymentStatisticsResponse> GetPaymentStatistics()
        {
            try
            {
                var allPayments = await _context.Payments.ToListAsync();

                var totalPayments = allPayments.Count;
                var successPayments = allPayments.Where(p => p.Status == "Success").ToList();
                var failedPayments = allPayments.Where(p => p.Status == "Failed").ToList();
                var pendingPayments = allPayments.Where(p => p.Status == "Pending").ToList();

                var totalRevenue = successPayments.Sum(p => p.Amount);
                var averageAmount = successPayments.Count > 0 ? successPayments.Average(p => p.Amount) : 0;

                var paymentMethodStats = allPayments
                    .GroupBy(p => p.PaymentMethod)
                    .Select(g => new PaymentMethodStatistic
                    {
                        PaymentMethod = g.Key,
                        Count = g.Count(),
                        TotalAmount = g.Sum(p => p.Amount),
                        SuccessCount = g.Count(p => p.Status == "Success")
                    })
                    .ToList();

                return new PaymentStatisticsResponse
                {
                    TotalPayments = totalPayments,
                    SuccessPayments = successPayments.Count,
                    FailedPayments = failedPayments.Count,
                    PendingPayments = pendingPayments.Count,
                    TotalRevenue = totalRevenue,
                    AverageAmount = averageAmount,
                    SuccessRate = totalPayments > 0 ? Math.Round((double)successPayments.Count / totalPayments * 100, 2) : 0,
                    PaymentMethodStatistics = paymentMethodStats
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting payment statistics: {ex.Message}");
                throw new Exception("Lỗi khi lấy thống kê: " + ex.Message);
            }
        }

        // Lấy doanh thu theo ngày
        public async Task<List<DailyRevenueResponse>> GetDailyRevenue(int days = 30)
        {
            try
            {
                var startDate = DateTime.Now.AddDays(-days);

                var dailyRevenue = await _context.Payments
                    .Where(p => p.CreatedAt >= startDate && p.Status == "Success")
                    .GroupBy(p => p.CreatedAt.Date)
                    .Select(g => new DailyRevenueResponse
                    {
                        Date = g.Key,
                        TotalRevenue = g.Sum(p => p.Amount),
                        TransactionCount = g.Count(),
                        AverageAmount = g.Average(p => p.Amount)
                    })
                    .OrderBy(r => r.Date)
                    .ToListAsync();

                return dailyRevenue;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting daily revenue: {ex.Message}");
                throw new Exception("Lỗi khi lấy doanh thu: " + ex.Message);
            }
        }

        // Lấy doanh thu theo phương thức thanh toán
        public async Task<List<PaymentMethodRevenueResponse>> GetRevenueByPaymentMethod()
        {
            try
            {
                var revenueByMethod = await _context.Payments
                    .Where(p => p.Status == "Success")
                    .GroupBy(p => p.PaymentMethod)
                    .Select(g => new PaymentMethodRevenueResponse
                    {
                        PaymentMethod = g.Key,
                        TotalRevenue = g.Sum(p => p.Amount),
                        TransactionCount = g.Count(),
                        AverageAmount = g.Average(p => p.Amount),
                        PercentageOfTotal = 0 // Sẽ tính sau
                    })
                    .ToListAsync();

                var totalRevenue = revenueByMethod.Sum(r => r.TotalRevenue);
                if (totalRevenue > 0)
                {
                    foreach (var revenue in revenueByMethod)
                    {
                        revenue.PercentageOfTotal = Math.Round((double)(revenue.TotalRevenue / totalRevenue) * 100, 2);
                    }
                }

                return revenueByMethod.OrderByDescending(r => r.TotalRevenue).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting revenue by payment method: {ex.Message}");
                throw new Exception("Lỗi khi lấy doanh thu: " + ex.Message);
            }
        }

        // Helper method
        private PaymentDetailResponse MapToPaymentDetailResponse(Payment payment)
        {
            return new PaymentDetailResponse
            {
                PaymentId = payment.PaymentId,
                OrderId = payment.OrderId,
                CustomerId = payment.CustomerId,
                CustomerName = payment.Customer?.FullName ?? "N/A",
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status,
                TransactionId = payment.TransactionId,
                ConfirmationCode = payment.ConfirmationCode,
                Notes = payment.Notes,
                PaymentDate = payment.PaymentDate,
                CompletedAt = payment.CompletedAt,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt,
                OrderStatus = payment.Order?.Status ?? "N/A"
            };
        }
    }
}
