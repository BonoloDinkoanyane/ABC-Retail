using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ABCRetailFunctions.Models;
using ABCRetailFunctions.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ABCRetailFunctions.Functions.Orders
{
    public class UploadLogsFunction
    {
        private readonly FileShareStorageService _fileShareStorageService;
        private readonly QueueStorageService _queueStorageService;

        public UploadLogsFunction(FileShareStorageService fileShareStorageService, QueueStorageService queueStorageService)
        {
            _fileShareStorageService = fileShareStorageService;
            _queueStorageService = queueStorageService;
        }
        [FunctionName("UploadLog")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "logs")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request to upload the log file.");

            // get file name
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string name = data?.name;

            if (string.IsNullOrEmpty(name))
            {
                return new BadRequestObjectResult("Please pass a name in the request body");
            }
            try
            {
                // get messages form queue
                List<QueueLogViewModel> logMessages = await _queueStorageService.GetMessagesAsync();
                //create CSV file
                var content = new StringBuilder();
                content.AppendLine("MessageId, InsertionTime,MessageText");
                foreach (var msg in logMessages)
                {
                    var msgText = msg.MessageText.Replace("\"", "\"\"");
                    content.AppendLine($"{msg.MessageId}, {msg.InsertionTime},\"{msg.MessageText}\"");
                }

                //upload file
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(content.ToString())))
                {
                    await _fileShareStorageService.UploadContractAsync(stream, name);
                }

                //clear queue
                await _queueStorageService.ClearQueueAsync();
                return new OkObjectResult($"Log file {name} uploaded successfully");

            }
            catch (Exception ex)
            {
                log.LogError($"Error uploading log file: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);

            }
        }
    }
}
