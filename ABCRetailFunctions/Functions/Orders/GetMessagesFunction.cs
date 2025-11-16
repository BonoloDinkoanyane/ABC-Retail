using System;
using System.IO;
using System.Threading.Tasks;
using ABCRetailFunctions.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ABCRetailFunctions.Functions.Orders
{
    public class GetMessagesFunction
    {
        private readonly QueueStorageService _queueStorageService;

        public GetMessagesFunction(QueueStorageService queueStorageService)
        {
            _queueStorageService = queueStorageService;
        }

        [FunctionName("GetMessages")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get",  Route = "logs")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var messages = await _queueStorageService.GetMessagesAsync();
            return new OkObjectResult(messages);
        }
    }
}
