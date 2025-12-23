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
    [Route("Color")]
    [ApiController]
    public class ColorController : ControllerBase
    {
        ColorRepository colorRepository;

        public ColorController()
        {
            colorRepository = new ColorRepository();
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<List<Color>>> GetAllColors(string? keyword)
        {
            try
            {
                var result = await colorRepository.GetAllColors(keyword);
                if (result == null)
                {
                    return Ok(new List<Color>());
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult<Color>> GetColorById(int id)
        {
            try
            {
                var result = await colorRepository.GetColorById(id);
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
        public async Task<ActionResult<Color>> AddColor([FromBody] Color color)
        {
            try
            {
                if (color == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                var result = await colorRepository.AddColor(color);
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
        public async Task<ActionResult<Color>> UpdateColor([FromBody] ColorUpdateDTO colorDTO)
        {
            try
            {
                if (colorDTO == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                var color = new Color
                {
                    Id = colorDTO.Id,
                    Name = colorDTO.Name,
                    HexCode = colorDTO.HexCode,
                    Status = colorDTO.Status,
                    UpdateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    UpdateAt = DateTime.Now,
                    Delete = colorDTO.Delete,
                    DeleteAt = colorDTO.Delete == true ? DateTime.Now : null
                };

                var result = await colorRepository.UpdateColor(color);
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
        public async Task<ActionResult<Color>> DeleteColor(int id)
        {
            try
            {
                var updateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await colorRepository.DeleteColor(id, updateBy);
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



