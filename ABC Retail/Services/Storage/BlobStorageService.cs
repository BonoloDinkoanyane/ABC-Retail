using System.IO;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace ABC_Retail.Services.Storage
{
    public class BlobStorageService
    {
        //defininig the table client
        private readonly BlobContainerClient _blobContainerClient;

        //initialising the constructor
        public BlobStorageService(string storageConnectionString, string conatainerName)
        {
            var serviceClient = new BlobServiceClient(storageConnectionString);
            _blobContainerClient = serviceClient.GetBlobContainerClient(conatainerName);
            _blobContainerClient.CreateIfNotExists();
        }

        //upload an image, and returns the blob name 
        public async Task<string> UploadPhotoAsync(string blobName, Stream stream)
        {
            var blobClient = _blobContainerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(stream, overwrite: true);
            return GetBlobUriWithSas(blobClient);
        }

        //get blob URI with SAS token
        public string GetBlobUriWithSas(BlobClient blobClient)
        {
            if (blobClient.CanGenerateSasUri)
            {
                // Creating a SAS token that's valid for 24 hours
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = blobClient.BlobContainerName,
                    BlobName = blobClient.Name,
                    Resource = "b",
                    ExpiresOn = DateTimeOffset.UtcNow.AddHours(72)
                };
                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                var sasUri = blobClient.GenerateSasUri(sasBuilder);
                return blobClient.GenerateSasUri(sasBuilder).ToString();
            }
            else
            {
                throw new InvalidOperationException("BlobClient does not support generating SAS URI's.");
            }
        }

         //get SAS URL from blob name
        public string GetImageSasUri(string blobName)
        {
            var blobClient = _blobContainerClient.GetBlobClient(blobName);
            return GetBlobUriWithSas(blobClient);
        }

        //delete photo delete.cshtml
        public async Task DeletePhotoAsync(string blobName)
        {
            var blobClient = _blobContainerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }

    }
}
