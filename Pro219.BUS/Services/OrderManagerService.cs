using Hangfire;
using Pro219.DAL.Models;
using Pro219.DAL.Repository;
namespace Pro219.API.Services
{
    public class OrderManagerService
    {
        private readonly OrderRepository _orderRepository;
        private readonly OrderItemRepository _orderItemRepository;
        private readonly ProductVariantRepository _productVariantRepository;

        public OrderManagerService(OrderRepository orderRepository, OrderItemRepository orderItemRepository, ProductVariantRepository productVariantRepository)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productVariantRepository = productVariantRepository;
        }

        public async Task CancelExpiredOrderAsync(int orderId)
        {
            var order = await _orderRepository.GetOrderById(orderId);
            if (order != null && order.PaymentStatus == Constant.OrderStatus.PaymentPending && order.PaymentExpiration < DateTime.Now)
            {
                order.OrderStatus = Constant.OrderStatus.OrderStatusPaymentExpired;
                order.Status = Constant.OrderStatus.StatusCanceledByUser;
                order.LastUpdate = DateTime.Now;
                order.PaymentStatus = Constant.OrderStatus.PaymentCancelled;
                await _orderRepository.UpdateOrder(order);
                List<OrderItem> orderItemsList = await _orderItemRepository.GetOrderItemsByOrderId(order.OrderId);
                if (orderItemsList != null && orderItemsList.Any())
                {
                    foreach (var orderItem in orderItemsList)
                    {
                        var re = await _productVariantRepository.IncreaseProductVariantQuantity(orderItem.ProductVariantId, orderItem.Quantity);                           
                    }
                }

            }
        }



        }
}
