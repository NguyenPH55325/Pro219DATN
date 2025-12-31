// Pro219.Web/Services/ProductService.cs
using Azure;
using Microsoft.AspNetCore.WebUtilities;
using Pro219.DAL.Models;
using Pro219.Web.Constants;
using Pro219.Web.DTOs;
using System.Net.Http.Json;
using static MudBlazor.Icons.Custom;
using static Pro219.Web.Constants.Constant;
using static System.Net.WebRequestMethods;

namespace Pro219.Web.Services
{
    public class ProductService
    {
        private readonly HttpClient _httpClient;

        public ProductService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ServiceResult<List<Product>>> GetAll(string? keyword = null, int? categoryId = null, int? brandId = null)
        {
            var queryParams = new Dictionary<string, string?>();

            if (!string.IsNullOrEmpty(keyword)) queryParams.Add("keyword", keyword);
            if (categoryId.HasValue && categoryId > 0) queryParams.Add("categoryId", categoryId.Value.ToString());
            if (brandId.HasValue && brandId > 0) queryParams.Add("brandId", brandId.Value.ToString());

            var uri = QueryHelpers.AddQueryString("/Product/GetAll", queryParams);

            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<Product>>();
                return ServiceResult<List<Product>>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;
                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result;
                return ServiceResult<List<Product>>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<List<DAL.Repository.ProductRepository.ProductDetailDto>>> GetAllSearch(
            int page,
            int pageSize,
            int? brandId = null,
            int? sizeId = null,
            int? colorId = null,
            string? sortOrder = null)
        {
            var url = $"/Product/GetAllProducts?page={page}&pageSize={pageSize}";

            if (brandId.HasValue && brandId > 0)
            {
                url += $"&brandId={brandId}";
            }
            if (sizeId.HasValue && sizeId > 0)
            {
                url += $"&sizeId={sizeId}";
            }
            if (colorId.HasValue && colorId > 0)
            {
                url += $"&colorId={colorId}";
            }
            if (!string.IsNullOrEmpty(sortOrder))
            {
                url += $"&sortOrder={sortOrder}";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<DAL.Repository.ProductRepository.ProductDetailDto>>();
                return ServiceResult<List<DAL.Repository.ProductRepository.ProductDetailDto>>.Success(result);
            }
            else
            {
                var errorCode = await response.Content.ReadAsStringAsync();

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                     ? Constant.Errors[errorCode ?? ""]
                                     : $"Lỗi không xác định: {response.ReasonPhrase}";

                return ServiceResult<List<DAL.Repository.ProductRepository.ProductDetailDto>>.Failure(
                    errorCode,
                    errorMess,
                    response.StatusCode.ToString()
                );
            }
        }

        public async Task<ServiceResult<List<CombineProductDTO>>> GetAllProductOfSaleCounter(
            string? keyword = null,
            int? brandId = null,
            int? sizeId = null,
            int? colorId = null)
        {
            var baseUrl = "/Product/SearchCombineProduct";
            var queryParams = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(keyword)) queryParams.Add("keyword", keyword);
            if (brandId > 0) queryParams.Add("brandId", brandId.ToString());
            if (sizeId > 0) queryParams.Add("sizeId", sizeId.ToString());
            if (colorId > 0) queryParams.Add("colorId", colorId.ToString());

            var url = QueryHelpers.AddQueryString(baseUrl, queryParams);

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<CombineProductDTO>>();
                return ServiceResult<List<CombineProductDTO>>.Success(result);
            }
            else
            {
                var errorCode = await response.Content.ReadAsStringAsync();

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                     ? Constant.Errors[errorCode ?? ""]
                                     : $"Lỗi không xác định: {response.ReasonPhrase}";

                return ServiceResult<List<CombineProductDTO>>.Failure(
                    errorCode,
                    errorMess,
                    response.StatusCode.ToString()
                );
            }
        }

        public async Task<ServiceResult<List<DAL.Repository.ProductRepository.ProductDetailDto>>> GetAllByCategory(
            int categoryId,
            int page,
            int pageSize,
            int? brandId = null,
            int? sizeId = null,
            int? colorId = null,
            string? sortOrder = null)
        {
            var url = $"/Product/GetAllProductsInCategory/{categoryId}?page={page}&pageSize={pageSize}";

            if (brandId.HasValue && brandId > 0)
            {
                url += $"&brandId={brandId}";
            }
            if (sizeId.HasValue && sizeId > 0)
            {
                url += $"&sizeId={sizeId}";
            }
            if (colorId.HasValue && colorId > 0)
            {
                url += $"&colorId={colorId}";
            }
            if (!string.IsNullOrEmpty(sortOrder))
            {
                url += $"&sortOrder={sortOrder}";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<DAL.Repository.ProductRepository.ProductDetailDto>>();
                return ServiceResult<List<DAL.Repository.ProductRepository.ProductDetailDto>>.Success(result ?? new List<DAL.Repository.ProductRepository.ProductDetailDto>());
            }
            else
            {
                var errorCode = await response.Content.ReadAsStringAsync();

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                     ? Constant.Errors[errorCode ?? ""]
                                     : errorCode; 

                return ServiceResult<List<DAL.Repository.ProductRepository.ProductDetailDto>>.Failure(
                    errorCode,
                    errorMess,
                    response.StatusCode.ToString()
                );
            }
        }

