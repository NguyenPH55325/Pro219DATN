using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using Net.payOS.Types;
using Pro219.API.DTOs;
using Pro219.API.Services;
using Pro219.API.Utilities;
using Pro219.DAL.Models;
using Pro219.DAL.Repository;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

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

        private readonly OrderManagerService _orderManagerService;
        private readonly IBackgroundJobClient _backgroundJobClient;

        OrderRepository orderRepository;
        DiscountCodeRepository discountCodeRepository;
        ProductVariantRepository productVariantRepository;
        OrderItemRepository orderItemRepository;
        ProductRepository productRepository;
        ColorRepository colorRepository;
        SizeRepository sizeRepository;
        BrandRepository brandRepository;

        public OrderController(OrderManagerService orderManagerService, IBackgroundJobClient backgroundJobClient)
        {
            _orderManagerService = orderManagerService;
            _backgroundJobClient = backgroundJobClient;
            orderRepository = new OrderRepository();
            productVariantRepository = new ProductVariantRepository();
            orderItemRepository = new OrderItemRepository();
            productRepository = new ProductRepository();
            colorRepository = new ColorRepository();
            sizeRepository = new SizeRepository();
            brandRepository = new BrandRepository();
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
                if(updateDto.Status == Constant.OrderStatus.StatusCanceledByUser || updateDto.Status == Constant.OrderStatus.StatusShippingFailed)
                {
                    var listOrderItem = await orderItemRepository.GetOrderItemsByOrderId(order.OrderId);
                    foreach (var item in listOrderItem)
                    {
                        await productVariantRepository.IncreaseProductVariantQuantity(item.ProductVariantId, item.Quantity);
                    }
                    if(order.DiscountId.HasValue && order.DiscountAmount>0)
                    {
                        var discountCode = await discountCodeRepository.GetDiscountCodeById(order.DiscountId.Value);
                        if (discountCode != null)
                        {
                            discountCode.UsageCount++;
                            await discountCodeRepository.UpdateDiscountCode(discountCode);
                        }
                    }    
                }
                if(updateDto.Status == Constant.OrderStatus.StatusConfirm)
                {
                    var listOrderItem = await orderItemRepository.GetOrderItemsByOrderId(order.OrderId);
                    foreach (var item in listOrderItem)
                    {
                        await productVariantRepository.DecreaseProductVariantQuantity(item.ProductVariantId, item.Quantity);
                    }
                }    
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpPut("ChangePaymentMethodToCash/{orderId}")]
        public async Task<ActionResult<Order>> ChangePaymentMethodToCash(int orderId)
        {
            try
            {
                var order = await orderRepository.GetOrderById(orderId);
                if (order == null)
                {
                    return NotFound(Constant.ErrorCode.DataNotFound);
                }
                if (order.PaymentMethodId == 1 || order.Status!=Constant.OrderStatus.StatusWaitingForPayment || order.IsOrderPOS==false)
                {
                    return BadRequest("Không đủ điều kiện chuyển đổi");
                }
                order.PaymentMethodId = 1;
                order.LastUpdate = DateTime.Now;
                order.UpdateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                order.Status = Constant.OrderStatus.StatusDone;
                order.PaymentStatus = Constant.OrderStatus.PaymentCompleted;
                order.OrderStatus = Constant.OrderStatus.OrderStatusDone;
                var statusHistory = ParseStatusHistory(order.StatusHistory);
                statusHistory.Add(new StatusHistoryEntry
                {
                    Index = statusHistory.Count + 1,
                    Status = order.Status ?? Constant.OrderStatus.StatusDone,
                    OrderStatus = Constant.OrderStatus.OrderStatusDone,
                    PaymentStatus = Constant.OrderStatus.PaymentCompleted,
                    DateTime = DateTime.Now.ToString("HH:mm dd/MM/yyyy")
                });
                order.StatusHistory = JsonSerializer.Serialize(statusHistory, _camelCaseJsonOptions);
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

        [HttpGet("GetDetailById/{id}")]
        public async Task<ActionResult<OrderDetailDTO>> GetOrderDetailById(int id)
        {
            try
            {
                var order = await orderRepository.GetOrderDetailById(id);
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
                    IsPOS = order.IsOrderPOS ?? false,
                    PaymentLink = order.PaymentLink ?? string.Empty,
                    CustomerId = order.CustomerId,
                    Customer = order.Customer == null ? null : new OrderDetailCustomerDTO
                    {
                        FullName = order.Customer.FullName,
                        Email = order.Customer.Email,
                        PhoneNumber = order.Customer.PhoneNumber,
                    },
                    Address = order.ShippingAddress == null ? null : new OrderDetailAddressDTO
                    {
                        Name = order.CustomerName ?? "",
                        Phone = order.Phone ?? "",
                        Street = order.WardName ?? "",
                        City = order.ProvinceName ?? "",
                        District = order.DistrictName ?? "",
                        OtherInfo = order.Address ?? ""
                    },
                    Items = order.OrderItems.Select(oi => new OrderDetailItemDTO
                    {
                        OrderItemId = oi.OrderItemId,
                        ProductVariantId = oi.ProductVariantId,
                        ProductId = oi.ProductVariant?.Product?.Id,
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
                    CustomerId = order.CustomerId,
                    Customer = order.Customer == null ? null : new OrderDetailCustomerDTO
                    {
                        FullName = order.Customer.FullName,
                        Email = order.Customer.Email,
                        PhoneNumber = order.Customer.PhoneNumber,
                    },
                    Address = order.ShippingAddress == null ? null : new OrderDetailAddressDTO
                    {
                        Name = order.ShippingAddress.FullName,
                        Phone = order.ShippingAddress.Phone,
                        Street = order.ShippingAddress.WardName,
                        City = order.ShippingAddress.ProvinceName,
                        District = order.ShippingAddress.DistrictName,
                        OtherInfo = order.ShippingAddress.OtherInfo ?? ""
                    },
                    Items = order.OrderItems.Select(oi => new OrderDetailItemDTO
                    {
                        OrderItemId = oi.OrderItemId,
                        ProductVariantId = oi.ProductVariantId,
                        ProductId = oi.ProductVariant?.Product?.Id,
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

        [HttpPost("checkout-pos")]
        public async Task<ActionResult<CheckoutDTO>> GetCheckoutForPOS([FromBody] CheckoutParamsDTO checkoutParam, string? phoneNumber = null, decimal discountAmount = 0, decimal shippingFee = 0, int? PaymentMethodTypeId = 2, int? discountId = null, string note = "", int? orderId = null)
        {

            if (orderId == null)
                return BadRequest("Hoá đơn không hợp lệ");
            if (checkoutParam == null || checkoutParam.ListItemCheckout == null || !checkoutParam.ListItemCheckout.Any())
            {
                return BadRequest("Dữ liệu checkout không hợp lệ");
            }
            foreach (var item in checkoutParam.ListItemCheckout)
            {
                if (item.ProductVariantId <= 0 || item.Quantity <= 0)
                {
                    return BadRequest("Sản phẩm bạn đã chọn đã hết");
                }

                var variant = await productVariantRepository.GetProductVariantById(item.ProductVariantId);
                if (variant == null || variant.Delete == true || variant.IsActive == false || (variant.Status.HasValue && variant.Status == 0))
                {
                    return BadRequest("Sản phẩm không còn hoạt động hoặc đã bị xóa");
                }

                if (variant.StockQuantity < item.Quantity)
                {
                    return BadRequest("Số lượng kho không đủ");
                }

                var product = await productRepository.GetProductById(variant.ProductId);
                if (product == null || product.Delete == true || (product.Status.HasValue && product.Status == 0))
                {
                    return BadRequest("Sản phẩm không còn hoạt động hoặc đã bị xóa");
                }

                var brand = await brandRepository.GetBrandById(product.BrandId);
                if (brand == null || brand.Delete == true || (brand.Status.HasValue && brand.Status == 0))
                {
                    return BadRequest("Thương hiệu đã ngừng kinh doanh");
                }

                if (variant.ColorId.HasValue)
                {
                    var color = await colorRepository.GetColorById(variant.ColorId.Value);
                    if (color == null || color.Delete == true || (color.Status.HasValue && color.Status == 0))
                    {
                        return BadRequest("Màu sắc không còn hoạt động");
                    }
                }

                if (variant.SizeId.HasValue)
                {
                    var size = await sizeRepository.GetSizeById(variant.SizeId.Value);
                    if (size == null || size.Delete == true || (size.Status.HasValue && size.Status == 0))
                    {
                        return BadRequest("Kích cỡ không còn kinh doanh");
                    }
                }
            }

            Order order;

            if (orderId != null && orderId.HasValue)
            {
                order = await orderRepository.GetOrderById(orderId.Value);
            }
            else
            {
                order = new Order();
            }

            Address newAddressFromUser = null;
            if (checkoutParam.isNewAddress == true & checkoutParam.AddressDTO != null)
            {
                AddressRepository addressRepo = new AddressRepository();
                var address = new Address
                {
                    CustomerId = checkoutParam.AddressDTO.CustomerId,
                    FullName = checkoutParam.AddressDTO.FullName,
                    Phone = checkoutParam.AddressDTO.Phone,
                    Ward = checkoutParam.AddressDTO.Street,
                    Province = checkoutParam.AddressDTO.City,
                    District = checkoutParam.AddressDTO.District,
                    ProvinceName = checkoutParam.AddressDTO.CityName,
                    DistrictName = checkoutParam.AddressDTO.DistrictName,
                    WardName = checkoutParam.AddressDTO.StreetName,
                    OtherInfo = checkoutParam.AddressDTO.OtherInfo,
                    IsDefault = checkoutParam.AddressDTO.IsDefault,
                    CreateAt = DateTime.Now,
                    Delete = false,
                    Status = 1
                };
                order.ProvinceName= address.ProvinceName;
                order.DistrictName= address.DistrictName;
                order.WardName= address.WardName;
                order.Address = address.OtherInfo;
                order.CustomerName = address.FullName;
                order.Phone = address.Phone;
                var re = await addressRepo.AddAddress(address);
                if (re == null)
                {
                    return BadRequest("Tạo địa chỉ mới thất bại");
                }
                else
                {
                    newAddressFromUser = re;
                }
            }

            CustomerRepository customerRepository = new CustomerRepository();
            Customer currentCustomer = customerRepository.FindCustomerByEmailAndPhone("", phoneNumber).Result;
            if (currentCustomer == null)
            {
                currentCustomer = new Customer();
                currentCustomer.Id = -1;
            }
            // PaymentMethodTypeId: 1 - Thanh toán tại quầy, 2 - Thanh toán Chuyển khoản tại quầy
            discountCodeRepository = new DiscountCodeRepository();
            PayOS payOS = new PayOS("8208b056-d435-43f6-94e8-9415d68154ae", "5e3bf8ac-b167-4681-8caf-80cdb522ced2", "08d586f8f9157aaca716641dc39b6b63d134b573f68c91b98c31f536140ff39a");
            List<ItemData> items = new List<ItemData>();
            foreach (var product in checkoutParam.ListItemCheckout)
            {
                ItemData item = new ItemData(product.ProductName, product.Quantity, (int)product.UnitPrice);
                items.Add(item);
            }

            decimal totalPrice = checkoutParam.ListItemCheckout.Sum(p => p.UnitPrice * p.Quantity);
            decimal finalAmount = 0;
            finalAmount = (totalPrice - discountAmount) + shippingFee;
            int ordCode = new Random().Next(1, int.MaxValue);
            try
            {
                var isShip = checkoutParam.isNewAddress == true || (checkoutParam.isNewAddress == false && checkoutParam.shippingAddressId != -1);
                if (PaymentMethodTypeId == 1)
                {
                    var statusHistory = ParseStatusHistory(order.StatusHistory);
                    statusHistory.Add(new StatusHistoryEntry
                    {
                        Index = statusHistory.Count + 1,
                        Status = isShip ? Constant.OrderStatus.StatusPending : Constant.OrderStatus.StatusDone,
                        OrderStatus = isShip ? Constant.OrderStatus.OrderStatusPending : Constant.OrderStatus.OrderStatusDone,
                        PaymentStatus = Constant.OrderStatus.PaymentCompleted,
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
                order.ShippingAddressId = null;
                order.Notes = note;
                order.FinalAmount = finalAmount;
                order.ShippingFee = 0;
                order.OrderDate = DateTime.Now;
                order.PaymentStatus = PaymentMethodTypeId == 2 ? Constant.OrderStatus.PaymentPending : isShip ? Constant.OrderStatus.PaymentPending : Constant.OrderStatus.PaymentCompleted;
                order.OrderStatus = PaymentMethodTypeId == 2 ? Constant.OrderStatus.OrderStatusWaitingForPayment : isShip ? Constant.OrderStatus.OrderStatusPending : Constant.OrderStatus.OrderStatusDone;
                order.DiscountId = discountId;
                order.Notes = note;
                order.CreateAt = DateTime.Now;
                order.LastUpdate = DateTime.Now;
                order.IsOrderPOS = true;
                order.UpdateBy = "System";
                order.Status = PaymentMethodTypeId == 2 ? Constant.OrderStatus.StatusWaitingForPayment : isShip ? Constant.OrderStatus.StatusPending : Constant.OrderStatus.StatusDone;
                order.CustomerId = currentCustomer.Id == -1 ? currentCustomer.Id : currentCustomer.Id;
                order.ShippingAddressId = null;
                order.DiscountId = discountId == null ? null : (int)discountId;
                order.PaymentMethodId = PaymentMethodTypeId;
                order.CustomerType = currentCustomer.FullName == null ? Constant.CustomerType.GuestOrder : Constant.CustomerType.RegisteredOrder;              
                if (newAddressFromUser != null && checkoutParam.isNewAddress == true)
                {
                    order.ShippingAddressId = newAddressFromUser.Id;
                    order.ShippingFee = shippingFee;
                    order.ProvinceName = newAddressFromUser.ProvinceName;
                    order.DistrictName = newAddressFromUser.DistrictName;
                    order.WardName = newAddressFromUser.WardName;
                    order.Address = newAddressFromUser.OtherInfo;
                    order.CustomerName = newAddressFromUser.FullName;
                    order.Phone = newAddressFromUser.Phone;
                }
                else if (checkoutParam.isNewAddress == false && checkoutParam.shippingAddressId != -1)
                {
                    AddressRepository addressRepo = new AddressRepository();
                    var addressTemp = addressRepo.GetById(int.Parse(checkoutParam.shippingAddressId.ToString())).Result;
                    order.ShippingAddressId = checkoutParam.shippingAddressId;
                    order.ShippingFee = shippingFee;
                    order.ProvinceName = addressTemp.ProvinceName;
                    order.DistrictName = addressTemp.DistrictName;
                    order.WardName = addressTemp.WardName;
                    order.Address = addressTemp.OtherInfo;
                    order.CustomerName = addressTemp.FullName;
                    order.Phone = addressTemp.Phone;
                }

                if (orderId == null)
                {
                    var result = await orderRepository.AddOrder(order);


                    if (result == null)
                    {
                        return BadRequest();
                    }

                }
                else
                {
                    var result = await orderRepository.UpdateOrder(order);
                    if (result == null)
                    {
                        return BadRequest();
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }

            if (discountId != null && discountAmount > 0)
            {
                var discountCode = await discountCodeRepository.GetDiscountCodeById((int)discountId);
                if (discountCode != null)
                {
                    discountCode.UsageCount++;
                    var result = await discountCodeRepository.UpdateDiscountCode(discountCode);
                    if (result == null)
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
                DateTimeOffset utcNow = DateTimeOffset.UtcNow;
                DateTimeOffset expirationTime = utcNow.AddMinutes(15);
                long expiredAt = expirationTime.ToUnixTimeSeconds();
                PaymentData paymentData = new PaymentData(ordCode, (int)finalAmount, "Adam Store Thanh toán", items, "http://localhost:5001/admin/order/payment-cancelled?order-id=" + order.OrderId, "http://localhost:5001/admin/order/payment-success?order-id=" + order.OrderId + "&pos=true", null, null, null, null, null, expiredAt);
                CreatePaymentResult createPayment = await payOS.createPaymentLink(paymentData);

                if (createPayment.status == "PENDING")
                {
                    checkoutDTO.URLPayment = createPayment.checkoutUrl;
                    OrderItemRepository orderItemRepository = new OrderItemRepository();
                    foreach (var product in checkoutParam.ListItemCheckout)
                    {
                        OrderItem orderItem = new OrderItem();
                        orderItem.OrderId = orderId.Value;
                        orderItem.ProductVariantId = product.ProductVariantId;
                        orderItem.Quantity = product.Quantity;
                        orderItem.UnitPrice = product.UnitPrice;
                        orderItem.Subtotal = product.UnitPrice * product.Quantity;
                        orderItem.Delete = false;
                        var re = await productVariantRepository.DecreaseProductVariantQuantity(orderItem.ProductVariantId, orderItem.Quantity);
                        if (re == false)
                            return BadRequest("Số lượng kho không đủ");

                        var resultItem = await orderItemRepository.AddOrderItem(orderItem);
                        if (resultItem == null)
                        {
                            return BadRequest();
                        }

                    }
                    order.PaymentExpiration = DateTime.Now.AddMinutes(15);
                    order.PaymentLink = createPayment.checkoutUrl;
                    await orderRepository.UpdateOrder(order);
                    _backgroundJobClient.Schedule<OrderManagerService>(
            x => x.CancelExpiredOrderAsync(order.OrderId),
            TimeSpan.FromMinutes(15)
        );
                    return Ok(checkoutDTO);
                }
                else
                {
                    return BadRequest();
                }
            }
            else if (PaymentMethodTypeId == 1)
            {
                checkoutDTO.URLPayment = null;
                foreach (var product in checkoutParam.ListItemCheckout)
                {
                    OrderItem orderItem = new OrderItem();
                    orderItem.OrderId = orderId.Value;
                    orderItem.ProductVariantId = product.ProductVariantId;
                    orderItem.Quantity = product.Quantity;
                    orderItem.UnitPrice = product.UnitPrice;
                    orderItem.Subtotal = product.UnitPrice * product.Quantity;
                    orderItem.Delete = false;
                    var re = await productVariantRepository.DecreaseProductVariantQuantity(orderItem.ProductVariantId, orderItem.Quantity);
                    if (re == false)
                        return BadRequest("Số lượng kho không đủ");

                    var resultItem = await orderItemRepository.AddOrderItem(orderItem);
                    if (resultItem == null)
                    {
                        return BadRequest();
                    }

                }
                return Ok(checkoutDTO);
            }

            return BadRequest("Lỗi");

        }

        [HttpPost("Checkout")]
        public async Task<ActionResult<CheckoutDTO>> GetCheckoutUrl([FromBody] CheckoutParamsDTO checkoutParam, decimal discountAmount = 0, decimal shippingFee = 0, int? PaymentMethodTypeId = 2, int? discountId = null, string note = "", int addressId = -99)
        {

            discountCodeRepository = new DiscountCodeRepository();
            // Basic request validation
            if (checkoutParam == null || checkoutParam.ListItemCheckout == null || !checkoutParam.ListItemCheckout.Any())
            {
                return BadRequest("Dữ liệu checkout không hợp lệ");
            }
            foreach (var item in checkoutParam.ListItemCheckout)
            {
                if (item.ProductVariantId <= 0 || item.Quantity <= 0)
                {
                    return BadRequest("Sản phẩm bạn đã chọn đã hết");
                }

                var variant = await productVariantRepository.GetProductVariantById(item.ProductVariantId);
                if (variant == null || variant.Delete == true || variant.IsActive == false || (variant.Status.HasValue && variant.Status == 0))
                {
                    return BadRequest("Sản phẩm không còn hoạt động hoặc đã bị xóa");
                }

                if (variant.StockQuantity < item.Quantity)
                {
                    return BadRequest("Số lượng kho không đủ");
                }

                var product = await productRepository.GetProductById(variant.ProductId);
                if (product == null || product.Delete == true || (product.Status.HasValue && product.Status == 0))
                {
                    return BadRequest("Sản phẩm không còn hoạt động hoặc đã bị xóa");
                }

                var brand = await brandRepository.GetBrandById(product.BrandId);
                if (brand == null || brand.Delete == true || (brand.Status.HasValue && brand.Status == 0))
                {
                    return BadRequest("Thương hiệu đã ngừng kinh doanh");
                }

                if (variant.ColorId.HasValue)
                {
                    var color = await colorRepository.GetColorById(variant.ColorId.Value);
                    if (color == null || color.Delete == true || (color.Status.HasValue && color.Status == 0))
                    {
                        return BadRequest("Màu sắc không còn hoạt động");
                    }
                }

                if (variant.SizeId.HasValue)
                {
                    var size = await sizeRepository.GetSizeById(variant.SizeId.Value);
                    if (size == null || size.Delete == true || (size.Status.HasValue && size.Status == 0))
                    {
                        return BadRequest("Kích cỡ không còn kinh doanh");
                    }
                }
            }

            PayOS payOS = new PayOS("8208b056-d435-43f6-94e8-9415d68154ae", "5e3bf8ac-b167-4681-8caf-80cdb522ced2", "08d586f8f9157aaca716641dc39b6b63d134b573f68c91b98c31f536140ff39a");
            List<ItemData> items = new List<ItemData>();
            foreach (var product in checkoutParam.ListItemCheckout)
            {
                ItemData item = new ItemData(product.ProductName, product.Quantity, (int)product.UnitPrice);
                items.Add(item);
            }

            decimal totalPrice = checkoutParam.ListItemCheckout.Sum(p => p.UnitPrice * p.Quantity);
            decimal finalAmount = 0;
            Address address = new Address();
            if (checkoutParam.AddressDTO != null)
            {
                AddressRepository addressRepository = new AddressRepository();
                // create address from DTO to get id
                address.CustomerId = -1;
                address.FullName = checkoutParam.AddressDTO.FullName;
                address.Phone = checkoutParam.AddressDTO.Phone;
                address.Ward = checkoutParam.AddressDTO.Street;
                address.Province = checkoutParam.AddressDTO.City;
                address.District = checkoutParam.AddressDTO.District;
                address.ProvinceName = checkoutParam.AddressDTO.CityName;
                address.DistrictName = checkoutParam.AddressDTO.DistrictName;
                address.WardName = checkoutParam.AddressDTO.StreetName;
                address.OtherInfo = checkoutParam.AddressDTO.OtherInfo;
                address.IsDefault = checkoutParam.AddressDTO.IsDefault;
                address.CreateAt = DateTime.Now;
                address.Delete = false;
                address.Status = 1;
                var resultAddress = await addressRepository.AddAddress(address);
                if (resultAddress == null)
                {
                    return BadRequest();
                }
                addressId = resultAddress.Id;
                finalAmount = totalPrice + shippingFee;
            }
            else
            {
                finalAmount = (totalPrice - discountAmount) + shippingFee;
            }
            if (addressId == -99)
            {
                return BadRequest("Địa chỉ giao hàng không hợp lệ");
            }
            int ordCode = new Random().Next(1, int.MaxValue);
            Order order = new Order();
            try
            {
                if (PaymentMethodTypeId == 1)
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
                order.IsOrderPOS = false;
                order.Status = PaymentMethodTypeId == 2 ? Constant.OrderStatus.StatusWaitingForPayment : Constant.OrderStatus.StatusPending;
                order.CustomerId = User.FindFirst(ClaimTypes.SerialNumber)?.Value == null ? -1 : int.Parse(User.FindFirst(ClaimTypes.SerialNumber)?.Value);
                order.DiscountId = discountId == null ? null : (int)discountId;
                order.PaymentMethodId = PaymentMethodTypeId;
                order.CustomerType = checkoutParam.AddressDTO != null ? Constant.CustomerType.GuestOrder : Constant.CustomerType.RegisteredOrder;
                if (addressId!=-99)
                {
                    address = await (new AddressRepository()).GetById(addressId);
                }
                order.ProvinceName = address.ProvinceName;
                order.DistrictName = address.DistrictName;
                order.WardName = address.WardName;
                order.Address = address.OtherInfo;
                order.CustomerName = address.FullName;
                order.Phone = address.Phone;
                var result = await orderRepository.AddOrder(order);


                if (result == null)
                {
                    return BadRequest();
                }
                else
                {
                    OrderItemRepository orderItemRepository = new OrderItemRepository();
                    foreach (var product in checkoutParam.ListItemCheckout)
                    {
                        OrderItem orderItem = new OrderItem();
                        orderItem.OrderId = result.OrderId;
                        orderItem.ProductVariantId = product.ProductVariantId;
                        orderItem.Quantity = product.Quantity;
                        orderItem.UnitPrice = product.UnitPrice;
                        orderItem.Subtotal = product.UnitPrice * product.Quantity;
                        orderItem.Delete = false;
                        var resultItem = await orderItemRepository.AddOrderItem(orderItem);
                        if (resultItem == null)
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

            if (discountId != null && discountAmount > 0)
            {
                var discountCode = await discountCodeRepository.GetDiscountCodeById((int)discountId);
                if (discountCode != null)
                {
                    discountCode.UsageCount++;
                    var result = await discountCodeRepository.UpdateDiscountCode(discountCode);
                    if (result == null)
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
                DateTimeOffset utcNow = DateTimeOffset.UtcNow;
                DateTimeOffset expirationTime = utcNow.AddMinutes(15);
                long expiredAt = expirationTime.ToUnixTimeSeconds();
                PaymentData paymentData = new PaymentData(ordCode, (int)finalAmount, "Adam Store Thanh toán", items, "http://localhost:5001/order/payment-cancelled?order-id=" + order.OrderId, "http://localhost:5001/order/payment-success?order-id=" + order.OrderId, null, null, null, null, null, expiredAt);

                CreatePaymentResult createPayment = await payOS.createPaymentLink(paymentData);

                if (createPayment.status == "PENDING")
                {
                    checkoutDTO.URLPayment = createPayment.checkoutUrl;
                    List<OrderItem> orderItemsList = await orderItemRepository.GetOrderItemsByOrderId(order.OrderId);
                    if (orderItemsList != null && orderItemsList.Any())
                    {
                        foreach (var orderItem in orderItemsList)
                        {
                            var re = await productVariantRepository.DecreaseProductVariantQuantity(orderItem.ProductVariantId, orderItem.Quantity);
                            if (re == false)
                                return BadRequest("Số lượng kho không đủ");
                        }
                    }
                    order.PaymentExpiration = DateTime.Now.AddMinutes(15);
                    order.PaymentLink = createPayment.checkoutUrl;
                    await orderRepository.UpdateOrder(order);
                    await orderRepository.UpdateOrder(order);
                    _backgroundJobClient.Schedule<OrderManagerService>(
            x => x.CancelExpiredOrderAsync(order.OrderId),
            TimeSpan.FromMinutes(15));
                    return Ok(checkoutDTO);
                }
                else
                {
                    return BadRequest();
                }
            }
            else if (PaymentMethodTypeId == 1)
            {
                checkoutDTO.URLPayment = null;
                //Trừ số lượng sản phẩm khi đặt
                //List<OrderItem> orderItemsList = await orderItemRepository.GetOrderItemsByOrderId(order.OrderId);
                //if (orderItemsList != null && orderItemsList.Any())
                //{
                //    foreach (var orderItem in orderItemsList)
                //    {
                //        var re = await productVariantRepository.DecreaseProductVariantQuantity(orderItem.ProductVariantId, orderItem.Quantity);
                //        if (re == false)
                //            return BadRequest("Số lượng kho không đủ");
                //    }
                //}
                return Ok(checkoutDTO);
            }

            return BadRequest("Lỗi");


        }

        [HttpGet("PaymentSuccess")]
        public async Task<ActionResult<Order>> PaymentSuccess([FromQuery] int orderId, [FromQuery] bool pos, [FromQuery] string errorMessage = null)
        {

            try
            {
                var order = await orderRepository.GetOrderById(orderId);
                if (order == null)
                {
                    return NotFound();
                }
                var statusHistory = ParseStatusHistory(order.StatusHistory);
                if (pos == true)
                {
                    statusHistory.Add(new StatusHistoryEntry
                    {
                        Index = statusHistory.Count + 1,
                        Status = Constant.OrderStatus.StatusDone,
                        OrderStatus = Constant.OrderStatus.OrderStatusDone,
                        PaymentStatus = Constant.OrderStatus.PaymentCompleted,
                        DateTime = DateTime.Now.ToString("HH:mm dd/MM/yyyy")
                    });

                    order.StatusHistory = JsonSerializer.Serialize(statusHistory, _camelCaseJsonOptions);
                    order.PaymentStatus = Constant.OrderStatus.PaymentCompleted;
                    order.OrderStatus = Constant.OrderStatus.OrderStatusDone;
                    order.Status = Constant.OrderStatus.StatusDone;
                    order.LastUpdate = DateTime.Now;
                    order.UpdateBy = "System";

                }
                else
                {
                    statusHistory.Add(new StatusHistoryEntry
                    {
                        Index = statusHistory.Count + 1,
                        Status = Constant.OrderStatus.StatusConfirm,
                        OrderStatus = Constant.OrderStatus.OrderStatusConfirm,
                        PaymentStatus = Constant.OrderStatus.PaymentCompleted,
                        DateTime = DateTime.Now.ToString("HH:mm dd/MM/yyyy")
                    });

                    order.StatusHistory = JsonSerializer.Serialize(statusHistory, _camelCaseJsonOptions);
                    order.PaymentStatus = Constant.OrderStatus.PaymentCompleted;
                    order.OrderStatus = Constant.OrderStatus.OrderStatusConfirm;
                    order.Status = Constant.OrderStatus.StatusConfirm;
                    order.LastUpdate = DateTime.Now;
                    order.UpdateBy = "System";
                }

                var result = await orderRepository.UpdateOrder(order);
                if (result == null)
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
                if (order.Status == Constant.OrderStatus.StatusCanceledByUser)
                    return Ok();
                if (order == null)
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

                List<OrderItem> orderItemsList = await orderItemRepository.GetOrderItemsByOrderId(order.OrderId);
                if (orderItemsList != null && orderItemsList.Any())
                {
                    foreach (var orderItem in orderItemsList)
                    {
                        var re = await productVariantRepository.IncreaseProductVariantQuantity(orderItem.ProductVariantId, orderItem.Quantity);
                        if (re == false)
                            return BadRequest("Số lượng kho không đủ");
                    }
                }

                if (result == null)
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
        public async Task<ActionResult<List<Order>>> GetAllOrders([FromQuery] string? keyword)
        {
            try
            {
                var result = await orderRepository.GetAllOrders(keyword);
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

        [HttpGet("get-order-sale-counter")]
        public async Task<ActionResult<List<Order>>> GetAllOrderSaleCounter()
        {
            try
            {
                var result = await orderRepository.GetAllSaleCounter();
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
            string userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            string userId = User.FindFirst(ClaimTypes.SerialNumber)?.Value;
            try
            {
                var result = await orderRepository.GetOrderByOrderCode(orderCode);
                if (result == null)
                {
                    return NotFound(Constant.ErrorCode.DataNotFound);
                }
                else
                {
                    var order = await orderRepository.GetOrderByOrderCode(orderCode);
                    if (order == null)
                    {
                        return NotFound(Constant.ErrorCode.DataNotFound);
                    }
                    if (order.CustomerId != -1 && string.IsNullOrEmpty(userId) && order.CustomerId != null)
                    {
                        return Forbid();
                    }

                    if (userRole != null && userRole == "Customer")
                    {
                        if (order.CustomerId == -1 || order.CustomerId.ToString() != userId)
                        {
                            return Forbid();
                        }
                    }
                    if (string.IsNullOrEmpty(userId) && order.CustomerId != -1 && order.CustomerId != null || string.IsNullOrEmpty(userRole) && order.CustomerId != -1 && order.CustomerId != null)
                    {
                        return Forbid();
                    }

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
                        Street = order.ShippingAddress.Ward,
                        City = order.ShippingAddress.Province,
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
                        FullName = order.CustomerName,
                        Phone = order.Phone,
                        Street = order.WardName,
                        City = order.ProvinceName,
                        District = order.DistrictName,
                        OtherInfo = order.Address
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
                if (order.ShippingFee != 0)
                {
                    invoice.ShippingFee = order.ShippingFee;
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
            string userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            string userId = User.FindFirst(ClaimTypes.SerialNumber)?.Value;
            try
            {
                var result = await orderRepository.GetAllByKeyword(keyword);
                if (result == null)
                {
                    return Ok(new List<Order>());
                }
                else
                {
                    foreach (var order in result.ToList())
                    {
                        if (order.CustomerId != -1 && string.IsNullOrEmpty(userId) && order.CustomerId != null)
                        {
                            result.Remove(order);
                            continue;
                        }
                        if (userRole != null && userRole == "Customer")
                        {
                            if (order.CustomerId == -1 || order.CustomerId.ToString() != userId)
                            {
                                result.Remove(order);
                                continue;
                            }
                        }
                        if (string.IsNullOrEmpty(userId) && order.CustomerId != -1 && order.CustomerId != null || string.IsNullOrEmpty(userRole) && order.CustomerId != -1 && order.CustomerId != null)
                        {
                            result.Remove(order);
                            continue;
                        }
                    }

                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }
    }
}



