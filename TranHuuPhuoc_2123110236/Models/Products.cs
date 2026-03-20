using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace TranHuuPhuoc_2123110236.Models
{
    public class Products
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
    }
}
