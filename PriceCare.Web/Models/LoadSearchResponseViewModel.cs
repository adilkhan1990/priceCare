using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class LoadSearchResponseViewModel
    {
        public IEnumerable<LoadViewModel> Loads { get; set; }
        public int PageNumber { get; set; }
        public bool IsLastPage { get; set; }
        public int TotalLoads { get; set; }
    }
}