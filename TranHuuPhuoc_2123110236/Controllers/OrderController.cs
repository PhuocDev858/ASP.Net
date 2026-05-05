using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TranHuuPhuoc_2123110236.DTOs;
using TranHuuPhuoc_2123110236.Services.OrderServices;
using System.Security.Claims;

namespace TranHuuPhuoc_2123110236.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        // POST: api/order
        [Authorize(Roles = "Customer,Staff,Admin")]
        [HttpPost]
        public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                var order = await _orderService.CreateOrder(request);
                return CreatedAtAction(nameof(GetOrderById), new { orderId = order.OrderId }, order);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Create order error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/order/{orderId}
        [Authorize(Roles = "Customer,Staff,Admin")]
        [HttpGet("{orderId}")]
        public async Task<ActionResult<OrderResponse>> GetOrderById(string orderId)
        {
            try
            {
                var order = await _orderService.GetOrderById(orderId);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET: api/order/customer/{customerId}
        [Authorize(Roles = "Customer,Staff,Admin")]
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<List<OrderResponse>>> GetOrdersByCustomer(string customerId)
        {
            try
            {
                var orders = await _orderService.GetOrdersByCustomer(customerId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/order
        [Authorize(Roles = "Staff,Admin")]
        [HttpGet]
        public async Task<ActionResult<List<OrderResponse>>> GetAllOrders()
        {
            try
            {
                var orders = await _orderService.GetAllOrders();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/order/{orderId}/status
        [Authorize(Roles = "Staff,Admin")]
        [HttpPut("{orderId}/status")]
        public async Task<IActionResult> UpdateOrderStatus(string orderId, [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                await _orderService.UpdateOrderStatus(orderId, request.NewStatus);
                return Ok(new { message = $"Cập nhật trạng thái đơn hàng {orderId} thành {request.NewStatus} thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/order/{orderId}
        [Authorize(Roles = "Customer,Staff,Admin")]
        [HttpDelete("{orderId}")]
        public async Task<IActionResult> CancelOrder(string orderId)
        {
            try
            {
                await _orderService.CancelOrder(orderId);
                return Ok(new { message = "Hủy đơn hàng thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/order/{orderId}/mark-paid
        [AllowAnonymous]
        [HttpPut("{orderId}/mark-paid")]
        public async Task<IActionResult> MarkOrderAsPaid(string orderId)
        {
            try
            {
                await _orderService.MarkOrderAsPaid(orderId);
                return Ok(new { message = $"Đánh dấu đơn hàng {orderId} đã thanh toán thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/order/{orderId}/mark-processing
        [Authorize(Roles = "Staff,Admin")]
        [HttpPut("{orderId}/mark-processing")]
        public async Task<IActionResult> MarkOrderAsProcessing(string orderId)
        {
            try
            {
                await _orderService.MarkOrderAsProcessing(orderId);
                return Ok(new { message = $"Đánh dấu đơn hàng {orderId} đang xử lý thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/order/status/{status}
        [Authorize(Roles = "Staff,Admin")]
        [HttpGet("status/{status}")]
        public async Task<ActionResult<List<OrderResponse>>> GetOrdersByStatus(string status)
        {
            try
            {
                var orders = await _orderService.GetOrdersByStatus(status);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/order/count
        [Authorize(Roles = "Admin")]
        [HttpGet("analytics/count")]
        public async Task<ActionResult<int>> GetOrderCount()
        {
            try
            {
                var count = await _orderService.GetOrderCount();
                return Ok(new { totalOrders = count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/order/revenue
        [Authorize(Roles = "Admin")]
        [HttpGet("analytics/revenue")]
        public async Task<ActionResult<decimal>> GetTotalRevenue()
        {
            try
            {
                var revenue = await _orderService.GetTotalRevenue();
                return Ok(new { totalRevenue = revenue });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}