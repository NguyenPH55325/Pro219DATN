using Pro219.Web.Constants;
using System.ComponentModel.DataAnnotations;

namespace Pro219.Web.DTOs
{
    public class CreateCustomerModel
    {
        [Required(ErrorMessage = Constant.MessageValid.Required)]
        [MaxLength(200, ErrorMessage = Constant.MessageValid.Max200)]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = Constant.MessageValid.Required)]
        [MaxLength(100, ErrorMessage = Constant.MessageValid.Max100)]
        [EmailAddress(ErrorMessage = Constant.MessageValid.Email)]
        public string CustomerEmail { get; set; } = string.Empty;

        [MaxLength(11, ErrorMessage = Constant.MessageValid.PhoneNumberLength)]
        [MinLength(10, ErrorMessage = Constant.MessageValid.PhoneNumberLength)]
        [RegularExpression(Constant.Regex.PhoneNumber, ErrorMessage = Constant.MessageValid.PhoneNumber)]
        public string CustomerPhone { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; } = null;

        [Required(ErrorMessage = Constant.MessageValid.Required)]
        [MaxLength(16, ErrorMessage = Constant.MessageValid.Password)]
        [MinLength(8, ErrorMessage = Constant.MessageValid.Password)]
        [RegularExpression(Constant.Regex.Password, ErrorMessage = Constant.MessageValid.Password)]
        public string Password { get; set; } = string.Empty;

        public string CityId { get; set; } = string.Empty;

        public string DistrictId { get; set; } = string.Empty;

        public string WardId { get; set; } = string.Empty;

        public string WardName { get; set; } = string.Empty;

        public string DistrictName { get; set; } = string.Empty;

        public string CityName { get; set; } = string.Empty;

        [Required(ErrorMessage = Constant.MessageValid.Required)]
        public string CustomerAddressName { get; set; } = string.Empty;

        [Required(ErrorMessage = Constant.MessageValid.Required)]
        public string CustomerAddressPhone { get; set; } = string.Empty;

        public string? OtherAddressInfo { get; set; } = string.Empty;

        public bool IsDefault { get; set; } = true;
    }
}
