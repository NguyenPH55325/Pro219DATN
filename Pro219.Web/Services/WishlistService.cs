using Pro219.DAL.Models;
using Pro219.Web.Constants;
using Pro219.Web.DTOs;

namespace Pro219.Web.Services
{
    public class WishlistService
    {
        private readonly HttpClient _httpClient;

        public WishlistService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<ServiceResult<List<Wishlist>>> GetAll()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/Wishlist/GetAll");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<Wishlist>>();
                return ServiceResult<List<Wishlist>>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result;
                return ServiceResult<List<Wishlist>>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<Wishlist>> GetById(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/Wishlist/GetById/{id}");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Wishlist>();
                return ServiceResult<Wishlist>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result;
                return ServiceResult<Wishlist>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<List<Wishlist>>> GetAllByCustomer(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/Wishlist/GetByCustomerId/{id}");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<Wishlist>>();
                return ServiceResult<List<Wishlist>>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result;
                return ServiceResult<List<Wishlist>>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<Wishlist>> GetByCustomerAndProduct(int customerId, int productId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/Wishlist/GetByCustomerAndProduct/{customerId}/{productId}");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Wishlist>();
                return ServiceResult<Wishlist>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result;
                return ServiceResult<Wishlist>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<Wishlist>> Create(Wishlist wishlist)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/Wishlist/Add");

            request.Content = JsonContent.Create(wishlist);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Wishlist>();
                return ServiceResult<Wishlist>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result;
                return ServiceResult<Wishlist>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<Wishlist>> Edit(Wishlist data, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, "/Wishlist/Update");

            if (!string.IsNullOrEmpty(token))
            {
                var formatToken = token.Trim('"');
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", formatToken);
            }

            request.Content = JsonContent.Create(data);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Wishlist>();
                return ServiceResult<Wishlist>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result;
                return ServiceResult<Wishlist>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<bool> Delete(int id, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/Wishlist/Delete/{id}");

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

        public async Task<ServiceResult<bool>> ToggleWishlist(int customerId, int productId, string token)
        {
            try
            {
                var existingWishlist = await GetByCustomerAndProduct(customerId, productId);

                if (existingWishlist.IsSuccess && existingWishlist.Data != null)
                {
                    var deleteResult = await Delete(existingWishlist.Data.Id, token);
                    if (deleteResult)
                    {
                        return ServiceResult<bool>.Success(false);
                    }
                    else
                    {
                        return ServiceResult<bool>.Failure("DELETE_FAILED", "Không thể xóa khỏi wishlist", "500");
                    }
                }
                else
                {
                    var newWishlist = new Wishlist
                    {
                        CustomerId = customerId,
                        ProductId = productId,
                        Status = 1,
                        Delete = false
                    };

                    var createResult = await Create(newWishlist);
                    if (createResult.IsSuccess)
                    {
                        return ServiceResult<bool>.Success(true);
                    }
                    else
                    {
                        return ServiceResult<bool>.Failure(createResult.ErrorCode, createResult.ErrorMessage, createResult.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure("EXCEPTION", $"Lỗi: {ex.Message}", "500");
            }
        }
    }
}
