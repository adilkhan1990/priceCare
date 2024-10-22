using System.Collections.Generic;
using System.Web.Http;
using PriceCare.Web.Models;
using PriceCare.Web.Repository;

namespace PriceCare.Web.Controllers
{
    [RoutePrefix("api/listToSalesImpact")]
    [Authorize]
    public class ListToSalesImpactController : ApiController
    {
         private readonly IListToSalesImpactRepository listToSalesImpactRepository;

        public ListToSalesImpactController(IListToSalesImpactRepository listToSalesImpactRepository)
        {
            this.listToSalesImpactRepository = listToSalesImpactRepository;
        }

        [Route("paged")]
        [HttpPost]
        public ListToSalesImpactSearchResponseViewModel GetPagedListToSalesImpact(
            ListToSalesSeachRequestViewModel listToSalesSeach)
        {
            return listToSalesImpactRepository.GetPagedListToSalesImpact(listToSalesSeach);
        }

        [Route("saveVersion")]
        [HttpPost]
        public void SaveVersion(List<ListToSalesImpactViewModel> listToSalesImpact)
        {
            listToSalesImpactRepository.SaveVersion(listToSalesImpact);
        }
    }
}