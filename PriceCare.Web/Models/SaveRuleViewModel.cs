using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Models
{
    public class SaveRuleViewModel
    {
        public RuleDefinitionViewModel RuleDefinition { get; set; }
        public DateTime ApplicableFrom { get; set; }
        public string Geography { get; set; }
        public int GeographyId { get; set; }
        public int GprmRuleTypeId { get; set; }
        public int ProductId { get; set; }
    }

    public class CacheRuleViewModel : SaveRuleViewModel
    {
        public int SimulationId { get; set; }
    }
}