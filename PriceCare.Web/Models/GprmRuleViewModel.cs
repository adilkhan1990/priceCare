using System;
using System.Collections.Generic;
using PriceCare.Web.Math.Utils;

namespace PriceCare.Web.Models
{
    public class RuleTypeViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class PriceMapSearchResponse
    {
        public GprmRuleDataTableViewModel DataTable { get; set; }
        public bool IsLastPage { get; set; }
        public int PageNumber { get; set; }

        public int TotalPriceMap { get; set; }
    }
    public class PriceMapViewModel : IRulegptaIdentifier
    {
        public PriceMapViewModel()
        {
            Edited = false;
            Active = true;
        }

        public string Geography { get; set; }
        public string ReviewedPriceType { get; set; }
        public DateTime ApplicableFrom { get; set; }
        public int GeographyId { get; set; }
        public int ProductId { get; set; }
        public int GprmRuleTypeId { get; set; }
        public int ReviewedPriceTypeId { get; set; }
        public double ReviewedPriceAdjustment { get; set; }
        public List<GprmReferencedPriceViewModel> ReferencedData { get; set; }
        public bool Default { get; set; }
        public bool Edited { get; set; }
        public bool Active { get; set; }
    }

    public class RuleViewModel : IRulegptaIdentifier
    {
        public RuleViewModel()
        {
            Active = true;
            Edited = false;
        }
        public bool Active { get; set; }
        public string Geography { get; set; }
        public string ReviewedPriceType { get; set; }
        public DateTime ApplicableFrom { get; set; }
        public int GeographyId { get; set; }
        public int ProductId { get; set; }
        public int GprmRuleTypeId { get; set; }
        public int ReviewedPriceTypeId { get; set; }
        public double ReviewedPriceAdjustment { get; set; }
        public bool Regular { get; set; }
        public int GprmMathId { get; set; }
        public int Argument { get; set; }
        public int WeightTypeId { get; set; }
        public int IrpRuleListId { get; set; }
        public int LookBack { get; set; }
        public int EffectiveLag { get; set; }
        public bool AllowIncrease { get; set; }
        public List<Basket> DefaultBasket { get; set; } 
        public List<SubRuleViewModel> ReferencedData { get; set; }
        public bool Default { get; set; }
        public bool Edited { get; set; }
    }

    public class SubRuleViewModel
    {
        public SubRuleViewModel()
        {
            Edited = false;
            Active = true;
        }
        public int SubRuleIndex { get; set; }
        public int GprmMathId { get; set; }
        public int Argument { get; set; }
        public int WeightTypeId { get; set; }
        public List<Basket> Basket { get; set; }
        public bool Default { get; set; }        
        public bool Edited { get; set; }
        public bool Active { get; set; }
    }

    public class SubRuleTable
    {
        public int GeographyId { get; set; }
        public int ProductId { get; set; }
        public int SubRuleIndex { get; set; }
        public int VersionId { get; set; }
        public int GprmRuleTypeId { get; set; }
        public int GprmMathId { get; set; }
        public double Argument { get; set; }
        public int WeightTypeId { get; set; }
        public int SaveTypeId  { get; set; }
        public int SaveId  { get; set; }
        public bool Active  { get; set; }
        public DateTime ApplicableFrom { get; set; }
    }

    public class Basket
    {
        public Basket()
        {
            Edited = false;
            Active = true;
        }

        public string ReferencedGeography { get; set; }
        public string ReferencedPriceType { get; set; }
        public int ReferencedGeographyId { get; set; }
        public int ReferencedPriceTypeId { get; set; }
        public double ReferencedPriceAdjustment { get; set; }
        public bool Default { get; set; }
        public List<PriceTypeViewModel> ReferencedPriceTypeOptions { get; set; }
        public bool Edited { get; set; }
        public string Tag { get; set; }
        public bool Active { get; set; }           
    }

