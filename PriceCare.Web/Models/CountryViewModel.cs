using DocumentFormat.OpenXml.Office2010.Excel;

namespace PriceCare.Web.Models
{
    public class CountryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Iso2 { get; set; }
        public string Iso3 { get; set; }
        public string ExportName { get; set; }
        public int DisplayCurrencyId { get; set; }
        public string DisplayCurrency { get; set; }
        public int CurrencyId { get; set; }
        public int? RegionId { get; set; }
        public bool Active { get; set; }
        public string Tag { get; set; }

    }
}