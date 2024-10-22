using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using PriceCare.Web.Math.Utils;

namespace PriceCare.Web.Models
{
    public class DataViewModel : IDataPermIdentifier, IProductIdentifier, IGeographySegmentDataTimeIdentifier, IGeographyDataTimePriceTypeIdentifier, IGeographyProductSegmentIdentifier
    {
        public DataViewModel()
        {
            Active = true;
            Edited = false;
            Reviewed = false;
            IsPublic = true;
        }
        public string PermId
        {
            get { return "" + GeographyId + "_" + ProductId + "_" + DataTypeId + "_" + PriceTypeId + "_" + DataTime.Ticks; }
        }

        public int GeographyId { get; set; }
        public int ProductId { get; set; }
        public int CurrencySpotId { get; set; }
        public int CurrencySpotVersionId { get; set; }
        public int PriceTypeId { get; set; }
        public int DataTypeId { get; set; }
	    public int EventTypeId { get; set; }
	    public int UnitTypeId { get; set; }
        public int SegmentId { get; set; }
        public DateTime DataTime { get; set; }
        public double? Value { get; set; }
	    public string Description { get; set; }
	    public double? UsdSpot { get; set; }
	    public double? EurSpot { get; set; }
	    public double? UsdBudget { get; set; }
        public double? EurBudget { get; set; }
        public double? PercentageVariation { get; set; }
        public bool Active { get; set; }
        public bool Edited { get; set; }
        public bool Reviewed { get; set; }

        public string Tag { get; set; }
        public object OldValue { get; set; }
        public double? PercentageVariationFromX { get; set; }
        public bool IsPublic { get; set; }
    }

    public class LatestUsdViewModel
    {
        public int CurrencyId { get; set; }
        public int VersionId { get; set; }
        public double UsdSpot { get; set; }
        public double UsdBudget { get; set; }
        public double EurSpot { get; set; }
        public double EurBudget { get; set; }
    }
    public class DataCurrencyViewModel
    {
        public int CurrencyId { get; set; }
        public int VersionId { get; set; }
        public double Usd { get; set; }
        public double Eur { get; set; }
    }

    public class PermIdViewModel
    {
        public int GeographyId { get; set; }
        public int ProductId { get; set; }
        public int PriceTypeId { get; set; }
        public int DataTypeId { get; set; }
        public int SegmentId { get; set; }
        public DateTime DataTime { get; set; }
        public bool Active { get; set; }
    }

    public enum DataTypes
    {
        Price = 41,
        Volume = 42,
        Event = 43,
        NetPrice = 51,
        Sales = 52
    }

    public class DataExportViewModel : DataViewModel
    {
        public string GeographyName { get; set; }
        public string ProductName { get; set; }
        public string EventName { get; set; }
        public string PriceType { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }

    public class SalesImpactViewModel
    {
        public string EventId { get; set; }
        public string Geography { get; set; }
        public string Product { get; set; }
        public DateTime EventDateTime { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string EventType { get; set; }
        public double Value { get; set; }
        public string ImpactedGeography { get; set; }
        public DateTime ImpactedDateTime { get; set; }
        public double Impact { get; set; }

    }

    public class SalesImpactSearchViewModel : RequestExcelDownload
    {
        public int SaveId { get; set; }
        public int SimulationId { get; set; }
        public IEnumerable<int> Products { get; set; }
        public IEnumerable<DataViewModel> Events { get; set; }
    }

    public class AspForecastSearchViewModel : RequestExcelDownload
    {
        public int SaveId { get; set; }
        public int SimulationId { get; set; }
        public IEnumerable<int> Countries { get; set; }
        public IEnumerable<int> Products { get; set; }
    }
    public class ExcelSearchRequestViewModel : RequestExcelDownload
    {
        public List<int> GeographyId { get; set; }
        public List<int> ProductId { get; set; }
        public List<int> EventTypeId { get; set; }
        public int DataTypeId { get; set; }
        public int? VersionId { get; set; }
        public int SimulationId { get; set; }
        public int SaveId { get; set; }
        public bool Validate { get; set; }
        public int ScenarioTypeId { get; set; }
        public string UserId { get; set; }
    }
    public class PriceRequestViewModel : RequestExcelDownload
    {
        public List<int> GeographyIds { get; set; }
        public int ProductId { get; set; }
        public int? VersionId { get; set; }
        public int SimulationId { get; set; }
        public int SaveId { get; set; }
        public bool Validate { get; set; }
        public bool CompareToSimulation { get; set; }
        public string UserId { get; set; }
        public int? CompareTo { get; set; }

    }

    public class CurrencyVersionViewModel
    {
        public int CurrencyId { get; set; }
        public int VersionId { get; set; }
    }
}