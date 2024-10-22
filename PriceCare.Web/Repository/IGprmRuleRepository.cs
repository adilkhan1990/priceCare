using System.Collections.Generic;
using PriceCare.Web.Constants;
using PriceCare.Web.Models;
using System;

namespace PriceCare.Web.Repository
{
    public interface IGprmRuleRepository
    {
        void SavePriceMap(List<PriceMapViewModel> priceMap, int saveId);

        List<RuleViewModel> GetVersionRule(int versionId, int productId, DateTime startTime, int duration,
            int saveId = ApplicationConstants.ImportedDataSaveId);
        List<RuleTypeViewModel> GetGprmRuleTypes();

        List<PriceMapViewModel> GetVersionPriceMap(int versionId, int geographyId, int productId, int gprmRuleTypeId,
            DateTime applicationFrom, int saveId = ApplicationConstants.ImportedDataSaveId);
        List<RuleViewModel> GetVersionRule(int versionId, int geographyId, int productId, int gprmRuleTypeId, DateTime applicationFrom);
        IEnumerable<DateTime> GetPriceMapApplicableFrom(int versionId, int geographyId, int productId, int gprmRuleTypeId);
        IEnumerable<DateTime> GetRuleApplicableFrom(int versionId, int geographyId, int productId, int gprmRuleTypeId);
        RuleDefinitionViewModel GetRules(int versionId, int geographyId, int productId, int gprmRuleTypeId,
            DateTime applicableFrom);
        void SaveVersionRule(RuleViewModel ruleCachegptCache);
        void SaveVersionRule(RuleViewModel ruleCachegptCache, int saveId);
        void SaveVersionRule(List<RuleViewModel> rule, int saveId);
        void SavePriceMap(List<PriceMapViewModel> priceMap);
    }
}
