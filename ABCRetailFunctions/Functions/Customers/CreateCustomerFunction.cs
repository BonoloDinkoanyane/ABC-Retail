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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ABCRetailFunctions.Functions.Customers
{
    public class CreateCustomerFunction
    {
        private readonly TableStorageService<Customer> _customerTableService;
        private readonly QueueStorageService _queueStorageService;

        public CreateCustomerFunction(TableStorageService<Customer> customerTableService,QueueStorageService queueStorageService)
        {
            _customerTableService = customerTableService;
            _queueStorageService = queueStorageService;
        }
        [FunctionName("CreateCustomer")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "customers")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request to create a new customer.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Customer>(requestBody);

            data.PartitionKey ??= "Customer";
            data.RowKey ??= Guid.NewGuid().ToString();

            // Store customer in Table Storage
            await _customerTableService.AddAsync(data);

            // Send a log message to the queue
            var logMessage = new
            {
                Action = "CustomerCreated",
                Timestamp = DateTime.UtcNow,
                Customer = new
                {
                    data.PartitionKey,
                    data.RowKey,
                    data.Name,
                    data.Email,
                    data.PhoneNumber,
                    data.Address
                }
            };
            await _queueStorageService.SendMessageAsync(logMessage);

            log.LogInformation($"Customer {data.Name} created successfully.");

            return new OkObjectResult(new
            {
                message = $"Customer {data.Name} created successfully.",
                customerId = data.RowKey
            });
        }
    }
}
