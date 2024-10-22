using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using PriceCare.Web.Models;
using PriceCare.Web.Repository;

namespace PriceCare.Web.Controllers
{
    [RoutePrefix("api/version")]
    public class VersionController : ApiController
    {
        private readonly IVersionRepository versionRepository;        

        public VersionController()
        {
            versionRepository = new VersionRepository();
        }

        [Route("rules")]
        [HttpPost]
        public List<VersionViewModel> GetVersions(RuleVersionRequest request) 
        {
            return versionRepository.GetGprmRuleVersion(request.GeographyId,request.ProductId, request.RuleTypeId).ToList();
        } 

        [Route("currency/{rateType}")]
        public List<VersionViewModel> GetCurrencyVersions(RateType rateType)
        {
            return versionRepository.GetCurrencyVersion(rateType).ToList();
        }
        [Route("eventType")]
        [HttpPost]
        public IEnumerable<VersionViewModel> GetEventTypesVersion(EventSearchRequestViewModel eventTypeSearch)
        {
            return versionRepository.GetDataVersion(eventTypeSearch.GeographyId,eventTypeSearch.ProductId,new List<int>((int) DataTypes.Event));
        }
        
        [Route("listToSales")]
        [HttpPost]
        public IEnumerable<VersionViewModel> GetListToSalesVersion(ListToSalesSeachRequestViewModel listToSalesSeach)
        {
            return versionRepository.GetListToSalesVersion(listToSalesSeach.CountriesId, listToSalesSeach.ProductsId);
        }

        [Route("priceMap")]
        [HttpPost]
        public List<VersionViewModel> GetVersionsPriceMap(PriceMapRequest request)
        {
            return versionRepository.GetPriceMapVersion(request.GeographyIds, request.ProductId, request.RuleTypeId).ToList();
        } 
    }

    public class RuleVersionRequest
    {
        public int ProductId { get; set; }
        public int GeographyId { get; set; }
        public int RuleTypeId { get; set; }
    }

    public class PriceMapRequest
    {
        public int ProductId { get; set; }
        public List<int> GeographyIds { get; set; }
        public int RuleTypeId { get; set; }
    }
}