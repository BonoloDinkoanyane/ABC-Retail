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
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ABCRetailFunctions.Functions.Products
{
    public class UpdateProductFunction
    {
        private readonly TableStorageService<Product> _tableStorageService;
        private readonly ProductService _productService;
        private readonly BlobStorageService _blobStorageService;
        private readonly QueueStorageService _queueStorageService;

        public UpdateProductFunction(TableStorageService<Product> tableStorageService, ProductService productService, QueueStorageService queueStorageService, BlobStorageService blobStorageService)
        {
            _tableStorageService = tableStorageService;
            _productService = productService;
            _blobStorageService = blobStorageService;
            _queueStorageService = queueStorageService;
        }
        [FunctionName("UpdateProduct")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "products/{partitionKey}/{rowKey}")] HttpRequest req,
            string partitionKey, string rowKey, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Read form data
            var formData = await req.ReadFormAsync();
            var name = formData["Name"];
            var description = formData["Description"];
            var category = formData["Category"];
            var priceValue = formData["Price"];
            var stockValue = formData["StockQuantity"];

            // Retrieve the existing product entity
            var existingProduct = await _tableStorageService.GetAsync(partitionKey, rowKey);
            if (existingProduct == null)
            {
                log.LogWarning($"Product not found at partitionKey: {partitionKey}, rowKey: {rowKey}");
                return new NotFoundResult();
            }

            // update the properties
            if (!string.IsNullOrEmpty(name)) existingProduct.Name = name;
            if (!string.IsNullOrEmpty(description)) existingProduct.Description = description;
            if (!string.IsNullOrEmpty(category)) existingProduct.Category = category;

            // updates only if the form values exist
            existingProduct.Price = double.TryParse(priceValue, out var price) ? price : existingProduct.Price;
            existingProduct.StockQuantity = int.TryParse(stockValue, out var stock) ? stock : existingProduct.StockQuantity;

            // handle photo
            if (formData.Files.Count > 0)
            {
                var photo = formData.Files[0];
                using var stream = photo.OpenReadStream();

                // Upload new photo and replace the old ImageUrl with SAS URL
                existingProduct.ImageUrl = await _blobStorageService.UploadPhotoAsync(Guid.NewGuid().ToString(), stream);
            }

            // Update in Table Storage
            await _tableStorageService.UpdateAsync(existingProduct);

            // convert dto
            var updatedProductDto = new ProductDto
            {
                PartitionKey = existingProduct.PartitionKey,
                RowKey = existingProduct.RowKey,
                ETag = existingProduct.ETag.ToString(),
                Name = existingProduct.Name,
                Description = existingProduct.Description,
                Category = existingProduct.Category,
                Price = existingProduct.Price,
                StockQuantity = existingProduct.StockQuantity,
                ImageUrl = existingProduct.ImageUrl
            };

            // semd the update message to the queue
            await _queueStorageService.SendMessageAsync(new { Action = "Update", Product = updatedProductDto });
            return new OkObjectResult(updatedProductDto);
        }
    }
}
