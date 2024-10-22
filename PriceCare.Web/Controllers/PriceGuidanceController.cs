using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using PriceCare.Web.Helpers;
using PriceCare.Web.Models;
using PriceCare.Web.Repository;
using PriceCare.Web.Models;

namespace PriceCare.Web.Controllers
{
    [RoutePrefix("api/priceguidance")]
    [Authorize]
    public class PriceGuidanceController : ApiController
    {
        private readonly IPriceGuidanceRepository priceGuidanceRepository = new PriceGuidanceRepository();

        [Route("all")]
        [HttpPost]
        public PriceMapSearchResponse GetPriceGuidances(PriceGuidanceRequest model)
        {
            return priceGuidanceRepository.GetAllPriceGuidances(model);
        }

        [Route("from")]
        [HttpPost]
        public List<FilterItemViewModel> GetApplicableFromList(GetApplicableFromModel model)
        {
            return priceGuidanceRepository.GetApplicableFromList(model);
        }

        [Route("excel")]
        public HttpResponseMessage GetExcel([FromUri]string token)
        {
            return priceGuidanceRepository.GetExcel(token);
        }

        [Route("savePriceMap")]
        [HttpPost]
        public bool SavePriceMap(SavePriceMapRequest model)
        {            
            return priceGuidanceRepository.SavePriceMap(model.Rows, model.ApplicableFrom, model.RuleTypeId, model.ProductId);
        }

    }
}