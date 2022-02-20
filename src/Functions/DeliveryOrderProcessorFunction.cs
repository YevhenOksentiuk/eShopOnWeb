using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.eShopWeb.ApplicationCore.Entities;

namespace Functions
{
    public static class DeliveryOrderProcessorFunction
    {
        [FunctionName("DeliveryOrderProcessorFunction")]
        [return: CosmosDB(
                databaseName: "Delivery",
                collectionName: "Orders",
                ConnectionStringSetting = "CosmosDBConnection")]
        public static async Task<OrderDeliveryInfo> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            using var streamReader = new StreamReader(req.Body);
            string requestBody = await streamReader.ReadToEndAsync();
            var deliveryInfo = JsonConvert.DeserializeObject<OrderDeliveryInfo>(requestBody);

            log.LogInformation("C# HTTP trigger function processed a request.");

            return deliveryInfo;
        }
    }
}
