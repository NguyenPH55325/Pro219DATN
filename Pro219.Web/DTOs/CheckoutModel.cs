using Pro219.Web.Constants;
using System.ComponentModel.DataAnnotations;

namespace Pro219.Web.DTOs
{
    public class CheckoutModel
    {
        public List<CheckoutListItem> ListItemCheckout { get; set; } = new List<CheckoutListItem>();

        public CheckoutAddressModel? AddressDTO { get; set; }
    }

    public class CheckoutPOSModel
    {
        public List<CheckoutListItem>? ListItemCheckout { get; set; } = new List<CheckoutListItem>();
        public CheckoutAddressModel? AddressDTO { get; set; }
        public bool? isNewAddress { get; set; } = false;
        public int? shippingAddressId { get; set; } = -1;
    }

    public class CheckoutListItem
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int ProductVariantId { get; set; }
        public decimal? Subtotal { get; set; } = 0;
    }

    public class CheckoutAddressModel
    {
        public int CustomerId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Street { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string District { get; set; } = string.Empty;

        public string CityName { get; set; } = string.Empty;

        public string DistrictName { get; set; } = string.Empty;

        public string StreetName { get; set; } = string.Empty;

        public string? OtherInfo { get; set; }

        public bool IsDefault { get; set; }= false;
    }

    public class GuestCheckoutModel
    {
        public int CustomerId { get; set; }

        [Required(ErrorMessage = Constant.MessageValid.Required)]
        [MaxLength(200, ErrorMessage = Constant.MessageValid.Max200)]
        public string FullName { get; set; }

        [Required(ErrorMessage = Constant.MessageValid.Required)]
        [MaxLength(11, ErrorMessage = Constant.MessageValid.PhoneNumberLength)]
        [MinLength(10, ErrorMessage = Constant.MessageValid.PhoneNumberLength)]
        [RegularExpression(Constant.Regex.PhoneNumber, ErrorMessage = Constant.MessageValid.PhoneNumber)]
        public string Phone { get; set; }

        [MaxLength(200, ErrorMessage = Constant.MessageValid.Max200)]
        public string Street { get; set; }

        [MaxLength(100, ErrorMessage = Constant.MessageValid.Max100)]
        public string City { get; set; }

        [MaxLength(100, ErrorMessage = Constant.MessageValid.Max100)]
        public string District { get; set; }

        public string CityName { get; set; }

        public string DistrictName { get; set; }

        public string StreetName { get; set; }

        [MaxLength(500, ErrorMessage = Constant.MessageValid.Max500)]
        public string? OtherInfo { get; set; }

        public bool IsDefault { get; set; } = false;

        public string Note { get; set; } = string.Empty;

        public int PaymentMethod { get; set; } = 1;
    }
}
