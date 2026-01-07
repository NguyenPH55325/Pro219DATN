using Pro219.Web.Constants;
using System.ComponentModel.DataAnnotations;

namespace Pro219.Web.DTOs
{
    public class AddressModel
    {
        public int Id { get; set; }

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

        [Required(ErrorMessage = Constant.MessageValid.Required)]
        [MaxLength(500, ErrorMessage = Constant.MessageValid.Max500)]
        public string? OtherInfo { get; set; }

        public bool IsDefault { get; set; }
    }

    public class CalculateFeeRequestModel
    {
        public int ToDistrictId { get; set; }
        public string ToWardCode { get; set; }
        public List<FeeItemModel> Items { get; set; }
    }

    public class FeeItemModel
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public int Length { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
    }
}
