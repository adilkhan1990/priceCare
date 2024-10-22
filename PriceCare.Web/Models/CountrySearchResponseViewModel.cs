using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class CountrySearchResponseViewModel
    {
        public IEnumerable<CountryViewModel> Countries { get; set; }
        public bool IsLastPage { get; set; }
        public int PageNumber { get; set; }
        public int TotalCountries { get; set; }
    }
}