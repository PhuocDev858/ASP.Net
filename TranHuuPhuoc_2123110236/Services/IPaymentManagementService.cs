using TranHuuPhuoc_2123110236.DTOs;

namespace TranHuuPhuoc_2123110236.Services
{
    public interface IPaymentManagementService
    {
        // Lấy tất cả thanh toán
        Task<List<PaymentDetailResponse>> GetAllPayments();

        // Lấy thanh toán theo ID
        Task<PaymentDetailResponse> GetPaymentById(string paymentId);

        // Lấy thanh toán theo Order ID
        Task<PaymentDetailResponse> GetPaymentByOrderId(string orderId);

        // Lấy thanh toán của khách hàng
        Task<List<PaymentDetailResponse>> GetPaymentsByCustomer(string customerId);

        // Lấy thanh toán theo trạng thái
        Task<List<PaymentDetailResponse>> GetPaymentsByStatus(string status);

        // Lấy thanh toán theo phương thức thanh toán
        Task<List<PaymentDetailResponse>> GetPaymentsByMethod(string method);

        // Tìm kiếm thanh toán
        Task<List<PaymentDetailResponse>> SearchPayments(PaymentSearchRequest request);

        // Lấy thống kê thanh toán
        Task<PaymentStatisticsResponse> GetPaymentStatistics();

        // Lấy doanh thu theo ngày
        Task<List<DailyRevenueResponse>> GetDailyRevenue(int days = 30);

        // Lấy doanh thu theo phương thức thanh toán
        Task<List<PaymentMethodRevenueResponse>> GetRevenueByPaymentMethod();
    }
}
