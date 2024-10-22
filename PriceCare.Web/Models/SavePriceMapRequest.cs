using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Models
{
    public class SavePriceMapRequest
    {
        public List<GprmRuleRowViewModel> Rows { get; set; }
        public int ProductId { get; set; }
        public int RuleTypeId { get; set; }
        public DateTime ApplicableFrom { get; set; }
    }
}