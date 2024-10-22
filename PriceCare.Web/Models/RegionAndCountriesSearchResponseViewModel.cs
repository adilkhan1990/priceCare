using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class RegionAndCountriesSearchResponseViewModel
    {
        public RegionAndCountriesViewModel RegionAndCountries { get; set; }
        public bool IsLastPage { get; set; }
        public int PageNumber { get; set; }

        public int TotalCountries { get; set; }
    }
}