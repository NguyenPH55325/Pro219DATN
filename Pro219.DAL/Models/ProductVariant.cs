using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pro219.DAL.Models
{
    [Table("ProductVariant")]
    public class ProductVariant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        public int? ColorId { get; set; }

        public int? SizeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string SKU { get; set; } = string.Empty; // Không còn sử dụng

        [Required]
        public int StockQuantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int? ArrivalTime { get; set; } // Không còn sử dụng

        public bool IsActive { get; set; }

        public bool? Delete { get; set; }

        public DateTime? CreateAt { get; set; }

        public DateTime? UpdateAt { get; set; }

        public DateTime? DeleteAt { get; set; }

        public byte? Status { get; set; }

        [MaxLength(255)]
        public string? UpdateBy { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; } = null!;

        [ForeignKey("ColorId")]
        public virtual Color? Color { get; set; }

        [ForeignKey("SizeId")]
        public virtual Size? Size { get; set; }

        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();
        public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
        public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
    }
}

