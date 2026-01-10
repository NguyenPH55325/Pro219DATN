using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pro219.API.DTOs;
using Pro219.DAL.Models;
using Pro219.DAL.Repository;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Pro219.API.Controllers
{
    [Route("Address")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        AddressRepository addressRepository;
        private readonly IConfiguration _configuration;
        public AddressController(IConfiguration configuration)

        {
            _configuration = configuration;
            addressRepository = new AddressRepository();
        }

        [HttpPost("Add")]
        public async Task<ActionResult<Address>> AddAddress([FromBody] AddressDTO addressDTO)
        {
            try
            {
                if (addressDTO == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                var address = new Address
                {
                    CustomerId = addressDTO.CustomerId,
                    FullName = addressDTO.FullName,
                    Phone = addressDTO.Phone,
                    Ward = addressDTO.Street,
                    Province = addressDTO.City,
                    District = addressDTO.District,
                    ProvinceName = addressDTO.CityName,
                    DistrictName = addressDTO.DistrictName,
                    WardName = addressDTO.StreetName,
                    OtherInfo = addressDTO.OtherInfo,
                    IsDefault = addressDTO.IsDefault,
                    CreateAt = DateTime.Now,
                    Delete = false,
                    Status = 1
                };

                var result = await addressRepository.AddAddress(address);
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

        [HttpPut("Update")]
        [Authorize(Roles = "Admin,Manager,Staff,Customer")]
        public async Task<ActionResult<Address>> UpdateAddress([FromBody] AddressUpdateDTO addressDTO)
        {
            try
            {
                if (addressDTO == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                var address = new Address
                {
                    Id = addressDTO.Id,
                    CustomerId = addressDTO.CustomerId,
                    FullName = addressDTO.FullName,
                    Phone = addressDTO.Phone,
                    Ward = addressDTO.Street,
                    Province = addressDTO.City,
                    District = addressDTO.District,
                    ProvinceName = addressDTO.CityName,
                    DistrictName = addressDTO.DistrictName,
                    WardName = addressDTO.StreetName,
                    OtherInfo = addressDTO.OtherInfo,
                    IsDefault = addressDTO.IsDefault,
                    UpdateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    UpdateAt = DateTime.Now,
                    Delete = addressDTO.Delete,
                    DeleteAt = addressDTO.Delete == true ? DateTime.Now : null
                };

                var result = await addressRepository.UpdateAddress(address);
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
        public async Task<ActionResult<List<Address>>> GetByCustomerId(int customerId)
        {
            try
            {
                var result = await addressRepository.GetByCustomerId(customerId);
                if (result == null)
                {
                    return Ok(new List<Address>());
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult<Address>> GetById(int id)
        {
            try
            {
                var result = await addressRepository.GetById(id);
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

        [HttpGet("GetAll")]
        public async Task<ActionResult<List<Address>>> GetAllAddresses()
        {
            try
            {
                var result = await addressRepository.GetAllAddresses();
                if (result == null)
                {
                    return Ok(new List<Address>());
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpDelete("Delete/{id}")]
        [Authorize(Roles = "Admin,Manager,Staff,Customer")]
        public async Task<ActionResult<Address>> DeleteAddress(int id)
        {
            try
            {
                var updateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await addressRepository.DeleteAddress(id, updateBy);
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

        [HttpGet("GetAllProvinces")]
        public async Task<ActionResult<List<ProvinceDTO>>> GetAllProvinces()
        {

            HttpClient client = new HttpClient();

            string token = _configuration["GHN:Token"];


            string url = "https://online-gateway.ghn.vn/shiip/public-api/master-data/province";
            try
            {
                client.DefaultRequestHeaders.Add("token", token);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var root = JsonSerializer.Deserialize<RootResponseForProvince>(responseBody, options);
                if (root == null || root.Data == null)
                {
                    return StatusCode(500, "Lỗi khi kéo data =)");
                }
                return Ok(root.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi khi kéo data =)");
            }



        }

        [HttpGet("GetAllDistricts")]
        public async Task<ActionResult<List<DistrictDTO>>> GetAllDistricts()
        {
            HttpClient client = new HttpClient();

            string token = _configuration["GHN:Token"];


            string url = "https://online-gateway.ghn.vn/shiip/public-api/master-data/district";
            try
            {
                client.DefaultRequestHeaders.Add("token", token);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var root = JsonSerializer.Deserialize<RootResponseForDistrict>(responseBody, options);
                if (root == null || root.Data == null)
                {
                    return StatusCode(500, "Lỗi khi kéo data =)");
                }
                return Ok(root.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi khi kéo data =)");
            }
        }

        [HttpGet("GetAllDistrictsByProvinceId/{provinceId}")]
        public async Task<ActionResult<List<DistrictDTO>>> GetAllDistrictsByProvinceId(string provinceId)
        {
            var token = _configuration["GHN:Token"];
            var url = $"https://online-gateway.ghn.vn/shiip/public-api/master-data/district?province_id={provinceId}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("token", token);

            try
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var root = JsonSerializer.Deserialize<RootResponseForDistrict>(responseBody, options);
                if (root == null || root.Data == null)
                {
                    return StatusCode(500, "Lỗi khi kéo data =)");
                }

                return Ok(root.Data);
            }
            catch (Exception)
            {
                return StatusCode(500, "Lỗi khi kéo data =)");
            }
        }


        [HttpGet("GetAllWardByDistrictCode/{districtCode}")]
        public async Task<ActionResult<List<WardDTO>>> GetAllWardByDistrictCode(string districtCode)
        {
            var token = _configuration["GHN:Token"];
            var url = $"https://online-gateway.ghn.vn/shiip/public-api/master-data/ward?district_id={districtCode}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("token", token);

            try
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var root = JsonSerializer.Deserialize<RootResponseForWard>(result, options);
                if (root == null || root.Data == null)
                {
                    return StatusCode(500, "Lỗi khi kéo data =)");
                }

                return Ok(root.Data);
            }
            catch (Exception)
            {
                return StatusCode(500, "Lỗi khi kéo data =)");
            }
        }

        [HttpPost("CalculateFee")]
        public async Task<IActionResult> CalculateFee(
            int to_district_id,
            string to_ward_code,
            [FromBody] List<FeeItemDTO> items)
        {
            int from_district_id = 3440;
            string from_ward_code = "13007";
            var token = _configuration["GHN:Token"];
            var shopId = _configuration["GHN:ShopId"];
            FeeItemDTO defaultItem = new FeeItemDTO
            {
                Name = "Hàng hóa",
                Quantity = 2,
                Length = 30,
                Width = 40,
                Height = 5,
                Weight = 400
            };
            if (items == null || items.Count == 0)
            {
                items = new List<FeeItemDTO> { defaultItem };
            }

            int totalWeight = 0;
            
            foreach (var item in items) { 
               totalWeight += item.Weight*item.Quantity;
            }

            var url = "https://online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/fee";

            var body = new
            {
                service_type_id = 2,
                from_district_id,
                from_ward_code,
                to_district_id,
                to_ward_code,
                length = 30,
                width = 40,
                height = 5,
                weight = totalWeight,
                insurance_value = 0,
                coupon = (string)null,
                items = items
            };

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Token", token);
            client.DefaultRequestHeaders.Add("ShopId", shopId);

            var jsonBody = JsonSerializer.Serialize(body);
            using var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();
                return Ok(JsonDocument.Parse(result));
            }
            catch (Exception)
            {
                return StatusCode(500, "Lỗi khi tính phí vận chuyển =)");
            }
        }




        public class RootResponseForProvince
        {
            public int Code { get; set; }
            public string Message { get; set; }
            public List<ProvinceDTO> Data { get; set; }
        }
        public class RootResponseForDistrict
        {
            public int Code { get; set; }
            public string Message { get; set; }
            public List<DistrictDTO> Data { get; set; }
        }
        public class RootResponseForWard
        {
            public int Code { get; set; }
            public string Message { get; set; }
            public List<WardDTO> Data { get; set; }
        }
    }
}


