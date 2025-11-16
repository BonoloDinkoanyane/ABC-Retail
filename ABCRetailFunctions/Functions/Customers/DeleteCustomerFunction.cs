using System;
using System.IO;
using System.Threading.Tasks;
using ABCRetailFunctions.Models;
using ABCRetailFunctions.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ABCRetailFunctions.Functions.Customers
{
    public class DeleteCustomerFunction
    {
        private readonly TableStorageService<Customer> _customerTableService;
        private readonly QueueStorageService _queueStorageService;

        public DeleteCustomerFunction(
            TableStorageService<Customer> customerTableService,
            QueueStorageService queueStorageService)
        {
            _customerTableService = customerTableService;
            _queueStorageService = queueStorageService;
        }

        [FunctionName("DeleteCustomer")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete",  Route = "customers/{partitionKey}/{rowKey}")] HttpRequest req,
            string partitionKey, string rowKey, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request to delete a customer.");

            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
                return new BadRequestObjectResult("PartitionKey and RowKey must be provided.");

            // Attempt to delete the customer
            var customer = await _customerTableService.GetAsync(partitionKey, rowKey);
            if (customer == null)
                return new NotFoundObjectResult($"Customer with RowKey '{rowKey}' not found.");

            await _customerTableService.DeleteAsync(partitionKey, rowKey);

            // Send a log message to the queue
            var logMessage = new
            {
                Action = "CustomerDeleted",
                Timestamp = DateTime.UtcNow,
                Customer = new
                {
                    partitionKey,
                    rowKey,
                    customer.Name,
                    customer.Email,
                    customer.PhoneNumber,
                    customer.Address
                }
            };
            await _queueStorageService.SendMessageAsync(logMessage);

            log.LogInformation($"Customer {rowKey} deleted successfully.");

            return new OkObjectResult(new
            {
                message = $"Customer '{customer.Name}' deleted successfully.",
                rowKey = rowKey
            });
        }
    }
}
