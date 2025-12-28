using Microsoft.AspNetCore.Mvc;
using Pro219.API.DTOs;
using Pro219.DAL.Models;
using Pro219.DAL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Pro219.API.Controllers
{
    [Route("OrderItem")]
    [ApiController]
    public class OrderItemController : ControllerBase
    {
        OrderItemRepository orderItemRepository;
        ProductVariantRepository productVariantRepository;

        public OrderItemController()
        {
            orderItemRepository = new OrderItemRepository();
            productVariantRepository = new ProductVariantRepository();
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<List<OrderItem>>> GetAllOrderItems()
        {
            try
            {
                var result = await orderItemRepository.GetAllOrderItems();
                if (result == null)
                {
                    return Ok(new List<OrderItem>());
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult<OrderItem>> GetOrderItemById(int id)
        {
            try
            {
                var result = await orderItemRepository.GetOrderItemById(id);
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

        [HttpGet("GetByOrderId/{orderId}")]
        public async Task<ActionResult<List<OrderItem>>> GetOrderItemsByOrderId(int orderId)
        {
            try
            {
                var result = await orderItemRepository.GetOrderItemsByOrderId(orderId);
                if (result == null)
                {
                    return Ok(new List<OrderItem>());
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("GetByProductVariantId/{productVariantId}")]
        public async Task<ActionResult<List<OrderItem>>> GetOrderItemsByProductVariantId(int productVariantId)
        {
            try
            {
                var result = await orderItemRepository.GetOrderItemsByProductVariantId(productVariantId);
                if (result == null)
                {
                    return Ok(new List<OrderItem>());
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpPost("Add")]
        public async Task<ActionResult<OrderItem>> AddOrderItem([FromBody] OrderItem orderItem)
        {
            try
            {
                if (orderItem == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                var result = await orderItemRepository.AddOrderItem(orderItem);
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

        [HttpPost("AddWithDTO")]
        public async Task<ActionResult<OrderItem>> AddOrderItemWithDTO([FromBody] OrderItemDTO orderItemDTO)
        {
            try
            {
                if (orderItemDTO == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                if (orderItemDTO.OrderId <= 0 || orderItemDTO.ProductVariantId <= 0 || orderItemDTO.Quantity <= 0)
                {
                    return BadRequest(Constant.ErrorCode.InvalidData);
                }

                if (!orderItemDTO.UnitPrice.HasValue || orderItemDTO.UnitPrice.Value <= 0)
                {
                    return BadRequest(Constant.ErrorCode.InvalidData);
                }

                var productVariant = await productVariantRepository.GetProductVariantById(orderItemDTO.ProductVariantId);
                if (productVariant == null)
                {
                    return NotFound(Constant.ErrorCode.DataNotFound);
                }

                var existingOrderItems = await orderItemRepository.GetOrderItemsByOrderId(orderItemDTO.OrderId);
                if (existingOrderItems == null)
                {
                    existingOrderItems = new List<OrderItem>();
                }

                var existingOrderItem = existingOrderItems.FirstOrDefault(x => x.ProductVariantId == orderItemDTO.ProductVariantId);

                if (existingOrderItem != null)
                {

                    int newTotalQuantity = existingOrderItem.Quantity + orderItemDTO.Quantity;

                    if (newTotalQuantity > productVariant.StockQuantity)
                    {
                        return BadRequest(Constant.ErrorCode.OutOfStock);
                    }

                    existingOrderItem.Quantity = newTotalQuantity;
                    existingOrderItem.Subtotal = existingOrderItem.UnitPrice * newTotalQuantity;
                    existingOrderItem.UpdateBy = "System";

                    var result = await orderItemRepository.UpdateOrderItem(existingOrderItem);
                    if (result == null)
                    {
                        return StatusCode(500, Constant.ErrorCode.DatabaseError);
                    }

                    return Ok(result);
                }
                else
                {
                    if (orderItemDTO.Quantity > productVariant.StockQuantity)
                    {
                        return BadRequest(Constant.ErrorCode.OutOfStock);
                    }

                    var orderItem = new OrderItem
                    {
                        OrderId = orderItemDTO.OrderId,
                        ProductVariantId = orderItemDTO.ProductVariantId,
                        Quantity = orderItemDTO.Quantity,
                        UnitPrice = orderItemDTO.UnitPrice.Value,
                        Subtotal = orderItemDTO.Subtotal ?? (orderItemDTO.UnitPrice.Value * orderItemDTO.Quantity),
                        UpdateBy = "System",
                        Status = 1
                    };

                    var result = await orderItemRepository.AddOrderItem(orderItem);
                    if (result == null)
                    {
                        return StatusCode(500, Constant.ErrorCode.DatabaseError);
                    }

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }
        

        [HttpPost("AddList")]
        public async Task<ActionResult<List<OrderItem>>> AddOrderItem([FromBody] List<OrderItem> listOrderItem)
        {
            try
            {
                if (listOrderItem == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                var result = await orderItemRepository.AddOrderItem(listOrderItem);
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
        public async Task<ActionResult<OrderItem>> UpdateOrderItem([FromBody] OrderItem orderItem)
        {
            try
            {
                if (orderItem == null)
                {
                    return BadRequest(Constant.ErrorCode.DataRequired);
                }

                orderItem.UpdateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var result = await orderItemRepository.UpdateOrderItem(orderItem);
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
        public async Task<ActionResult<OrderItem>> DeleteOrderItem(int id)
        {
            try
            {
                var updateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await orderItemRepository.DeleteOrderItem(id, updateBy);
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



