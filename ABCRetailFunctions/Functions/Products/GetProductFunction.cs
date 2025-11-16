using System;
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

namespace ABCRetailFunctions.Functions.Products
{
    public  class GetProductFunction
    {
        private readonly TableStorageService<Product> _tableStorageService;
        private readonly ProductService _productService;

        public GetProductFunction(ProductService productService, TableStorageService<Product> tableStorageService)
        {
            _tableStorageService = tableStorageService;
            _productService = productService;
        }

        [FunctionName("GetProduct")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/{partitionKey}/{rowKey}")] HttpRequest req,
            string partitionKey, string rowKey, ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request to get product details based on partitionKey: {partitionKey} and rowKey: {rowKey}.");

            // retrieve a customer based on partitionkey and rowkey
            var product = await _productService.GetProductAsync(partitionKey, rowKey);
            if (product == null)
            {
                log.LogWarning($"Product not found on partitionkey {partitionKey} and row key: {rowKey}");
                return new NotFoundResult();
            }

            //convert to product dto
            var productDto = new ProductDto
            {
                PartitionKey = product.PartitionKey,
                RowKey = product.RowKey,
                ETag = product.ETag.ToString(),
                Name = product.Name,
                Description = product.Description,
                Category = product.Category,
                ImageUrl = product.ImageUrl,
                Price = product.Price,
                StockQuantity = product.StockQuantity
            };

            //return product
            return new OkObjectResult(productDto);
        }
    }
}
