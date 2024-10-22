using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Models
{
    public class UpdateCountryRegionsRequest
    {
        public List<UpdateCountryRegionsViewModel> Updates { get; set; }
    }

    public class UpdateCountryRegionsViewModel
    {
        public int RegionId { get; set; }
        public int CountryId { get; set; }
        public bool IsAssign { get; set; }
    }
}