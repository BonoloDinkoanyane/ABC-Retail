using System.Collections.Concurrent;
using ABC_Retail.Models;

namespace ABC_Retail.Services.Functions.ProductFunctions
{
    public class ProductFunctionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _functionBaseUrl;

        public ProductFunctionService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _functionBaseUrl = configuration["AzureFunctionsBaseUrlProd"] ?? throw new InvalidOperationException("Azure Functions Base Url is missing.");
        }

        //get all products
        public async Task<List<Product>> GetAllProductsAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<List<Product>>($"{_functionBaseUrl}/api/products");
            return response ?? new List<Product>();
        }

        //get a single product's details
        public async Task<Product?> GetProductAsync(string partitionKey, string rowKey)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Product>($"{_functionBaseUrl}/api/products/{partitionKey}/{rowKey}");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching product: {ex.Message}");
                return null;
            }
        }

        //create a new product
        public async Task<bool> AddProductAsync(Product product, IFormFile? photo)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(product.Name ?? string.Empty), "name");
            content.Add(new StringContent(product.Category ?? string.Empty), "category");
            content.Add(new StringContent(product.Description ?? string.Empty), "description");
            content.Add(new StringContent(product.StockQuantity.ToString()), "stockQuantity");
            content.Add(new StringContent(product.Price.ToString()), "price");

            if (photo != null)
            {
                var streamContent = new StreamContent(photo.OpenReadStream());
                content.Add(streamContent, "Image", photo.FileName);
            }

            var response = await _httpClient.PostAsync($"{_functionBaseUrl}/api/products", content);
            return response.IsSuccessStatusCode;
        }

        //delete a product
        public async Task<bool> DeleteProductAsync(string partitionKey, string rowKey)
        {
            var requestUrl = $"{_functionBaseUrl}/api/products/{partitionKey}/{rowKey}";
            var response = await _httpClient.DeleteAsync(requestUrl);
            return response.IsSuccessStatusCode;
        }

        //update a product
        public async Task<bool> UpdateProductAsync(Product product, IFormFile? photo)
        {
            var requestUrl = $"{_functionBaseUrl}/api/products/{product.PartitionKey}/{product.RowKey}";

            using var form = new MultipartFormDataContent();
            // Add text fields
            if (!string.IsNullOrEmpty(product.Name))
                form.Add(new StringContent(product.Name), "Name");

            if (!string.IsNullOrEmpty(product.Description))
                form.Add(new StringContent(product.Description), "Description");

            if (!string.IsNullOrEmpty(product.Category))
                form.Add(new StringContent(product.Category), "Category");

            form.Add(new StringContent(product.Price.ToString()), "Price");
            form.Add(new StringContent(product.StockQuantity.ToString()), "StockQuantity");

            // Add image if provided
            if (photo != null)
            {
                var streamContent = new StreamContent(photo.OpenReadStream());
                form.Add(streamContent, "Image", photo.FileName);
            }

            var response = await _httpClient.PostAsync(requestUrl, form);
            return response.IsSuccessStatusCode;
        }
    }
}
