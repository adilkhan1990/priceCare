using System.Collections.Generic;
using PriceCare.Web.Math.Utils;

namespace PriceCare.Web.Models
{
    public class ListToSalesViewModel : IGeographyProductSegmentIdentifier
    {
        public ListToSalesViewModel()
        {
            Active = true;
            SaveId = 1;
            SaveTypeId = 39;
            CurrencySpotId = 118;
            CurrencySpotVersionId = 1;
        }
        public int Id { get; set; }
        public int GeographyId { get; set; }
        public int ProductId { get; set; }
        public int CurrencySpotId { get; set; }
        public int CurrencySpotVersionId { get; set; }
        public int SegmentId { get; set; }
        public double Asp { get; set; }
        public double MarketPercentage { get; set; }
        public double ImpactPercentage { get; set; }
        public int VersionId { get; set; }
        public int SaveId { get; set; }
        public int SaveTypeId { get; set; }
        public bool Active { get; set; }
        public double AspDisplay { get; set; }
        public double AspUsdBudget { get; set; }
        public double AspEurBudget { get; set; }
        public string GeographyName { get; set; }
        public string SegmentName { get; set; }
        public string Tag { get; set; }
        public object OldValue { get; set; }
        public double PercentageVariationFromX { get; set; }
    }

    public class ListToSalesImpactViewModel : IGeographyProductSegmentIdentifier
    {
        public ListToSalesImpactViewModel()
        {
            Active = true;
            SaveId = 1;
            SaveTypeId = 39;
        }
        public int Id { get; set; }
        public int GeographyId { get; set; }
        public int ProductId { get; set; }
        public int SegmentId { get; set; }
        public int ImpactDelay { get; set; }
        public double ImpactPercentage { get; set; }
        public int VersionId { get; set; }
        public int SaveId { get; set; }
        public int SaveTypeId { get; set; }
        public bool Active { get; set; }
        public string GeographyName { get; set; }
        public string SegmentName { get; set; }
    }


    public class ListToSalesExportViewModel : ListToSalesViewModel
    {
        public string ProductName { get; set; }
        public string Currency { get; set; }
        public double EurSpot { get; set; }
        public double UsdSpot { get; set; }
        public string Segment { get; set; }
        public double M1 { get; set; }
        public double M2 { get; set; }
        public double M3 { get; set; }
        public double M4 { get; set; }
        public double M5 { get; set; }
        public double M6 { get; set; }
        public double M7 { get; set; }
        public double M8 { get; set; }
        public double M9 { get; set; }
        public double M10 { get; set; }
        public double M11 { get; set; }
        public double M12 { get; set; }
    }
}