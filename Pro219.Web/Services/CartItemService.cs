using Pro219.DAL.Models;
using Pro219.Web.Components.Pages.User.Cart;
using Pro219.Web.Constants;
using Pro219.Web.DTOs;

namespace Pro219.Web.Services
{
    public class CartItemService
    {
        private readonly HttpClient _httpClient;

        public CartItemService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ServiceResult<List<CartItemWithProductDTO>>> GetAllCartItemWithDetailByCartId(int cartId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/CartItem/get-all-cart-item-with-detail/{cartId}");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<CartItemWithProductDTO>>();
                return ServiceResult<List<CartItemWithProductDTO>>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result; 
                return ServiceResult<List<CartItemWithProductDTO>>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<List<CartItemWithProductDTO>>> GetAllCartItemWithDetailOfGuest(List<AddCartModel> cartId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/CartItem/get-all-cart-item-by-guest");
            request.Content = JsonContent.Create(cartId);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<CartItemWithProductDTO>>();
                return ServiceResult<List<CartItemWithProductDTO>>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result;
                return ServiceResult<List<CartItemWithProductDTO>>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<CartItem>> UpdateQuantity(AddCartModel addToCartDTO, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, "/CartItem/update-quantity");

            if (!string.IsNullOrEmpty(token))
            {
                var formatToken = token.Trim('"');
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", formatToken);
            }

            request.Content = JsonContent.Create(addToCartDTO);

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

        public async Task<ServiceResult<CartItem>> Update(CartItemUpdateDTO cartItemUpdateDTO, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, "/CartItem/Update");

            if (!string.IsNullOrEmpty(token))
            {
                var formatToken = token.Trim('"');
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", formatToken);
            }

            request.Content = JsonContent.Create(cartItemUpdateDTO);

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

        public async Task<bool> Delete(int id, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/CartItem/Delete/{id}");

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
    }
}
