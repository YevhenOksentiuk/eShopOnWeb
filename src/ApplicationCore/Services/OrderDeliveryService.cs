using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.ApplicationCore.Services;

public class OrderDeliveryService : IOrderDeliveryService
{
    private readonly IAppLogger<OrderDeliveryService> _logger;
    private readonly OrderDeliverySettings _config;

    public OrderDeliveryService(IAppLogger<OrderDeliveryService> logger, IOrderDeliveryConfigProvider configProvider)
    {
        _logger = logger;
        _config = configProvider.GetConfig();
    }

    public async Task Process(Order order)
    {
        var orderDeliveryInfo = new OrderDeliveryInfo
        {
            OrderId = order.Id,
            ShippingAddress = order.ShipToAddress,
            Items = ToOrderDeliveryItems(order.OrderItems),
            TotalPrice = order.Total()
        };

        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("x-functions-key", _config.OrderDeliveryFunctionKey);

        var response = await httpClient.PostAsync(_config.OrderDeliveryFunctionUri, JsonContent.Create(orderDeliveryInfo));

        if (!response.IsSuccessStatusCode)
            _logger.LogWarning($"Failed to process order delivery. Response code: {response.StatusCode}");
    }

    private IEnumerable<OrderDeliveryItem> ToOrderDeliveryItems(IEnumerable<OrderItem> orderItems)
    {
        foreach (var item in orderItems)
        {
            yield return new OrderDeliveryItem
            {
                CatalogItemId = item.ItemOrdered.CatalogItemId,
                ProductName = item.ItemOrdered.ProductName,
                Units = item.Units
            };
        }
    }
}
