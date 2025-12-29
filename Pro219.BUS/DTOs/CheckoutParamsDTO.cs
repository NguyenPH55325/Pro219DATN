namespace Pro219.API.DTOs
{
    public class CheckoutParamsDTO
    {
        public List<CheckoutItemDTO>? ListItemCheckout { get; set; }
        public AddressDTO? AddressDTO { get; set; } = null;
        public bool? isNewAddress { get; set; } = false;
        public int? shippingAddressId { get; set; } = -1;
    }
}
