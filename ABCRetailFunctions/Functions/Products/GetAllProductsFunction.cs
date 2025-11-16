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

namespace ABCRetailFunctions.Functions.Products
{
    public class GetAllProductsFunction
    {
        private readonly TableStorageService<Product> _tableStorageService;
        private readonly ProductService _productService;

        public GetAllProductsFunction(ProductService productService, TableStorageService<Product> tableStorageService)
        {
            _tableStorageService = tableStorageService;
            _productService = productService;
        }

        [FunctionName("GetAllProducts")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get",  Route = "products")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            //retrieve the customers from the customers table
            var products = await _productService.GetAllProductsAsync();

            //converts customer to a customer data transfer object (DTO) because the etag is not a string; its an
            //object
            var productDtos = products.Select(p => new ProductDto
            {
                PartitionKey = p.PartitionKey,
                RowKey = p.RowKey,
                Timestamp = p.Timestamp,
                ETag = p.ETag.ToString(),
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                Category = p.Category,
                ImageUrl = p.ImageUrl
            }).ToList();

            //return the list of the students as an API response
            return new OkObjectResult(productDtos);
        }
    }
}
