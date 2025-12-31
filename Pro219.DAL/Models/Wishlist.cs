using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pro219.DAL.Models
{
    [Table("Wishlist")]
    public class Wishlist
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }


        public int? ProductVariantId { get; set; }

        public int? ProductId { get; set; }

        public bool? Delete { get; set; }

        public DateTime? CreateAt { get; set; }

        public DateTime? UpdateAt { get; set; }

        public DateTime? DeleteAt { get; set; }

        public byte? Status { get; set; }

        [MaxLength(255)]
        public string? UpdateBy { get; set; }

        // Foreign key navigation properties
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; } = null!;

        [ForeignKey("ProductVariantId")]
        public virtual ProductVariant? ProductVariant { get; set; } = null!;
    }
}

