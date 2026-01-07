using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pro219.API.DTOs;
using Pro219.DAL.Models;
using Pro219.DAL.Repository;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Pro219.API.Controllers
{
    [Route("DiscountCode")]
    [ApiController]
    public class DiscountCodeController : ControllerBase
    {
        DiscountCodeRepository discountCodeRepository;

        public DiscountCodeController()
        {
            discountCodeRepository = new DiscountCodeRepository();
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<List<DiscountCode>>> GetAllDiscountCodes(
            [FromQuery] string? code = null,
            [FromQuery] string? discountType = null,
            [FromQuery] byte? type = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await discountCodeRepository.GetAllDiscountCodes(code, discountType, type, startDate, endDate);
                if (result == null)
                {
                    return Ok(new List<DiscountCode>());
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("GetAllNow")]
        public async Task<ActionResult<List<DiscountCode>>> GetAllDiscountCodeNow(
            [FromQuery] string? code = null,
            [FromQuery] string? discountType = null,
            [FromQuery] byte? type = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await discountCodeRepository.GetAllDiscountCodes(code, discountType, type, startDate, endDate);
                if (result == null)
                {
                    return Ok(new List<DiscountCode>());
                }

                var filter = result.Where(x => x.EndDate >= DateTime.Now && x.StartDate <= DateTime.Now).ToList();

                if (filter == null)
                {
                    return Ok(new List<DiscountCode>());
                }

                return Ok(filter);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("ApplyDiscountCodeValue")]
        public async Task<ActionResult<ApplyDiscountCodeDTO>> ApplyDiscountCodeValue(string code, decimal totalAmount, decimal shippingFee)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.SerialNumber)?.Value;

                if (userIdClaim == null)
                {
                    return BadRequest("Chỉ áp dụng cho khách hàng đã đăng nhập");
                }

                var discountCode = await discountCodeRepository.GetDiscountCodeByCode(code);
                if (discountCode == null)
                {
                    return NotFound(Constant.ErrorCode.DataNotFound);
                }

                int userTimeUsed = await discountCodeRepository.GetUserTimeUsed(code, int.Parse(userIdClaim));

                if (discountCode.IsReusable == false && userTimeUsed >= 1)
                {
                    return BadRequest("Mã giảm giá không thể sử dụng lại");
                }
                else if (userTimeUsed >= discountCode.MaxUsage)
                {
                    return BadRequest("Mã giảm giá đã hết lượt sử dụng");
                }

                if (discountCode.StartDate > DateTime.Now)
                {
                    return BadRequest("Mã giảm giá chưa khả dụng");
                }
                if (discountCode.EndDate < DateTime.Now)
                {
                    return BadRequest("Mã giảm giá đã hết hạn");
                }
                if (discountCode.IsActive == false)
                {
                    return BadRequest("Mã giảm giá chưa khả dụng");
                }
                if (discountCode.MaxUsage != null && discountCode.UsageCount >= discountCode.MaxUsage)
                {
                    return BadRequest("Mã giảm giá đã hết lượt sử dụng");

                }
                if (discountCode.MinOrderValue != null && totalAmount < discountCode.MinOrderValue)
                {
                    return BadRequest("Đơn hàng không đủ giá trị để sử dụng mã giảm giá");
                }

                ApplyDiscountCodeDTO applyDiscountCodeDTO = new ApplyDiscountCodeDTO
                {
                    ShippingDiscount = 0,
                    DiscountAmount = 0,
                    Type = discountCode.Type
                };

                if (discountCode.Status == 1) // Percent
                {

                    if (applyDiscountCodeDTO.Type == 1)
                    {
                        var discount = totalAmount * (discountCode.Value / 100);
                        if(discountCode.MaxDiscountAmount != null && discountCode.MaxDiscountAmount < discount)
                        {
                            discount = discountCode.MaxDiscountAmount ?? discount;
                        }
                        applyDiscountCodeDTO.DiscountAmount = discount > totalAmount ? totalAmount : discount;
                    }
                    else
                    if (applyDiscountCodeDTO.Type == 2)
                    {
                        var discount = shippingFee * (discountCode.Value / 100);
                        applyDiscountCodeDTO.ShippingDiscount = discount > shippingFee ? shippingFee : discount;
                    }

                    return Ok(applyDiscountCodeDTO);
                }
                else if (discountCode.Status == 2) // Fixed Amount
                {

                    if (applyDiscountCodeDTO.Type == 1)
                    {
                        applyDiscountCodeDTO.DiscountAmount = discountCode.Value > totalAmount ? totalAmount : discountCode.Value;
                    }
                    else
                   if (applyDiscountCodeDTO.Type == 2)
                    {
                        applyDiscountCodeDTO.ShippingDiscount = discountCode.Value > shippingFee ? shippingFee : discountCode.Value;
                    }

                    return Ok(applyDiscountCodeDTO);
                }
                else
                {
                    return BadRequest("Mã giảm giá không hợp lệ");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult<DiscountCode>> GetDiscountCodeById(int id)
        {
            try
            {
                var result = await discountCodeRepository.GetDiscountCodeById(id);
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

        [HttpGet("GetByCode/{code}")]
        public async Task<ActionResult<DiscountCode>> GetDiscountCodeByCode(string code)
        {
            try
            {
                var result = await discountCodeRepository.GetDiscountCodeByCode(code);
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

        [HttpPost("Add")]
        public async Task<ActionResult<DiscountCode>> AddDiscountCode([FromBody] DiscountCode discountCode)
        {
            try
            {
                if (discountCode == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                var hasCode = await discountCodeRepository.GetDiscountCodeByCode(discountCode.Code);

                if (hasCode != null)
                {
                    return BadRequest("Đã có mã giảm giá này");
                }

                var result = await discountCodeRepository.AddDiscountCode(discountCode);
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
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<ActionResult<DiscountCode>> UpdateDiscountCode([FromBody] DiscountCodeUpdateDTO discountCodeDTO)
        {
            try
            {
                if (discountCodeDTO == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                var discountCode = new DiscountCode
                {
                    DiscountId = discountCodeDTO.DiscountId,
                    Code = discountCodeDTO.Code,
                    DiscountType = discountCodeDTO.DiscountType,
                    Value = discountCodeDTO.Value,
                    MinOrderValue = discountCodeDTO.MinOrderValue,
                    StartDate = discountCodeDTO.StartDate,
                    EndDate = discountCodeDTO.EndDate,
                    IsActive = discountCodeDTO.IsActive,
                    Status = discountCodeDTO.Status,
                    UpdateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    UpdateAt = DateTime.Now,
                    Delete = discountCodeDTO.Delete,
                    Type = discountCodeDTO.Type,
                    DeleteAt = discountCodeDTO.Delete == true ? DateTime.Now : null
                };

                var result = await discountCodeRepository.UpdateDiscountCode(discountCode);
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
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<ActionResult<DiscountCode>> DeleteDiscountCode(int id)
        {
            try
            {
                var updateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await discountCodeRepository.DeleteDiscountCode(id, updateBy);
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
    }
}



