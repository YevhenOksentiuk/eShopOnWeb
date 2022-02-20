using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.ApplicationCore.Services;

public class OrderDeliveryConfigProvider : IOrderDeliveryConfigProvider
{
    private readonly OrderDeliverySettings _config;

    public OrderDeliveryConfigProvider(OrderDeliverySettings config) => _config = config;

    public OrderDeliverySettings GetConfig()
    {
        return _config;
    }
}
