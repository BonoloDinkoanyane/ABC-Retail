using System;
using System.IO;
using System.Linq;
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
    public class GetAllCustomersFunction
    {
        private readonly TableStorageService<Customer> _tableStorageService;
        private readonly CustomerService _customerService;

        public GetAllCustomersFunction(CustomerService customerService, TableStorageService<Customer> tableStorageService)
        {
            _tableStorageService = tableStorageService;
            _customerService = customerService;
        }

        [FunctionName("GetAllCustomers")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customers")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request to get a list of all customers.");

            //retrieve the customers from the customers table
            var customers = await _customerService.GetAllCustomersAsync();

            //converts customer to a customer data transfer object (DTO) because the etag is not a string; its an
            //object
            var customerDtos = customers.Select(c => new CustomerDto
            {
                PartitionKey = c.PartitionKey,
                RowKey = c.RowKey,
                Timestamp = c.Timestamp,
                ETag = c.ETag.ToString(),
                Name = c.Name,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                Address = c.Address,
            }).ToList();

            //return the list of the students as an API response
            return new OkObjectResult(customerDtos);
        }
    }
}
