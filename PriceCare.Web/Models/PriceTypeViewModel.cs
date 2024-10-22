using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class PriceTypeViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public string StatusAsString { get; set; }
        public bool Status { get; set; }
        public string Tag { get; set; }
        public int GeographyId { get; set; }
    }

    public class PriceTypeExport : PriceTypeViewModel
    {
        public string CountryProduct { get; set; }
    }

    public class RulePriceTypeViewModel : PriceTypeViewModel
    {
        public List<PriceTypeViewModel> PriceTypeOptions { get; set; } 
    }

}