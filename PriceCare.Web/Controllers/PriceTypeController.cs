using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using PriceCare.Web.Models;
using PriceCare.Web.Repository;

namespace PriceCare.Web.Controllers
{
    [RoutePrefix("api/pricetype")]
    [Authorize]
    public class PriceTypeController : ApiController
    {
        private readonly IPriceTypeRepository priceTypeRepository;
        private readonly ILoadRepository loadRepository = new LoadRepository();

        public PriceTypeController(IPriceTypeRepository priceTypeRepository)
        {
            this.priceTypeRepository = priceTypeRepository;
        }

        [Route("all/{currencyId}")]
        public IEnumerable<PriceTypeViewModel> GetAllPriceTypes(int currencyId)
        {
            return priceTypeRepository.GetAllPriceTypes(currencyId);
        }
        
        [Route("paged")]
        [HttpPost]
        public PriceTypeSearchResponseViewModel GetPagedPriceTypes(
            PriceTypeSearchRequestViewModel priceTypeSearchRequest)
        {
            if (priceTypeSearchRequest.Validate)
                return loadRepository.GetPriceTypesToValidate(priceTypeSearchRequest);
            return priceTypeRepository.GetPagedPriceTypes(priceTypeSearchRequest);
        }

        [Route("save")]
        [HttpPost]
        public void Save(PriceTypeSaveModel save)
        {
            if (save.Validate)
                priceTypeRepository.SaveLoad(save);
            priceTypeRepository.Save(save.PriceTypes);
        }

        [Route("add")]
        [HttpPost]
        public bool AddPriceType(PriceTypeViewModel model)
        {
            model.Status = true;
            return priceTypeRepository.AddPriceType(model);
        }


        [Route("isValid")]
        [HttpPost]
        public bool IsValid(PriceTypeViewModel priceType)
        {
            return priceTypeRepository.IsValid(priceType);
        }

        [Route("excel")]
        public HttpResponseMessage GetExcel([FromUri]string token)
        {
            return priceTypeRepository.GetExcel(token);
        }
    }
}