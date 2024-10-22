using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class SaveProductModel
    {
        public IEnumerable<ProductViewModel> Products { get; set; }
        public bool Validate { get; set; }
        public int LoadId { get; set; }
    }
}