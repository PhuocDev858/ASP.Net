using TranHuuPhuoc_2123110236.DTOs;

namespace TranHuuPhuoc_2123110236.Services.OrderServices
{
    public interface IOrderService
    {
        Task<OrderResponse> CreateOrder(CreateOrderRequest request);
        Task<OrderResponse> GetOrderById(string orderId);
        Task<List<OrderResponse>> GetOrdersByCustomer(string customerId);
        Task<List<OrderResponse>> GetAllOrders();
        Task<bool> UpdateOrderStatus(string orderId, string newStatus);
        Task<bool> CancelOrder(string orderId);
        Task<List<OrderResponse>> GetOrdersByStatus(string status);
        Task<int> GetOrderCount();
        Task<decimal> GetTotalRevenue();
    }
}