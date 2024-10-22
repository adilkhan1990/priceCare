using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class ListToSalesSaveModel
    {
        public List<ListToSalesViewModel> ListToSales { get; set; }
        public bool Validate { get; set; }
        public int LoadId { get; set; }
        public List<int> CountryIds { get; set; }
        public int ProductId { get; set; }
    }
}