namespace PriceCare.Web.Models
{
    public class GetCurrenciesRequest
    {
        public int VersionId { get; set; }
        public RateType RateType { get; set; } 
    }
}