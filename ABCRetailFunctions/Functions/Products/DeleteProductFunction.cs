using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ABCRetailFunctions.Models;
using ABCRetailFunctions.Services;
using ABCRetailFunctions.Services.Storage;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ABCRetailFunctions.Functions.Products
{
    public class DeleteProductFunction
    {
        private readonly TableStorageService<Product> _tableStorageService;
        private readonly ProductService _productService;
        private readonly BlobStorageService _blobStorageService;
        private readonly QueueStorageService _queueStorageService;

        public DeleteProductFunction(TableStorageService<Product> tableStorageService, ProductService productService, QueueStorageService queueStorageService, BlobStorageService blobStorageService)
        {
            _tableStorageService = tableStorageService;
            _productService = productService;
            _blobStorageService = blobStorageService;
            _queueStorageService = queueStorageService;
        }

        [FunctionName("DeleteProduct")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "products/{partitionKey}/{rowKey}")] HttpRequest req,
            string partitionKey, string rowKey, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request to delete a product.");

            try
            {
                // Retrieve the product from table
                var product = await _productService.GetProductAsync(partitionKey, rowKey);
                if (product == null)
                {
                    log.LogWarning($"Product not found at PartitionKey: {partitionKey}, RowKey: {rowKey}");
                    return new NotFoundResult();
                }

                // Delete the product details from the table storage
                await _productService.DeleteProductAsync(partitionKey, rowKey);
                log.LogInformation($"Product {rowKey} deleted successfully from Table Storage.");

                // Delete associated photo if it exists
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    var blobName = new Uri(product.ImageUrl).Segments.Last();
                    await _blobStorageService.DeletePhotoAsync(blobName);
                    log.LogInformation($"Deleted product photo: {blobName}");
                }

                // Send the deletion message to queue for logging
                var message = new
                {
                    Action = "Delete",
                    Product = new
                    {
                        product.PartitionKey,
                        product.RowKey,
                        product.Name,
                        product.Category,
                        product.Price
                    }
                };

                await _queueStorageService.SendMessageAsync(message);
                log.LogInformation("Delete message sent to queue.");

                return new OkObjectResult(new { Message = "Product deleted successfully." });
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                log.LogWarning($"Product not found: PartitionKey: {partitionKey}, RowKey: {rowKey}");
                return new NotFoundResult();
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Error deleting product: PartitionKey: {partitionKey}, RowKey: {rowKey}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}

