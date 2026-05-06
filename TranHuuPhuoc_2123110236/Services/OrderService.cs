using Microsoft.EntityFrameworkCore;
using TranHuuPhuoc_2123110236.Data;
using TranHuuPhuoc_2123110236.DTOs;
using TranHuuPhuoc_2123110236.Models;
using System.Linq;

namespace TranHuuPhuoc_2123110236.Services.OrderServices
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(AppDbContext context, ILogger<OrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Create Order
        public async Task<OrderResponse> CreateOrder(CreateOrderRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.CustomerId))
                    throw new Exception("CustomerId không được để trống");

                if (string.IsNullOrWhiteSpace(request.ShippingAddress))
                    throw new Exception("ShippingAddress không được để trống");

                if (request.Products == null || request.Products.Count == 0)
                    throw new Exception("Phải chọn ít nhất 1 sản phẩm");

                var customer = await _context.Customers.FindAsync(request.CustomerId);
                if (customer == null || !customer.IsActive)
                    throw new Exception("Khách hàng không tồn tại hoặc không hoạt động");

                var employee = await _context.Employees..FirstOrDefaultAsync(e => e.IsActive);
                if (employee == null)
                    throw new Exception("Không có nhân viên nào để xử lý đơn hàng");

                var orderId = GenerateOrderId();
                var order = new Order
                {
                    OrderId = orderId,
                    CustomerId = request.CustomerId,
                    EmployeeId = employee.EmployeeId,
                    OrderDate = DateTime.Now,
                    Status = "Pending",
                    ShippingAddress = request.ShippingAddress,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                decimal totalAmount = 0;
                var orderDetails = new List<OrderDetail>();

                foreach (var item in request.Products)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null)
                        throw new Exception($"Sản phẩm {item.ProductId} không tồn tại");

                    if (product.Stock < item.Quantity)
                        throw new Exception($"Sản phẩm {product.ProductName} không đủ số lượng. Tồn kho: {product.Stock}");

                    var orderDetail = new OrderDetail
                    {
                        OrderDetailId = GenerateOrderDetailId(),
                        OrderId = orderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        TotalPrice = product.Price * item.Quantity
                    };

                    orderDetails.Add(orderDetail);
                    totalAmount += orderDetail.TotalPrice;

                    product.Stock -= item.Quantity;
                    _context.Products.Update(product);
                }

                order.TotalAmount = totalAmount;
                order.OrderDetails = orderDetails;

                customer.TotalOrders++;
                customer.TotalSpent += totalAmount;
                customer.UpdatedAt = DateTime.Now;
                _context.Customers.Update(customer);

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Tạo đơn hàng {orderId} thành công");

                return MapToOrderResponse(order);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi tạo đơn hàng: {ex.Message}");
                throw new Exception("Lỗi khi tạo đơn hàng: " + ex.Message);
            }
        }

        // Get Order By Id
        public async Task<OrderResponse> GetOrderById(string orderId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Employee)
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                    throw new Exception("Đơn hàng không tồn tại");

                return MapToOrderResponse(order);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy đơn hàng: " + ex.Message);
            }
        }

        // Get Orders By Customer
        public async Task<List<OrderResponse>> GetOrdersByCustomer(string customerId)
        {
            try
            {
                var orders = await _context.Orders
                    .Where(o => o.CustomerId == customerId)
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                return orders.Select(MapToOrderResponse).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy đơn hàng: " + ex.Message);
            }
        }

        // Get All Orders
        public async Task<List<OrderResponse>> GetAllOrders()
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                return orders.Select(MapToOrderResponse).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy danh sách đơn hàng: " + ex.Message);
            }
        }

        // Update Order Status
        public async Task<bool> UpdateOrderStatus(string orderId, string newStatus)
        {
            try
            {
                var validStatuses = new[] { "Pending", "Paid", "Processing", "Shipped", "Delivered", "Cancelled" };
                if (!validStatuses.Contains(newStatus))
                    throw new Exception("Trạng thái không hợp lệ");

                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                    throw new Exception("Đơn hàng không tồn tại");

                order.Status = newStatus;
                order.UpdatedAt = DateTime.Now;

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Cập nhật trạng thái đơn hàng {orderId} thành {newStatus}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi cập nhật trạng thái: {ex.Message}");
                throw new Exception("Lỗi khi cập nhật trạng thái: " + ex.Message);
            }
        }

        // Cancel Order
        public async Task<bool> CancelOrder(string orderId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                    throw new Exception("Đơn hàng không tồn tại");

                if (order.Status == "Delivered" || order.Status == "Cancelled")
                    throw new Exception($"Không thể hủy đơn hàng có trạng thái {order.Status}");

                foreach (var detail in order.OrderDetails)
                {
                    var product = await _context.Products.FindAsync(detail.ProductId);
                    if (product != null)
                    {
                        product.Stock += detail.Quantity;
                        _context.Products.Update(product);
                    }
                }

                var customer = await _context.Customers.FindAsync(order.CustomerId);
                if (customer != null)
                {
                    customer.TotalSpent -= order.TotalAmount;
                    customer.TotalOrders--;
                    _context.Customers.Update(customer);
                }

                order.Status = "Cancelled";
                order.UpdatedAt = DateTime.Now;

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Hủy đơn hàng {orderId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi hủy đơn hàng: {ex.Message}");
                throw new Exception("Lỗi khi hủy đơn hàng: " + ex.Message);
            }
        }

        // Mark Order as Paid
        public async Task<bool> MarkOrderAsPaid(string orderId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                    throw new Exception("Đơn hàng không tồn tại");

                if (order.Status != "Pending")
                    throw new Exception($"Chỉ có thể thanh toán đơn hàng có trạng thái 'Pending'. Hiện tại: {order.Status}");

                order.Status = "Paid";
                order.UpdatedAt = DateTime.Now;

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Đánh dấu đơn hàng {orderId} đã thanh toán");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi đánh dấu thanh toán: {ex.Message}");
                throw new Exception("Lỗi khi đánh dấu thanh toán: " + ex.Message);
            }
        }

        // Mark Order as Processing
        public async Task<bool> MarkOrderAsProcessing(string orderId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                    throw new Exception("Đơn hàng không tồn tại");

                if (order.Status != "Paid")
                    throw new Exception($"Chỉ có thể xử lý đơn hàng có trạng thái 'Paid'. Hiện tại: {order.Status}");

                order.Status = "Processing";
                order.UpdatedAt = DateTime.Now;

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Đánh dấu đơn hàng {orderId} đang xử lý");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi đánh dấu xử lý: {ex.Message}");
                throw new Exception("Lỗi khi đánh dấu xử lý: " + ex.Message);
            }
        }

        // Get Orders By Status
        public async Task<List<OrderResponse>> GetOrdersByStatus(string status)
        {
            try
            {
                var orders = await _context.Orders
                    .Where(o => o.Status == status)
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                return orders.Select(MapToOrderResponse).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy đơn hàng: " + ex.Message);
            }
        }

        // Get Order Count
        public async Task<int> GetOrderCount()
        {
            try
            {
                return await _context.Orders.CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi đếm đơn hàng: " + ex.Message);
            }
        }

        // Get Total Revenue
        public async Task<decimal> GetTotalRevenue()
        {
            try
            {
                return await _context.Orders
                    .Where(o => o.Status == "Delivered")
                    .SumAsync(o => o.TotalAmount);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tính doanh thu: " + ex.Message);
            }
        }

        private string GenerateOrderId()
        {
            return "ORD" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        private string GenerateOrderDetailId()
        {
            return "ODT" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        }

        private OrderResponse MapToOrderResponse(Order order)
        {
            return new OrderResponse
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                EmployeeId = order.EmployeeId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress,
                OrderDetails = order.OrderDetails?.Select(od => new OrderDetailResponse
                {
                    OrderDetailId = od.OrderDetailId,
                    ProductId = od.ProductId,
                    ProductName = od.Product?.ProductName ?? "",
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice,
                    TotalPrice = od.TotalPrice
                }).ToList() ?? new List<OrderDetailResponse>(),
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };
        }
    }
}