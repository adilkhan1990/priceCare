using System.Collections.Generic;
using System.Net.Http;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public interface ICurrencyRepository
    {
        IEnumerable<CurrencyViewModel> GetAllCurrencies(RateType rateType, int versionId);
        IEnumerable<FilterItemViewModel> GetActiveCurrenciesForFilter();
        void SaveCurrencies(SaveCurrenciesModel saveCurrencies, bool updateCurrency = true);
        void SaveLoadCurrencies(SaveCurrenciesModel saveCurrencies);
        CurrencySearchResponseViewModel GetPagedCurrencies(CurrencySearchRequestViewModel currencySearchRequest);
        HttpResponseMessage GetExcel(string token, bool isBudget);
    }
}