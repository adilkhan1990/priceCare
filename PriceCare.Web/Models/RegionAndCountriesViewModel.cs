using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class RegionAndCountriesViewModel
    {
        public RegionViewModel Region { get; set; }
        public IEnumerable<CountryViewModel> Countries { get; set; }
    }
}