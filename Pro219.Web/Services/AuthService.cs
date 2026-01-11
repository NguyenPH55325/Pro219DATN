using Microsoft.AspNetCore.Identity.Data;
using Pro219.Web.DTOs;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Pro219.Web.Constants;
using Microsoft.AspNetCore.Http;

namespace Pro219.Web.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public string HashPassword(string password)
        {
            // 4297F44B13955235245B2497399D7A93 (123123)
            // 26dc318942685872cf79c5eb96c9bb13 (Admin@12345)
            // b855e41c5c5f5061ecba4fd8613a7760 (User@12345)
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(password);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            md5.Clear();
            return sb.ToString();

        }

        public async Task<LoginResponseDTO> LoginCustomer(string username, string password)
        {
            var loginRequest = new LoginModel
            {
                Username = username,
                PasswordHash = HashPassword(password)
            };

            var response = await _httpClient.PostAsJsonAsync("/Access/LoginCustomer", loginRequest);

            if (response.IsSuccessStatusCode)
            {
                var responseDTO = await response.Content.ReadFromJsonAsync<LoginResponseDTO>();
                return responseDTO;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var responseDTO = await response.Content.ReadFromJsonAsync<LoginResponseDTO>();
                return responseDTO ?? new LoginResponseDTO { LoginSuccess = false };
            }
            else
            {
                return new LoginResponseDTO { LoginSuccess = false };
            }
        }

        public async Task<LoginResponseDTO> LoginStaff(string username, string password)
        {
            var loginRequest = new LoginModel
            {
                Username = username,
                PasswordHash = HashPassword(password)
            };

            var response = await _httpClient.PostAsJsonAsync("/Access/LoginStaff", loginRequest);

            if (response.IsSuccessStatusCode)
            {
                var responseDTO = await response.Content.ReadFromJsonAsync<LoginResponseDTO>();
                return responseDTO;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var responseDTO = await response.Content.ReadFromJsonAsync<LoginResponseDTO>();
                return responseDTO ?? new LoginResponseDTO { LoginSuccess = false };
            }
            else
            {
                return new LoginResponseDTO { LoginSuccess = false };
            }
        }

        public async Task<GetMeResponseDTO> AccessCheck(string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/Access/Check");

            if (!string.IsNullOrEmpty(token))
            {
                var formatToken = token.Trim('"');
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", formatToken);
            }

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseDTO = await response.Content.ReadFromJsonAsync<GetMeResponseDTO>();
                return responseDTO;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var responseDTO = await response.Content.ReadFromJsonAsync<GetMeResponseDTO>();
                return responseDTO ?? new GetMeResponseDTO { IsExpired = true };
            }
            else
            {
                var responseDTO = await response.Content.ReadFromJsonAsync<GetMeResponseDTO>();
                return responseDTO ?? new GetMeResponseDTO { IsExpired = true };
            }
        }

        public async Task<RegisterResponseDTO> RegisterCustomer(RegisterModel payload)
        {
            var request = new RegisterModel()
            {
                DateOfBirth = payload.DateOfBirth,
                PasswordHash = HashPassword(payload.PasswordHash.Trim()),
                Email = payload.Email.Trim(),
                FullName = payload.FullName.Trim(),
                PhoneNumber = payload.PhoneNumber.Trim(),
            };

            var response = await _httpClient.PostAsJsonAsync("/Access/Register", request);

            if (response.IsSuccessStatusCode)
            {
                return new RegisterResponseDTO
                {
                    isSuccess = true,
                    Code = null,
                };
            } else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return new RegisterResponseDTO
                {
                    isSuccess = false,
                    Code = responseContent.Trim('"'),
                };
            }
        }
         
        public async Task<ForgotResponseDTO> ForgotPassword(ResetPasswordModel request)
        {
            var response = await _httpClient.PostAsJsonAsync("/Access/ResetPassword", request);

            if (response.IsSuccessStatusCode)
            {
                return new ForgotResponseDTO
                {
                    isSuccess = true,
                    Code = null,
                    StatusCode = response.StatusCode.ToString(),
                };
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return new ForgotResponseDTO
                {
                    isSuccess = false,
                    Code = responseContent,
                    StatusCode = response.StatusCode.ToString(),
                };
            }
        }

        public async Task<ChangePasswordDTO> ChangePassword(ChangePasswordModel request, string token)
        {

            var payload = new ChangePasswordModel()
            {
                CurrentPassword = HashPassword(request.CurrentPassword.Trim()),
                NewHashPassword = HashPassword(request.NewHashPassword.Trim())
            };

            var r = new HttpRequestMessage(HttpMethod.Post, "/Access/ChangePassword");

            r.Content = JsonContent.Create(payload);

            if (!string.IsNullOrEmpty(token))
            {
                var formatToken = token.Trim('"');
                r.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", formatToken);
            }

            var response = await _httpClient.SendAsync(r);

            if (response.IsSuccessStatusCode)
            {
                return new ChangePasswordDTO
                {
                    isSuccess = true,
                    Code = null,
                    StatusCode = response.StatusCode.ToString(),
                };
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return new ChangePasswordDTO
                {
                    isSuccess = false,
                    Code = responseContent,
                    StatusCode = response.StatusCode.ToString(),
                };
            }
        }
    }
}
