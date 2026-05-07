using Microsoft.EntityFrameworkCore;
using TranHuuPhuoc_2123110236.Data;
using TranHuuPhuoc_2123110236.Models;

namespace TranHuuPhuoc_2123110236.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả sản phẩm
        public async Task<List<Product>> GetAllProducts()
        {
            try
            {
                return await _context.Products.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy danh sách sản phẩm: " + ex.Message);
            }
        }

        // Lấy sản phẩm theo ID
        public async Task<Product> GetProductById(string id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    throw new Exception("Sản phẩm không tồn tại");
                }
                return product;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy sản phẩm: " + ex.Message);
            }
        }

        // Tạo sản phẩm mới
        public async Task<Product> CreateProduct(Product product)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(product.ProductName))
                {
                    throw new Exception("Tên sản phẩm không được để trống");
                }

                if (product.Price <= 0)
                {
                    throw new Exception("Giá sản phẩm phải lớn hơn 0");
                }

                if (product.Stock < 0)
                {
                    throw new Exception("Số lượng tồn kho không được âm");
                }

                product.CreatedAt = DateTime.Now;
                product.UpdatedAt = DateTime.Now;

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return product;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tạo sản phẩm: " + ex.Message);
            }
        }

        // Cập nhật sản phẩm
        public async Task<Product> UpdateProduct(string id, Product product)
        {
            try
            {
                var existingProduct = await _context.Products.FindAsync(id);

                if (existingProduct == null)
                {
                    throw new Exception("Sản phẩm không tồn tại");
                }

                if (string.IsNullOrWhiteSpace(product.ProductName))
                {
                    throw new Exception("Tên sản phẩm không được để trống");
                }

                if (product.Price <= 0)
                {
                    throw new Exception("Giá sản phẩm phải lớn hơn 0");
                }

                if (product.Stock < 0)
                {
                    throw new Exception("Số lượng tồn kho không được âm");
                }

                existingProduct.ProductName = product.ProductName;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.Price = product.Price;
                existingProduct.Stock = product.Stock;
                existingProduct.Description = product.Description;
                existingProduct.ImageUrl = product.ImageUrl;
                existingProduct.UpdatedAt = DateTime.Now;

                _context.Products.Update(existingProduct);
                await _context.SaveChangesAsync();

                return existingProduct;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi cập nhật sản phẩm: " + ex.Message);
            }
        }

        // Xóa sản phẩm
        public async Task<bool> DeleteProduct(string id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);

                if (product == null)
                {
                    throw new Exception("Sản phẩm không tồn tại");
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi xóa sản phẩm: " + ex.Message);
            }
        }

        // Tìm sản phẩm theo tên
        public async Task<List<Product>> SearchProductByName(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new Exception("Tên sản phẩm không được để trống");
                }

                return await _context.Products
                    .Where(p => p.ProductName.Contains(name))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tìm kiếm sản phẩm: " + ex.Message);
            }
        }

        // Lấy sản phẩm theo danh mục
        public async Task<List<Product>> GetProductsByCategory(string category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category))
                {
                    throw new Exception("Danh mục không được để trống");
                }

                return await _context.Products
                    .Where(p => p.CategoryId == category)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy sản phẩm theo danh mục: " + ex.Message);
            }
        }

        // Lấy sản phẩm theo khoảng giá
        public async Task<List<Product>> GetProductsByPriceRange(decimal minPrice, decimal maxPrice)
        {
            try
            {
                if (minPrice < 0 || maxPrice < 0)
                {
                    throw new Exception("Giá không được âm");
                }

                if (minPrice > maxPrice)
                {
                    throw new Exception("Giá tối thiểu không được lớn hơn giá tối đa");
                }

                return await _context.Products
                    .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy sản phẩm theo khoảng giá: " + ex.Message);
            }
        }

        // Đếm tổng số sản phẩm
        public async Task<int> GetProductCount()
        {
            try
            {
                return await _context.Products.CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi đếm sản phẩm: " + ex.Message);
            }
        }

        // Lấy giá trung bình theo danh mục
        public async Task<decimal> GetAveragePriceByCategory(string category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category))
                {
                    throw new Exception("Danh mục không được để trống");
                }

                var products = await _context.Products
                    .Where(p => p.CategoryId == category)
                    .ToListAsync();

                if (products.Count == 0)
                {
                    throw new Exception("Không tìm thấy sản phẩm trong danh mục này");
                }

                return products.Average(p => p.Price);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy giá trung bình: " + ex.Message);
            }
        }
    }
}