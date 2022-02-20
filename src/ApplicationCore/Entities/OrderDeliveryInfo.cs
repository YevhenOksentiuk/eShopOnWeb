using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.ApplicationCore.Entities
{
    public class OrderDeliveryInfo
    {
        public int OrderId { get; set; }

        public Address ShippingAddress { get; set; }

        public IEnumerable<OrderDeliveryItem> Items { get; set; }

        public decimal TotalPrice { get; set; }
    }
}
