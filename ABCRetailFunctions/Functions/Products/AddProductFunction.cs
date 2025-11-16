using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection.Metadata;
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
    public class AddProductFunction
    {
        private readonly TableStorageService<Product> _tableStorageService;
        private readonly ProductService _productService;
        private readonly BlobStorageService _blobStorageService;
        private readonly QueueStorageService _queueStorageService;

        public AddProductFunction(TableStorageService<Product> tableStorageService, ProductService productService, QueueStorageService queueStorageService, BlobStorageService blobStorageService)
        {
            _tableStorageService = tableStorageService;
            _productService = productService;
            _blobStorageService = blobStorageService;
            _queueStorageService = queueStorageService;
        }

        [FunctionName("AddProduct")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "products")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request to add a new product.");

            // change partionkey to student
            var form = await req.ReadFormAsync();
            var partitionKey = "Product";
            var rowKey = Guid.NewGuid().ToString();
            var product = new Product
            {
                PartitionKey = partitionKey,
                RowKey = rowKey,
                Name = form["name"],
                Description = form["description"],
                Category = form["category"],
            };

            // Preserve existing values if not supplied
            if (!string.IsNullOrEmpty(form["price"]) && double.TryParse(form["price"], out var price))
                product.Price = price;

            if (!string.IsNullOrEmpty(form["stockQuantity"]) && int.TryParse(form["stockQuantity"], out var stock))
                product.StockQuantity = stock;

            log.LogInformation($"Creating Product with partitionkey: {partitionKey}, rowkey: {rowKey}");

            // handle the photo if it is uploaded
            if (req.Form.Files.Count > 0)
            {
                var photo = req.Form.Files[0];
                using var stream = photo.OpenReadStream();

                // Upload and store the returned SAS URL
                product.ImageUrl = await _blobStorageService.UploadPhotoAsync(Guid.NewGuid().ToString(), stream);
            }

            // validate
            if (string.IsNullOrEmpty(product.PartitionKey) || string.IsNullOrEmpty(product.RowKey))
            {
                return new BadRequestObjectResult("Partition key and rowkey are required");

            }
            await _tableStorageService.AddAsync(product);

            // send message to queue
            await _queueStorageService.SendMessageAsync(new { Action = "Create", Product = product });

            return new OkObjectResult(product);
        }
    }
}
