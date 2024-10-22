using System.Net.Http;
using System.Web.Http;
using PriceCare.Web.Models;
using PriceCare.Web.Repository;

namespace PriceCare.Web.Controllers
{
    [RoutePrefix("api/sku")]
    [Authorize]
    public class SkuController : ApiController
    {
        private readonly ISkuRepository skuRepository;
        private readonly ILoadRepository loadRepository = new LoadRepository();

        public SkuController(ISkuRepository skuRepository)
        {
            this.skuRepository = skuRepository;
        }

        [Route("")]
        [HttpPost]
        public SkuResponseViewModel GetSku(SkuRequestViewModel model)
        {
            if (model.Validate)
                return loadRepository.GetSkuToValidate(model);
            return skuRepository.GetSkus(model);            
        }

        [Route("save")]
        [HttpPost]
        public object SaveSku(SkuSaveModel save)
        {
            if (save.Validate)
                return skuRepository.SaveLoad(save);

            skuRepository.Save(save.Skus);
            return null;
        }

        [Route("addSku")]
        [HttpPost]
        public bool AddSku(SkuViewModel model)
        {
            return skuRepository.AddSku(model);
        }

        [Route("excel")]
        public HttpResponseMessage GetExcel([FromUri]string token)
        {
            return skuRepository.GetExcel(token);
        }

    }
}