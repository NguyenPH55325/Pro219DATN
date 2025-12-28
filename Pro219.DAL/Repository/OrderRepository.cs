using Microsoft.EntityFrameworkCore;
using Pro219.DAL.Context;
using Pro219.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace Pro219.DAL.Repository
{
    public class OrderRepository
    {
        private readonly ClothesDbContext _context;

        public OrderRepository()
        {
            _context = new ClothesDbContext();
        }

        public OrderRepository(ClothesDbContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetAllOrders(string? keyword = null)
        {
            try
            {
                var query = _context.Orders
                    .Include(o => o.ShippingAddress)
                    .Where(x => x.Delete != true)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(x => x.OrderCode.Contains(keyword) || (x.ShippingAddress != null && x.ShippingAddress.FullName.Contains(keyword)) || x.ShippingAddress != null && x.ShippingAddress.Phone.Contains(keyword));
                }

                var orders = await query.ToListAsync();
                // Break navigation cycles for serialization
                foreach (var order in orders)
                {
                    if (order.ShippingAddress != null)
                    {
                        order.ShippingAddress.Orders = null;
                    }
                }
                return orders;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<Order>> GetAllSaleCounter()
        {
            try
            {
                var orders = await _context.Orders.Where(x => x.IsOrderPOS == true && x.Delete != true && x.Status == 99).ToListAsync();

                if(orders.Count == 0)
                {
                    return new List<Order>();
                }

                return orders;

            } catch (Exception) { 
                return new List<Order>();
            }
        }

        public async Task<Order> GetOrderById(int id)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order != null && order.Delete == true)
                    return null;
                return order;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Order> GetOrderDetailById(int id) 
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.ShippingAddress)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.ProductVariant)
                            .ThenInclude(pv => pv.Product)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.ProductVariant)
                            .ThenInclude(pv => pv.Color)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.ProductVariant)
                            .ThenInclude(pv => pv.Size)
                    .FirstOrDefaultAsync(x => x.OrderId == id && x.Delete != true);

                if (order != null && order.Delete == true)
                    return null;

                return order;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Order> GetOrderDetailByCode(string code)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.ShippingAddress)
                    .Include(o => o.Customer)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.ProductVariant)
                            .ThenInclude(pv => pv.Product)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.ProductVariant)
                            .ThenInclude(pv => pv.Color)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.ProductVariant)
                            .ThenInclude(pv => pv.Size)
                    .FirstOrDefaultAsync(x => x.OrderCode == code && x.Delete != true);

                if (order != null && order.Delete == true)
                    return null;

                return order;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Order> GetOrderByOrderCode(string orderCode)
        {
            try
            {
                var order = await _context.Orders
                    .FirstOrDefaultAsync(x => x.OrderCode == orderCode && x.Delete != true);
                return order;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<Order>> GetOrdersByCustomerId(int customerId)
        {
            try
            {
                var orders = await _context.Orders
                    .Where(x => x.CustomerId == customerId && x.Delete != true)
                    .ToListAsync();
                return orders;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Order> AddOrder(Order order)
        {
            try
            {
                order.CreateAt = DateTime.Now;
                order.LastUpdate = DateTime.Now;
                order.Delete = false;
                var addedOrder = _context.Orders.Add(order).Entity;
                await _context.SaveChangesAsync();
                return addedOrder;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Order> UpdateOrder(Order order)
        {
            try
            {
                
                var existingOrder = await _context.Orders.FindAsync(order.OrderId);

                if (existingOrder == null || existingOrder.Delete == true) return null;

                existingOrder.CustomerId = order.CustomerId;
                existingOrder.ShippingAddressId = order.ShippingAddressId;
                existingOrder.DiscountId = order.DiscountId;
                existingOrder.PaymentMethodId = order.PaymentMethodId;
                existingOrder.OrderCode = order.OrderCode;
                existingOrder.OrderDate = order.OrderDate;
                existingOrder.TotalAmount = order.TotalAmount;
                existingOrder.DiscountAmount = order.DiscountAmount;
                existingOrder.FinalAmount = order.FinalAmount;
                existingOrder.PaymentStatus = order.PaymentStatus;
                existingOrder.OrderStatus = order.OrderStatus;
                existingOrder.Status = order.Status;
                existingOrder.StatusHistory = order.StatusHistory;
                existingOrder.Notes = order.Notes;
                existingOrder.LastUpdate = DateTime.Now;
                if (!string.IsNullOrEmpty(order.UpdateBy))
                {
                    existingOrder.UpdateBy = order.UpdateBy;
                }

                var updatedOrder = _context.Orders.Update(existingOrder).Entity;
                await _context.SaveChangesAsync();
                return updatedOrder;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Order> DeleteOrder(int id, string? updateBy = null)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);

                if (order == null) return null;

                order.Delete = true;
                order.DeleteAt = DateTime.Now;
                order.LastUpdate = DateTime.Now;
                if (!string.IsNullOrEmpty(updateBy))
                {
                    order.UpdateBy = updateBy;
                }

                var updatedOrder = _context.Orders.Update(order).Entity;
                await _context.SaveChangesAsync();
                return updatedOrder;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Order> GetOrderByIdForInvoice(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.ShippingAddress)
                    .Include(o => o.DiscountCode)
                    .Include(o => o.PaymentMethod)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.ProductVariant)
                            .ThenInclude(pv => pv.Product)
                                .ThenInclude(p => p.Brand)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.ProductVariant)
                            .ThenInclude(pv => pv.Size)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.ProductVariant)
                            .ThenInclude(pv => pv.Color)
                    .Where(x => x.OrderId == id && x.Delete != true && x.PaymentStatus== "Đã thanh toán")
                    .FirstOrDefaultAsync();

                return order;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<Order>> GetAllByKeyword(string keyword)
        {
            try
            {
                if(string.IsNullOrEmpty(keyword)) {
                    return await _context.Orders.ToListAsync();
                }

                var orders = await (from o in _context.Orders
                                   join sp in _context.Addresses
                                   on o.ShippingAddressId equals sp.Id
                                   where ((o.OrderCode.Contains(keyword) || sp.Phone.Contains(keyword)) && o.Delete != true)
                                   select o).ToListAsync();

                if (orders.Count > 0 )
                {
                    return orders;
                }
                return new List<Order>();
            } catch (Exception)
            {
                return new List<Order>();
            }
        }
    }
}

