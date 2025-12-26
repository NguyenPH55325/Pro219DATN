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
    [Route("Product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        ProductRepository productRepository;

        public ProductController()
        {
            productRepository = new ProductRepository();
        }

        [HttpGet("GetAllProducts")]
        public async Task<ActionResult<List<DAL.Repository.ProductRepository.ProductDetailDto>>> GetAllProducts(
            [FromQuery] int? page = null,
            [FromQuery] int? pageSize = null,
            [FromQuery] int? brandId = null,
            [FromQuery] int? sizeId = null,
            [FromQuery] int? colorId = null,
            [FromQuery] string? sortOrder = null)
        {
            try
            {
                var result = await productRepository.GetAllProducts(page, pageSize, brandId, sizeId, colorId, sortOrder);
                if (result == null)
                {
                    return Ok(new List<DAL.Repository.ProductRepository.ProductDetailDto>());
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("GetAllProductsInCategory/{categoryId}")]
        public async Task<ActionResult<List<DAL.Repository.ProductRepository.ProductDetailDto>>> GetAllProductsInCategory(
            int categoryId, 
            [FromQuery] int? page = null, 
            [FromQuery] int? pageSize = null,
            [FromQuery] int? brandId = null,
            [FromQuery] int? sizeId = null,
            [FromQuery] int? colorId = null,
            [FromQuery] string? sortOrder = null)
        {
            try
            {
                var result = await productRepository.GetAllProductsInCategory(categoryId, page, pageSize, brandId, sizeId, colorId, sortOrder);
                if (result == null)
                {
                    return Ok(new List<DAL.Repository.ProductRepository.ProductDetailDto>());
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("GetAllProductByKeyWord/{keyWord}")]
        public async Task<ActionResult<List<DAL.Repository.ProductRepository.ProductDetailDto>>> GetAllProductByKeyWord(
            string keyWord, 
            [FromQuery] int? page = null, 
            [FromQuery] int? pageSize = null,
            [FromQuery] int? brandId = null,
            [FromQuery] int? sizeId = null,
            [FromQuery] int? colorId = null,
            [FromQuery] string? sortOrder = null)
        {
            try
            {
                var result = await productRepository.GetAllProductByKeyWord(keyWord, page, pageSize, brandId, sizeId, colorId, sortOrder);
                if (result == null)
                {
                    return Ok(new List<DAL.Repository.ProductRepository.ProductDetailDto>());
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("SearchCombineProduct")]
        public async Task<ActionResult<List<SearchCombineProductDto>>> SearchCombineProduct(
            [FromQuery] int? page = null,
            [FromQuery] int? pageSize = null,
            [FromQuery] int? brandId = null,
            [FromQuery] int? sizeId = null,
            [FromQuery] int? colorId = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] bool? getDeleted = null)
        {
            try
            {
                var result = await productRepository.SearchCombineProduct(page, pageSize, brandId, sizeId, colorId, sortOrder, getDeleted);
                if (result == null)
                {
                    return Ok(new List<SearchCombineProductDto>());
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<List<Product>>> GetAllProducts(string? keyword = null, int? categoryId = null, int? brandId = null)
        {
            try
            {
                var result = await productRepository.GetAllProducts(keyword, categoryId, brandId);
                if (result == null)
                {
                    return Ok(new List<Product>());
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            try
            {
                var result = await productRepository.GetProductById(id);
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
        public async Task<ActionResult<Product>> AddProduct([FromBody] Product product)
        {
            try
            {
                if (product == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                var result = await productRepository.AddProduct(product);
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
        public async Task<ActionResult<Product>> UpdateProduct([FromBody] ProductUpdateDTO productDTO)
        {
            try
            {
                if (productDTO == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                var product = new Product
                {
                    Id = productDTO.Id,
                    CategoryId = productDTO.CategoryId,
                    BrandId = productDTO.BrandId,
                    SaleId = productDTO.SaleId,
                    Name = productDTO.Name,
                    Description = productDTO.Description,
                    BasePrice = productDTO.BasePrice,
                    Status = productDTO.Status,
                    UpdateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    UpdateAt = DateTime.Now,
                    Delete = productDTO.Delete,
                    DeleteAt = productDTO.Delete == true ? DateTime.Now : null
                };

                var result = await productRepository.UpdateProduct(product);
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
        public async Task<ActionResult<Product>> DeleteProduct(int id)
        {
            try
            {
                // Check if product exists first
                var product = await productRepository.GetProductById(id);
                if (product == null)
                {
                    return NotFound(Constant.ErrorCode.DataNotFound);
                }

                var updateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await productRepository.DeleteProduct(id, updateBy);
                if (result == null)
                {
                    // Product exists but cannot be deleted (likely in active order)
                    return BadRequest(Constant.ErrorCode.ProductInActiveOrder);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("product-show")]
        public async Task<IActionResult> GetTopNewest()
        {
            var products = await productRepository.GetTopProductAsync();

            if (products == null || !products.Any())
                return NoContent();

            var result = products.Select(p => new ProductShowDto
            {
                Id = p.Id,
                ProductName = p.Name,
                Price = p.BasePrice,

                Images = p.ProductImages
                    .Where(img => img.Delete != true && img.ProductVariantId == null)
                    .Select(img => img.ImageUrl).ToList(),

                Colors = p.ProductVariants
                    .Where(v => v.Color != null)
                    .Select(v => new ColorDto
                    {
                        Name = v.Color.Name,
                        HexCode = v.Color.HexCode
                    })
                    .Distinct()
                    .ToList()
            });

            return Ok(result);
        }

        [HttpGet("best-seller")]
        public async Task<IActionResult> GetBestSeller()
        {
            var products = await productRepository.GetTopBestSellerAsync();

            if (products == null || !products.Any())
                return NoContent();

            var result = products.Select(p => new ProductShowDto
            {
                Id = p.Id,
                ProductName = p.Name,
                Price = p.BasePrice,
                Images = p.ProductImages
                    .Where(img => img.Delete != true)
                    .Select(img => img.ImageUrl)
                    .ToList(),
                Colors = p.ProductVariants
                    .Where(v => v.Color != null)
                    .Select(v => new ColorDto
                    {
                        Name = v.Color.Name,
                        HexCode = v.Color.HexCode
                    })
                    .Distinct()
                    .ToList()
            }).ToList();

            return Ok(result);
        }

        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetProductDetail(int id)
        {
            var product = await productRepository.GetProductDetailAsync(id); 
            if (product == null) return NotFound();

            var activeVariants = product.ProductVariants
                .Where(v => v.Delete != true && v.IsActive == true)
                .ToList();

            var result = new ProductDetailDto
            {
                Id = product.Id,
                ProductName = product.Name,
                BasePrice = product.BasePrice,
                Description = product.Description,
                Images = product.ProductImages
                            .Where(i => i.Delete != true)
                            .Select(i => i.ImageUrl).ToList(),

                Variants = activeVariants.Select(v => new ProductVariantDto
                {
                    VariantId = v.Id,
                    ColorId = v.ColorId ?? 1,
                    SizeId = v.SizeId ?? 1,
                    ColorName = v.Color?.Name ?? "N/A",
                    HexCode = v.Color?.HexCode ?? "#000",
                    SizeName = v.Size?.Name ?? "FreeSize",
                    StockQuantity = v.StockQuantity,
                    VariantPrice = v.Price 
                }).ToList(),

                UniqueColors = activeVariants
                    .Where(v => v.Color != null)
                    .GroupBy(v => v.Color.Id) 
                    .Select(g => new ColorDto
                    {
                        Id = g.Key,
                        Name = g.First().Color.Name,
                        HexCode = g.First().Color.HexCode
                    }).ToList(),

                UniqueSizes = activeVariants
                    .Where(v => v.Size != null)
                    .GroupBy(v => v.Size.Id)
                    .Select(g => new SizeDto
                    {
                        Id = g.Key,
                        Name = g.First().Size.Name,
                    }).ToList(),
            };

            return Ok(result);
        }

        [HttpGet("get-new-products")]
        public async Task<ActionResult<List<DAL.Repository.ProductRepository.ProductDetailDto>>> GetNewProducts()
        {
            try
            {
                var result = await productRepository.GetAllAndDetailOptimized();
                if (result == null)
                {
                    return BadRequest();
                }
                var order = result.OrderByDescending(p => p.CreateAt).ToList().Take(8);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("get-favourite-products")]
        public async Task<ActionResult<List<DAL.Repository.ProductRepository.ProductDetailDto>>> GetFavouriteProducts()
        {
            try
            {
                var result = await productRepository.GetAllAndDetailOptimized();
                if (result == null)
                {
                    return BadRequest();
                }
                var order = result.OrderByDescending(p => p.ReviewCount).ToList().Take(8);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("get-detail/{id}")]
        public async Task<ActionResult<List<DAL.Repository.ProductRepository.ProductDetailDto>>> GetDetail(int id)
        {
            try
            {
                var result = await productRepository.GetDetail(id);
                if (result == null)
                {
                    return NotFound();
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

