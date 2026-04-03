using TranHuuPhuoc_2123110236.Models;

public class Category
{
    public string CategoryId { get; set; }
    public string CategoryName { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Thêm ? để cho phép null, thêm = null! để tránh warning
    public ICollection<Product>? Products { get; set; } = null!;
}