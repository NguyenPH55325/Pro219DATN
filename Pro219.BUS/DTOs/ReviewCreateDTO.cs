using System.ComponentModel.DataAnnotations;

namespace Pro219.API.DTOs
{
    public class ReviewCreateDTO
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int CustomerId { get; set; }
        [Required] 
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        public int OrderItemId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Content { get; set; }

        [Required]
        public int Overall { get; set; }

        public byte? Status { get; set; }
    }
}

