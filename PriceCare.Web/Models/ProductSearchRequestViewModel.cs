namespace PriceCare.Web.Models
{
    public class ProductSearchRequestViewModel
    {
        public int PageNumber { get; set; }
        public int ItemsPerPage { get; set; }
        public bool Validate { get; set; }
    }
}