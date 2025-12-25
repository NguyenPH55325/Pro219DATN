namespace Pro219.API.DTOs
{
    public class SearchCombineProductDto
    {
        public int ProductId { get; set; }
        public int ProductVariantId { get; set; }
        public string ProductName { get; set; }
        public int? ColorId { get; set; }
        public string? ColorName { get; set; }
        public int? SizeId { get; set; }
        public string? SizeName { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}

