using Microsoft.AspNetCore.WebUtilities;
using Pro219.DAL.Models;
using Pro219.Web.Constants;
using Pro219.Web.DTOs;

namespace Pro219.Web.Services
{
    public class CouponService
    {
        private readonly HttpClient _httpClient;

        public CouponService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ServiceResult<List<DiscountCode>>> GetAll()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/DiscountCode/GetAll");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<DiscountCode>>();
                return ServiceResult<List<DiscountCode>>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result; 
                return ServiceResult<List<DiscountCode>>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<List<DiscountCode>>> GetAllNow()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/DiscountCode/GetAllNow");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<DiscountCode>>();
                return ServiceResult<List<DiscountCode>>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result;
                return ServiceResult<List<DiscountCode>>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<DiscountCodeModel>> GetById(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/DiscountCode/GetById/{id}");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<DiscountCodeModel>();
                return ServiceResult<DiscountCodeModel>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result; 
                return ServiceResult<DiscountCodeModel>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<DiscountCode>> GetByCode(string code)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/DiscountCode/GetByCode/{code}");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<DiscountCode>();
                return ServiceResult<DiscountCode>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result; 
                return ServiceResult<DiscountCode>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<DiscountCode>> Create(DiscountCodeModel discountCode)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/DiscountCode/Add");

            request.Content = JsonContent.Create(discountCode);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<DiscountCode>();
                return ServiceResult<DiscountCode>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result; 
                return ServiceResult<DiscountCode>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<DiscountCode>> Update(DiscountCodeModel discountCode, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, "/DiscountCode/Update");

            if (!string.IsNullOrEmpty(token))
            {
                var formatToken = token.Trim('"');
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", formatToken);
            }

            request.Content = JsonContent.Create(discountCode);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<DiscountCode>();
                return ServiceResult<DiscountCode>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result; 
                return ServiceResult<DiscountCode>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<bool> Delete(int id, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/DiscountCode/Delete/{id}");

            if (!string.IsNullOrEmpty(token))
            {
                var formatToken = token.Trim('"');
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", formatToken);
            }

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<ServiceResult<ApplyDiscountDTO>> ApplyDiscountCodeValue(string code, decimal totalAmount, decimal shippingFee, string token)
        {
            string baseUrl = "/DiscountCode/ApplyDiscountCodeValue"; 

            var queryParams = new Dictionary<string, string?>
            {
                { "code", code },
                { "totalAmount", totalAmount.ToString() },
                { "shippingFee", shippingFee.ToString() }
            };

            string url = QueryHelpers.AddQueryString(baseUrl, queryParams!);

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
                var discountValue = await response.Content.ReadFromJsonAsync<ApplyDiscountDTO>();
                return ServiceResult<ApplyDiscountDTO>.Success(discountValue);
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                var errorCode = responseContent;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : responseContent; 

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    errorMess = responseContent;
                }


                return ServiceResult<ApplyDiscountDTO>.Failure(
                    errorCode,
                    errorMess,
                    response.StatusCode.ToString()
                );
            }
        }
    }
}
