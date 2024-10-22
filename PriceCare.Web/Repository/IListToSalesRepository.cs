using System.Net.Http;
using PriceCare.Web.Models;
using System.Collections.Generic;

namespace PriceCare.Web.Repository
{
    public interface IListToSalesRepository
    {
        ListToSalesSearchResponseViewModel GetPagedListToSales(ListToSalesSeachRequestViewModel listToSalesSeachRequest);
        IEnumerable<ListToSalesViewModel> GetVersionListToSales(int versionId, List<int> geographyId, List<int> productId);
        IEnumerable<ListToSalesViewModel> GetForecastListToSales(int saveId, List<int> geographyId, List<int> productId);
        void SaveVersion(List<ListToSalesViewModel> listToSales);
        HttpResponseMessage GetExcel(string token);
    }
}