using System.Collections.Generic;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public interface IVersionRepository
    {
        IEnumerable<VersionViewModel> GetGprmRuleVersion(int geographyId = 0, int productId = 0, int gprmRuleTypeId = 0);
        IEnumerable<VersionViewModel> GetPriceMapVersion(int geographyId = 0, int productId = 0, int gprmRuleTypeId = 0);
        IEnumerable<VersionViewModel> GetPriceMapVersion(List<int> geographyId = null, int productId = 0, int gprmRuleTypeId = 0);
        IEnumerable<VersionViewModel> GetCurrencyVersion(RateType rateType);

        IEnumerable<VersionViewModel> GetDataVersion(List<int> geographyId, List<int> productId, List<int> dataTypeId);

        IEnumerable<VersionViewModel> GetListToSalesVersion(List<int> geographyId, List<int> productId);

        int CreateNewVersion(string information);
        int GetCurrentVersion();

        VersionViewModel GetVersionInfos(int versionId);
    }    
}

    

