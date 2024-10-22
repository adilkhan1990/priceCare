using System;
using PriceCare.Web.Math.Utils;

namespace PriceCare.Web.Models
{
    public class CacheTables
    {
        public CacheTables(string userId)
        {
            Data = "CacheData_" + userId.Replace("-", "_");
            Rule = "CacheRule_" + userId.Replace("-", "_");
            SubRule = "CacheSubRule_" + userId.Replace("-", "_");
            Basket = "CacheBasket_" + userId.Replace("-", "_");
        }

        public string Data { get; set; }
        public string Rule { get; set; }
        public string SubRule { get; set; }
        public string Basket { get; set; }
    }
    public class CacheViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int SaveId { get; set; }
        public int DataVersionId { get; set; }
        public int CurrencyBudgetVersionId { get; set; }
        public int AssumptionsSaveId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public int SimulationCurrencyId { get; set; }
        public bool Edited { get; set; }

        public bool IsCurrentUser { get; set; }
        public bool IsLaunch { get; set; }
    }

    public class RuleCache : IRulegptaIdentifier
    {
        public int SimulationId { get; set; }
        public int GeographyId { get; set; }
        public int ProductId { get; set; }
        public int GprmRuleTypeId { get; set; }
        public DateTime ApplicableFrom { get; set; }
        public bool Regular { get; set; }
        public int GprmMathId { get; set; }
        public int Argument { get; set; }
        public int WeightTypeId { get; set; }
        public int IrpRuleListId { get; set; }
        public int LookBack { get; set; }
        public int EffectiveLag { get; set; }
        public bool AllowIncrease { get; set; }
        public int ReviewedPriceTypeId { get; set; }
        public double ReviewedPriceAdjustment { get; set; }
        public string Geography { get; set; }
        public string ReviewedPriceType { get; set; }
        public bool Edited { get; set; }
        public bool IsDefault { get; set; }
        public bool Active { get; set; }
    }

    public class SubRuleCache : IRulegptaIdentifier
    {
        public int SimulationId { get; set; }
        public int GeographyId { get; set; }
        public int ProductId { get; set; }
        public int GprmRuleTypeId { get; set; }
        public int SubRuleIndex { get; set; }
        public DateTime ApplicableFrom { get; set; }
        public int GprmMathId { get; set; }
        public int Argument { get; set; }
        public int WeightTypeId { get; set; }
        public bool Edited { get; set; }
        public bool IsDefault { get; set; }
        public bool Active { get; set; }
    }

    public class BasketCache : IRulegptaIdentifier
    {
        public int SimulationId { get; set; }
        public int GeographyId { get; set; }
        public int ProductId { get; set; }
        public int GprmRuleTypeId { get; set; }
        public int SubRuleIndex { get; set; }
        public int ReferencedGeographyId { get; set; }
        public DateTime ApplicableFrom { get; set; }
        public int ReferencedPriceTypeId { get; set; }
        public double ReferencedPriceAdjustment { get; set; }
        public bool IsDefault { get; set; }
        public bool Edited { get; set; }
        public string ReferencedGeography { get; set; }
        public string ReferencedPriceType { get; set; }
        public bool Active { get; set; }

    }
}