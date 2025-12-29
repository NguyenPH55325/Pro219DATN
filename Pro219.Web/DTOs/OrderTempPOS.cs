namespace Pro219.Web.DTOs
{
    public class OrderTempPOS
    {
        public  int OrderId { get; set; }
        public List<OrderTempPOSItem> ListItem { get; set; }
        public DiscountObj? discountObj { get; set; }
    }

    public class DiscountObj
    {
        public int Id { get; set; }
        public string DiscountCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Value { get; set; }
        public string DiscountType { get; set; }
        public decimal? maxValueUse { get; set; } = null;
    }

    public class OrderTempPOSItem
    {
        public string Image { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int ProductVariantId { get; set; }
        public decimal Subtotal { get; set; } = 0;
    }

    public class CheckoutPOS
    {
        public List<OrderTempPOSItem> ListItemCheckout { get; set; }
        public CheckoutAddressModel? AddressDTO { get; set; }
        public bool isNewAddress { get; set; }
        public int? shippingAddressId { get; set; } = null;
        public decimal DiscountAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public int PaymentMethodTypeId { get; set; }
        public int DiscountId { get; set; }
        public string Note { get; set; }
        public int OrderId { get; set; }
        public string? PhoneNumber { get; set; } = string.Empty;
    }
}
