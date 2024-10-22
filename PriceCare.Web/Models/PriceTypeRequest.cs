using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Models
{
    public class PriceTypeRequest
    {
        public int ProductId { get; set; }
        public int CountryId { get; set; }
        public DateTime ApplicableFrom { get; set; }
    }
}