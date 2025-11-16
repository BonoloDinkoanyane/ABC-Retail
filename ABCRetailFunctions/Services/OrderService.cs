using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABCRetailFunctions.Models;
using ABCRetailFunctions.Services.Storage;

namespace ABCRetailFunctions.Services
{
    public class OrderService
    {
        private readonly TableStorageService<Order> _tableService;
        private readonly QueueStorageService _queueService;

        public OrderService(string tableConnectionString, string tableName, QueueStorageService queueService)
        {
            _tableService = new TableStorageService<Order>(tableConnectionString, tableName);
            _queueService = queueService;
        }

        // Get all orders
        public Task<List<Order>> GetAllOrdersAsync()
            => _tableService.GetAllAsync();

        // Get a single order
        public Task<Order> GetOrderAsync(string partitionKey, string rowKey)
            => _tableService.GetAsync(partitionKey, rowKey);

        // Add a new order
        public async Task AddOrderAsync(Order order)
        {
            var orderId = order.RowKey ?? Guid.NewGuid().ToString();
            order.RowKey = orderId;

            // Constructs a message to describe the order processing task
            var message = new
            {
                Action = "ProcessOrder",
                OrderId = orderId,
                CustomerId = order.CustomerId,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                Items = order.Items,
                Timestamp = DateTime.UtcNow
            };

            // Sends the constructed message to the queue
            await _queueService.SendMessageAsync(message);
        }

        // Update existing order
        public async Task UpdateOrderStatusAsync(string orderId, string newStatus)
        {
            var message = new
            {
                Action = "UpdateOrderStatus",
                OrderId = orderId,
                NewStatus = newStatus,
                Timestamp = DateTime.UtcNow
            };

            await _queueService.SendMessageAsync(message);
        }

        // Delete an order
        public Task DeleteOrderAsync(string partitionKey, string rowKey)
            => _tableService.DeleteAsync(partitionKey, rowKey);
    }
}
