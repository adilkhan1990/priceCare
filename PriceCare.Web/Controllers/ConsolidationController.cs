using System.Web.Http;
using PriceCare.Web.Repository;

namespace PriceCare.Web.Controllers
{
    [RoutePrefix("api/consolidation")]
    [Authorize]
    public class ConsolidationController : ApiController
    {
        private readonly IConsolidationRepository consolidationRepository;

        public ConsolidationController(IConsolidationRepository consolidationRepository)
        {
            this.consolidationRepository = consolidationRepository;
        }

        [Route("{productId:int}")]
        public object GetConsolidationForProduct(int productId)
        {
            return consolidationRepository.GetConsolidationForProduct(productId);
        }
    }
}