namespace Pro219.Web.DTOs
{
    public class ReportProductSoldDTO
    {
        public string ProductName { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int Unit { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ReportCategorySoldDTO
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public int Unit { get; set; }
        public decimal Value { get; set; }
    }

    public class ReportPeriodDTO
    {
        public string Date { get; set; } = string.Empty;
        public int TotalUnitSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrder { get; set; }
        public int OrderSuccess { get; set; }
        public int OrderCanceled { get; set; }
        public int TotalCustomer { get; set; }
        public int NewCustomerRegistered { get; set; }
        public List<ReportProductSoldDTO> ProductSold { get; set; } = new();
    }

    public class ReportResponseDTO
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SplitData { get; set; } = "month";
        public List<ReportPeriodDTO> DataMonth { get; set; } = new();
        public List<ReportCategorySoldDTO> ParentCategorySold { get; set; } = new();

        public int TotalUnitSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrder { get; set; }
        public int OrderSuccess { get; set; }
        public int OrderCanceled { get; set; }
        public int TotalCustomer { get; set; }
        public int NewCustomerRegistered { get; set; }
        public List<ReportProductSoldDTO> ProductSold { get; set; } = new();
    }
}
