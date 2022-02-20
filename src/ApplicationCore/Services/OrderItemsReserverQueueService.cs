using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.Extensions.Logging;

namespace Microsoft.eShopWeb.ApplicationCore.Services;

public class OrderItemsReserverQueueService: IOrderItemsReserverQueueService
{
    private static string _connectionString = "Endpoint=sb://eshoponwebio.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=p8ELZBJWXwZbFMLuCDKoud4Ng86AKhfaxo5EwSL5b2Y=";
    private static string _queueName = "order-items-reserver-queue";

    private readonly ILogger<OrderItemsReserverQueueService> _logger;

    public OrderItemsReserverQueueService(ILogger<OrderItemsReserverQueueService> logger)
    {
        _logger = logger;
    }

    public async Task ReserveOrderItemsAsync(IList<OrderItem> orderItems)
    {
        await using var client = new ServiceBusClient(_connectionString);
        await using var sender = client.CreateSender(_queueName);

        try
        {
            var message = new ServiceBusMessage(JsonExtensions.ToJson(orderItems));
            await sender.SendMessageAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to the queue.");
        }
    }
}
