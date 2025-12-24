using Pro219.DAL.Models;
using System.ComponentModel.DataAnnotations;

namespace Pro219.API.DTOs
{
    public class CartItemUpdateDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int CartId { get; set; }

        [Required]
        public int VariantId { get; set; }

        [Required]
        public int Quantity { get; set; }

        public decimal? UnitPrice { get; set; }

        public bool? IsSelectedForCheckout { get; set; }

        public byte? Status { get; set; }

        public bool? Delete { get; set; }
    }

    public class CartItemWithProductDTO: CartItem
    {
        public string productName { get; set; } = string.Empty;

        public string ColorName { get; set; } = string.Empty;

        public string SizeName { get; set; } = string.Empty;

        public string ImageUrl {  get; set; } = string.Empty;

        public int StockQuantity { get; set; }
    }

    public class AddCartModel
    {
        public int VariantId { get; set; }

        public int Quantity { get; set; }
    }
}


