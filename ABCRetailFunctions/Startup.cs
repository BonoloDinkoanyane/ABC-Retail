using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABCRetailFunctions.Models;
using ABCRetailFunctions.Services.Storage;
using ABCRetailFunctions.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(ABCRetailFunctions.Startup))]

namespace ABCRetailFunctions
{
    public class Startup : FunctionsStartup
    {
        //method registers the storage services
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            
            // Registering the table storage services
            builder.Services.AddSingleton(sp =>
                new TableStorageService<Customer>(storageConnectionString, "Customer"));

            builder.Services.AddSingleton(sp =>
                new TableStorageService<Product>(storageConnectionString, "ProductInfo"));

            builder.Services.AddSingleton(sp =>
                new TableStorageService<Order>(storageConnectionString, "Orders"));

            builder.Services.AddSingleton(sp =>
                CreateStorageService<BlobStorageService>(sp, "product-photos", "blob"));

            builder.Services.AddSingleton(sp =>
                CreateStorageService<QueueStorageService>(sp, "order-log-messages", "queue"));

            builder.Services.AddSingleton(sp =>
                CreateStorageService<FileShareStorageService>(sp, "contracts", "fileshare"));

            builder.Services.AddSingleton(sp =>
            {
                var tableService = sp.GetRequiredService<TableStorageService<Customer>>();
                return new CustomerService(storageConnectionString, "Customer");
            });

            builder.Services.AddSingleton(sp =>
            {
                var blobService = sp.GetRequiredService<BlobStorageService>();
                return new ProductService(storageConnectionString, "ProductInfo", blobService);
            });

            builder.Services.AddSingleton(sp =>
            {
                var queueService = sp.GetRequiredService<QueueStorageService>();
                return new OrderService(storageConnectionString, "Orders", queueService);
            });
        }

        //identifies what kind of storage to create on what service and creates the required container
        private T CreateStorageService<T>(IServiceProvider sp, string serviceIdentifier, string serviceType) where T : class
        {
            var logger = sp.GetRequiredService<ILogger<Startup>>();
            var storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            if (string.IsNullOrEmpty(storageConnectionString) || string.IsNullOrEmpty(serviceIdentifier))
            {
                logger.LogError("Storage connection string, or service identifier is not set.");
                throw new InvalidOperationException("Configuration is invalid");
            }

            logger.LogInformation($"Using {serviceType} identifier: {serviceIdentifier}");

            //the below switch case statement is from chatGPT
            // GPT-5 language model
            //accessed on 05 Oct 2025
            //chat link: https://chatgpt.com/share/68e1b15f-fd2c-8002-b179-d831400f6077 

            // method handles each type of storage service
            switch (serviceType.ToLower())
            {
                case "table":
                    // Decide which entity type to store
                    if (serviceIdentifier.Equals("Customer", StringComparison.OrdinalIgnoreCase))
                        return new TableStorageService<Customer>(storageConnectionString, serviceIdentifier) as T;
                    else if (serviceIdentifier.Equals("ProductInfo", StringComparison.OrdinalIgnoreCase))
                        return new TableStorageService<Product>(storageConnectionString, serviceIdentifier) as T;
                    else
                        throw new NotSupportedException($"No matching table model found for {serviceIdentifier}");

                case "blob":
                    return new BlobStorageService(storageConnectionString, serviceIdentifier) as T;

                case "fileshare":
                    return new FileShareStorageService(storageConnectionString, serviceIdentifier) as T;

                case "queue":
                    return new QueueStorageService(storageConnectionString, serviceIdentifier) as T;

                default:
                    throw new NotImplementedException($"{serviceType} is not supported.");
            }
        }
    }
}