        public async Task<ServiceResult<List<DAL.Repository.ProductRepository.ProductDetailDto>>> GetAllByKeyword(
            string keyword,
            int page,
            int pageSize,
            int? brandId = null,
            int? sizeId = null,
            int? colorId = null,
            string? sortOrder = null)
        {
            var url = $"/Product/GetAllProductByKeyWord/{keyword}?page={page}&pageSize={pageSize}";

            if (brandId.HasValue && brandId > 0)
            {
                url += $"&brandId={brandId}";
            }
            if (sizeId.HasValue && sizeId > 0)
            {
                url += $"&sizeId={sizeId}";
            }
            if (colorId.HasValue && colorId > 0)
            {
                url += $"&colorId={colorId}";
            }
            if (!string.IsNullOrEmpty(sortOrder))
            {
                url += $"&sortOrder={sortOrder}";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<DAL.Repository.ProductRepository.ProductDetailDto>>();
                return ServiceResult<List<DAL.Repository.ProductRepository.ProductDetailDto>>.Success(result ?? new List<DAL.Repository.ProductRepository.ProductDetailDto>());
            }
            else
            {
                var errorCode = await response.Content.ReadAsStringAsync();

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                     ? Constant.Errors[errorCode ?? ""]
                                     : $"Lỗi không xác định: {response.ReasonPhrase}";

                return ServiceResult<List<DAL.Repository.ProductRepository.ProductDetailDto>>.Failure(
                    errorCode,
                    errorMess,
                    response.StatusCode.ToString()
                );
            }
        }

        public async Task<ServiceResult<Product>> GetById(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/Product/GetById/{id}");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Product>();
                return ServiceResult<Product>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result; 
                return ServiceResult<Product>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<DAL.Repository.ProductRepository.ProductDetailDto>> GetDetail(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/Product/get-detail/{id}");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<DAL.Repository.ProductRepository.ProductDetailDto>();
                return ServiceResult<DAL.Repository.ProductRepository.ProductDetailDto>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result; 
                return ServiceResult<DAL.Repository.ProductRepository.ProductDetailDto>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<Product>> Create(ProductModel product)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/Product/Add");

            request.Content = JsonContent.Create(product);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Product>();
                return ServiceResult<Product>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result; 
                return ServiceResult<Product>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<Product>> Update(ProductModel product, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, "/Product/Update");

            if (!string.IsNullOrEmpty(token))
            {
                var formatToken = token.Trim('"');
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", formatToken);
            }

            request.Content = JsonContent.Create(product);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Product>();
                return ServiceResult<Product>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result; 
                return ServiceResult<Product>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<bool> Delete(int id, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/Product/Delete/{id}");

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

        public async Task<ServiceResult<List<DAL.Repository.ProductRepository.ProductDetailDto>>> GetAllWithDetail()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/Product/get-with-detail");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<DAL.Repository.ProductRepository.ProductDetailDto>>();
                return ServiceResult<List<DAL.Repository.ProductRepository.ProductDetailDto>>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result;
                return ServiceResult<List<DAL.Repository.ProductRepository.ProductDetailDto>>.Failure(result, errorMess, response.StatusCode.ToString());
            }
        }

        public async Task<ServiceResult<List<DAL.Repository.ProductRepository.ProductDetailDto>>> GetNewProducts()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/Product/get-new-products");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<DAL.Repository.ProductRepository.ProductDetailDto>>();
                return ServiceResult<List<DAL.Repository.ProductRepository.ProductDetailDto>>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result; 
                return ServiceResult<List<DAL.Repository.ProductRepository.ProductDetailDto>>.Failure(result, errorMess, response.StatusCode.ToString());
            }

        }
        public async Task<ServiceResult<List<DAL.Repository.ProductRepository.ProductDetailDto>>> GetFavouriteProducts()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/Product/get-favourite-products");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<DAL.Repository.ProductRepository.ProductDetailDto>>();
                return ServiceResult<List<DAL.Repository.ProductRepository.ProductDetailDto>>.Success(result);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                var errorCode = result;

                var errorMess = Constant.Errors.ContainsKey(errorCode ?? "")
                                    ? Constant.Errors[errorCode ?? ""]
                                    : result; 
                return ServiceResult<List<DAL.Repository.ProductRepository.ProductDetailDto>>.Failure(result, errorMess, response.StatusCode.ToString());
            }

        }
    }
}