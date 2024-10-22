using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class SaveCountryModel
    {
        public IEnumerable<CountryViewModel> Countries { get; set; }
        public bool Validate { get; set; }
        public int LoadId { get; set; }
    }
}