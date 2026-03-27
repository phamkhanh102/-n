using System.ComponentModel.DataAnnotations;

namespace D.A.sneaker.DTOs
{
    public class AddCartDTO
    {
        [Required]
        public int VariantId { get; set; }

        [Range(1, 100)]
        public int Quantity { get; set; }
    }
}
