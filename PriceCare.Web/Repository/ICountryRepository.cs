using System.Collections.Generic;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public interface ICountryRepository
    {
        IEnumerable<RegionAndCountriesViewModel> GetRegionsAndCountries();
        RegionAndCountriesSearchResponseViewModel GetPagedRegionsAndCountries(RegionAndCountriesSearchRequestViewModel regionAndCountriesSearchRequest);
        IEnumerable<CountryViewModel> GetAllCountries();
        IEnumerable<CountryViewModel> GetRuleCountries(List<int> referencedCountriesId);
        IEnumerable<RegionViewModel> GetAllRegions();
        bool AddRegion(string regionName);
        bool DeleteRegion(int geographyId);
        CountrySearchResponseViewModel GetCountriesByRegionPaged(CountrySearchRequestViewModel countrySearch);
        void Save(IEnumerable<CountryViewModel> countries);
        void SaveLoad(SaveCountryModel saveCountryModel);
        IEnumerable<CountryViewModel> GetCountryExport();
        //bool UpdateRegionCountries(UpdateCountryRegionsRequest model);
        bool UpdateRegionCountries(List<UpdateCountryRegionsViewModel> model);
    }
}