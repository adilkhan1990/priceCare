using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class SaveCurrenciesModel
    {
        public List<CurrencyViewModel> Currencies { get; set; }
        public RateType RateType { get; set; }
        public bool Validate { get; set; }
        public int LoadId { get; set; }
    }
}