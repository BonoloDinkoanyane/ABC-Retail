using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;

namespace ABC_Retail.Services.Storage
{
    public class FileShareStorageService
    {
        private readonly ShareClient _shareClient;

        // Initialising the File Share client
        public FileShareStorageService(string storageConnectionString, string shareName)
        {
            _shareClient = new ShareClient(storageConnectionString, shareName);
            _shareClient.CreateIfNotExists();
        }

        // Upload a dummy contract to Azure File Share.
        public async Task UploadContractAsync(Stream contractStream, string fileName)
        {
            var rootDirectory = _shareClient.GetRootDirectoryClient();
            var fileClient = rootDirectory.GetFileClient(fileName);

            await fileClient.CreateAsync(contractStream.Length);
            contractStream.Position = 0;
            await fileClient.UploadAsync(contractStream);
        }

        // List all dummy contract files in the File Share root directory
        public async Task<List<string>> ListContractsAsync()
        {
            var contractFiles = new List<string>();
            var rootDirectory = _shareClient.GetRootDirectoryClient();

            await foreach (ShareFileItem item in rootDirectory.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory)
                {
                    contractFiles.Add(item.Name);
                }
            }

            return contractFiles;
        }

        // Download a contract file from File Share
        public async Task<Stream?> DownloadContractAsync(string fileName)
        {
            var rootDirectory = _shareClient.GetRootDirectoryClient();
            var fileClient = rootDirectory.GetFileClient(fileName);

            if (await fileClient.ExistsAsync())
            {
                var download = await fileClient.DownloadAsync();
                return download.Value.Content;
            }

            return null;
        }

        /// Delete a contract file from File Share
        public async Task DeleteContractAsync(string fileName)
        {
            var rootDirectory = _shareClient.GetRootDirectoryClient();
            var fileClient = rootDirectory.GetFileClient(fileName);
            await fileClient.DeleteIfExistsAsync();
        }
    }
}

