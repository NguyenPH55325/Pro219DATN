using Newtonsoft.Json.Linq;
using Pro219.DAL.Models;
using Pro219.Web.Constants;
using Pro219.Web.DTOs;

namespace Pro219.Web.Services
{
    public class CartService
    {
        private readonly HttpClient _httpClient;

        public CartService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ServiceResult<List<Cart>>> GetAll()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/Cart/GetAll");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<Cart>>();
                return ServiceResult<List<Cart>>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result; 
                return ServiceResult<List<Cart>>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<Cart>> GetById(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/Cart/GetById/{id}");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Cart>();
                return ServiceResult<Cart>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result; 
                return ServiceResult<Cart>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<Cart>> GetByCustomerId(int customerId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/Cart/GetByCustomerId/{customerId}");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Cart>();
                return ServiceResult<Cart>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result; 
                return ServiceResult<Cart>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<Cart>> Create(CartModel cart)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/Cart/Add");

            request.Content = JsonContent.Create(cart);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Cart>();
                return ServiceResult<Cart>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result; 
                return ServiceResult<Cart>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<CartItem>> AddToCart(AddCartModel addCartModel, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/Cart/AddToCart");

            if (!string.IsNullOrEmpty(token))
            {
                var formatToken = token.Trim('"');
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", formatToken);
            }

            request.Content = JsonContent.Create(addCartModel);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CartItem>();
                return ServiceResult<CartItem>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result; 
                return ServiceResult<CartItem>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<bool> MergeCart(List<AddCartModel> addCartModel, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/Cart/UpdateCartFromGuestLogin");

            if (!string.IsNullOrEmpty(token))
            {
                var formatToken = token.Trim('"');
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", formatToken);
            }

            request.Content = JsonContent.Create(addCartModel);

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
    }
}
