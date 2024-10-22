using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class ValidateLoadItemDetailViewModel
    {
        public int LoadId { get; set; }
        public string LoadItemName { get; set; }
        public int ProductId { get; set; }
        public List<int> GeographyIds { get; set; }
    }
}