using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class RuleDefinitionViewModel
    {        
        public int GprmMathId { get; set; }        
        public List<PriceTypeViewModel> ReviewedPriceTypeOptions { get; set; }
        public int SelectedReviewedPriceTypeId { get; set; }
        public double Adjustement { get; set; }
        public List<SubRuleViewModel> ReferencedData { get; set; }
        public List<Basket> DefaultBasket { get; set; }
        public bool Active { get; set; }
        public bool AllowIncrease { get; set; }
        public int Argument { get; set; }
        public bool Default { get; set; }
        public bool Edited { get; set; }
        public int EffectiveLag { get; set; }
        public int IrpRuleListId { get; set; }
        public int LookBack { get; set; }
        public bool Regular { get; set; }
        public int WeightTypeId { get; set; }
    }        
}