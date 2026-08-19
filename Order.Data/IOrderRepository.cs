using Order.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Data
{
    public interface IOrderRepository
    {
        // optional status filter (defaults to null if no filter is requested)
        Task<IEnumerable<OrderSummary>> GetOrdersAsync(string? status = null);

        Task<OrderDetail> GetOrderByIdAsync(Guid orderId);

        // updates order's status in the database by its ID
        Task<bool> UpdateOrderStatusAsync(Guid orderId, string status);

        // Asynchronously creates a new order in the database and returns its detail view
        Task<OrderDetail> CreateOrderAsync(CreateOrderRequest request);
    }
}
