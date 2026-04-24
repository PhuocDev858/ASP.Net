public class Product
{
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public string CategoryId { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }   // ← thêm ?
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }       // ← thêm ?
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;
    public Category? Category { get; set; }    // ← thêm ?
}