using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceCare.Web.Models
{
    public class DescriptionViewModel
    {
        public string Comment { get; set; }
        public string Event { get; set; }
        public double? PercentageVariation { get; set; }
        public double? LaunchPrice { get; set; }
        public string Math { get; set; }
        public string ReviewedPrice { get; set; }
        public double? ReviewedPriceAdjustment { get; set; }
        public int? LookBack { get; set; }
        public int? EfffectiveLag { get; set; }
        public bool? AllowIncrease { get; set; }
        public List<string> Basket { get; set; }
    }
}
