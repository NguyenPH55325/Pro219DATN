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
    [Route("Wishlist")]
    [ApiController]
    public class WishlistController : ControllerBase
    {
        WishlistRepository wishlistRepository;

        public WishlistController()
        {
            wishlistRepository = new WishlistRepository();
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<List<Wishlist>>> GetAllWishlists()
        {
            try
            {
                var result = await wishlistRepository.GetAllWishlists();
                if (result == null)
                {
                    return Ok(new List<Wishlist>());
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult<Wishlist>> GetWishlistById(int id)
        {
            try
            {
                var result = await wishlistRepository.GetWishlistById(id);
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
        public async Task<ActionResult<List<Wishlist>>> GetWishlistsByCustomerId(int customerId)
        {
            try
            {
                var result = await wishlistRepository.GetWishlistsByCustomerId(customerId);
                if (result == null)
                {
                    return Ok(new List<Wishlist>());
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("GetByCustomerAndProduct/{customerId}/{productId}")]
        public async Task<ActionResult<Wishlist>> GetWishlistByCustomerAndProduct(int customerId, int productId)
        {
            try
            {
                var result = await wishlistRepository.GetWishlistByCustomerAndProduct(customerId, productId);
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
        public async Task<ActionResult<Wishlist>> AddWishlist([FromBody] Wishlist wishlist)
        {
            try
            {
                if (wishlist == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                var result = await wishlistRepository.AddWishlist(wishlist);
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
        public async Task<ActionResult<Wishlist>> UpdateWishlist([FromBody] WishlistUpdateDTO wishlistDTO)
        {
            try
            {
                if (wishlistDTO == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                var wishlist = new Wishlist
                {
                    Id = wishlistDTO.Id,
                    CustomerId = wishlistDTO.CustomerId,
                    ProductVariantId = wishlistDTO.ProductVariantId,
                    Status = wishlistDTO.Status,
                    UpdateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    UpdateAt = DateTime.Now,
                    Delete = wishlistDTO.Delete,
                    DeleteAt = wishlistDTO.Delete == true ? DateTime.Now : null
                };

                var result = await wishlistRepository.UpdateWishlist(wishlist);
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
        public async Task<ActionResult<Wishlist>> DeleteWishlist(int id)
        {
            try
            {
                var updateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await wishlistRepository.DeleteWishlist(id, updateBy);
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



