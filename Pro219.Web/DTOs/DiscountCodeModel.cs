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

        [Required(ErrorMessage = Constant.MessageValid.Required)]
        [Range(1.0, (double)decimal.MaxValue, ErrorMessage = "Giá trị tối thiểu là 1.")]
        public decimal Value { get; set; } = 10;

        [ConditionalMinValue(ErrorMessage = "Giá trị tối thiểu là 1.")]
        public decimal? MinOrderValue { get; set; }

        public decimal? MaxDiscountAmount { get; set; }

        [Required(ErrorMessage = Constant.MessageValid.Required)]
        [Range(1, int.MaxValue, ErrorMessage = "Số lần sử dụng tối thiểu là 1.")]
        public int? MaxUsage { get; set; } = 10;

        public int? UsageCount { get; set; } = 0;

        public bool? IsReusable { get; set; } = false;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public byte Type { get; set; }

        public bool IsActive { get; set; } = true;

        public byte Status { get; set; }
    }

    public class ConditionalMinValueAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success!;

            var decimalValue = value as decimal?;

            if (decimalValue.HasValue && decimalValue.Value < 1)
            {
                return new ValidationResult(ErrorMessage ?? "Giá trị tối thiểu là 1.");
            }

            return ValidationResult.Success!;
        }
    }

    public class StartDateValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            var startDateNullable = value as DateTime?;

            if (!startDateNullable.HasValue)
                return ValidationResult.Success!;

            var startDate = startDateNullable.Value;
            var now = DateTime.Now;

            if (startDate <= now)
            {
                return new ValidationResult(ErrorMessage ?? "Ngày và giờ bắt đầu phải lớn hơn thời điểm hiện tại.");
            }

            var endDateProperty = validationContext.ObjectType.GetProperty("EndDate");
            if (endDateProperty != null)
            {
                var endDateValue = endDateProperty.GetValue(validationContext.ObjectInstance) as DateTime?;
                if (endDateValue.HasValue && startDate >= endDateValue.Value)
                {
                    return new ValidationResult(ErrorMessage ?? "Ngày và giờ bắt đầu phải nhỏ hơn ngày kết thúc.");
                }
            }

            return ValidationResult.Success!;
        }
    }

    public class EndDateValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            var endDateNullable = value as DateTime?;

            if (!endDateNullable.HasValue)
                return ValidationResult.Success!;

            var endDate = endDateNullable.Value;
            var now = DateTime.Now;

            if (endDate <= now)
            {
                return new ValidationResult(ErrorMessage ?? "Ngày và giờ kết thúc phải lớn hơn thời điểm hiện tại.");
            }

            var startDateProperty = validationContext.ObjectType.GetProperty("StartDate");
            if (startDateProperty != null)
            {
                var startDateValue = startDateProperty.GetValue(validationContext.ObjectInstance) as DateTime?;
                if (startDateValue.HasValue && endDate <= startDateValue.Value)
                {
                    return new ValidationResult(ErrorMessage ?? "Ngày và giờ kết thúc phải lớn hơn ngày bắt đầu.");
                }
            }

            return ValidationResult.Success!;
        }
    }
}
