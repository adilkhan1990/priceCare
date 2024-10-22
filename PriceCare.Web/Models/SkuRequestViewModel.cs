using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class SkuRequestViewModel : RequestExcelDownload
    {
        public int FormulationId { get; set; }
        public int Status { get; set; }
        public int CountryId { get; set; }
        public int ProductId { get; set; }
        public int PageNumber { get; set; }
        public int ItemsPerPage { get; set; }
        public bool Validate { get; set; }

    }

    public class SkuResponseViewModel
    {
        public List<SkuViewModel> Skus { get; set; }
        public bool IsLastPage { get; set; }
        public int PageNumber { get; set; }
        public int TotalSkus { get; set; }
    }

    public class SkuSaveModel
    {
        public List<SkuViewModel> Skus { get; set; }
        public bool Validate { get; set; }
        public int LoadId { get; set; }
        public int CountryId { get; set; }
        public int ProductId { get; set; }
    }
}