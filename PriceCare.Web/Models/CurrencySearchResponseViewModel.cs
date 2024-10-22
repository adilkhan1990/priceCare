using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class CurrencySearchResponseViewModel
    {
        public IEnumerable<CurrencyViewModel> Currencies { get; set; }
        public int PageNumber { get; set; }
        public bool IsLastPage { get; set; }
        public int TotalCurrencies { get; set; }
    }
}