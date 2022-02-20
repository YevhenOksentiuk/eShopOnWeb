using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.ApplicationCore.Entities
{
    public class OrderDeliveryItem
    {
        public int CatalogItemId { get; set; }

        public string ProductName { get; set; }

        public int Units { get; set; }
    }
}
