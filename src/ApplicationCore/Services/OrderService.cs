using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;

namespace Microsoft.eShopWeb.ApplicationCore.Services;

public class OrderService : IOrderService
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IUriComposer _uriComposer;
    private readonly IRepository<Basket> _basketRepository;
    private readonly IRepository<CatalogItem> _itemRepository;
    private readonly IOrderItemsReserverQueueService _orderItemsReserverQueueService;
    private readonly IOrderDeliveryService _orderDeliveryService;
    private readonly IAppLogger<OrderService> _logger;

    public OrderService(IRepository<Basket> basketRepository,
        IRepository<CatalogItem> itemRepository,
        IRepository<Order> orderRepository,
        IUriComposer uriComposer,
        IOrderItemsReserverQueueService orderItemsReserverQueueService,
        IOrderDeliveryService orderDeliveryService,
        IAppLogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _uriComposer = uriComposer;
        _basketRepository = basketRepository;
        _itemRepository = itemRepository;
        _orderItemsReserverQueueService = orderItemsReserverQueueService;
        _orderDeliveryService = orderDeliveryService;
        _logger = logger;
    }

    public async Task CreateOrderAsync(int basketId, Address shippingAddress)
    {
        var basketSpec = new BasketWithItemsSpecification(basketId);
        var basket = await _basketRepository.GetBySpecAsync(basketSpec);

        Guard.Against.NullBasket(basketId, basket);
        Guard.Against.EmptyBasketOnCheckout(basket.Items);

        var catalogItemsSpecification = new CatalogItemsSpecification(basket.Items.Select(item => item.CatalogItemId).ToArray());
        var catalogItems = await _itemRepository.ListAsync(catalogItemsSpecification);

        var items = basket.Items.Select(basketItem =>
        {
            var catalogItem = catalogItems.First(c => c.Id == basketItem.CatalogItemId);
            var itemOrdered = new CatalogItemOrdered(catalogItem.Id, catalogItem.Name, _uriComposer.ComposePicUri(catalogItem.PictureUri));
            var orderItem = new OrderItem(itemOrdered, basketItem.UnitPrice, basketItem.Quantity);
            return orderItem;
        }).ToList();

        var order = new Order(basket.BuyerId, shippingAddress, items);

        await _orderRepository.AddAsync(order);

        await _orderItemsReserverQueueService.ReserveOrderItemsAsync(items);

        await _orderDeliveryService.Process(order);
    }

    private async Task ReserveOrderItemsAsync(IList<OrderItem> orderItems)
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("x-functions-key", "R9yIRRBOPqGzlIrt9gSfBRrZdiBpykluxpTuCuzayDx4kau4aW8KBg==");

        var response = await httpClient.PostAsync("https://orderitemsreserver-io.azurewebsites.net/api/ReserveOrderItemsFunction", JsonContent.Create(orderItems));

        _logger.LogInformation($"Reserve order items response status code: {response.StatusCode}");
    }
}
