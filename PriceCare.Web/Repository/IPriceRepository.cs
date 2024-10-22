using System.Net.Http;
using System.Collections.Generic;
using PriceCare.Web.Controllers;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public interface IPriceRepository
    {
        object GetPrices(PriceRequestViewModel model, bool fillData = true);
        object GetVersions(PriceRequestViewModel model);
        List<DataViewModel> GetDataCache(PriceRequestViewModel dataSearch);
    }
}