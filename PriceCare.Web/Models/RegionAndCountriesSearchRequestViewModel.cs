namespace PriceCare.Web.Models
{
    public class RegionAndCountriesSearchRequestViewModel
    {
        public int RegionId { get; set; }
        public int PageNumber { get; set; }
        public int ItemsPerPage { get; set; }
        public int Status { get; set; }
    }
}