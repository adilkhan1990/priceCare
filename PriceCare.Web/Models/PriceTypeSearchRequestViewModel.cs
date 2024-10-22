using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class PriceTypeSearchRequestViewModel : RequestExcelDownload
    {
        public int PageNumber { get; set; }
        public int ItemsPerPage { get; set; }
        public string SearchText { get; set; }
        public int Status { get; set; }
        public bool Validate { get; set; }
    }

    public class PriceTypeSaveModel
    {
        public List<PriceTypeViewModel> PriceTypes { get; set; }
        public bool Validate { get; set; }
        public int LoadId { get; set; }
    }
}