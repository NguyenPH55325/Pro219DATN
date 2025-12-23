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
    [Route("CartItem")]
    [ApiController]
    public class CartItemController : ControllerBase
    {
        CartItemRepository cartItemRepository;
        ProductVariantRepository productVariantRepository;
        ProductRepository productRepository;
        ProductImageRepository productImageRepository;
        SizeRepository sizeRepository;
        ColorRepository colorRepository;

        public CartItemController()
        {
            cartItemRepository = new CartItemRepository();
            productVariantRepository = new ProductVariantRepository();
            productRepository = new ProductRepository();
            productImageRepository = new ProductImageRepository();
            sizeRepository = new SizeRepository();
            colorRepository = new ColorRepository();
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<List<CartItem>>> GetAllCartItems()
        {
            try
            {
                var result = await cartItemRepository.GetAllCartItems();
                if (result == null)
                {
                    return Ok(new List<CartItem>());
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult<CartItem>> GetCartItemById(int id)
        {
            try
            {
                var result = await cartItemRepository.GetCartItemById(id);
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

        [HttpGet("GetByCartId/{cartId}")]
        public async Task<ActionResult<List<CartItem>>> GetCartItemsByCartId(int cartId)
        {
            try
            {
                var result = await cartItemRepository.GetCartItemsByCartId(cartId);
                if (result == null)
                {
                    return Ok(new List<CartItem>());
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpPost("Add")]
        public async Task<ActionResult<CartItem>> AddCartItem([FromBody] CartItem cartItem)
        {
            try
            {
                if (cartItem == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                var result = await cartItemRepository.AddCartItem(cartItem);
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
        public async Task<ActionResult<CartItem>> UpdateCartItem([FromBody] CartItemUpdateDTO cartItemDTO)
        {
            try
            {
                if (cartItemDTO == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                var cartItem = new CartItem
                {
                    Id = cartItemDTO.Id,
                    CartId = cartItemDTO.CartId,
                    VariantId = cartItemDTO.VariantId,
                    Quantity = cartItemDTO.Quantity,
                    UnitPrice = cartItemDTO.UnitPrice,
                    IsSelectedForCheckout = cartItemDTO.IsSelectedForCheckout,
                    Status = cartItemDTO.Status,
                    UpdateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    UpdateAt = DateTime.Now,
                    Delete = cartItemDTO.Delete,
                    DeleteAt = cartItemDTO.Delete == true ? DateTime.Now : null
                };

                var result = await cartItemRepository.UpdateCartItem(cartItem);
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
        [Authorize(Roles = "Admin,Manager,Staff,Customer")]
        public async Task<ActionResult<CartItem>> DeleteCartItem(int id)
        {
            try
            {
                var updateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await cartItemRepository.DeleteCartItem(id, updateBy);
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

        [HttpGet("get-all-cart-item-with-detail/{cartId}")]
        public async Task<ActionResult<IEnumerable<CartItemWithProductDTO>>> GetCartItemDetailBycartId(int cartId)
        {
            try
            {
                var cartItems = await cartItemRepository.GetCartItemsByCartId(cartId);

                if (cartItems == null || !cartItems.Any())
                {
                    return Ok(Enumerable.Empty<CartItemWithProductDTO>());
                }

                var allProductVariants = await productVariantRepository.GetAllProductVariants();
                var allProducts = await productRepository.GetAllProducts();
                var allProductImages = await productImageRepository.GetAllProductImages();
                var allSizes = await sizeRepository.GetAllSizes(null);
                var allColors = await colorRepository.GetAllColors(null);

                var variantMap = allProductVariants.ToDictionary(pv => pv.Id);
                var productMap = allProducts.ToDictionary(p => p.Id);
                var colorMap = allColors.ToDictionary(p => p.Id);
                var sizeMap  = allSizes.ToDictionary(p => p.Id);

                var resultDtoList = cartItems.Select(cartItem =>
                {
                    variantMap.TryGetValue(cartItem.VariantId, out var productVariant);

                    Product product = null;
                    Size size = null;
                    Color color = null;
                    if (productVariant != null)
                    {
                        productMap.TryGetValue(productVariant.ProductId, out product);
                        colorMap.TryGetValue(productVariant.ColorId ?? -1, out color);
                        sizeMap.TryGetValue(productVariant.SizeId ?? -1, out size);
                    }

                    var image = allProductImages.Where(pi => pi.ProductVariantId == productVariant?.Id && pi.ProductId == product?.Id).FirstOrDefault();
                    return new CartItemWithProductDTO
                    {
                        Id = cartItem.Id,
                        VariantId = cartItem.VariantId,
                        CartId = cartItem.CartId,
                        productName = product?.Name ?? "",
                        Quantity = cartItem.Quantity,
                        ColorName = color?.Name ?? "",
                        SizeName = size?.Name ?? "",
                        ImageUrl = image != null ? image.ImageUrl : "/Assets/Images/default-image.png",
                        UnitPrice = cartItem.UnitPrice ?? product?.BasePrice ?? 0,
                    };
                }).ToList();

                return Ok(resultDtoList);

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("get-all-cart-item-by-guest")]
        public async Task<ActionResult<IEnumerable<CartItemWithProductDTO>>> GetAllCartItemByGuest([FromBody] List<AddCartModel> addCarts)
        {
            try
            {
                var allProductVariants = await productVariantRepository.GetAllProductVariants();
                var allProducts = await productRepository.GetAllProducts();
                var allProductImages = await productImageRepository.GetAllProductImages();
                var allSizes = await sizeRepository.GetAllSizes(null);
                var allColors = await colorRepository.GetAllColors(null);

                var variantMap = allProductVariants.ToDictionary(pv => pv.Id);
                var productMap = allProducts.ToDictionary(p => p.Id);
                var colorMap = allColors.ToDictionary(p => p.Id);
                var sizeMap = allSizes.ToDictionary(p => p.Id);

                var resultDtoList = addCarts.Select(cartItem =>
                {
                    variantMap.TryGetValue(cartItem.VariantId, out var productVariant);

                    Product product = null;
                    Size size = null;
                    Color color = null;
                    if (productVariant != null)
                    {
                        productMap.TryGetValue(productVariant.ProductId, out product);
                        colorMap.TryGetValue(productVariant.ColorId ?? -1, out color);
                        sizeMap.TryGetValue(productVariant.SizeId ?? -1, out size);
                    }

                    var image = allProductImages.Where(pi => pi.ProductVariantId == productVariant?.Id && pi.ProductId == product?.Id).FirstOrDefault();
                    return new CartItemWithProductDTO
                    {
                        VariantId = cartItem.VariantId,
                        CartId = -1,
                        productName = product?.Name ?? "",
                        Quantity = cartItem.Quantity,
                        ColorName = color?.Name ?? "",
                        SizeName = size?.Name ?? "",
                        ImageUrl = image != null ? image.ImageUrl : "/Assets/Images/default-image.png",
                        UnitPrice = productVariant?.Price ?? product?.BasePrice ?? 0,
                    };
                }).ToList();

                return Ok(resultDtoList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}



