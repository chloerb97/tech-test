using Microsoft.EntityFrameworkCore;
using Order.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Order.Data
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderContext _orderContext;

        public OrderRepository(OrderContext orderContext)
        {
            _orderContext = orderContext;
        }

        // optional status filter
        public async Task<IEnumerable<OrderSummary>> GetOrdersAsync(string? status = null)
        {
            // sets up query builder
            var query = _orderContext.Order
                .Include(x => x.Items)
                .Include(x => x.Status)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                // applies optional case-insensitive status filter
                query = query.Where(x => x.Status.Name.ToLower() == status.ToLower());
            }

            // maps entity records to OrderSummary, sorts descending & executes the query
            var orders = await query 
                .Select(x => new OrderSummary
                {
                    Id = new Guid(x.Id),
                    ResellerId = new Guid(x.ResellerId),
                    CustomerId = new Guid(x.CustomerId),
                    StatusId = new Guid(x.StatusId),
                    StatusName = x.Status.Name,
                    ItemCount = x.Items.Count,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    CreatedDate = x.CreatedDate
                })
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return orders;
        }

        public async Task<OrderDetail> GetOrderByIdAsync(Guid orderId)
        {
            var orderIdBytes = orderId.ToByteArray();

            var order = await _orderContext.Order
                .Where(x => _orderContext.Database.IsInMemory() ? x.Id.SequenceEqual(orderIdBytes) : x.Id == orderIdBytes)
                .Select(x => new OrderDetail
                {
                    Id = new Guid(x.Id),
                    ResellerId = new Guid(x.ResellerId),
                    CustomerId = new Guid(x.CustomerId),
                    StatusId = new Guid(x.StatusId),
                    StatusName = x.Status.Name,
                    CreatedDate = x.CreatedDate,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    Items = x.Items.Select(i => new Model.OrderItem
                    {
                        Id = new Guid(i.Id),
                        OrderId = new Guid(i.OrderId),
                        ServiceId = new Guid(i.ServiceId),
                        ServiceName = i.Service.Name,
                        ProductId = new Guid(i.ProductId),
                        ProductName = i.Product.Name,
                        UnitCost = i.Product.UnitCost,
                        UnitPrice = i.Product.UnitPrice,
                        TotalCost = i.Product.UnitCost * i.Quantity.Value,
                        TotalPrice = i.Product.UnitPrice * i.Quantity.Value,
                        Quantity = i.Quantity.Value
                    })
                }).SingleOrDefaultAsync();
            
            return order;
        }
        // updates the status of an order in the database
        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, string status)
        {
            var orderIdBytes = orderId.ToByteArray();

            var order = await _orderContext.Order
                .Where(x => _orderContext.Database.IsInMemory() ? x.Id.SequenceEqual(orderIdBytes) : x.Id == orderIdBytes)
                .SingleOrDefaultAsync();

            if (order == null)
            {
                return false;
            }

            var statusEntity = await _orderContext.OrderStatus
                .Where(s => s.Name.ToLower() == status.ToLower())
                .SingleOrDefaultAsync();

            if (statusEntity == null)
            {
                return false;
            }

            order.StatusId = statusEntity.Id;

            await _orderContext.SaveChangesAsync();
            return true;
        }
        // Asynchronously creates a new order in the database and returns its detail view
        public async Task<OrderDetail> CreateOrderAsync(CreateOrderRequest request)
        {
            // 1. Get the default status (e.g., "Pending") for a newly created order
            var defaultStatus = await _orderContext.OrderStatus
                .Where(s => s.Name.ToLower() == "pending")
                .SingleOrDefaultAsync();

            // Fallback if "Pending" isn't explicitly found, take the first available status
            if (defaultStatus == null)
            {
                defaultStatus = await _orderContext.OrderStatus.FirstOrDefaultAsync();
            }

            // 2. Map request Guids to byte arrays (matching your database schema design)
            var newOrderId = Guid.NewGuid();
            var newOrderIdBytes = newOrderId.ToByteArray();
            var resellerIdBytes = Guid.Parse(request.ResellerId).ToByteArray();
            var customerIdBytes = Guid.Parse(request.CustomerId).ToByteArray();

            // 3. Create the new Order database entity
            var newOrder = new Data.Entities.Order // Adjust namespace if your EF entities live elsewhere
            {
                Id = newOrderIdBytes,
                ResellerId = resellerIdBytes,
                CustomerId = customerIdBytes,
                StatusId = defaultStatus.Id,
                CreatedDate = DateTime.UtcNow,
                Items = new List<Data.Entities.OrderItem>()
            };

            // 4. Map and add each item, fetching product details to link ServiceId and pricing
            foreach (var itemDto in request.Items)
            {
                var productIdGuid = Guid.Parse(itemDto.ProductId);
                var productIdBytes = productIdGuid.ToByteArray();

                var product = await _orderContext.OrderProduct
                    .Where(p => p.Id == productIdBytes)
                    .SingleOrDefaultAsync();

                if (product != null)
                {
                    newOrder.Items.Add(new Data.Entities.OrderItem
                    {
                        Id = Guid.NewGuid().ToByteArray(),
                        OrderId = newOrderIdBytes,
                        ProductId = productIdBytes,
                        ServiceId = product.ServiceId,
                        Quantity = itemDto.Quantity
                    });
                }
            }

            // 5. saves to the database context
            _orderContext.Order.Add(newOrder);
            await _orderContext.SaveChangesAsync();

            // 6. returns the fully populated OrderDetail using your existing helper method
            return await GetOrderByIdAsync(newOrderId);
        }
        public async Task<IEnumerable<MonthlyProfitDto>> GetProfitByMonthAsync()
        {
            // 1. fetches completed orders including items and product pricing details
            var completedOrders = await _orderContext.Order
                .Include(x => x.Items)
                .Include(x => x.Status)
                .Where(x => x.Status.Name.ToLower() == "completed")
                .ToListAsync();

            // 2. groups the orders by Year and Month in memory (handles byte array ID mapping safely)
            var profitByMonth = completedOrders
                .GroupBy(x => new { x.CreatedDate.Year, x.CreatedDate.Month })
                .Select(group => new MonthlyProfitDto
                {
                    Year = group.Key.Year,
                    Month = group.Key.Month,
                    MonthName = new DateTime(group.Key.Year, group.Key.Month, 1).ToString("MMMM"),
                    CompletedOrdersCount = group.Count(),
                    TotalProfit = group.Sum(order =>
                        order.Items.Sum(i => (i.Product.UnitPrice - i.Product.UnitCost) * i.Quantity.Value)
                    )
                })
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .ToList();

            return profitByMonth;
        }
    }
}
