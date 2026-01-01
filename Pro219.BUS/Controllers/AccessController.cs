using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Pro219.API.DTOs;
using Pro219.API.Utilities;
using Pro219.DAL.Models;
using Pro219.DAL.Repository;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Pro219.API.Controllers
{
    [Route("Access")]
    [ApiController]
    public class AccessController : ControllerBase
    {
        CustomerRepository _customerRepository;
        CartRepository cartRepository;
        UserRepository _userRepository;

        private readonly IConfiguration _configuration;
        private readonly TimeZoneInfo _gmtPlus7 = TimeZoneInfo.CreateCustomTimeZone("GMT+7", TimeSpan.FromHours(7), "GMT+7", "GMT+7");

        public AccessController(IConfiguration configuration, CustomerRepository customerRepository, UserRepository userRepository)
        {
            _configuration = configuration;
            _customerRepository = customerRepository;
            _userRepository = userRepository;
            cartRepository = new CartRepository();
        }

        [HttpPost("LoginCustomer")]
        public IActionResult Login([FromBody] LoginModel loginModel)
        {
            string username = loginModel.Username;
            string passwordHash = loginModel.PasswordHash;
            _customerRepository = new CustomerRepository();
            Customer customer = _customerRepository.GetByKeyAndPassword(username, passwordHash).Result;
            if (customer != null)
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, loginModel.Username),
                  new Claim(ClaimTypes.SerialNumber, customer.Id.ToString()),
                new Claim(ClaimTypes.Role, "Customer"),
                 new Claim(ClaimTypes.Email, customer.Email),
                  new Claim(ClaimTypes.Name, customer.FullName),
                     new Claim(ClaimTypes.MobilePhone, customer.PhoneNumber)
            };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var gmtPlus7Now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _gmtPlus7);
                var expirationGmt7 = gmtPlus7Now.AddMinutes(50);
                var expirationUtc = TimeZoneInfo.ConvertTimeToUtc(expirationGmt7, _gmtPlus7);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Issuer"],
                    claims: claims,
                    expires: expirationUtc,
                    signingCredentials: creds
                );

                return Ok(new LoginResponseDTO
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    Expiration = expirationGmt7,
                    LoginSuccess = true,
                    FirstLogin = customer.LastLogin == null,
                });
            }
            else
            {
                return StatusCode(403, Constant.ErrorCode.Unauthorized);
            }
        }

        [HttpPost("LoginStaff")]
        public IActionResult LoginStaff([FromBody] LoginModel loginModel)
        {
            string username = loginModel.Username;
            string passwordHash = loginModel.PasswordHash;
            _userRepository = new UserRepository();
            User user = _userRepository.GetByKeyAndPassword(username, passwordHash).Result;
            if (user != null)
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, loginModel.Username),
                   new Claim(ClaimTypes.SerialNumber, user.UserID.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
                 new Claim(ClaimTypes.Name, user.UserName)
            };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var gmtPlus7Now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _gmtPlus7);
                var expirationGmt7 = gmtPlus7Now.AddMinutes(180);
                var expirationUtc = TimeZoneInfo.ConvertTimeToUtc(expirationGmt7, _gmtPlus7);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Issuer"],
                    claims: claims,
                    expires: expirationUtc,
                    signingCredentials: creds
                );

                return Ok(new LoginResponseDTO
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    Expiration = expirationGmt7,
                    LoginSuccess = true
                });
            }
            else
            {
                return StatusCode(403, Constant.ErrorCode.Unauthorized);
            }
        }

        [HttpGet("Check")]
        [Authorize]
        public IActionResult GetSecureData()
        {
            try
            {
                DateTime? expirationTime = null;
                bool isExpired = false;

                var authHeader = Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    var token = authHeader.Replace("Bearer ", "");
                    if (!string.IsNullOrEmpty(token))
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var jsonToken = handler.ReadJwtToken(token);
                        var expirationUtc = jsonToken.ValidTo;
                        expirationTime = TimeZoneInfo.ConvertTimeFromUtc(expirationUtc, _gmtPlus7);
                        isExpired = expirationUtc < DateTime.UtcNow;

                        if (isExpired)
                        {
                            return StatusCode(403, Constant.ErrorCode.TokenExpired);
                        }
                    }
                }

                var userInfo = new
                {
                    id = User.FindFirst(ClaimTypes.SerialNumber)?.Value,
                    username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    role = User.FindFirst(ClaimTypes.Role)?.Value,
                    email = User.FindFirst(ClaimTypes.Email)?.Value,
                    fullName = User.FindFirst(ClaimTypes.Name)?.Value,
                    phoneNumber = User.FindFirst(ClaimTypes.MobilePhone)?.Value,
                    expirationTime = expirationTime,
                    isExpired = isExpired
                };

                return Ok(userInfo);
            }
            catch (SecurityTokenExpiredException)
            {
                return StatusCode(403, Constant.ErrorCode.TokenExpired);
            }
            catch
            {
                return StatusCode(403, Constant.ErrorCode.InvalidToken);
            }
        }

        [HttpPost("Register")]
        public async Task<ActionResult<bool>> RegisterUser([FromBody] RegisterModel registerModel)
        {
            Customer cus = new Customer();
            cus.PhoneNumber = registerModel.PhoneNumber;
            cus.Email = registerModel.Email;
            cus.FullName = registerModel.FullName;
            cus.DateOfBirth = registerModel.DateOfBirth ?? DateTime.Now;
            cus.CreateAt = DateTime.Now;
            cus.PasswordHash = registerModel.PasswordHash;
            cus.Status = 1;
            cus.LastLogin = DateTime.Now;
            _customerRepository = new CustomerRepository();
            var user = _customerRepository.FindCustomerExistByKeyWord(cus.Email).Result;
            if (user == null)
            {
                Customer a = _customerRepository.AddCustomer(cus).Result;
                var customerCart = new Cart
                {
                    CustomerId = a.Id,
                    Status = 1,
                    CreateAt = DateTime.Now,
                    Delete = false
                };
                customerCart = await cartRepository.AddCart(customerCart);
                return Ok(a);
            }
            else
            {
                return BadRequest(Constant.ErrorCode.EmailOrPhoneAlreadyExit);
            }
        }

        [HttpPost("ResetPassword")]
        public async Task<ActionResult<bool>> ResetPassword([FromBody] ResetPasswordModel resetPasswordModel)
        {
            if (string.IsNullOrEmpty(resetPasswordModel.Email))
            {
                return BadRequest(Constant.ErrorCode.EmailOrPhoneRequired);
            }

            _customerRepository = new CustomerRepository();
            var customer = await _customerRepository.FindCustomerByEmailAndPhone(resetPasswordModel.Email, string.Empty);

            if (customer == null)
            {
                return BadRequest(Constant.ErrorCode.EmailOrPhoneNotFound);
            }

            UtilityFunc utilityFunc = new UtilityFunc();
            string newPassword = utilityFunc.GenerateRandomString(10);
            //string newPassword = "User@12345";

            customer.PasswordHash = utilityFunc.HashPassword(newPassword);
            customer.LastLogin = null;

            var updatedCustomer = await _customerRepository.UpdateCustomer(customer);
            StringBuilder sb = new StringBuilder();
            sb.Append($"Kính chào quý khách hàng <b>{customer.FullName}</b><br><br>Mật khẩu truy cập vào tài khoản Adam Store đã được thay đổi thành <b>{newPassword} </b> Vui lòng truy cập trang web và thay đổi mật khẩu, xin trân trọng cám ơn!<br><br>Adam Store");
            if (updatedCustomer == null)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
            else
            {
                bool re = await utilityFunc.SendEmailToAddress(customer.Email, customer.FullName, "Khôi phục mật khẩu tài khoản Adam Store", "", sb.ToString());
            } 
                

            return Ok(true);
        }

        [HttpPost("ChangePassword")]
        public async Task<ActionResult<bool>> ChangePassword([FromBody] ChangePasswordDTO changePasswordDTO)
        {
            string userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userName == null)
            {

                return BadRequest(Constant.ErrorCode.CustomerNotFound);
            }
            else
            {
                _customerRepository = new CustomerRepository();
                var customer = await _customerRepository.FindCustomerByEmailAndPhone(userName, userName);

                if (customer == null)
                {
                    return NotFound(Constant.ErrorCode.CustomerNotFoundWidthEmailOrPhone);
                }
                UtilityFunc utilityFunc = new UtilityFunc();
                //string hashedPassword = utilityFunc.GenerateRandomString(6);
                customer.PasswordHash = changePasswordDTO.NewHashPassword;
                if (customer.LastLogin == null)
                {
                    customer.LastLogin = DateTime.Now;
                }
                var updatedCustomer = await _customerRepository.UpdateCustomer(customer);

                if (updatedCustomer == null)
                {
                    return StatusCode(500, Constant.ErrorCode.OtherError);
                }

                return Ok(true);
            }


        }
    }

}
