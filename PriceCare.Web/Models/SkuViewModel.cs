using System;

namespace PriceCare.Web.Models
{
    public class SkuViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Dosage { get; set; }
        public double PackSize { get; set; }
        public string Formulation { get; set; }
        public bool Status { get; set; }
        public int GeographyId { get; set; }
        public int ProductId { get; set; }
        public string ProductNumber { get; set; }
        public int FormulationId { get; set; }
        public double FactorUnit { get; set; }
        public string Tag { get; set; }
        public object OldValue { get; set; }
        public bool Active { get; set; }
        public string Unit { get; set; }
    }

    public class SkuExportViewModel : SkuViewModel
    {
        public string GeographyName { get; set; }
        public string Product { get; set; }
        public string ProductName { get; set; }
        public string Unit { get; set; }

       // public string Sku { get; set; }
        //public int PriceTypeId { get; set; }
        //public string PriceType { get; set; }
        //public int CurrencySpotId { get; set; }
        //public DateTime EffectiveDate { get; set; }
        //public DateTime ExpirationDate { get; set; }
        //public double PricelistMcg { get; set; }
        //public int CurrencySpotVersionId { get; set; }
        //public int DataTypeId { get; set; }

        //public int EventTypeId { get; set; }
        //public int UnitTypeId { get; set; }
        //public int SegmentId { get; set; }
        //public DateTime DataTime { get; set; }
        //public double Value { get; set; }
        //public int VersionId { get; set; }
        //public int SaveTypeId { get; set; }
        //public int SaveId { get; set; }
        //public bool Active { get; set; }
    }


}