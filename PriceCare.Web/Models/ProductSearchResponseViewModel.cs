using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class ProductSearchResponseViewModel
    {
        public IEnumerable<ProductViewModel> Products { get; set; }
        public bool IsLastPage { get; set; }
        public int PageNumber { get; set; }

        public int TotalProducts { get; set; }
    }
}