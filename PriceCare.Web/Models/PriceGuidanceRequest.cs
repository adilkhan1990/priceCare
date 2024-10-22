using System;
using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class PriceGuidanceRequest : RequestExcelDownload
    {
        public List<int> CountriesId { get; set; }                
        public int ProductId { get; set; }
        public int VersionId { get; set; }
        public int RuleTypeId { get; set; }
        public DateTime ApplicableFrom { get; set; }
        public int PageNumber { get; set; }
        public int ItemsPerPage { get; set; }
        public bool Validate { get; set; }

    }

    public class PriceGuidanceSaveModel
    {
        public List<PriceGuidanceViewModel> PriceGuidances { get; set; }
        public bool Validate { get; set; }
    }
}