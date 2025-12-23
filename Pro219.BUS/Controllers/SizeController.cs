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
    [Route("Size")]
    [ApiController]
    public class SizeController : ControllerBase
    {
        SizeRepository sizeRepository;

        public SizeController()
        {
            sizeRepository = new SizeRepository();
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<List<Size>>> GetAllSizes(string keywordName)
        {
            try
            {
                var result = await sizeRepository.GetAllSizes(keywordName);
                if (result == null)
                {
                    return Ok(new List<Size>());
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult<Size>> GetSizeById(int id)
        {
            try
            {
                var result = await sizeRepository.GetSizeById(id);
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
        public async Task<ActionResult<Size>> AddSize([FromBody] Size size)
        {
            try
            {
                if (size == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                var result = await sizeRepository.AddSize(size);
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
        public async Task<ActionResult<Size>> UpdateSize([FromBody] SizeUpdateDTO sizeDTO)
        {
            try
            {
                if (sizeDTO == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                var size = new Size
                {
                    Id = sizeDTO.Id,
                    Name = sizeDTO.Name,
                    Status = sizeDTO.Status,
                    UpdateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    UpdateAt = DateTime.Now,
                    Delete = sizeDTO.Delete,
                    DeleteAt = sizeDTO.Delete == true ? DateTime.Now : null
                };

                var result = await sizeRepository.UpdateSize(size);
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
        public async Task<ActionResult<Size>> DeleteSize(int id)
        {
            try
            {
                var updateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await sizeRepository.DeleteSize(id, updateBy);
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



