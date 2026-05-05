using Microsoft.AspNetCore.Mvc;
using TranHuuPhuoc_2123110236.Models;
using TranHuuPhuoc_2123110236.Services;

namespace TranHuuPhuoc_2123110236.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IFileUploadService _fileUploadService;

        public ProductController(IProductService productService, IFileUploadService fileUploadService)
        {
            _productService = productService;
            _fileUploadService = fileUploadService;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
        {
            try
            {
                var products = await _productService.GetAllProducts();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProductById(string id)  // ← int → string
        {
            try
            {
                var product = await _productService.GetProductById(id);
                return Ok(product);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/products
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            try
            {
                var newProduct = await _productService.CreateProduct(product);
                return CreatedAtAction(nameof(GetProductById), new { id = newProduct.ProductId }, newProduct);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/products/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(string id, Product product)  // ← int → string
        {
            try
            {
                var updatedProduct = await _productService.UpdateProduct(id, product);
                return Ok(new { message = "Cập nhật sản phẩm thành công", data = updatedProduct });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)  // ← int → string
        {
            try
            {
                await _productService.DeleteProduct(id);
                return Ok(new { message = "Xóa sản phẩm thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/products/search?name=xxx
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Product>>> SearchProductByName(string name)
        {
            try
            {
                var products = await _productService.SearchProductByName(name);
                return Ok(products);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/products/category?category=xxx
        [HttpGet("category")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsByCategory(string category)
        {
            try
            {
                var products = await _productService.GetProductsByCategory(category);
                return Ok(products);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/products/pricerange?minPrice=xxx&maxPrice=yyy
        [HttpGet("pricerange")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsByPriceRange(decimal minPrice, decimal maxPrice)
        {
            try
            {
                var products = await _productService.GetProductsByPriceRange(minPrice, maxPrice);
                return Ok(products);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/products/count
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetProductCount()
        {
            try
            {
                var count = await _productService.GetProductCount();
                return Ok(new { totalProducts = count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/products/averageprice?category=xxx
        [HttpGet("averageprice")]
        public async Task<ActionResult<decimal>> GetAveragePriceByCategory(string category)
        {
            try
            {
                var averagePrice = await _productService.GetAveragePriceByCategory(category);
                return Ok(new { category = category, averagePrice = averagePrice });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/products/upload-image
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "File không được trống" });
                }

                var imageUrl = await _fileUploadService.UploadImageAsync(file);
                return Ok(new 
                { 
                    message = "Upload ảnh thành công",
                    imageUrl = imageUrl,
                    fileName = Path.GetFileName(imageUrl)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/products/delete-image?imageUrl=xxx
        [HttpDelete("delete-image")]
        public async Task<IActionResult> DeleteImage(string imageUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    return BadRequest(new { message = "URL ảnh không được trống" });
                }

                var result = await _fileUploadService.DeleteImageAsync(imageUrl);
                if (result)
                {
                    return Ok(new { message = "Xóa ảnh thành công" });
                }
                else
                {
                    return NotFound(new { message = "Ảnh không tồn tại" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}