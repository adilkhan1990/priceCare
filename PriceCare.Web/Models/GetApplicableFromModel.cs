using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Models
{
    public class GetApplicableFromModel
    {
        public int VersionId { get; set; }
        public List<int> GeographyIds { get; set; }
        public int ProductId { get; set; }
        public int GprmRuleTypeId { get; set; }
    }
}