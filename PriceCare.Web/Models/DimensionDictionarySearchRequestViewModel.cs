namespace PriceCare.Web.Models
{
    public class DimensionDictionarySearchRequestViewModel
    {
        public int PageNumber { get; set; }
        public int ItemsPerPage { get; set; }
        public string DimensionType { get; set; }
    }
}