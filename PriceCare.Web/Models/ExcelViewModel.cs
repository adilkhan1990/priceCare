using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DocumentFormat.OpenXml.Office2013.Word;

namespace PriceCare.Web.Models
{
    public class ExcelDataViewModel
    {
        public int SaveId { get; set; }
        public bool DeactivatePreviousVersion { get; set; }
        public int SimulationId { get; set; }
        public List<ExcelData> Data { get; set; }
    }

    public class ExcelData
    {
        public int GeographyId { get; set; }
        public int ProductId { get; set; }
        public int DataTypeId { get; set; }
        public int PriceTypeId { get; set; }
        public DateTime DataTime { get; set; }
        public int SegmentId { get; set; }
        public int CurrencySpotId { get; set; }
        public int CurrencySpotVersionId { get; set; }
        public int EventTypeId { get; set; }
        public int UnitTypeId { get; set; }
        public double Value { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
    }

    public class ExcelCurrencyViewModel
    {
        public int SaveId { get; set; }
        public List<ExcelCurrency> CurrencySpot { get; set; }
        public List<ExcelCurrency> CurrencyBudget { get; set; } 

    }

    public class ExcelCurrency
    {
        public int CurrencyId { get; set; }
// ReSharper disable once InconsistentNaming
        public double USD { get; set; }
// ReSharper disable once InconsistentNaming
        public double EUR { get; set; }
    }

    public class ExcelListToSalesViewModel
    {
        public int SaveId { get; set; }
        public List<ExcelListToSales> ListToSales { get; set; }
        public List<ExcelListToSalesImpact> ListToSalesImpact { get; set; } 

    }

    public class ExcelListToSales
    {
        public int GeographyId { get; set; }
        public int ProductId { get; set; }
        public int CurrencySpotId { get; set; }
        public int CurrencySpotVersionId { get; set; }
        public int SegmentId { get; set; }
        public double Asp { get; set; }
        public double MarketPercentage { get; set; }
        public double ImpactPercentage { get; set; }

    }

    public class ExcelListToSalesImpact
    {
        public int GeographyId { get; set; }
        public int ProductId { get; set; }
        public int SegmentId { get; set; }
        public int ImpactDelay { get; set; }
        public double ImpactPercentage { get; set; }

    }

    public class ExcelPriceMapViewModel
    {
        public int SaveId { get; set; }
        public DateTime ApplicableFrom { get; set; }
        public List<ExcelReviewed> Reviewed { get; set; }
        public List<ExcelReferenced> Referenced { get; set; } 
    }

    public class ExcelReviewed
    {
        public int GeographyId { get; set; }
        public int ProductId { get; set; }
        public int GprmRuleTypeId { get; set; }
        public int ReviewedPriceTypeId { get; set; }
        public double ReviewedPriceAdjustment { get; set; }

    }

    public class ExcelReferenced
    {
        public int GeographyId { get; set; }
        public int ProductId { get; set; }
        public int SubRuleIndex { get; set; }
        public int ReferencedGeographyId { get; set; }
        public int VersionId { get; set; }
        public int GprmRuleTypeId { get; set; }
        public int ReferencedPriceTypeId { get; set; }
        public double ReferencedPriceAdjustment { get; set; }

    }

    public class ExcelPriceTypeViewModel
    {
        public int SaveId { get; set; }
        public bool DeactivatePreviousVersion { get; set; }
        public List<PriceTypeViewModel> Data { get; set; }
    }

    public class ExcelSkuViewModel
    {
        public int SaveId { get; set; }
        public bool DeactivatePreviousVersion { get; set; }
        public List<SkuViewModel> Data { get; set; }
    }
}