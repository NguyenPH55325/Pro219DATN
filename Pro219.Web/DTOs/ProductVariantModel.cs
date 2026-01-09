using Pro219.Web.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pro219.Web.DTOs
{
    public class ProductVariantModel
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = Constant.MessageValid.Required)]
        public int ColorId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = Constant.MessageValid.Required)]
        public int SizeId { get; set; }

        [Required(ErrorMessage = Constant.MessageValid.Required)]
        [MaxLength(100, ErrorMessage = Constant.MessageValid.Max100)]
        [RegularExpression(Constant.Regex.SKU, ErrorMessage = "Vui lòng nhập mã là chữ hoa, số, dấu -, dấu _")]
        public string SKU { get; set; } = string.Empty;

        [Range(1, 100000000, ErrorMessage = "Vui lòng nhập số lượng tối thiểu 1 và tối đa 100000000")]
        public int StockQuantity { get; set; } = 10;


        [Range(1000, 9999999999999999, ErrorMessage = "Vui lòng nhập giá trị tối thiểu là 1000đ và tối đa 9999.999.999.999.999đ")]
        public decimal Price { get; set; } = 100000;

        public int? ArrivalTime { get; set; }

        public bool IsActive { get; set; }

        public byte? Status { get; set; }
    }
}
