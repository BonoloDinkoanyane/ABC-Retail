#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;

namespace ABCRetailFunctions.Models
{
    public class Order : ITableEntity
    {
        // ITableEntity properties for Azure Table Storage
        public string? PartitionKey { get; set; } // Often used for Customer ID or a fixed value like "Order"
        public string? RowKey { get; set; }       // Unique Order ID (e.g., GUID)
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Order-specific properties
        // Re-purposing CustomerPartitionKey and CustomerRowKey as actual properties
        public string CustomerId { get; set; } = string.Empty;  // Direct reference to the Customer's RowKey or a unique customer ID
        public List<OrderItem> Items { get; set; } = new List<OrderItem>(); // Initialise to prevent null reference
        public DateTime OrderDate { get; set; }
        public string? Status { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class OrderItem
    {
        public string? ProductPartitionKey { get; set; }  // References Product's PartitionKey
        public string? ProductRowKey { get; set; }        // References Product's RowKey
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}
