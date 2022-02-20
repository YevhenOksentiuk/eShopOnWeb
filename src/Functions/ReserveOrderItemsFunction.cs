using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.eShopWeb;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using System.Text;
using System.Net.Http;
using System.Net.Http.Json;

namespace OrderItemsReserver
{
    public static class ReserveOrderItemsFunction
    {
        [FunctionName("ReserveOrderItemsFunction")]
        public static async Task Run(
            [ServiceBusTrigger("order-items-reserver-queue", Connection = "ServiceBusConnectionString")] string myQueueItem,
            ILogger log)
        {
            try
            {
                var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                var cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
                var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                var reservedOrderItemsContainer = cloudBlobClient.GetContainerReference("reserved-order-items");

                var blob = reservedOrderItemsContainer.GetBlockBlobReference($"{DateTime.UtcNow.ToString("o")}.json");
                blob.Properties.ContentType = "application/json";

                await blob.UploadTextAsync(myQueueItem);
            }
            catch (Exception ex)
            {
                var body = new 
                {
                    Error = ex.Message,
                    OrderItems = myQueueItem
                };
                var url = Environment.GetEnvironmentVariable("FailureLogicAppUrl");
                await new HttpClient().PostAsync(url, JsonContent.Create(body));
            }

            log.LogInformation("C# HTTP trigger function processed a request.");
        }
    }
}
