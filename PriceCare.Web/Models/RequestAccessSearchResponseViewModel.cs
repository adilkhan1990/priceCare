using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class RequestAccessSearchResponseViewModel
    {
        public IEnumerable<RequestAccessInfoViewModel> RequestAccesses { get; set; }
        public int PageNumber { get; set; }
        public bool IsLastPage { get; set; }

        public int TotalRequestAccess { get; set; }
    }
}