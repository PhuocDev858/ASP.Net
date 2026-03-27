using TranHuuPhuoc_2123110236.Models;

namespace TranHuuPhuoc_2123110236.Services
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllCategories();
        Task<Category> GetCategoryById(string id);
        Task<Category> CreateCategory(Category category);
        Task<Category> UpdateCategory(string id, Category category);
        Task<bool> DeleteCategory(string id);
        Task<List<Category>> SearchCategoryByName(string name);
        Task<int> GetCategoryCount();
    }
}