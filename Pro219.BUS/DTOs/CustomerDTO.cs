namespace Pro219.API.DTOs
{
    public class CreateCustomerDTO
    {
        public string CustomerName { get; set; } = string.Empty;

        public string CustomerEmail { get; set; } = string.Empty;

        public string CustomerPhone { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; } = null;

        public string Password { get; set; } = string.Empty;

        public string CityId { get; set; } = string.Empty;

        public string DistrictId { get; set; } = string.Empty;

        public string WardId { get; set; } = string.Empty;

        public string WardName { get; set; } = string.Empty;

        public string DistrictName { get; set; } = string.Empty;

        public string CityName { get; set; } = string.Empty;
        
        public string CustomerAddressName { get; set; } = string.Empty;

        public string CustomerAddressPhone {  get; set; } = string.Empty;

        public string? OtherAddressInfo {  get; set; } = string.Empty;

        public bool IsDefault { get; set; } = true;
    }
}