    public class BasketTable
    {
        public int GeographyId { get; set; }
        public int ProductId { get; set; }
        public int SubRuleIndex { get; set; }
        public int ReferencedGeographyId  { get; set; }
        public int VersionId  { get; set; }
        public int GprmRuleTypeId { get; set; }
        public int ReferencedPriceTypeId { get; set; }
        public double ReferencedPriceAdjustment  { get; set; }
        public int SaveId  { get; set; }
        public int SaveTypeId  { get; set; }
        public bool Active  { get; set; }
        public DateTime ApplicableFrom  { get; set; }
    }
    public class GprmRuleViewModel : IGprmRuleDataViewModel
    {
        public DateTime ApplicableFrom { get; set; }
        public int GeographyId { get; set; }
        public int ProductId { get; set; }
        public int GprmRuleTypeId { get; set; }
        public bool Regular { get; set; }
        public int GprmMathId { get; set; }
        public int Argument { get; set; }
        public int WeightTypeId { get; set; }
        public int IrpRuleListId { get; set; }
	    public int LookBack { get; set; }
	    public int EffectiveLag { get; set; }
        public bool AllowIncrease { get; set; }
        public bool Default { get; set; }
        public bool Active { get; set; }
    }
    public class GprmSubRuleViewModel : IGprmRuleDataViewModel
    {
        public DateTime ApplicableFrom { get; set; }
        public int GeographyId { get; set; }
        public int ProductId { get; set; }
        public int SubRuleIndex { get; set; }
        public int GprmRuleTypeId { get; set; }
        public int GprmMathId { get; set; }
        public int Argument { get; set; }
        public int WeightTypeId { get; set; }
        public bool Default { get; set; }
        public bool Active { get; set; }
    }              
	public class GprmRuleDataTableViewModel
	{
        public List<string> Columns
        {
            get
            {
                return new List<string>{"Referencing", "Referenced"};
            }            
        }

        public List<GprmRuleRowViewModel> Rows { get; set; }           	
    }
    public class GprmRuleRowViewModel
    {
        public List<GprmRuleCellViewModel> Cells { get; set; }
        public GprmReviewedPriceViewModel ReviewedPrice { get; set; }       
    }
    public class GprmRuleCellViewModel
    {
        public bool Edited { get; set; }
        public bool IsEditable { get; set; }
        public string Text { get; set; }     
        public GprmReviewedPriceViewModel ReviewedPrice { get; set; }
        public GprmReferencedPriceViewModel ReferencedPrice { get; set; }
        public List<PriceTypeViewModel> PriceTypeOptions { get; set; } 
    }
    public class GprmReviewedPriceViewModel : IGprmRuleDataViewModel
	{
        public string Geography { get; set; }
        public string ReviewedPriceType { get; set; }
        public DateTime ApplicableFrom { get; set; }
        public int GeographyId { get; set; }
	    public int ProductId { get; set; }
	    public int GprmRuleTypeId { get; set; }
	    public int ReviewedPriceTypeId { get; set; }
        public double ReviewedPriceAdjustment { get; set; }
        public bool Default { get; set; }
        public bool Active { get; set; }
    }
    public class GprmReferencedPriceViewModel : IGprmRuleDataViewModel
    {
        public GprmReferencedPriceViewModel()
        {
            Edited = false;
            Active = true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ReferencedGeographyId*397) ^ SubRuleIndex;
            }
        }

        public string Geography { get; set; }
        public string ReferencedGeography { get; set; }
        public string ReferencedPriceType { get; set; }
        public DateTime ApplicableFrom { get; set; }
        public int GeographyId { get; set; }
        public int ProductId { get; set; }
        public int SubRuleIndex { get; set; }
        public int GprmRuleTypeId { get; set; }
        public int ReferencedGeographyId { get; set; }
        public int ReferencedPriceTypeId { get; set; }
        public double ReferencedPriceAdjustment { get; set; }
        public bool Default { get; set; }
        public bool Edited { get; set; }
        public bool Active { get; set; }

        public override bool Equals(Object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((GprmReferencedPriceViewModel) obj);
        }

        protected bool Equals(GprmReferencedPriceViewModel p)
        {
            return ReferencedGeographyId == p.ReferencedGeographyId && SubRuleIndex == p.SubRuleIndex;
        }
    }
    public class GprmBasketViewModel : IGprmRuleDataViewModel
    {
        public string Geography { get; set; }
        public string ReferencedGeography { get; set; }
        public DateTime ApplicableFrom { get; set; }
        public int GeographyId { get; set; }
        public int ProductId { get; set; }
        public int SubRuleIndex { get; set; }
        public int GprmRuleTypeId { get; set; }
        public int ReferencedGeographyId { get; set; }
        public bool Default { get; set; }
        public bool Active { get; set; }
    }
    public interface IGprmRuleDataViewModel
    {
        DateTime ApplicableFrom { get; set; }
        int GeographyId { get; set; }
        int ProductId { get; set; }
        int GprmRuleTypeId { get; set; }
        bool Default { get; set; }
        bool Active { get; set; }
    }
    public enum GprmRuleTypes
    {
        Default = 30,
        Launch = 32
    }
}