using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Models
{
    public class CountrySearchRequestViewModel
    {
        public int RegionId { get; set; }
        public int PageNumber { get; set; }
        public int ItemsPerPage { get; set; }
        public bool Validate { get; set; }
        public int Status { get; set; }
    }

    public enum SearchCountryStatus
    {
        Inactive = 0,
        Active = 1,
        All = 2
    }
}