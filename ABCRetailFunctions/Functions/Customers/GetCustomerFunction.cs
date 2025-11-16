using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using ABCRetailFunctions.Models;
using ABCRetailFunctions.Services;
using ABCRetailFunctions.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ABCRetailFunctions.Functions.Customers
{

    public class GetCustomerFunction
    {
        private readonly TableStorageService<Customer> _tableStorageService;
        private readonly CustomerService _customerService;

        public GetCustomerFunction(CustomerService customerService, TableStorageService<Customer> tableStorageService)
        {
            _tableStorageService = tableStorageService;
            _customerService = customerService;
        }

        [FunctionName("GetCustomer")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customers/{partitionKey}/{rowKey}")] HttpRequest req,
            string partitionKey, string rowKey, ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request to get customer details based on partitionKey: {partitionKey} and rowKey: {rowKey}.");

            // retrieve a customer based on partitionkey and rowkey
            var customer = await _customerService.GetCustomerAsync(partitionKey, rowKey);
            if (customer == null)
            {
                log.LogWarning($"Customer not found on partitionkey {partitionKey} and row key: {rowKey}");
                return new NotFoundResult();
            }

            //convert to customer dto
            var customerDto = new CustomerDto
            {
                PartitionKey = customer.PartitionKey,
                RowKey = customer.RowKey,
                Timestamp = customer.Timestamp,
                ETag = customer.ETag.ToString(),
                Name = customer.Name,
                PhoneNumber = customer.PhoneNumber,
                Email = customer.Email,
                Address = customer.Address,
            };

            //return customer
            return new OkObjectResult(customerDto);
        }
    }
}
