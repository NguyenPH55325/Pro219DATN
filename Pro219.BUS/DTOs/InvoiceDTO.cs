using System;
using System.Collections.Generic;

namespace Pro219.API.DTOs
{
    public class InvoiceDTO
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string? Notes { get; set; }

        public string CustomerFullName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }

        public ShippingAddressDTO? ShippingAddress { get; set; }

        public DiscountInfoDTO? DiscountInfo { get; set; }

        public PaymentMethodInfoDTO? PaymentMethod { get; set; }

        public List<InvoiceItemDTO> OrderItems { get; set; } = new List<InvoiceItemDTO>();

        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public decimal ShippingFee { get; set; }
    }

    public class ShippingAddressDTO
    {
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string? OtherInfo { get; set; }
    }

    public class DiscountInfoDTO
    {
        public string Code { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal DiscountAmount { get; set; }
    }

    public class PaymentMethodInfoDTO
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class InvoiceItemDTO
    {
        public int OrderItemId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public string? ColorName { get; set; }
        public string? SizeName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
    }
}

