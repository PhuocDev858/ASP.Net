using TranHuuPhuoc_2123110236.Models;

namespace TranHuuPhuoc_2123110236.Services
{
    public interface ProductServiceImp
    {
        Task<List<Product>> GetAllProducts();
        Task<Product> GetProductById(int id);
        Task<Product> CreateProduct(Product product);
        Task<Product> UpdateProduct(int id, Product product);
        Task<bool> DeleteProduct(int id);
        Task<List<Product>> SearchProductByName(string name);
        Task<List<Product>> GetProductsByCategory(string category);
        Task<List<Product>> GetProductsByPriceRange(decimal minPrice, decimal maxPrice);
        Task<int> GetProductCount();
        Task<decimal> GetAveragePriceByCategory(string category);
    }
}
