using Pro219.API.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Linq;

namespace Pro219.API.Utilities
{
    public class InvoiceDocument : IDocument
    {
        private readonly InvoiceDTO _invoice;

        public InvoiceDocument(InvoiceDTO invoice)
        {
            _invoice = invoice;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(25);

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("HÓA ĐƠN BÁN HÀNG").FontSize(18).Bold().FontColor(Color.FromHex("#5a4dd4"));
                    column.Item().Text("Adam Store").FontSize(15).SemiBold().FontColor(Colors.Grey.Medium);
                });

                row.ConstantItem(150).Column(column =>
                {
                    column.Item().AlignRight().Text($"Mã đơn: {_invoice.OrderCode}").FontSize(9).SemiBold();
                    column.Item().AlignRight().Text($"Ngày: {_invoice.OrderDate:dd/MM/yyyy HH:mm}").FontSize(8);
                    column.Item().AlignRight().Text($"{_invoice.OrderStatus} | {_invoice.PaymentStatus}").FontSize(8);
                });
            });
        }

        void ComposeContent(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(10);

                column.Item().Element(ComposeCustomerInfo);

                if (_invoice.ShippingAddress != null)
                {
                    column.Item().Element(ComposeShippingAddress);
                }

                column.Item().Element(ComposeOrderItems);

                column.Item().Element(ComposeSummary);
            });
        }

        void ComposeCustomerInfo(IContainer container)
        {
            container.Background(Colors.Grey.Lighten3).Padding(8).Column(column =>
            {
                column.Item().Text("THÔNG TIN KHÁCH HÀNG").FontSize(11).Bold().FontColor(Color.FromHex("#7e6fff"));
                column.Item().PaddingTop(3);

                if (!string.IsNullOrEmpty(_invoice.CustomerFullName))
                {
                    column.Item().Row(row =>
                    {
                        row.ConstantItem(70).Text("Họ tên:").FontSize(9).SemiBold();
                        row.RelativeItem().Text(_invoice.CustomerFullName).FontSize(9);
                    });
                }

                if (!string.IsNullOrEmpty(_invoice.CustomerPhone))
                {
                    column.Item().Row(row =>
                    {
                        row.ConstantItem(70).Text("SĐT:").FontSize(9).SemiBold();
                        row.RelativeItem().Text(_invoice.CustomerPhone).FontSize(9);
                    });
                }

                if (!string.IsNullOrEmpty(_invoice.CustomerEmail))
                {
                    column.Item().Row(row =>
                    {
                        row.ConstantItem(70).Text("Email:").FontSize(9).SemiBold();
                        row.RelativeItem().Text(_invoice.CustomerEmail).FontSize(9);
                    });
                }
            });
        }

        void ComposeShippingAddress(IContainer container)
        {
            container.Background(Colors.Grey.Lighten4).Padding(8).Column(column =>
            {
                column.Item().Text("ĐỊA CHỈ GIAO HÀNG").FontSize(11).Bold().FontColor(Color.FromHex("#7e6fff"));
                column.Item().PaddingTop(3);

                if (!string.IsNullOrEmpty(_invoice.ShippingAddress.FullName))
                {
                    column.Item().Row(row =>
                    {
                        row.ConstantItem(70).Text("Người nhận:").FontSize(9).SemiBold();
                        row.RelativeItem().Text(_invoice.ShippingAddress.FullName).FontSize(9);
                    });
                }

                if (!string.IsNullOrEmpty(_invoice.ShippingAddress.Phone))
                {
                    column.Item().Row(row =>
                    {
                        row.ConstantItem(70).Text("Điện thoại:").FontSize(9).SemiBold();
                        row.RelativeItem().Text(_invoice.ShippingAddress.Phone).FontSize(9);
                    });
                }

                var addressParts = new List<string>();
                if (!string.IsNullOrEmpty(_invoice.ShippingAddress.Street))
                    addressParts.Add(_invoice.ShippingAddress.Street);
                if (!string.IsNullOrEmpty(_invoice.ShippingAddress.District))
                    addressParts.Add(_invoice.ShippingAddress.District);
                if (!string.IsNullOrEmpty(_invoice.ShippingAddress.City))
                    addressParts.Add(_invoice.ShippingAddress.City);
                if (!string.IsNullOrEmpty(_invoice.ShippingAddress.OtherInfo))
                    addressParts.Add(_invoice.ShippingAddress.OtherInfo);

                if (addressParts.Any())
                {
                    column.Item().Row(row =>
                    {
                        row.ConstantItem(70).Text("Địa chỉ:").FontSize(9).SemiBold();
                        row.RelativeItem().Text(string.Join(", ", addressParts)).FontSize(9);
                    });
                }
            });
        }

        void ComposeOrderItems(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text("CHI TIẾT ĐƠN HÀNG").FontSize(11).Bold().FontColor(Color.FromHex("#7e6fff"));
                column.Item().PaddingTop(3);

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.ConstantColumn(35);
                        columns.ConstantColumn(60);
                        columns.ConstantColumn(70);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Sản phẩm").FontSize(9).Bold().FontColor(Colors.White);
                        header.Cell().Element(CellStyle).AlignCenter().Text("SL").FontSize(9).Bold().FontColor(Colors.White);
                        header.Cell().Element(CellStyle).AlignRight().Text("Đơn giá").FontSize(9).Bold().FontColor(Colors.White);
                        header.Cell().Element(CellStyle).AlignRight().Text("Thành tiền").FontSize(9).Bold().FontColor(Colors.White);

                        static IContainer CellStyle(IContainer container)
                        {
                            return container
                                .Background(Color.FromHex("#7e6fff"))
                                .Padding(5)
                                .BorderColor(Color.FromHex("#5a4dd4"));
                        }
                    });

                    foreach (var item in _invoice.OrderItems)
                    {
                        table.Cell().Element(CellStyle).PaddingVertical(4).Column(column =>
                        {
                            var productText = item.ProductName;
                            if (!string.IsNullOrEmpty(item.BrandName))
                            {
                                productText = $"{item.BrandName} - {productText}";
                            }
                            column.Item().Text(productText).FontSize(8).SemiBold();

                            var variantInfo = new List<string>();
                            if (!string.IsNullOrEmpty(item.SizeName))
                                variantInfo.Add($"Size: {item.SizeName}");
                            if (!string.IsNullOrEmpty(item.ColorName))
                                variantInfo.Add($"Màu: {item.ColorName}");

                            if (variantInfo.Any())
                            {
                                column.Item().Text(string.Join(" | ", variantInfo)).FontSize(7).FontColor(Colors.Grey.Darken1);
                            }
                        });

                        table.Cell().Element(CellStyle).AlignCenter().PaddingVertical(4).Text(item.Quantity.ToString()).FontSize(8);
                        table.Cell().Element(CellStyle).AlignRight().PaddingVertical(4).Text(FormatCurrency(item.UnitPrice)).FontSize(8);
                        table.Cell().Element(CellStyle).AlignRight().PaddingVertical(4).Text(FormatCurrency(item.Subtotal)).FontSize(8).SemiBold();

                        static IContainer CellStyle(IContainer container)
                        {
                            return container
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .PaddingHorizontal(3);
                        }
                    }
                });
            });
        }

        void ComposeSummary(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().AlignRight().Column(summaryColumn =>
                {
                    summaryColumn.Item().Width(220).Background(Colors.Grey.Lighten3).Padding(10).Column(innerColumn =>
                    {
                        innerColumn.Item().Text("TỔNG KẾT").FontSize(11).Bold().FontColor(Color.FromHex("#7e6fff"));
                        innerColumn.Item().PaddingTop(3);

                        innerColumn.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Tổng tiền hàng:").FontSize(9).SemiBold();
                            row.ConstantItem(90).AlignRight().Text(FormatCurrency(_invoice.TotalAmount)).FontSize(9);
                        });

                        if (_invoice.DiscountAmount > 0)
                        {
                            innerColumn.Item().PaddingTop(2).Row(row =>
                            {
                                row.RelativeItem().Text($"Giảm ({_invoice.ShippingFee}):").FontSize(9).SemiBold().FontColor(Colors.Red.Darken1);
                                row.ConstantItem(90).AlignRight().Text($"-{FormatCurrency(_invoice.DiscountAmount)}").FontSize(9).FontColor(Colors.Red.Darken1);
                            });
                        }

                        if (_invoice.ShippingFee > 0)
                        {
                            innerColumn.Item().PaddingTop(2).Row(row =>
                            {
                                row.RelativeItem().Text($"Phí giao hàng ({_invoice.ShippingFee}):").FontSize(9).SemiBold().FontColor(Colors.Red.Darken1);
                                row.ConstantItem(90).AlignRight().Text($"+{FormatCurrency(_invoice.ShippingFee)}").FontSize(9).FontColor(Colors.Red.Darken1);
                            });
                        }

                        innerColumn.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Text("TỔNG CỘNG:").FontSize(10).Bold().FontColor(Color.FromHex("#5a4dd4"));
                            row.ConstantItem(90).AlignRight().Text(FormatCurrency(_invoice.FinalAmount)).FontSize(10).Bold().FontColor(Color.FromHex("#5a4dd4"));
                        });

                        if (_invoice.PaymentMethod != null)
                        {
                            innerColumn.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text("Thanh toán:").FontSize(8).SemiBold();
                                row.RelativeItem().AlignRight().Text(_invoice.PaymentMethod.Name).FontSize(8);
                            });
                        }

                        if (!string.IsNullOrEmpty(_invoice.Notes))
                        {
                            innerColumn.Item().PaddingTop(5).Text("Ghi chú:").FontSize(8).SemiBold();
                            innerColumn.Item().Text(_invoice.Notes).FontSize(8).FontColor(Colors.Grey.Darken1);
                        }
                    });
                });
            });
        }

        void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Column(column =>
            {
                column.Item().Text("Cảm ơn quý khách đã mua sắm!").FontSize(9).SemiBold().FontColor(Color.FromHex("#7e6fff"));
                column.Item().PaddingTop(3).Text($"Hóa đơn được xử lý bởi LVT Team và xuất bằng QuestPDF - Mã đơn: {_invoice.OrderCode}").FontSize(7).FontColor(Colors.Grey.Darken1);
            });
        }

        private string FormatCurrency(decimal amount)
        {
            return $"{amount:N0} đ";
        }
    }
}
