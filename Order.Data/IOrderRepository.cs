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
    }
}
