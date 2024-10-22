using System.Collections.Generic;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public interface IListToSalesImpactRepository
    {
        IEnumerable<ListToSalesImpactViewModel> GetVersionListToSalesImpact(int versionId, List<int> geographyId,
            List<int> productId);

        // todo add forecast ??

        ListToSalesImpactSearchResponseViewModel GetPagedListToSalesImpact(ListToSalesSeachRequestViewModel listToSalesSeachRequest);

        void SaveVersion(List<ListToSalesImpactViewModel> listToSalesImpact);
    }
}