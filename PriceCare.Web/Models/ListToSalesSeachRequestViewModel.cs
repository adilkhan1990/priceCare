using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class ListToSalesSeachRequestViewModel : RequestExcelDownload
    {
        public List<int> ProductsId { get; set; }
        public List<int> CountriesId { get; set; }
        public int VersionId { get; set; }
        public int PageNumber { get; set; }
        public int ItemsPerPage { get; set; }
        public bool Validate { get; set; }
    }
}