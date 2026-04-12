using TranHuuPhuoc_2123110236.DTOs;

namespace TranHuuPhuoc_2123110236.Services.CartServices
{
    public interface ICartService
    {
        Task<CartItemResponse> AddToCart(string customerId, AddToCartRequest request);
        Task<bool> RemoveFromCart(string customerId, string productId);
        Task<CartItemResponse> UpdateCartItem(string customerId, string productId, UpdateCartItemRequest request);
        Task<GetCartResponse> GetCart(string customerId);
        Task<bool> ClearCart(string customerId);
        Task<CheckoutResponse> Checkout(string customerId, CheckoutRequest request);
        Task<int> GetCartItemCount(string customerId);
    }
}