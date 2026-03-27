using Microsoft.AspNetCore.Mvc;
using TranHuuPhuoc_2123110236.Models;
using TranHuuPhuoc_2123110236.Services;

namespace TranHuuPhuoc_2123110236.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: api/categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetAllCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategories();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET: api/categories/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategoryById(string id)
        {
            try
            {
                var category = await _categoryService.GetCategoryById(id);
                return Ok(category);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/categories
        [HttpPost]
        public async Task<ActionResult<Category>> CreateCategory(Category category)
        {
            try
            {
                var newCategory = await _categoryService.CreateCategory(category);
                return CreatedAtAction(nameof(GetCategoryById), new { id = newCategory.CategoryId }, newCategory);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/categories/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(string id, Category category)
        {
            try
            {
                var updatedCategory = await _categoryService.UpdateCategory(id, category);
                return Ok(new { message = "Cập nhật danh mục thành công", data = updatedCategory });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/categories/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            try
            {
                await _categoryService.DeleteCategory(id);
                return Ok(new { message = "Xóa danh mục thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/categories/search?name=xxx
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Category>>> SearchCategoryByName(string name)
        {
            try
            {
                var categories = await _categoryService.SearchCategoryByName(name);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/categories/count
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetCategoryCount()
        {
            try
            {
                var count = await _categoryService.GetCategoryCount();
                return Ok(new { totalCategories = count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}