using ABC_Retail.Models;
using ABC_Retail.Services.Storage;
using ABC_Retail.Data;
using Microsoft.EntityFrameworkCore;

namespace ABC_Retail.Services
{
    public class OrderService
    {
        //private readonly TableStorageService<Order> _tableService;
        private readonly AppDbContext _context;
        private readonly QueueStorageService _queueService;

        public OrderService(AppDbContext dbContext, QueueStorageService queueService)
        {
            _context = dbContext;
            _queueService = queueService;
        }

        // Get all orders
        public Task<List<Order>> GetAllOrdersAsync()
        => _context.Order.Include(o => o.Items).ToListAsync();

        // Get a single order
        public Task<Order?> GetOrderAsync(int id)
            => _context.Order.Include(o => o.Items)
                              .FirstOrDefaultAsync(x => x.OrderId == id);

        // Add a new order
        public async Task AddOrderAsync(Order order)
        {
            //var orderId = order.RowKey ?? Guid.NewGuid().ToString();
            //order.RowKey = orderId;
            _context.Order.Add(order);
            await _context.SaveChangesAsync();

            // Constructs a message to describe the order processing task
            await _queueService.SendMessageAsync(new
            {
                Action = "ProcessOrder",
                OrderId = order.OrderId,
                order.Status
            });
            //var message = new
            //{
            //    Action = "ProcessOrder",
            //    OrderId = orderId,
            //    CustomerId = order.CustomerId,
            //    OrderDate = order.OrderDate,
            //    Status = order.Status,
            //    TotalAmount = order.TotalAmount,
            //    Items = order.Items,
            //    Timestamp = DateTime.UtcNow
            //};

            // Sends the constructed message to the queue
            //await _queueService.SendMessageAsync(message);
        }

        // Update existing order
        public async Task UpdateOrderStatusAsync(string orderId, string newStatus)
        {
            var order = await _context.Order.FindAsync(orderId);
            if (order == null) return;

            order.Status = newStatus;
            await _context.SaveChangesAsync();

            await _queueService.SendMessageAsync(new
            {
                Action = "UpdateOrderStatus",
                OrderId = orderId,
                NewStatus = newStatus,
                Timestamp = DateTime.UtcNow
            });
        }

        // deleting an order
        public async Task DeleteOrderAsync(int orderId)
        {
            var order = await _context.Order.FindAsync(orderId);
            if (order == null) return;

            _context.Order.Remove(order);
            await _context.SaveChangesAsync();

            // Optionally send a queue message
            await _queueService.SendMessageAsync(new
            {
                Action = "DeleteOrder",
                OrderId = orderId,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}