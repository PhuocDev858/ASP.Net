using Microsoft.AspNetCore.Mvc;
using TranHuuPhuoc_2123110236.Models;
using TranHuuPhuoc_2123110236.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TranHuuPhuoc_2123110236.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
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
        public async Task<ActionResult<Product>> GetProductById(int id)
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
        public async Task<IActionResult> UpdateProduct(int id, Product product)
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
        public async Task<IActionResult> DeleteProduct(int id)
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
    }
}
