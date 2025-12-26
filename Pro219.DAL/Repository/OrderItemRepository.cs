using Microsoft.EntityFrameworkCore;
using Pro219.DAL.Context;
using Pro219.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pro219.DAL.Repository
{
    public class OrderItemRepository
    {
        private readonly ClothesDbContext _context;

        public OrderItemRepository()
        {
            _context = new ClothesDbContext();
        }

        public OrderItemRepository(ClothesDbContext context)
        {
            _context = context;
        }

        public async Task<List<OrderItem>> GetAllOrderItems()
        {
            try
            {
                var orderItems = await _context.OrderItems
                    .Where(x => x.Delete != true)
                    .ToListAsync();
                return orderItems;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<OrderItem> GetOrderItemById(int id)
        {
            try
            {
                var orderItem = await _context.OrderItems.FindAsync(id);
                if (orderItem != null && orderItem.Delete == true)
                    return null;
                return orderItem;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<OrderItem>> GetOrderItemsByOrderId(int orderId)
        {
            try
            {
                var orderItems = await _context.OrderItems
                    .Where(x => x.OrderId == orderId && x.Delete != true)
                    .ToListAsync();
                return orderItems;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<OrderItem>> GetOrderItemsByProductVariantId(int productVariantId)
        {
            try
            {
                var orderItems = await _context.OrderItems
                    .Where(x => x.ProductVariantId == productVariantId && x.Delete != true)
                    .ToListAsync();
                return orderItems;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<OrderItem> AddOrderItem(OrderItem orderItem)
        {
            try
            {
                orderItem.CreateAt = DateTime.Now;
                orderItem.Delete = false;
                var addedOrderItem = _context.OrderItems.Add(orderItem).Entity;
                await _context.SaveChangesAsync();
                return addedOrderItem;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<OrderItem>> AddOrderItem(List<OrderItem> listOrderItem)
        {
            try
            {

                foreach (var orderItem in listOrderItem)
                {
                    orderItem.CreateAt = DateTime.Now;
                    orderItem.Delete = false;
                    var addedOrderItem = _context.OrderItems.Add(orderItem).Entity;
                    await _context.SaveChangesAsync();
                }
                return listOrderItem;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<OrderItem> UpdateOrderItem(OrderItem orderItem)
        {
            try
            {
                var existingOrderItem = await _context.OrderItems.FindAsync(orderItem.OrderItemId);

                if (existingOrderItem == null || existingOrderItem.Delete == true) return null;

                existingOrderItem.OrderId = orderItem.OrderId;
                existingOrderItem.ProductVariantId = orderItem.ProductVariantId;
                existingOrderItem.Quantity = orderItem.Quantity;
                existingOrderItem.UnitPrice = orderItem.UnitPrice;
                existingOrderItem.Subtotal = orderItem.Subtotal;
                existingOrderItem.UpdateAt = DateTime.Now;
                existingOrderItem.Status = orderItem.Status;
                if (!string.IsNullOrEmpty(orderItem.UpdateBy))
                {
                    existingOrderItem.UpdateBy = orderItem.UpdateBy;
                }

                var updatedOrderItem = _context.OrderItems.Update(existingOrderItem).Entity;
                await _context.SaveChangesAsync();
                return updatedOrderItem;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<OrderItem> DeleteOrderItem(int id, string? updateBy = null)
        {
            try
            {
                var orderItem = await _context.OrderItems.FindAsync(id);

                if (orderItem == null) return null;

                orderItem.Delete = true;
                orderItem.DeleteAt = DateTime.Now;
                orderItem.UpdateAt = DateTime.Now;
                if (!string.IsNullOrEmpty(updateBy))
                {
                    orderItem.UpdateBy = updateBy;
                }

                var updatedOrderItem = _context.OrderItems.Update(orderItem).Entity;
                await _context.SaveChangesAsync();
                return updatedOrderItem;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

