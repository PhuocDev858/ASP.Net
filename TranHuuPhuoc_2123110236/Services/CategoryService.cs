using Microsoft.EntityFrameworkCore;
using TranHuuPhuoc_2123110236.Data;
using TranHuuPhuoc_2123110236.Models;

namespace TranHuuPhuoc_2123110236.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả danh mục
        public async Task<List<Category>> GetAllCategories()
        {
            try
            {
                return await _context.Categories.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy danh sách danh mục: " + ex.Message);
            }
        }

        // Lấy danh mục theo ID
        public async Task<Category> GetCategoryById(string id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    throw new Exception("Danh mục không tồn tại");
                }
                return category;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy danh mục: " + ex.Message);
            }
        }

        // Tạo danh mục mới
        public async Task<Category> CreateCategory(Category category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category.CategoryId))
                {
                    throw new Exception("ID danh mục không được để trống");
                }

                if (string.IsNullOrWhiteSpace(category.CategoryName))
                {
                    throw new Exception("Tên danh mục không được để trống");
                }

                var existingCategory = await _context.Categories.FindAsync(category.CategoryId);
                if (existingCategory != null)
                {
                    throw new Exception("ID danh mục đã tồn tại");
                }

                category.CreatedAt = DateTime.Now;
                category.UpdatedAt = DateTime.Now;

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return category;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tạo danh mục: " + ex.Message);
            }
        }

        // Cập nhật danh mục
        public async Task<Category> UpdateCategory(string id, Category category)
        {
            try
            {
                var existingCategory = await _context.Categories.FindAsync(id);

                if (existingCategory == null)
                {
                    throw new Exception("Danh mục không tồn tại");
                }

                if (string.IsNullOrWhiteSpace(category.CategoryName))
                {
                    throw new Exception("Tên danh mục không được để trống");
                }

                existingCategory.CategoryName = category.CategoryName;
                existingCategory.Description = category.Description;
                existingCategory.UpdatedAt = DateTime.Now;

                _context.Categories.Update(existingCategory);
                await _context.SaveChangesAsync();

                return existingCategory;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi cập nhật danh mục: " + ex.Message);
            }
        }

        // Xóa danh mục
        public async Task<bool> DeleteCategory(string id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);

                if (category == null)
                {
                    throw new Exception("Danh mục không tồn tại");
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi xóa danh mục: " + ex.Message);
            }
        }

        // Tìm danh mục theo tên
        public async Task<List<Category>> SearchCategoryByName(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new Exception("Tên danh mục không được để trống");
                }

                return await _context.Categories
                    .Where(c => c.CategoryName.Contains(name))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tìm kiếm danh mục: " + ex.Message);
            }
        }

        // Đếm tổng số danh mục
        public async Task<int> GetCategoryCount()
        {
            try
            {   
                return await _context.Categories.CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi đếm danh mục: " + ex.Message);
            }
        }
    }
}