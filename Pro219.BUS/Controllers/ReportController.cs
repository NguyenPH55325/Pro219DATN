using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pro219.API.DTOs;
using Pro219.DAL.Context;
using Pro219.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pro219.API.Controllers
{
    [Route("Report")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        [HttpGet("Statistics")]
        public async Task<ActionResult<ReportResponseDTO>> GetStatistics(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string splitData = "month")
        {
            if (startDate == default || endDate == default)
            {
                return BadRequest("Khoảng thời gian không hợp lệ");
            }

            if (endDate < startDate)
            {
                return BadRequest("Thời gian kết thúc phải lớn hơn bắt đầu");
            }

            splitData = (splitData ?? "month").Trim().ToLower();
            if (splitData != "month" && splitData != "day" && splitData != "year")
            {
                splitData = "month";
            }

            using var context = new ClothesDbContext();

            var orders = await context.Orders
                .Include(o => o.OrderItems.Where(oi => oi.Delete != true))
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                            .ThenInclude(p => p.Category)
                                .ThenInclude(c => c.ParentCategory)
                .Where(o => o.Delete != true &&
                            o.OrderDate.Date >= startDate.Date &&
                            o.OrderDate.Date <= endDate.Date)
                .ToListAsync();

            var customers = await context.Customers
                .Where(c => c.Delete != true &&
                            c.CreateAt >= startDate &&
                            c.CreateAt <= endDate)
                .ToListAsync();

            var response = BuildReport(startDate, endDate, splitData, orders, customers);
            return Ok(response);
        }

        private static ReportResponseDTO BuildReport(
            DateTime startDate,
            DateTime endDate,
            string splitData,
            List<Order> orders,
            List<Customer> customers)
        {
            var successfulOrders = orders.Where(IsSuccessOrder).ToList();
            var canceledOrders = orders.Where(IsCanceledOrder).ToList();

            var response = new ReportResponseDTO
            {
                StartDate = startDate,
                EndDate = endDate,
                SplitData = splitData,
                TotalOrder = orders.Count,
                OrderSuccess = successfulOrders.Count,
                OrderCanceled = canceledOrders.Count,
                TotalRevenue = successfulOrders.Sum(o => o.FinalAmount),
                TotalUnitSold = successfulOrders.SelectMany(o => o.OrderItems).Sum(oi => oi.Quantity),
                TotalCustomer = orders.Select(o => o.CustomerId).Where(id => id.HasValue).Distinct().Count(),
                NewCustomerRegistered = customers.Count,
                ProductSold = BuildProductSold(successfulOrders, 10),
                ParentCategorySold = BuildParentCategorySold(successfulOrders, 10)
            };

            var cursor = splitData == "day"
                ? startDate.Date
                : splitData == "year"
                    ? new DateTime(startDate.Year, 1, 1)
                    : new DateTime(startDate.Year, startDate.Month, 1);

            while (cursor <= endDate.Date)
            {
                var periodEnd = splitData == "day"
                    ? cursor
                    : splitData == "year"
                        ? new DateTime(cursor.Year, 12, 31)
                        : new DateTime(cursor.Year, cursor.Month, DateTime.DaysInMonth(cursor.Year, cursor.Month));

                if (periodEnd > endDate.Date)
                {
                    periodEnd = endDate.Date;
                }

                var periodOrders = orders
                    .Where(o => o.OrderDate.Date >= cursor && o.OrderDate.Date <= periodEnd)
                    .ToList();

                var periodCustomers = customers
                    .Where(c => c.CreateAt >= cursor && c.CreateAt <= periodEnd)
                    .ToList();

                response.DataMonth.Add(BuildPeriod(cursor, periodEnd, splitData, periodOrders, periodCustomers));
                cursor = splitData == "day" 
                    ? cursor.AddDays(1) 
                    : splitData == "year"
                        ? cursor.AddYears(1)
                        : cursor.AddMonths(1);
            }

            return response;
        }

        private static ReportPeriodDTO BuildPeriod(
            DateTime periodStart,
            DateTime periodEnd,
            string splitData,
            List<Order> orders,
            List<Customer> customers)
        {
            var successfulOrders = orders.Where(IsSuccessOrder).ToList();
            var canceledOrders = orders.Where(IsCanceledOrder).ToList();

            return new ReportPeriodDTO
            {
                Date = splitData == "day"
                    ? periodStart.ToString("dd/MM/yyyy")
                    : splitData == "year"
                        ? periodStart.ToString("yyyy")
                        : periodStart.ToString("MM/yyyy"),
                TotalOrder = orders.Count,
                OrderSuccess = successfulOrders.Count,
                OrderCanceled = canceledOrders.Count,
                TotalRevenue = successfulOrders.Sum(o => o.FinalAmount),
                TotalUnitSold = successfulOrders.SelectMany(o => o.OrderItems).Sum(oi => oi.Quantity),
                TotalCustomer = orders.Select(o => o.CustomerId).Where(id => id.HasValue).Distinct().Count(),
                NewCustomerRegistered = customers.Count,
                ProductSold = BuildProductSold(successfulOrders, 10)
            };
        }

        private static List<ReportProductSoldDTO> BuildProductSold(IEnumerable<Order> successfulOrders, int topCount = 10)
        {
            return successfulOrders
                .SelectMany(o => o.OrderItems.Select(oi => new
                {
                    OrderItem = oi,
                    Product = oi.ProductVariant?.Product,
                    Category = oi.ProductVariant?.Product?.Category
                }))
                .Where(x => x.Product != null)
                .GroupBy(x => x.Product!.Id)
                .Select(g =>
                {
                    var first = g.First();
                    return new ReportProductSoldDTO
                    {
                        ProductName = first.Product!.Name,
                        CategoryId = first.Product.CategoryId,
                        CategoryName = first.Category?.Name,
                        Unit = g.Sum(x => x.OrderItem.Quantity),
                        Revenue = g.Sum(x => x.OrderItem.Subtotal)
                    };
                })
                .OrderByDescending(x => x.Unit)
                .Take(topCount)
                .ToList();
        }

        private static List<ReportCategorySoldDTO> BuildParentCategorySold(IEnumerable<Order> successfulOrders, int topCount = 10)
        {
            var categoryData = successfulOrders
                .SelectMany(o => o.OrderItems.Select(oi => new
                {
                    OrderItem = oi,
                    Category = oi.ProductVariant?.Product?.Category
                }))
                .Where(x => x.Category != null)
                .Select(x => new
                {
                    RootCategory = GetRootCategory(x.Category!),
                    x.OrderItem
                })
                .ToList();

            return categoryData
                .GroupBy(x => x.RootCategory.Id)
                .Select(g =>
                {
                    var root = g.First().RootCategory;
                    return new ReportCategorySoldDTO
                    {
                        CategoryId = root.Id,
                        CategoryName = root.Name,
                        ParentCategoryId = root.ParentCategoryId,
                        ParentCategoryName = root.ParentCategory?.Name,
                        Unit = g.Sum(x => x.OrderItem.Quantity),
                        Value = g.Sum(x => x.OrderItem.Subtotal)
                    };
                })
                .OrderByDescending(x => x.Unit)
                .Take(topCount)
                .ToList();
        }

        private static Category GetRootCategory(Category category)
        {
            var current = category;
            while (current.ParentCategory != null)
            {
                current = current.ParentCategory;
            }

            return current;
        }

        private static bool IsSuccessOrder(Order order)
        {
            return order.PaymentStatus == Constant.OrderStatus.PaymentCompleted ||
                   order.OrderStatus == Constant.OrderStatus.OrderStatusDone ||
                   order.OrderStatus == Constant.OrderStatus.OrderStatusShippingDone;
        }

        private static bool IsCanceledOrder(Order order)
        {
            return order.OrderStatus == Constant.OrderStatus.OrderStatusCanceledByUser ||
                   order.PaymentStatus == Constant.OrderStatus.PaymentCancelled;
        }
    }
}
