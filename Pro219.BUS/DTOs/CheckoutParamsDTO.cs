namespace Pro219.API.DTOs
{
    public class CheckoutParamsDTO
    {
        public List<CheckoutItemDTO>? ListItemCheckout { get; set; }
        public AddressDTO? AddressDTO { get; set; } = null;
        public bool? isNewAddress = false;
        public int? shippingAddressId = -1;
    }
}
