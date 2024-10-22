using System;
using PriceCare.Web.Constants;

namespace PriceCare.Web.Models
{
    public class RuleRequest
    {
        public RuleRequest()
        {
            SystemId = ApplicationConstants.IrpLoadSystem;
        }
        public int VersionId { get; set; }
        public int SimulationId { get; set; }
        public int GeographyId { get; set; }
        public int ProductId { get; set; }
        public int GprmRuleTypeId { get; set; }
        public bool Forecast { get; set; }
        public DateTime ApplicableFrom { get; set; }
        public bool Validate { get; set; }
        public int SystemId { get; set; }
    }
}