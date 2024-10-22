using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class PriceTypeSearchResponseViewModel : RequestExcelDownload
    {
        public IEnumerable<PriceTypeViewModel> PriceTypes { get; set; }
        public int PageNumber { get; set; }
        public bool IsLastPage { get; set; }
        public int TotalPriceTypes { get; set; }
    }
}