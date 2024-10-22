namespace PriceCare.Web.Models
{
    public class CurrencySearchRequestViewModel : RequestExcelDownload
    {
        public int PageNumber { get; set; }
        public int ItemsPerPage { get; set; }
        public int RateTypeId { get; set; }
        public int VersionId { get; set; }
        public bool Validate { get; set; }
    }
}