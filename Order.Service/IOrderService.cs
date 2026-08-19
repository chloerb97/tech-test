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
    }
}
