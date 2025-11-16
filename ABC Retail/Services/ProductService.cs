using ABC_Retail.Models;
using ABC_Retail.Services.Storage;
using Azure.Storage.Blobs;

namespace ABC_Retail.Services
{
    public class ProductService
    {
        private readonly TableStorageService<Product> _tableService;
        private readonly BlobStorageService _blobService;

        // method injects both TableStorageService and BlobStorageService
        public ProductService(string tableConnectionString, string tableName, BlobStorageService blobService)
        {
            _tableService = new TableStorageService<Product>(tableConnectionString, tableName);
            _blobService = blobService;
        }

        public Task<List<Product>> GetAllProductsAsync()
            => _tableService.GetAllAsync();

        public Task<Product?> GetProductAsync(string partitionKey, string rowKey)
            => _tableService.GetAsync(partitionKey, rowKey);

        // Add product with optional image
        public async Task AddProductAsync(Product product, Stream? imageStream = null, string? fileName = null)
        {

            if (imageStream != null && fileName != null)
            {
                product.ImageUrl = await _blobService.UploadPhotoAsync(fileName, imageStream);
            }

            await _tableService.AddAsync(product);
        }

        // Update product and optionally replace image
        public async Task UpdateProductAsync(Product product, Stream? newImageStream = null, string? newFileName = null)
        {
            if (newImageStream != null && !string.IsNullOrEmpty(newFileName))
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    await _blobService.DeletePhotoAsync(product.ImageUrl);
                }

                // Upload new image
                product.ImageUrl = await _blobService.UploadPhotoAsync(newFileName, newImageStream);
            }

            await _tableService.UpdateAsync(product);
        }

        public Task DeleteProductAsync(string partitionKey, string rowKey)
        {
            // Optionally: delete image from blob storage
            return _tableService.DeleteAsync(partitionKey, rowKey);
        }
    }
}