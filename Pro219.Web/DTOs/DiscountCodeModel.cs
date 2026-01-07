using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Pro219.Web.Constants;

namespace Pro219.Web.DTOs
{
    public class DiscountCodeModel
    {
        public int DiscountId { get; set; }

        [Required(ErrorMessage = Constant.MessageValid.Required)]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = Constant.MessageValid.Required)]
        [MaxLength(20, ErrorMessage = Constant.MessageValid.Max20)]
        public string DiscountType { get; set; } = string.Empty;

        public decimal Value { get; set; } = 10;

        [ConditionalMinValue(ErrorMessage = "Giá trị tối thiểu là 1000")]
        public decimal? MinOrderValue { get; set; }

        [ConditionalMinValue(ErrorMessage = "Giá trị tối thiểu là 1000")]
        public decimal? MaxDiscountAmount { get; set; }

        [Required(ErrorMessage = Constant.MessageValid.Required)]
        [Range(1, int.MaxValue, ErrorMessage = "Số mã phát hành tối thiểu là 1")]
        public int? MaxUsage { get; set; } = 10;

        public int? UsageCount { get; set; } = 0;

        public bool? IsReusable { get; set; } = false;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public byte Type { get; set; }

        public bool IsActive { get; set; } = true;

        public byte Status { get; set; } = 1;
    }

    public class ConditionalMinValueAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success!;

            var decimalValue = value as decimal?;

            if (decimalValue.HasValue && decimalValue.Value < 1000)
            {
                return new ValidationResult(ErrorMessage ?? "Giá trị tối thiểu là 1000");
            }

            return ValidationResult.Success!;
        }
    }
}
