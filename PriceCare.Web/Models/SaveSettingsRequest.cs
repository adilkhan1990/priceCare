using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Models
{
    public class SaveSettingsRequest
    {
        public int DefaultRegionId { get; set; }
        public int DefaultCountryId { get; set; }
        public int DefaultProductId { get; set; }
    }
}