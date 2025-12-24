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
    [Route("Cart")]
    [ApiController]
    public class CartController : ControllerBase
    {
        CartRepository cartRepository;
        CartItemRepository cartItemRepository;
        ProductVariantRepository productVariantRepository;


        public CartController()
        {
            cartRepository = new CartRepository();

        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<List<Cart>>> GetAllCarts()
        {
            try
            {
                var result = await cartRepository.GetAllCarts();
                if (result == null)
                {
                    return Ok(new List<Cart>());
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult<Cart>> GetCartById(int id)
        {
            try
            {
                var result = await cartRepository.GetCartById(id);
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
        public async Task<ActionResult<Cart>> GetCartByCustomerId(int customerId)
        {
            try
            {
                var result = await cartRepository.GetCartByCustomerId(customerId);
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
        public async Task<ActionResult<Cart>> AddCart([FromBody] Cart cart)
        {
            try
            {
                if (cart == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                var result = await cartRepository.AddCart(cart);
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

        [HttpPost("UpdateCartFromGuestLogin")]
        public async Task<ActionResult<Cart>> UpdateCartFromGuestLogin([FromBody] List<AddToCartDTO> localCartItems)
        {
            try
            {
                if (localCartItems == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                productVariantRepository = new ProductVariantRepository();
                cartItemRepository = new CartItemRepository();
                var cartRepository = new CartRepository();
                string userId = User.FindFirst(ClaimTypes.SerialNumber)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {

                    foreach (var item in localCartItems)
                    {
                        var productVariant = await productVariantRepository.GetProductVariantById(item.VariantId);
                        if (productVariant == null)
                        {
                            return NotFound(Constant.ErrorCode.DataNotFound);
                        }
                        if (productVariant.StockQuantity < item.Quantity)
                        {
                            return BadRequest(Constant.ErrorCode.OutOfStock);
                        }
                        var customerCart = await cartRepository.GetCartByCustomerId(int.Parse(userId));
                        var cartItem = await cartRepository.GetCartItemByProductVariantId(customerCart.Id, item.VariantId);
                        if (cartItem != null && (cartItem.Quantity + item.Quantity) > productVariant.StockQuantity)
                        {
                            return BadRequest(Constant.ErrorCode.OutOfStock);
                        }
                        if (cartItem != null)
                        {
                            cartItem.Quantity += item.Quantity;
                            cartItem.UpdateAt = DateTime.Now;
                            cartItem.UpdateBy = userId;
                            var updatedCartItem = await cartItemRepository.UpdateCartItem(cartItem);
                            if (updatedCartItem == null)
                            {
                                return BadRequest(Constant.ErrorCode.DatabaseError);
                            }
                        }
                        else
                        {
                            var newCartItem = new CartItem
                            {
                                CartId = customerCart.Id,
                                VariantId = item.VariantId,
                                Quantity = item.Quantity,
                                UnitPrice = productVariant.Price,
                                AddedAt = DateTime.Now,
                                CreateAt = DateTime.Now,
                                UpdateBy = userId,
                                IsSelectedForCheckout = false,
                                Status = 1
                            };
                            var addedCartItem = await cartItemRepository.AddCartItem(newCartItem);
                            if (addedCartItem == null)
                            {
                                return BadRequest(Constant.ErrorCode.DatabaseError);
                            }
                        }
                    }
                    return Ok();


                }
                else
                {
                    return Unauthorized();

                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpPost("AddToCart")]
        public async Task<ActionResult<CartItem>> AddProductToCart([FromBody] AddToCartDTO addToCartDto)
        {
            try
            {
                productVariantRepository = new ProductVariantRepository();
                cartItemRepository = new CartItemRepository();
                var productVariant = await productVariantRepository.GetProductVariantById(addToCartDto.VariantId);
                if (productVariant == null)
                {
                    return NotFound(Constant.ErrorCode.DataNotFound);
                }

                if (productVariant.StockQuantity < addToCartDto.Quantity)
                {
                    return BadRequest(Constant.ErrorCode.OutOfStock);
                }
                string userId = User.FindFirst(ClaimTypes.SerialNumber)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var cartRepository = new CartRepository();
                    var customerCart = await cartRepository.GetCartByCustomerId(int.Parse(userId));
                    var cartItem = await cartRepository.GetCartItemByProductVariantId(customerCart.Id, addToCartDto.VariantId);
                    if (cartItem != null && (cartItem.Quantity + addToCartDto.Quantity) > productVariant.StockQuantity)
                    {
                        return BadRequest(Constant.ErrorCode.OutOfStock);
                    }
                }

                if (addToCartDto == null || addToCartDto.Quantity <= 0)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                if (string.IsNullOrEmpty(userId))
                {
                    var newCartItem = new CartItem
                    {
                        VariantId = addToCartDto.VariantId,
                        Quantity = addToCartDto.Quantity,
                        UnitPrice = productVariant.Price,
                        AddedAt = DateTime.Now,
                        CreateAt = DateTime.Now,
                        UpdateBy = "Guest",
                        Status = 1
                    };
                    return Ok(newCartItem);
                }
                else
                {
                    var customerCart = await cartRepository.GetCartByCustomerId(int.Parse(userId));
                    if (customerCart == null)
                    {
                        return BadRequest(Constant.ErrorCode.DatabaseError);
                    }

                    var cartItem = await cartRepository.GetCartItemByProductVariantId(customerCart.Id, addToCartDto.VariantId);

                    if (cartItem != null)
                    {
                        cartItem.Quantity += addToCartDto.Quantity;
                        cartItem.UpdateAt = DateTime.Now;
                        cartItem.UpdateBy = userId;
                        var updatedCartItem = await cartItemRepository.UpdateCartItem(cartItem);
                        if (updatedCartItem == null)
                        {
                            return StatusCode(500, Constant.ErrorCode.DatabaseError);
                        }
                        return Ok(updatedCartItem);
                    }
                    else
                    {
                        var newCartItem = new CartItem
                        {
                            CartId = customerCart.Id,
                            VariantId = addToCartDto.VariantId,
                            Quantity = addToCartDto.Quantity,
                            UnitPrice = productVariant.Price,
                            AddedAt = DateTime.Now,
                            CreateAt = DateTime.Now,
                            UpdateBy = userId,
                            IsSelectedForCheckout = false,
                            Status = 1
                        };
                        var addedCartItem = await cartItemRepository.AddCartItem(newCartItem);
                        if (addedCartItem == null)
                        {
                            return StatusCode(500, Constant.ErrorCode.DatabaseError);
                        }
                        return Ok(addedCartItem);
                    }

                }
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }


        [HttpPut("Update")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<ActionResult<Cart>> UpdateCart([FromBody] CartUpdateDTO cartDTO)
        {
            try
            {
                if (cartDTO == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                var cart = new Cart
                {
                    Id = cartDTO.Id,
                    CustomerId = cartDTO.CustomerId,
                    SessionId = cartDTO.SessionId,
                    Status = cartDTO.Status,
                    UpdateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    UpdateAt = DateTime.Now,
                    Delete = cartDTO.Delete,
                    DeleteAt = cartDTO.Delete == true ? DateTime.Now : null
                };

                var result = await cartRepository.UpdateCart(cart);
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
        public async Task<ActionResult<Cart>> DeleteCart(int id)
        {
            try
            {
                var updateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await cartRepository.DeleteCart(id, updateBy);
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



