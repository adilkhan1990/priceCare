using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using PriceCare.Web.Models;
using PriceCare.Web.Repository;

namespace PriceCare.Web.Controllers
{
    [RoutePrefix("api/country")]
    [Authorize]
    public class CountryController : ApiController
    {
        private readonly ICountryRepository countryRepository;
        private readonly ILoadRepository loadRepository;

        public CountryController(ICountryRepository countryRepository, ILoadRepository loadRepository)
        {
            this.countryRepository = countryRepository;
            this.loadRepository = loadRepository;
        }

        [Route("all")]
        public IEnumerable<CountryViewModel> GetCountries()
        {
            return countryRepository.GetAllCountries();
        }

        [Route("countries")]
        [HttpPost]
        public IEnumerable<CountryViewModel> GetCountries(RuleCountriesRequest model)
        {            
            var countriesId = new List<int>{model.ReviewedGeographyId};
            countriesId.AddRange(model.Basket.Select(b => b.ReferencedGeographyId));
            return countryRepository.GetRuleCountries(countriesId).Where(c => !countriesId.Contains(c.Id));
        }
        
        [Route("regioncountries")]
        public object GetRegionAndCountries()
        {
            return countryRepository.GetRegionsAndCountries();
        }

        [Route("pagedRegionAndCountries")]
        [HttpPost]
        public object GetPagedRegionsAndCountries(RegionAndCountriesSearchRequestViewModel regionAndCountriesSearch)
        {
            return countryRepository.GetPagedRegionsAndCountries(regionAndCountriesSearch);
        }

        [Route("allRegions")]
        public IEnumerable<RegionViewModel> GetAllRegions()
        {
            return countryRepository.GetAllRegions();
        }

        [Route("addRegion")]
        [HttpPost]
        public bool AddRegion(RegionViewModel model)
        {
            return countryRepository.AddRegion(model.Name);
        }

        [Route("deleteRegion")]
        [HttpPost]
        public bool DeleteRegion(CountryViewModel model)
        {
            return countryRepository.DeleteRegion(model.Id);
        }

        [Route("paged")]
        [HttpPost]
        public CountrySearchResponseViewModel GetPagedRegionsAndCountries(CountrySearchRequestViewModel countrySearch)
        {
            if (countrySearch.Validate)
                return loadRepository.GetCountryToValidate(countrySearch);
            return countryRepository.GetCountriesByRegionPaged(countrySearch);
        }

        [Route("save")]
        [HttpPost]
        public void SaveProduct(SaveCountryModel save)
        {
            if (save.Validate)
                countryRepository.SaveLoad(save);
            countryRepository.Save(save.Countries);
        }

        [Route("update")]
        [HttpPost]        
        public bool UpdateRegionCountries(List<UpdateCountryRegionsViewModel> model)
        {
            return countryRepository.UpdateRegionCountries(model);
        }
    }
}