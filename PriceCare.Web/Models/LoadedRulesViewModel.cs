using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PriceCare.Web.Constants;

namespace PriceCare.Web.Models
{
    public class LoadedRulesViewModel
    {
        public LoadedRulesViewModel()
        {
            EffectiveLag = -1;
            LookBack = -1;
            GprmRuleTypeId = (int) GprmRuleTypes.Default;
        }
        public int ReferencedCountryId { get; set; }
        public int ReferencingCountryId { get; set; }
        public string IrpRule { get; set; }
        public int IrpRuleId { get; set; }
        public int GprmRuleTypeId { get; set; }
        public string ReferencedGeography { get; set; }
        public int EffectiveLag { get; set; }
        public int LookBack { get; set; }
        public string Information { get; set; }
        public string Comment { get; set; }
    }

}