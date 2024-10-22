using System;
using System.Collections.Generic;
using System.Net.Http;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public interface IPriceGuidanceRepository
    {
        PriceMapSearchResponse GetAllPriceGuidances(PriceGuidanceRequest priceGuidanceRequest);
        HttpResponseMessage GetExcel(string token);
        List<FilterItemViewModel> GetApplicableFromList(GetApplicableFromModel model);
        bool SavePriceMap(List<GprmRuleRowViewModel> model, DateTime applicableFrom, int ruleTypeId, int productId);
    }
}