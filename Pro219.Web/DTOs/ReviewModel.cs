using Pro219.Web.Constants;
using System.ComponentModel.DataAnnotations;

namespace Pro219.Web.DTOs
{
    public class ReviewModel
    {
        public int ProductId { get; set; }

        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;

        public int OrderItemId { get; set; }

        [Required(ErrorMessage = Constant.MessageValid.Required)]
        [MaxLength(200, ErrorMessage = Constant.MessageValid.Max200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = Constant.MessageValid.Required)]
        [MinLength(20, ErrorMessage = "Tối thiểu 20 kí tự.")]
        [MaxLength(2000, ErrorMessage = Constant.MessageValid.Max2000)]
        public string? Content { get; set; }

        public int Overall { get; set; }

        public byte? Status { get; set; }
    }
}
