using System;
using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class SimulationDimension
    {
        public int GeographyId { get; set; }
        public int ProductId { get; set; }
    }
    public class SimulationData
    {
        public int GeographyId { get; set; }
        public int ProductId { get; set; }
        public int CurrencyId { get; set; }
        public double UsdBudget { get; set; }
        public double EurBudget { get; set; }
        public CurrencyData[] Currencies { get; set; }
        public EventData[] Events { get; set; }
        public MonthlyData[] ReviewedPrice { get; set; }
        public MonthlyData[] TotalVolume { get; set; }
        public MonthlyData[] TotalSales { get; set; }
        public List<ListPrice> ListPrices { get; set; } 
    }
    public class CurrencyData
    {
        public DateTime DataTime { get; set; }
        public int VersionId { get; set; }
        public double UsdSpot { get; set; }
        public double EurSpot { get; set; }
    }
    public class EventData
    {
        public DateTime DataTime { get; set; }
        public int EventTypeId { get; set; }
        public string Description { get; set; }
        public double PriceImpact { get; set; }
        public List<EventImpact> Impact { get; set; } 
    }
    public class SegmentData
    {
        public int SegmentId { get; set; }
        public MonthlyData[] Data { get; set; }
    }
    public class EventImpact
    {
        public int GeographyId { get; set; }
        public int SegmentId { get; set; }
        public MonthlyData[] Impact { get; set; }
    }
    public class ListPrice
    {
        public int PriceTypeId { get; set; }
        public MonthlyData[] Data { get; set; }
    }
    public class MonthlyData
    {
        public DateTime DataTime { get; set; }
        public double Value { get; set; }
        public string Description { get; set; }
    }

    public enum SimulationTypes
    {
        Budget = 0,
        Reference = 1,
        Public = 2,
        User = 3
    }

}
