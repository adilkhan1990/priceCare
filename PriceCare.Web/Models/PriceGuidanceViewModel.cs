using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class PriceGuidanceViewModel
    {
        public int Id { get; set; }
        public string Referencing { get; set; }
        public IEnumerable<string> Referenced { get; set; }
        public bool Validate { get; set; }
    }
}