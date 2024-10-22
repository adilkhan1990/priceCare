using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Models
{
    public class RuleCountriesRequest
    {
        public int ReviewedGeographyId { get; set; }
        public List<GprmBasketViewModel> Basket { get; set; }
    }
}