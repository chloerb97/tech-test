using Order.Data;
using Order.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        //updated to accept the optional status parameter and pass it to repository
        public async Task<IEnumerable<OrderSummary>> GetOrdersAsync(string? status = null)
        {
            var orders = await _orderRepository.GetOrdersAsync(status);
            return orders;
        }

        public async Task<OrderDetail> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            return order;
        }
        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, string status)
        {
            return await _orderRepository.UpdateOrderStatusAsync(orderId, status);
        }
        // Asynchronously creates a new order via the repository and returns the created order details
        public async Task<OrderDetail> CreateOrderAsync(CreateOrderRequest request)
        {
            // Calls repository layer to save the new order to the database
            return await _orderRepository.CreateOrderAsync(request);
        }
    
}
}
