using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PriceCare.Web.Helpers;
using PriceCare.Web.Models;
using PriceCare.Web.Repository;

namespace PriceCare.Web.Controllers
{
    [RoutePrefix("api/currency")]
    [Authorize]
    public class CurrencyController : ApiController
    {
        private readonly ICurrencyRepository currencyRepository;
        private readonly ILoadRepository loadRepository;

        public CurrencyController(ICurrencyRepository currencyRepository, ILoadRepository loadRepository)
        {
            this.currencyRepository = currencyRepository;
            this.loadRepository = loadRepository;
        }

        [Route("all")]
        [HttpPost]
        public IEnumerable<CurrencyViewModel> GetCurrencies(GetCurrenciesRequest model)
        {
            return currencyRepository.GetAllCurrencies(model.RateType, model.VersionId);            
        }

        [Route("filter")]
        public IEnumerable<FilterItemViewModel> GetActiveCurrenciesForFilter()
        {
            return currencyRepository.GetActiveCurrenciesForFilter();
        }

        [Route("paged")]
        [HttpPost]
        public CurrencySearchResponseViewModel GetPaged(CurrencySearchRequestViewModel currencySearchRequest)
        {
            if (currencySearchRequest.Validate)
                return loadRepository.GetCurrencyToValidate(currencySearchRequest);
            return currencyRepository.GetPagedCurrencies(currencySearchRequest);
        }

        [Route("save")]
        [HttpPost]
        public void SaveCurrencies(SaveCurrenciesModel model)
        {
            if(model.Validate)
                currencyRepository.SaveLoadCurrencies(model);
            else
                currencyRepository.SaveCurrencies(model);
        }

        [Route("excel")]
        public HttpResponseMessage GetExcel([FromUri]string token, [FromUri]bool isBudget)
        {
            return currencyRepository.GetExcel(token, isBudget);
        }
    }
}