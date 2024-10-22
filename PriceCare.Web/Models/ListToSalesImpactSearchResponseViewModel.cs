using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class ListToSalesImpactSearchResponseViewModel
    {
        public IEnumerable<ListToSalesImpactViewModel> ListToSalesImpacts { get; set; }
        public bool IsLastPage { get; set; }
        public int PageNumber { get; set; }
        public int TotalListToSales { get; set; }
    }
}