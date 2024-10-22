using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class DimensionDictionarySearchResponseViewModel
    {
        public IEnumerable<DimensionDictionaryModel> DimensionDictionary { get; set; }
        public int PageNumber { get; set; }
        public bool IsLastPage { get; set; }
        public int TotalRows { get; set; }
    }
}