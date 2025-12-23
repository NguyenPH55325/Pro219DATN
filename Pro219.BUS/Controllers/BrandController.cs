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
    [Route("Brand")]
    [ApiController]
    public class BrandController : ControllerBase
    {
        BrandRepository brandRepository;

        public BrandController()
        {
            brandRepository = new BrandRepository();
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<List<Brand>>> GetAllBrands(string? keyword)
        {
            try
            {
                var result = await brandRepository.GetAllBrands(keyword);
                if (result == null)
                {
                    return Ok(new List<Brand>());
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult<Brand>> GetBrandById(int id)
        {
            try
            {
                var result = await brandRepository.GetBrandById(id);
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
        public async Task<ActionResult<Brand>> AddBrand([FromBody] Brand brand)
        {
            try
            {
                if (brand == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                var result = await brandRepository.AddBrand(brand);
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
        public async Task<ActionResult<Brand>> UpdateBrand([FromBody] BrandUpdateDTO brandDTO)
        {
            try
            {
                if (brandDTO == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                var brand = new Brand
                {
                    Id = brandDTO.Id,
                    Name = brandDTO.Name,
                    Description = brandDTO.Description,
                    Status = brandDTO.Status,
                    UpdateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    UpdateAt = DateTime.Now,
                    Delete = brandDTO.Delete,
                    DeleteAt = brandDTO.Delete == true ? DateTime.Now : null
                };

                var result = await brandRepository.UpdateBrand(brand);
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
        public async Task<ActionResult<Brand>> DeleteBrand(int id)
        {
            try
            {
                var updateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await brandRepository.DeleteBrand(id, updateBy);
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



