using Microsoft.EntityFrameworkCore;
using TranHuuPhuoc_2123110236.Data;
using TranHuuPhuoc_2123110236.DTOs;
using TranHuuPhuoc_2123110236.Models;
using TranHuuPhuoc_2123110236.Services.OrderServices;

namespace TranHuuPhuoc_2123110236.Services.CartServices
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;
        private readonly IOrderService _orderService;
        private readonly ILogger<CartService> _logger;

        public CartService(AppDbContext context, IOrderService orderService, ILogger<CartService> logger)
        {
            _context = context;
            _orderService = orderService;
            _logger = logger;
        }

        // Add to Cart
        public async Task<CartItemResponse> AddToCart(string customerId, AddToCartRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(customerId))
                    throw new Exception("CustomerId không được để trống");

                if (string.IsNullOrWhiteSpace(request.ProductId))
                    throw new Exception("ProductId không được để trống");

                if (request.Quantity <= 0)
                    throw new Exception("Số lượng phải > 0");

                var customer = await _context.Customer.FindAsync(customerId);
                if (customer == null || !customer.IsActive)
                    throw new Exception("Khách hàng không tồn tại hoặc không hoạt động");

                var product = await _context.Product.FindAsync(request.ProductId);
                if (product == null)
                    throw new Exception("Sản phẩm không tồn tại");

                if (product.Stock < request.Quantity)
                    throw new Exception($"Số lượng tồn kho không đủ. Tồn kho: {product.Stock}");

                var existingCart = await _context.Cart
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.ProductId == request.ProductId);

                if (existingCart != null)
                {
                    existingCart.Quantity += request.Quantity;
                    existingCart.TotalPrice = existingCart.Quantity * product.Price;
                    existingCart.UpdatedAt = DateTime.Now;
                    _context.Cart.Update(existingCart);
                }
                else
                {
                    var cartItem = new Cart
                    {
                        CartId = GenerateCartId(),
                        CustomerId = customerId,
                        ProductId = request.ProductId,
                        Quantity = request.Quantity,
                        UnitPrice = product.Price,
                        TotalPrice = product.Price * request.Quantity,
                        AddedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _context.Cart.Add(cartItem);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Customer {customerId} thêm sản phẩm {request.ProductId} vào giỏ hàng");

                return new CartItemResponse
                {
                    CartId = existingCart?.CartId ?? GenerateCartId(),
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Quantity = request.Quantity,
                    UnitPrice = product.Price,
                    TotalPrice = product.Price * request.Quantity,
                    ImageUrl = product.ImageUrl ?? "",  // ✅ Fix lỗi 2
                    AddedAt = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi thêm vào giỏ hàng: {ex.Message}");
                throw new Exception("Lỗi khi thêm vào giỏ hàng: " + ex.Message);
            }
        }

        // Remove from Cart
        public async Task<bool> RemoveFromCart(string customerId, string productId)
        {
            try
            {
                var cartItem = await _context.Cart
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.ProductId == productId);

                if (cartItem == null)
                    throw new Exception("Sản phẩm không trong giỏ hàng");

                _context.Cart.Remove(cartItem);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Customer {customerId} xóa sản phẩm {productId} khỏi giỏ hàng");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi xóa khỏi giỏ hàng: {ex.Message}");
                throw new Exception("Lỗi khi xóa khỏi giỏ hàng: " + ex.Message);
            }
        }

        // Update Cart Item
        public async Task<CartItemResponse> UpdateCartItem(string customerId, string productId, UpdateCartItemRequest request)
        {
            try
            {
                if (request.Quantity <= 0)
                    throw new Exception("Số lượng phải > 0");

                var cartItem = await _context.Cart
                    .Include(c => c.Product)
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.ProductId == productId);

                if (cartItem == null)
                    throw new Exception("Sản phẩm không trong giỏ hàng");

                var product = await _context.Product.FindAsync(productId);
                if (product.Stock < request.Quantity)
                    throw new Exception($"Số lượng tồn kho không đủ. Tồn kho: {product.Stock}");

                cartItem.Quantity = request.Quantity;
                cartItem.TotalPrice = cartItem.UnitPrice * request.Quantity;
                cartItem.UpdatedAt = DateTime.Now;

                _context.Cart.Update(cartItem);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Customer {customerId} cập nhật sản phẩm {productId} trong giỏ hàng");

                return new CartItemResponse
                {
                    CartId = cartItem.CartId,
                    ProductId = cartItem.ProductId,
                    ProductName = product.ProductName,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice,
                    TotalPrice = cartItem.TotalPrice,
                    ImageUrl = product.ImageUrl ?? "",  // ✅ Fix lỗi 2
                    AddedAt = cartItem.AddedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi cập nhật giỏ hàng: {ex.Message}");
                throw new Exception("Lỗi khi cập nhật giỏ hàng: " + ex.Message);
            }
        }

        // Get Cart
        public async Task<GetCartResponse> GetCart(string customerId)
        {
            try
            {
                var cartItems = await _context.Cart
                    .Where(c => c.CustomerId == customerId)
                    .Include(c => c.Product)
                    .ToListAsync();

                var items = cartItems.Select(c => new CartItemResponse
                {
                    CartId = c.CartId,
                    ProductId = c.ProductId,
                    ProductName = c.Product?.ProductName ?? "",
                    Quantity = c.Quantity,
                    UnitPrice = c.UnitPrice,
                    TotalPrice = c.TotalPrice,
                    ImageUrl = c.Product?.ImageUrl ?? "",  // ✅ Fix lỗi 2
                    AddedAt = c.AddedAt
                }).ToList();

                var subTotal = cartItems.Sum(c => c.TotalPrice);
                var tax = subTotal * 0.08m;
                var shippingFee = subTotal > 500000 ? 0 : 50000;
                var grandTotal = subTotal + tax + shippingFee;

                return new GetCartResponse
                {
                    Items = items,
                    TotalItems = items.Count,  // ✅ Fix lỗi 4
                    SubTotal = subTotal,
                    Tax = tax,
                    ShippingFee = shippingFee,
                    GrandTotal = grandTotal
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy giỏ hàng: {ex.Message}");
                throw new Exception("Lỗi khi lấy giỏ hàng: " + ex.Message);
            }
        }

        // Clear Cart
        public async Task<bool> ClearCart(string customerId)
        {
            try
            {
                var cartItems = await _context.Cart
                    .Where(c => c.CustomerId == customerId)
                    .ToListAsync();

                _context.Cart.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Customer {customerId} đã xóa hết giỏ hàng");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi xóa giỏ hàng: {ex.Message}");
                throw new Exception("Lỗi khi xóa giỏ hàng: " + ex.Message);
            }
        }

        // Checkout - Convert Cart to Order
        public async Task<CheckoutResponse> Checkout(string customerId, CheckoutRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ShippingAddress))
                    throw new Exception("Địa chỉ giao hàng không được để trống");

                if (string.IsNullOrWhiteSpace(request.PaymentMethod))
                    throw new Exception("Phương thức thanh toán không được để trống");

                var cartItems = await _context.Cart
                    .Where(c => c.CustomerId == customerId)
                    .Include(c => c.Product)
                    .ToListAsync();

                if (cartItems.Count == 0)  // ✅ Fix lỗi 4
                    throw new Exception("Giỏ hàng trống");

                var createOrderRequest = new CreateOrderRequest
                {
                    CustomerId = customerId,
                    ShippingAddress = request.ShippingAddress,
                    Products = cartItems.Select(c => new OrderItemRequest
                    {
                        ProductId = c.ProductId,
                        Quantity = c.Quantity
                    }).ToList()
                };

                var order = await _orderService.CreateOrder(createOrderRequest);

                await ClearCart(customerId);

                _logger.LogInformation($"Customer {customerId} checkout thành công. Order: {order.OrderId}");

                return new CheckoutResponse
                {
                    OrderId = order.OrderId,
                    TotalAmount = order.TotalAmount,
                    Status = "Pending",
                    Message = $"Tạo đơn hàng thành công. Vui lòng thanh toán để hoàn tất đơn hàng"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi checkout: {ex.Message}");
                throw new Exception("Lỗi khi checkout: " + ex.Message);
            }
        }

        // Get Cart Item Count
        public async Task<int> GetCartItemCount(string customerId)
        {
            try
            {
                return await _context.Cart
                    .Where(c => c.CustomerId == customerId)
                    .SumAsync(c => c.Quantity);  // ✅ Fix lỗi 3
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy số lượng giỏ hàng: " + ex.Message);
            }
        }

        private string GenerateCartId()
        {
            return "CART" + DateTime.Now.ToString("yyyyMMddHHmmss") + Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
        }
    }
}