using PriceCare.Web.Controllers;
using PriceCare.Web.Models;
using System.Collections.Generic;
using PriceCare.Web.Models.Oracle;

namespace PriceCare.Web.Repository
{
    public interface ILoadRepository
    {
        IEnumerable<LoadViewModel> GetLoads(int status, int? loadId = null);
        LoadSearchResponseViewModel GetPagedLoads(LoadSearchRequest request);
        IEnumerable<FilterItemViewModel> GetLoadStatusForFilter();
        LoadDetailViewModel GetLoadDetail(int loadId);
        int Create(LoadViewModel load);
        void Cancel(int loadId);
        bool AnyItemForLoad(int loadId);
        string StartLoadSource(int loadId);
        CurrencySearchResponseViewModel GetCurrencyToValidate(CurrencySearchRequestViewModel currencySearchRequest);
        CountrySearchResponseViewModel GetCountryToValidate(CountrySearchRequestViewModel countrySearch);
        ProductSearchResponseViewModel GetProductToValidate(ProductSearchRequestViewModel productSearch);
        SkuResponseViewModel GetSkuToValidate(SkuRequestViewModel skuRequest);
        PriceTypeSearchResponseViewModel GetPriceTypesToValidate(PriceTypeSearchRequestViewModel priceTypeSearchRequest);
        void CreateProductGeographyScenario(int loadId, string loadItemName);
        void CreateLoadItemDetailScenarioForSku(int loadId);
        void CreateLoadItemDetailGeography(int loadId, string loadItemName);
        void ValidateLoadItemDetail(int loadId, string loadItemName, int productId, int geographyId);
        void ValidateLoadItem(int loadId, string loadItemName);
        void ValidateLoadItemId(int loadItemId);
        IEnumerable<PriceListModel> GetSkuPriceListExcelExport();
        void SavePriceList(List<PriceListModel> prices);
        List<LoadItemDetailViewModel> GetLoadItemDetailToValidate(int loadId, string loadItemName);
        List<DataViewModel> GetPriceToValidate(PriceRequestViewModel model, bool fillData = true);
        List<DataViewModel> GetVolumeToValidate(VolumeRequest model, bool fillData = true);
        List<DataViewModel> GetEventToValidate(EventSearchRequestViewModel model, bool fillData = true);
        List<VolumeScenarioViewModel> GetLoadedVolumeScenario();
        void UpdateLoadedVolumeScenario(List<VolumeScenarioViewModel> model);
        RuleDefinitionViewModel GetRulesToValidate(RuleRequest model);
        LoadViewModel GetLoad(int loadId);
        LoadDetailItemViewModel GetNextLoadItemToValidate(int loadId);
        LoadDetailItemViewModel GetLoadItem(int loadId, string loadItemName);
    }
}
