using System.Text;
using System.Text.Json;
using ABC_Retail.Models;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Azure.Storage.Queues.Specialized;

namespace ABC_Retail.Services.Storage
{
    public class QueueStorageService
    {
        private readonly QueueClient _queueClient;

        public QueueStorageService(string storageConnectionString, string queueName)
        {
            var queueServiceClient = new QueueServiceClient(storageConnectionString);
            _queueClient = queueServiceClient.GetQueueClient(queueName);
            _queueClient.CreateIfNotExists();
        }

        //send message to the queue
        public async Task SendMessageAsync(object message)
        {
            //convert message to json
            var messageJson = JsonSerializer.Serialize(message);
            await _queueClient.SendMessageAsync(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(messageJson)));
        }

        //gets the messages from the queue for the log
        public async Task<List<QueueLogViewModel>> GetMessagesAsync()
        {
            var entryList = new List<QueueLogViewModel>();
            var entries = await _queueClient.PeekMessagesAsync(maxMessages: 32);

            foreach (PeekedMessage entry in entries.Value)
            {
                try
                {
                    var json = Encoding.UTF8.GetString(Convert.FromBase64String(entry.Body.ToString()));

                    var deserialized = JsonSerializer.Deserialize<QueueLogViewModel>(json);

                    if (deserialized != null)
                    {
                        deserialized.MessageId = entry.MessageId;
                        deserialized.InsertionTime = entry.InsertedOn;
                        entryList.Add(deserialized);
                    }
                }
                catch
                {
                    entryList.Add(new QueueLogViewModel
                    {
                        MessageId = entry.MessageId,
                        InsertionTime = entry.InsertedOn,
                        RawMessage = entry.Body.ToString()
                    });
                }
            }
            return entryList;
        }

    }
}
