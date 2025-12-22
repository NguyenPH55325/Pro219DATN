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
    [Route("Review")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        ReviewRepository reviewRepository;

        public ReviewController()
        {
            reviewRepository = new ReviewRepository();
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<List<Review>>> GetAllReviews()
        {
            try
            {
                var result = await reviewRepository.GetAllReviews();
                if (result == null)
                {
                    return Ok(new List<Review>());
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult<Review>> GetReviewById(int id)
        {
            try
            {
                var result = await reviewRepository.GetReviewById(id);
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

        [HttpGet("GetByProductId/{productId}")]
        public async Task<ActionResult<List<Review>>> GetReviewsByProductId(int productId)
        {
            try
            {
                var result = await reviewRepository.GetReviewsByProductId(productId);
                if (result == null)
                {
                    return Ok(new List<Review>());
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("GetByCustomerId/{customerId}")]
        public async Task<ActionResult<List<Review>>> GetReviewsByCustomerId(int customerId)
        {
            try
            {
                var result = await reviewRepository.GetReviewsByCustomerId(customerId);
                if (result == null)
                {
                    return Ok(new List<Review>());
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpPost("Add")]
        public async Task<ActionResult<Review>> AddReview([FromBody] ReviewCreateDTO reviewDTO)
        {
            try
            {
                if (reviewDTO == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var review = new Review
                {
                    ProductId = reviewDTO.ProductId,
                    CustomerId = reviewDTO.CustomerId,
                    OrderItemId = reviewDTO.OrderItemId,
                    CustomerName = reviewDTO.CustomerName,
                    Title = reviewDTO.Title,
                    Content = reviewDTO.Content,
                    Overall = reviewDTO.Overall,
                    Status = reviewDTO.Status,
                    UpdateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                };

                var result = await reviewRepository.AddReview(review);
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
        public async Task<ActionResult<Review>> UpdateReview([FromBody] ReviewUpdateDTO reviewDTO)
        {
            try
            {
                if (reviewDTO == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var review = new Review
                {
                    Id = reviewDTO.Id,
                    ProductId = reviewDTO.ProductId,
                    CustomerId = reviewDTO.CustomerId,
                    OrderItemId = reviewDTO.OrderItemId,
                    Title = reviewDTO.Title,
                    Content = reviewDTO.Content,
                    Overall = reviewDTO.Overall,
                    Status = reviewDTO.Status,
                    UpdateBy = updateBy,
                    UpdateAt = DateTime.Now
                };

                var result = await reviewRepository.UpdateReview(review);
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
        public async Task<ActionResult<Review>> DeleteReview(int id)
        {
            try
            {
                var updateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await reviewRepository.DeleteReview(id, updateBy);
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



