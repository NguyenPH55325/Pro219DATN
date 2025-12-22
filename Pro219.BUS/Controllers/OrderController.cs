using Microsoft.AspNetCore.Mvc;
using Pro219.API.DTOs;
using Pro219.API.Utilities;
using Pro219.DAL.Models;
using Pro219.DAL.Repository;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Text.Json;
using Net.payOS.Types;
using Net.payOS;

namespace Pro219.API.Controllers
{
    [Route("Order")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private static readonly JsonSerializerOptions _camelCaseJsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        OrderRepository orderRepository;
        DiscountCodeRepository discountCodeRepository;

        public OrderController()
        {
            orderRepository = new OrderRepository();
        }

        [HttpPut("UpdateStatus")]
        public async Task<ActionResult<Order>> UpdateOrderStatus([FromBody] OrderUpdateStatusDTO updateDto)
        {
            try
            {
                if (updateDto == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                var order = await orderRepository.GetOrderById(updateDto.OrderId);
                if (order == null)
                {
                    return NotFound(Constant.ErrorCode.DataNotFound);
                }

                var statusHistory = ParseStatusHistory(order.StatusHistory);
                statusHistory.Add(new StatusHistoryEntry
                {
                    Index = statusHistory.Count + 1,
                    Status = updateDto.Status,
                    OrderStatus = updateDto.OrderStatus,
                    PaymentStatus = updateDto.PaymentStatus,
                    DateTime = DateTime.Now.ToString("HH:mm dd/MM/yyyy")
                });
                order.StatusHistory = JsonSerializer.Serialize(statusHistory, _camelCaseJsonOptions);

                order.OrderStatus = updateDto.OrderStatus;
                order.PaymentStatus = updateDto.PaymentStatus;
                order.Status = updateDto.Status;
                order.LastUpdate = DateTime.Now;
                order.UpdateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await orderRepository.UpdateOrder(order);
                if (result == null)
                {
                    return StatusCode(500, Constant.ErrorCode.DatabaseError);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }
        private List<StatusHistoryEntry> ParseStatusHistory(string? statusHistoryJson)
        {
            if (string.IsNullOrWhiteSpace(statusHistoryJson))
            {
                return new List<StatusHistoryEntry>();
            }

            try
            {
                var history = JsonSerializer.Deserialize<List<StatusHistoryEntry>>(statusHistoryJson, _camelCaseJsonOptions);
                return history ?? new List<StatusHistoryEntry>();
            }
            catch
            {
                return new List<StatusHistoryEntry>();
            }
        }

        [HttpGet("CustomerOrderDetail/{orderCode}")]
        public async Task<ActionResult<OrderDetailDTO>> GetOrderDetail(string orderCode)
        {
            try
            {
                var order = await orderRepository.GetOrderDetailByCode(orderCode);
                if (order == null)
                {
                    return NotFound(Constant.ErrorCode.DataNotFound);
                }

                var orderDetailDto = new OrderDetailDTO
                {
                    OrderId = order.OrderId,
                    OrderDate = order.OrderDate,
                    OrderCode = order.OrderCode,
                    FinalAmount = order.FinalAmount,
                    TotalAmount = order.TotalAmount,
                    ShippingFee = order.ShippingFee,
                    PaymentMethodId = order.PaymentMethodId,
                    OrderStatus = order.OrderStatus,
                    PaymentStatus = order.PaymentStatus,
                    DiscountAmount = order.DiscountAmount,
                    Status = order.Status,
                    Note = order.Notes,
                    Address = order.ShippingAddress == null ? null : new OrderDetailAddressDTO
                    {
                        Name = order.ShippingAddress.FullName,
                        Phone = order.ShippingAddress.Phone,
                        Street = order.ShippingAddress.StreetName,
                        City = order.ShippingAddress.CityName,
                        District = order.ShippingAddress.DistrictName,
                        OtherInfo = order.ShippingAddress.OtherInfo ?? ""
                    },                   
                    Items = order.OrderItems.Select(oi => new OrderDetailItemDTO
                    {
                        CustomerId = order.CustomerId,
                        OrderItemId = oi.OrderItemId,
                        ProductVariantId = oi.ProductVariantId,
                        ProductName = oi.ProductVariant?.Product?.Name ?? string.Empty,
                        Color = oi.ProductVariant?.Color?.Name,
                        Size = oi.ProductVariant?.Size?.Name,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        IsReviewed = oi.IsReviewed,
                    }).ToList()
                };
                orderDetailDto.StatusHistory = ParseStatusHistory(order.StatusHistory);

                return Ok(orderDetailDto);
            }
            catch (Exception)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpPost("Checkout")]
        public async Task<ActionResult<CheckoutDTO>> GetCheckoutUrl([FromBody] List<CheckoutItemDTO> listProduct, decimal discountAmount = 0, decimal shippingFee = 0, int? PaymentMethodTypeId = 2, int? discountId = null, string note ="", int addressId=-99)
       {
            if(addressId==-99)
            {
                return BadRequest("Địa chỉ giao hàng không hợp lệ");
            }
            discountCodeRepository = new DiscountCodeRepository();
            PayOS payOS = new PayOS("09b8a42b-6105-4cd4-a4ee-8492e42e909c", "15cfbaf8-79a4-48a0-908f-248c30538001", "00b20c6b94e21bf27e6cb0ae2f26515637c93d70b2eeb832e7b51e299cba433d");
            List<ItemData> items = new List<ItemData>();
            foreach (var product in listProduct)
            {
                ItemData item = new ItemData(product.ProductName, product.Quantity, (int)product.UnitPrice);
                items.Add(item);
            }

            decimal totalPrice = listProduct.Sum(p => p.UnitPrice * p.Quantity);
           
            decimal finalAmount = (totalPrice - discountAmount)+shippingFee;
            int ordCode = new Random().Next(1, int.MaxValue);
            Order order = new Order();
            try
            {
                if(PaymentMethodTypeId == 1)
                {
                    var statusHistory = ParseStatusHistory(order.StatusHistory);
                    statusHistory.Add(new StatusHistoryEntry
                    {
                        Index = statusHistory.Count + 1,
                        Status = Constant.OrderStatus.StatusPending,
                        OrderStatus = Constant.OrderStatus.OrderStatusPending,
                        PaymentStatus = Constant.OrderStatus.PaymentPending,
                        DateTime = DateTime.Now.ToString("HH:mm dd/MM/yyyy")
                    });
                    order.StatusHistory = JsonSerializer.Serialize(statusHistory, _camelCaseJsonOptions);
                } 
                
                else
                {
                    var statusHistory = ParseStatusHistory(order.StatusHistory);
                    statusHistory.Add(new StatusHistoryEntry
                    {
                        Index = statusHistory.Count + 1,
                        Status = Constant.OrderStatus.StatusWaitingForPayment,
                        OrderStatus = Constant.OrderStatus.OrderStatusWaitingForPayment,
                        PaymentStatus = Constant.OrderStatus.PaymentPending,
                        DateTime = DateTime.Now.ToString("HH:mm dd/MM/yyyy")
                    });
                    order.StatusHistory = JsonSerializer.Serialize(statusHistory, _camelCaseJsonOptions);
                }    


                order.OrderCode = "DH" + ordCode.ToString();
                order.TotalAmount = totalPrice;
                order.DiscountAmount = discountAmount;
                order.ShippingAddressId = addressId;
                order.Notes = note;
                order.FinalAmount = finalAmount;
                order.ShippingFee = shippingFee;
                order.OrderDate = DateTime.Now;
                order.PaymentStatus = Constant.OrderStatus.PaymentPending;
                order.OrderStatus = "Đặt hàng"; // status = 1;
                order.DiscountId = discountId;
                order.Notes = User.FindFirst(ClaimTypes.SerialNumber)?.Value == null ? "Khách hàng không đăng nhập" : "";
                order.CreateAt = DateTime.Now;
                order.LastUpdate = DateTime.Now;
                order.UpdateBy = "System";
                order.Status = PaymentMethodTypeId == 2 ? Constant.OrderStatus.StatusWaitingForPayment : Constant.OrderStatus.StatusPending;
                order.CustomerId = User.FindFirst(ClaimTypes.SerialNumber)?.Value == null ? null : int.Parse(User.FindFirst(ClaimTypes.SerialNumber)?.Value);
                order.ShippingAddressId = 1;
                order.DiscountId = discountId == null ? null : (int)discountId;
                order.PaymentMethodId = PaymentMethodTypeId;
                var result = await orderRepository.AddOrder(order);


                if (result == null)
                {
                    return BadRequest();
                }
                else
                {
                    OrderItemRepository orderItemRepository = new OrderItemRepository();
                    foreach (var product in listProduct)
                    {
                        OrderItem orderItem = new OrderItem();
                        orderItem.OrderId = result.OrderId;
                        orderItem.ProductVariantId = product.ProductVariantId;
                        orderItem.Quantity = product.Quantity;
                        orderItem.UnitPrice = product.UnitPrice;
                        orderItem.Subtotal = product.UnitPrice * product.Quantity;
                        orderItem.Delete = false;
                        var resultItem = await orderItemRepository.AddOrderItem(orderItem);
                        if(resultItem == null)
                        {
                            return BadRequest();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }

            if(discountId != null && discountAmount > 0)
            {
                var discountCode = await discountCodeRepository.GetDiscountCodeById((int)discountId);
                if(discountCode != null)
                {
                    discountCode.UsageCount++;
                    var result = await discountCodeRepository.UpdateDiscountCode(discountCode);
                    if(result == null)
                    {
                        return BadRequest(Constant.ErrorCode.DatabaseError);
                    }
                   
                }
            }

            CheckoutDTO checkoutDTO = new CheckoutDTO();
            checkoutDTO.OrderCode = order.OrderCode;
            checkoutDTO.OrderId = order.OrderId;
            checkoutDTO.PaymentType = PaymentMethodTypeId;
            

            if (PaymentMethodTypeId == 2)
            {
                PaymentData paymentData = new PaymentData(ordCode, (int)finalAmount, "Adam Store Thanh toán", items, "http://localhost:5001/order/payment-cancelled?order-id=" + order.OrderId, "http://localhost:5001/order/payment-success?order-id=" + order.OrderId);

                CreatePaymentResult createPayment = await payOS.createPaymentLink(paymentData);

                if (createPayment.status == "PENDING")
                {
                    checkoutDTO.URLPayment = createPayment.checkoutUrl;

                    return Ok(checkoutDTO);
                }
                else
                {
                    return BadRequest();
                }
            }
            else if(PaymentMethodTypeId == 1)            
            {
                checkoutDTO.URLPayment = null ;
                return Ok(checkoutDTO);
            }
           
            return BadRequest("Lỗi");
           

        }

        [HttpGet("PaymentSuccess")]
        public async Task<ActionResult<Order>> PaymentSuccess( [FromQuery] int orderId, [FromQuery] string errorMessage = null)
        {

            try
            {
                var order = await orderRepository.GetOrderById(orderId);
                if(order == null)
                {
                    return NotFound();
                }
                var statusHistory = ParseStatusHistory(order.StatusHistory);
                statusHistory.Add(new StatusHistoryEntry
                {
                    Index = statusHistory.Count + 1,
                    Status = Constant.OrderStatus.StatusPending,
                    OrderStatus = Constant.OrderStatus.OrderStatusPending,
                    PaymentStatus = Constant.OrderStatus.PaymentCompleted,
                    DateTime = DateTime.Now.ToString("HH:mm dd/MM/yyyy")
                });
                order.StatusHistory = JsonSerializer.Serialize(statusHistory, _camelCaseJsonOptions);
                order.PaymentStatus = Constant.OrderStatus.PaymentCompleted;
                order.OrderStatus = Constant.OrderStatus.OrderStatusPending;
                order.Status = Constant.OrderStatus.StatusPending;
                order.LastUpdate = DateTime.Now;
                order.UpdateBy = "System";
                var result = await orderRepository.UpdateOrder(order);
                if(result == null)
                {
                    return BadRequest();
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }

        }

        [HttpGet("PaymentCanceled")]
        public async Task<ActionResult<Order>> PaymentCanceled([FromQuery] int orderId, [FromQuery] string errorMessage = null)
        {
            try
            {
                var order = await orderRepository.GetOrderById(orderId);
                if(order == null)
                {
                    return NotFound();
                }
                var statusHistory = ParseStatusHistory(order.StatusHistory);
                statusHistory.Add(new StatusHistoryEntry
                {
                    Index = statusHistory.Count + 1,
                    Status = Constant.OrderStatus.StatusCanceledByUser,
                    OrderStatus = Constant.OrderStatus.OrderStatusCanceledByUser,
                    PaymentStatus = Constant.OrderStatus.PaymentCancelled,
                    DateTime = DateTime.Now.ToString("HH:mm dd/MM/yyyy")
                });
                order.StatusHistory = JsonSerializer.Serialize(statusHistory, _camelCaseJsonOptions);
                order.PaymentStatus = Constant.OrderStatus.PaymentCancelled;
                order.OrderStatus = Constant.OrderStatus.OrderStatusCanceledByUser;
                order.Status = Constant.OrderStatus.StatusCanceledByUser;
                order.LastUpdate = DateTime.Now;
                order.UpdateBy = "System";
                var result = await orderRepository.UpdateOrder(order);
                if(result == null)
                {
                    return BadRequest();
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }


      
        [HttpGet("GetAll")]
        public async Task<ActionResult<List<Order>>> GetAllOrders()
        {
            try
            {
                var result = await orderRepository.GetAllOrders();
                if (result == null)
                {
                    return Ok(new List<Order>());
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult<Order>> GetOrderById(int id)
        {
            try
            {
                var result = await orderRepository.GetOrderById(id);
                if (result == null)
                {
                    return NotFound(Constant.ErrorCode.DataNotFound);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("GetByOrderCode/{orderCode}")]
        public async Task<ActionResult<Order>> GetOrderByOrderCode(string orderCode)
        {
            try
            {
                var result = await orderRepository.GetOrderByOrderCode(orderCode);
                if (result == null)
                {
                    return NotFound(Constant.ErrorCode.DataNotFound);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("GetByCustomerId/{customerId}")]
        public async Task<ActionResult<List<Order>>> GetOrdersByCustomerId(int customerId)
        {
            try
            {
                var result = await orderRepository.GetOrdersByCustomerId(customerId);
                if (result == null)
                {
                    return Ok(new List<Order>());
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpPost("Add")]
        public async Task<ActionResult<Order>> AddOrder([FromBody] Order order)
        {
            try
            {
                if (order == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                var result = await orderRepository.AddOrder(order);
                if (result == null)
                {
                    return StatusCode(500, Constant.ErrorCode.DatabaseError);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpPost("Update")]
        public async Task<ActionResult<Order>> UpdateOrder([FromBody] Order order)
        {
            try
            {
                if (order == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                order.UpdateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var result = await orderRepository.UpdateOrder(order);
                if (result == null)
                {
                    return NotFound(Constant.ErrorCode.DataNotFound);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult<Order>> DeleteOrder(int id)
        {
            try
            {
                var updateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await orderRepository.DeleteOrder(id, updateBy);
                if (result == null)
                {
                    return NotFound(Constant.ErrorCode.DataNotFound);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("GetInvoice/{id}")]
        public async Task<ActionResult<InvoiceDTO>> GetInvoice(int id)
        {
            try
            {
                var order = await orderRepository.GetOrderByIdForInvoice(id);
                if (order == null)
                {
                    return NotFound(Constant.ErrorCode.DataNotFound);
                }

                var invoice = new InvoiceDTO
                {
                    OrderId = order.OrderId,
                    OrderCode = order.OrderCode,
                    OrderDate = order.OrderDate,
                    OrderStatus = order.OrderStatus,
                    PaymentStatus = order.PaymentStatus,
                    Notes = order.Notes,
                    TotalAmount = order.TotalAmount,
                    DiscountAmount = order.DiscountAmount,
                    FinalAmount = order.FinalAmount
                };

                if (order.Customer != null)
                {
                    invoice.CustomerFullName = order.Customer.FullName;
                    invoice.CustomerPhone = order.Customer.PhoneNumber;
                    invoice.CustomerEmail = order.Customer.Email;
                }

                if (order.ShippingAddressId > 0 && order.ShippingAddress != null)
                {
                    invoice.ShippingAddress = new ShippingAddressDTO
                    {
                        FullName = order.ShippingAddress.FullName,
                        Phone = order.ShippingAddress.Phone,
                        Street = order.ShippingAddress.Street,
                        City = order.ShippingAddress.City,
                        District = order.ShippingAddress.District,
                        OtherInfo = order.ShippingAddress.OtherInfo
                    };
                }

                if (order.DiscountId.HasValue && order.DiscountCode != null)
                {
                    invoice.DiscountInfo = new DiscountInfoDTO
                    {
                        Code = order.DiscountCode.Code,
                        DiscountType = order.DiscountCode.DiscountType,
                        Value = order.DiscountCode.Value,
                        DiscountAmount = order.DiscountAmount
                    };
                }

                if (order.PaymentMethodId.HasValue && order.PaymentMethod != null)
                {
                    invoice.PaymentMethod = new PaymentMethodInfoDTO
                    {
                        Name = order.PaymentMethod.Name,
                        Description = order.PaymentMethod.Description
                    };
                }

                if (order.OrderItems != null && order.OrderItems.Any())
                {
                    foreach (var orderItem in order.OrderItems.Where(oi => oi.Delete != true))
                    {
                        var invoiceItem = new InvoiceItemDTO
                        {
                            OrderItemId = orderItem.OrderItemId,
                            UnitPrice = orderItem.UnitPrice,
                            Quantity = orderItem.Quantity,
                            Subtotal = orderItem.Subtotal
                        };

                        if (orderItem.ProductVariant != null)
                        {
                            if (orderItem.ProductVariant.Product != null)
                            {
                                invoiceItem.ProductName = orderItem.ProductVariant.Product.Name;
                                
                                if (orderItem.ProductVariant.Product.Brand != null)
                                {
                                    invoiceItem.BrandName = orderItem.ProductVariant.Product.Brand.Name;
                                }
                            }

                            if (orderItem.ProductVariant.Size != null)
                            {
                                invoiceItem.SizeName = orderItem.ProductVariant.Size.Name;
                            }

                            if (orderItem.ProductVariant.Color != null)
                            {
                                invoiceItem.ColorName = orderItem.ProductVariant.Color.Name;
                            }
                        }

                        invoice.OrderItems.Add(invoiceItem);
                    }
                }

                return Ok(invoice);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("GetInvoicePdf/{id}")]
        public async Task<IActionResult> GetInvoicePdf(int id)
        {
            try
            {
                var order = await orderRepository.GetOrderByIdForInvoice(id);
                if (order == null)
                {
                    return NotFound(Constant.ErrorCode.DataNotFound);
                }

                var invoice = new InvoiceDTO
                {
                    OrderId = order.OrderId,
                    OrderCode = order.OrderCode,
                    OrderDate = order.OrderDate,
                    OrderStatus = order.OrderStatus,
                    PaymentStatus = order.PaymentStatus,
                    Notes = order.Notes,
                    TotalAmount = order.TotalAmount,
                    DiscountAmount = order.DiscountAmount,
                    FinalAmount = order.FinalAmount
                };

                if (order.Customer != null)
                {
                    invoice.CustomerFullName = order.Customer.FullName;
                    invoice.CustomerPhone = order.Customer.PhoneNumber;
                    invoice.CustomerEmail = order.Customer.Email;
                }

                if (order.ShippingAddressId > 0 && order.ShippingAddress != null)
                {
                    invoice.ShippingAddress = new ShippingAddressDTO
                    {
                        FullName = order.ShippingAddress.FullName,
                        Phone = order.ShippingAddress.Phone,
                        Street = order.ShippingAddress.Street,
                        City = order.ShippingAddress.City,
                        District = order.ShippingAddress.District,
                        OtherInfo = order.ShippingAddress.OtherInfo
                    };
                }

                if (order.DiscountId.HasValue && order.DiscountCode != null)
                {
                    invoice.DiscountInfo = new DiscountInfoDTO
                    {
                        Code = order.DiscountCode.Code,
                        DiscountType = order.DiscountCode.DiscountType,
                        Value = order.DiscountCode.Value,
                        DiscountAmount = order.DiscountAmount
                    };
                }

                if (order.PaymentMethodId.HasValue && order.PaymentMethod != null)
                {
                    invoice.PaymentMethod = new PaymentMethodInfoDTO
                    {
                        Name = order.PaymentMethod.Name,
                        Description = order.PaymentMethod.Description
                    };
                }

                if (order.OrderItems != null && order.OrderItems.Any())
                {
                    foreach (var orderItem in order.OrderItems.Where(oi => oi.Delete != true))
                    {
                        var invoiceItem = new InvoiceItemDTO
                        {
                            OrderItemId = orderItem.OrderItemId,
                            UnitPrice = orderItem.UnitPrice,
                            Quantity = orderItem.Quantity,
                            Subtotal = orderItem.Subtotal
                        };

                        if (orderItem.ProductVariant != null)
                        {
                            if (orderItem.ProductVariant.Product != null)
                            {
                                invoiceItem.ProductName = orderItem.ProductVariant.Product.Name;
                                
                                if (orderItem.ProductVariant.Product.Brand != null)
                                {
                                    invoiceItem.BrandName = orderItem.ProductVariant.Product.Brand.Name;
                                }
                            }

                            if (orderItem.ProductVariant.Size != null)
                            {
                                invoiceItem.SizeName = orderItem.ProductVariant.Size.Name;
                            }

                            if (orderItem.ProductVariant.Color != null)
                            {
                                invoiceItem.ColorName = orderItem.ProductVariant.Color.Name;
                            }
                        }

                        invoice.OrderItems.Add(invoiceItem);
                    }
                }
                var document = new InvoiceDocument(invoice);
                QuestPDF.Settings.License = LicenseType.Community;
                var pdfBytes = document.GeneratePdf();

                return File(pdfBytes, "application/pdf", $"HoaDon_{invoice.OrderCode}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("get-all-by-key-word")]
        public async Task<ActionResult<List<Order>>> GetAllByKeyword([FromQuery] string keyword)
        {
            try
            {
                var result = await orderRepository.GetAllByKeyword(keyword);
                if (result == null)
                {
                    return Ok(new List<Order>());
                }
                return Ok(result);
            } catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }
    }
}



