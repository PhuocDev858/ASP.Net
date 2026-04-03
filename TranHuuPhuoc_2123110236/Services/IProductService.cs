using TranHuuPhuoc_2123110236.Models;
namespace TranHuuPhuoc_2123110236.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetAllProducts();
        Task<Product> GetProductById(string id);        // ← int → string
        Task<Product> CreateProduct(Product product);
        Task<Product> UpdateProduct(string id, Product product);  // ← int → string
        Task<bool> DeleteProduct(string id);            // ← int → string
        Task<List<Product>> SearchProductByName(string name);
        Task<List<Product>> GetProductsByCategory(string category);
        Task<List<Product>> GetProductsByPriceRange(decimal minPrice, decimal maxPrice);
        Task<int> GetProductCount();
        Task<decimal> GetAveragePriceByCategory(string category);
    }
}