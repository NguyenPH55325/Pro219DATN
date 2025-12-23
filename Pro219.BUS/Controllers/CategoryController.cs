using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pro219.API.DTOs;
using Pro219.DAL.Models;
using Pro219.DAL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Pro219.API.Controllers
{
    [Route("Category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        CategoryRepository categoryRepository;

        public CategoryController()
        {
            categoryRepository = new CategoryRepository();
        }

        [HttpGet("GetAllCategories")]
        public async Task<ActionResult<List<CategoryDTO>>> GetAllCategories(string keyword)
        {
            try
            {
                List<Category> result = await categoryRepository.GetAllCategories(keyword);
                if (result == null)
                {
                    return NoContent();
                }
                
                var categoryDTOs = result.Select(c => ConvertToSimpleCategoryDTO(c)).ToList();
                return Ok(categoryDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpPost("Add")]
        public async Task<ActionResult<Category>> AddCategory([FromBody] Category category)
        {
            try
            {
                if (category == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                var result = await categoryRepository.AddCategory(category);
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
        public async Task<ActionResult<Category>> UpdateCategory([FromBody] CategoryUpdateDTO categoryDTO)
        {
            try
            {
                if (categoryDTO == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                var category = new Category
                {
                    Id = categoryDTO.Id,
                    ParentCategoryId = categoryDTO.ParentCategoryId,
                    Name = categoryDTO.Name,
                    Description = categoryDTO.Description,
                    Status = categoryDTO.Status,
                    UpdateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    UpdateAt = DateTime.Now,
                    Delete = categoryDTO.Delete,
                    DeleteAt = categoryDTO.Delete == true ? DateTime.Now : null
                };

                var result = await categoryRepository.UpdateCategory(category);
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

        [HttpGet("GetAllParentCategories")]
        public async Task<ActionResult<List<CategoryDTO>>> GetAllParentCategories()
        {
            try
            {
                var result = await categoryRepository.GetAllParentCategories();
                if (result == null)
                {
                    return Ok(new List<CategoryDTO>());
                }

                var categoryDTOs = result.Select(c => ConvertToSimpleCategoryDTO(c)).ToList();
                return Ok(categoryDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("GetSubCategoriesByParentId/{parentId}")]
        public async Task<ActionResult<List<CategoryDTO>>> GetSubCategoriesByParentId(int parentId)
        {
            try
            {
                var result = await categoryRepository.GetAllSubCategoriesByParentId(parentId);
                if (result == null)
                {
                    return Ok(new List<CategoryDTO>());
                }

                var categoryDTOs = result.Select(c => ConvertToSimpleCategoryDTO(c)).ToList();
                return Ok(categoryDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("GetCategoryById/{id}")]
        public async Task<ActionResult<CategoryDTO>> GetCategoryById(int id)
        {
            try
            {
                var result = await categoryRepository.GetCategoryById(id);
                if (result == null)
                {
                    return NotFound(Constant.ErrorCode.DataNotFound);
                }
                
                var categoryDTO = ConvertToSimpleCategoryDTO(result);
                return Ok(categoryDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("GetAllCategoriesWithSubCategories")]
        public async Task<ActionResult<List<CategoryDTO>>> GetAllCategoriesWithSubCategories()
        {
            try
            {
                var allCategories = await categoryRepository.GetAllCategories(null);
                if (allCategories == null || allCategories.Count == 0)
                {
                    return Ok(new List<CategoryDTO>());
                }

                var parentCategories = allCategories.Where(c => c.ParentCategoryId == null).ToList();
                var categoryDTOs = parentCategories.Select(c => ConvertToCategoryDTO(c, allCategories)).ToList();

                return Ok(categoryDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        private CategoryDTO ConvertToSimpleCategoryDTO(Category category)
        {
            return new CategoryDTO
            {
                Id = category.Id,
                ParentCategoryId = category.ParentCategoryId,
                UpdateBy = category.UpdateBy,
                Name = category.Name,
                Description = category.Description,
                Status = category.Status,
                isParent = false,
                SubCategory = null
            };
        }

        private CategoryDTO ConvertToCategoryDTO(Category category, List<Category> allCategories)
        {
            var subCategories = allCategories.Where(c => c.ParentCategoryId == category.Id).ToList();
            var hasSubCategories = subCategories.Any();

            return new CategoryDTO
            {
                Id = category.Id,
                ParentCategoryId = category.ParentCategoryId,
                UpdateBy = category.UpdateBy,
                Name = category.Name,
                Description = category.Description,
                Status = category.Status,
                isParent = hasSubCategories,
                SubCategory = hasSubCategories 
                    ? subCategories.Select(c => ConvertToCategoryDTO(c, allCategories)).ToList() 
                    : null
            };
        }

        [HttpDelete("Delete/{id}")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<ActionResult<Category>> DeleteCategory(int id)
        {
            try
            {
                var updateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await categoryRepository.DeleteCategory(id, updateBy);
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
