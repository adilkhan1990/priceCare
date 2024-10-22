using System.Collections.Generic;
using System.Net.Http;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public interface ISkuRepository
    {
        List<SkuViewModel> GetAllSkus(int countryId, int productId, int status, int formulationId);
        SkuResponseViewModel GetSkus(SkuRequestViewModel skuRequest);
        bool AddSku(SkuViewModel sku);
        bool SaveLoad(SkuSaveModel skuSaveModel);
        void Save(IEnumerable<SkuViewModel> skus);

        HttpResponseMessage GetExcel(string token);
    }
}