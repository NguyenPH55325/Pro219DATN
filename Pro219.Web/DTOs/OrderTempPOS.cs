namespace Pro219.Web.DTOs
{
    public class OrderTempPOS
    {
        public  int OrderId { get; set; }
        public List<CheckoutListItem> ListItem { get; set; }
        public DiscountObj? discountObj { get; set; }
    }

    public class DiscountObj
    {
        public int Id { get; set; }
        public string DiscountCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Value { get; set; }
        public string DiscountType { get; set; }
        public decimal? maxValueUse { get; set; } = null;
    }
}
