namespace Pro219.API.DTOs
{
    public class CheckoutParamsDTO
    {
        public List<CheckoutItemDTO>? ListItemCheckout { get; set; }
        public AddressDTO? AddressDTO { get; set; } = null;
    }
}
