using Order.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Service
{
    public interface IOrderService
    {
        // fetches all orders with an optional status filter
        Task<IEnumerable<OrderSummary>> GetOrdersAsync(string? status = null);
        
        Task<OrderDetail> GetOrderByIdAsync(Guid orderId);

        // asynchronously update status of an order by its ID
        Task<bool> UpdateOrderStatusAsync(Guid orderId, string status);

        // Asynchronously creates a new order based on the incoming request data
        Task<OrderDetail> CreateOrderAsync(CreateOrderRequest request);
    }
}
