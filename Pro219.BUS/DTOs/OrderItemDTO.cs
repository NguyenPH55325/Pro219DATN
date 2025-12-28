namespace Pro219.API.DTOs
{
    public class OrderItemDTO
    {
        public int OrderId { get; set; }

        public int ProductVariantId { get; set; }

        public int Quantity { get; set; }

        public decimal? UnitPrice { get; set; }

        public decimal? Subtotal { get; set; }
    }
}
