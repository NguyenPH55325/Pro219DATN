using Azure;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json.Linq;
using Pro219.API.DTOs;
using Pro219.DAL.Models;
using Pro219.Web.Components.Pages.User.Order;
using Pro219.Web.Components.Pages.User.Search;
using Pro219.Web.Constants;
using Pro219.Web.DTOs;
using System;

namespace Pro219.Web.Services
{
    public class OrderService
    {
        private readonly HttpClient _httpClient;

        public OrderService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ServiceResult<Pro219.Web.DTOs.CheckoutDTO>> CheckoutPOS(CheckoutPOS checkoutPOS)
        {
            var lstItem = new List<CheckoutListItem>();

            if(checkoutPOS != null && checkoutPOS.ListItemCheckout.Any()) 
            { 
                foreach(var item in checkoutPOS.ListItemCheckout)
                {
                    var obj = new CheckoutListItem
                    {
                        ProductName = item.ProductName,
                        ProductVariantId = item.ProductVariantId,
                        Quantity = item.Quantity,
                        Subtotal = item.Subtotal,
                        UnitPrice = item.UnitPrice,
                    };
                    lstItem.Add(obj);
                }
            }
            var checkoutParam = new CheckoutPOSModel 
            { 
                AddressDTO = checkoutPOS.AddressDTO,
                ListItemCheckout = lstItem,
                isNewAddress = checkoutPOS.isNewAddress,
                shippingAddressId = checkoutPOS.shippingAddressId != null && checkoutPOS.shippingAddressId > 0 ? checkoutPOS.shippingAddressId : -1,
            };

            var queryParams = new Dictionary<string, string?>
            {
                { "orderId", checkoutPOS.OrderId.ToString() },
                { "discountAmount", checkoutPOS.DiscountAmount.ToString() },
                { "shippingFee", checkoutPOS.ShippingFee.ToString() },
                { "PaymentMethodTypeId", checkoutPOS.PaymentMethodTypeId.ToString() },
            };

            if (checkoutPOS.DiscountId > 0) queryParams.Add("discountId", checkoutPOS.DiscountId.ToString());
            if (!string.IsNullOrEmpty(checkoutPOS.PhoneNumber)) queryParams.Add("phoneNumber", checkoutPOS.PhoneNumber);
            if (!string.IsNullOrEmpty(checkoutPOS.Note)) queryParams.Add("note", checkoutPOS.Note);

            var baseUrl = "/Order/checkout-pos";

            string url = QueryHelpers.AddQueryString(baseUrl, queryParams!);
            var request = new HttpRequestMessage(HttpMethod.Post, url);

            request.Content = JsonContent.Create(checkoutParam);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Pro219.Web.DTOs.CheckoutDTO>();
                return ServiceResult<Pro219.Web.DTOs.CheckoutDTO>.Success(result);
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                var errorMess = Constant.Errors.ContainsKey(errorMessage ?? "")
                    ? Constant.Errors[errorMessage ?? ""]
                    : $"Lỗi không xác định: {response.ReasonPhrase}";
                return ServiceResult<Pro219.Web.DTOs.CheckoutDTO>.Failure(errorMessage, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<Pro219.Web.DTOs.CheckoutDTO>> GetCheckoutUrl(
            string token,
            CheckoutModel checkoutBody,
            decimal discountAmount = 0,
            decimal shippingFee = 0,
            string note = "",
            int shippingAddressId = -99,
            int? paymentMethodTypeId = 2,
            int? discountId = null)
        {
            var queryParams = new Dictionary<string, string?>
            {
                { "discountAmount", discountAmount.ToString() },
                { "shippingFee", shippingFee.ToString() },
                { "PaymentMethodTypeId", paymentMethodTypeId.ToString() },
                { "note", note },
                { "addressId", shippingAddressId.ToString() }
            };

            if (discountId.HasValue)
            {
                queryParams.Add("discountId", discountId.Value.ToString());
            }

            string baseUrl = "Order/Checkout";

            string url = QueryHelpers.AddQueryString(baseUrl, queryParams!);

            var request = new HttpRequestMessage(HttpMethod.Post, url);

            if (!string.IsNullOrEmpty(token))
            {
                var formatToken = token.Trim('"');
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", formatToken);
            }

            request.Content = JsonContent.Create(checkoutBody);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Pro219.Web.DTOs.CheckoutDTO>();
                return ServiceResult<Pro219.Web.DTOs.CheckoutDTO>.Success(result);
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                var errorMess = Constant.Errors.ContainsKey(errorMessage ?? "")
                    ? Constant.Errors[errorMessage ?? ""]
                    : $"Lỗi không xác định: {response.ReasonPhrase}";
                return ServiceResult<Pro219.Web.DTOs.CheckoutDTO>.Failure(errorMessage, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<List<DAL.Models.Order>>> GetAll(string? keyword)
        {
            var queryParams = new Dictionary<string, string?>();

            if (!string.IsNullOrEmpty(keyword))
            {
                queryParams.Add("keyword", keyword);
            }

            string url = QueryHelpers.AddQueryString("/Order/GetAll", queryParams!);

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<DAL.Models.Order>>();
                return ServiceResult<List<DAL.Models.Order>>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result; 
                return ServiceResult<List<DAL.Models.Order>>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<DAL.Models.Order>> GetById(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/Order/GetById/{id}");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<DAL.Models.Order>();
                return ServiceResult<DAL.Models.Order>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result; 
                return ServiceResult<DAL.Models.Order>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<List<DAL.Models.Order>>> GetAllByCustomerId(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/Order/GetByCustomerId/{id}");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<DAL.Models.Order>>();
                return ServiceResult<List<DAL.Models.Order>>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result; 
                return ServiceResult<List<DAL.Models.Order>>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<List<OrderItem>>> GetAllOrderItemByOrderId(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/OrderItem/GetByOrderId/{id}");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<OrderItem>>();
                return ServiceResult<List<OrderItem>>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result; 
                return ServiceResult<List<OrderItem>>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<Pro219.Web.DTOs.OrderDetailDTO>> GetOrderDetailById(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/Order/GetDetailById/{id}");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Pro219.Web.DTOs.OrderDetailDTO>();
                return ServiceResult<Pro219.Web.DTOs.OrderDetailDTO>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorMess = Constant.Errors[result];
                return ServiceResult<Pro219.Web.DTOs.OrderDetailDTO>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<Pro219.Web.DTOs.OrderDetailDTO>> GetOrderDetailByUserId(string orderCode)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/Order/CustomerOrderDetail/{orderCode}");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Pro219.Web.DTOs.OrderDetailDTO>();
                return ServiceResult<Pro219.Web.DTOs.OrderDetailDTO>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorMess = Constant.Errors[result];
                return ServiceResult<Pro219.Web.DTOs.OrderDetailDTO>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<DAL.Models.Order>> UpdateStatus(OrderUpdateSatusModel order, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Put, "/Order/UpdateStatus");

                if (!string.IsNullOrEmpty(token))
                {
                    var formatToken = token.Trim('"');
                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", formatToken);
                }

                request.Content = JsonContent.Create(order);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<DAL.Models.Order>();
                    return ServiceResult<DAL.Models.Order>.Success(result);
                }
                else
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var errorMess = Constant.Errors.ContainsKey(result ?? "") 
                        ? Constant.Errors[result ?? ""] 
                        : $"Lỗi không xác định: {response.ReasonPhrase} (Status: {response.StatusCode})";
                    return ServiceResult<DAL.Models.Order>.Failure(result, errorMess, response.StatusCode.ToString());
                }
            }
            catch (Exception ex)
            {
                return ServiceResult<DAL.Models.Order>.Failure("EXCEPTION", $"Lỗi khi gọi API: {ex.Message}", "500");
            }
        }

        public async Task<ServiceResult<DAL.Models.Order>> PaymentSuccess(int orderId, bool pos = false)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/Order/PaymentSuccess?orderId={orderId}&pos={pos}");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<DAL.Models.Order>();
                return ServiceResult<DAL.Models.Order>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result; 
                return ServiceResult<DAL.Models.Order>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<DAL.Models.Order>> PaymentCanceled(int orderId, bool pos = false)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/Order/PaymentCanceled?orderId={orderId}&pos={pos}");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<DAL.Models.Order>();
                return ServiceResult<DAL.Models.Order>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result; 
                return ServiceResult<DAL.Models.Order>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<List<DAL.Models.Order>>> GetAllSaleCounter()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/Order/get-order-sale-counter");

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<DAL.Models.Order>>();
                return ServiceResult<List<DAL.Models.Order>>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;
                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result;
                return ServiceResult<List<DAL.Models.Order>>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<DAL.Models.Order>> Create(DAL.Models.Order order)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/Order/Add");

            request.Content = JsonContent.Create(order);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<DAL.Models.Order>();
                return ServiceResult<DAL.Models.Order>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result;
                return ServiceResult<DAL.Models.Order>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<DAL.Models.Order>> Update(DAL.Models.Order order, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/Order/Update");

            request.Content = JsonContent.Create(order);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<DAL.Models.Order>();
                return ServiceResult<DAL.Models.Order>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result;
                return ServiceResult<DAL.Models.Order>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<byte[]>> GetInvoicePdf(int id)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"/Order/GetInvoicePdf/{id}");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                    return ServiceResult<byte[]>.Success(pdfBytes);
                }
                else
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var errorMess = Constant.Errors.ContainsKey(result ?? "")
                                    ? Constant.Errors[result ?? ""]
                                    : "Không thể tải hóa đơn PDF";
                    return ServiceResult<byte[]>.Failure(result, errorMess, response.StatusCode.ToString());
                }
            }
            catch (Exception ex)
            {
                return ServiceResult<byte[]>.Failure("EXCEPTION", ex.Message, "500");
            }
        }

        public async Task<ServiceResult<List<DAL.Models.Order>>> GetAllByKeyword(string keyword, string token)
        {
            var url = string.IsNullOrEmpty(keyword) ? "/Order/get-all-by-key-word" : $"/Order/get-all-by-key-word?keyword={keyword}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            if (!string.IsNullOrEmpty(token))
            {
                var formatToken = token.Trim('"');
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", formatToken);
            }

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<DAL.Models.Order>>();
                return ServiceResult<List<DAL.Models.Order>>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;
                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result; 
                return ServiceResult<List<DAL.Models.Order>>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<DAL.Models.OrderItem>> CreateOrderItem(DAL.Models.OrderItem orderItem)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/OrderItem/Add", orderItem);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<DAL.Models.OrderItem>();
                    return ServiceResult<DAL.Models.OrderItem>.Success(result);
                }
                else
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var errorCode = result;

                    var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                        ? Constant.Errors[errorCode ?? ""]
                                        : result;
                    return ServiceResult<DAL.Models.OrderItem>.Failure(result, errorMess, response.StatusCode.ToString());
                }
            } catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return ServiceResult<DAL.Models.OrderItem>.Failure("Exception", ex.Message, "500");
            }
        }

        public async Task<bool> DeleteOrder(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/Order/Delete/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public async Task<ServiceResult<DAL.Models.Order>> ChangePaymentToCash(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"/Order/ChangePaymentMethodToCash/{id}");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<DAL.Models.Order>();
                return ServiceResult<DAL.Models.Order>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result;
                return ServiceResult<DAL.Models.Order>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }
    }
}
