using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using PriceCare.Web.Helpers;
using PriceCare.Web.Models;
using PriceCare.Web.Repository;

namespace PriceCare.Web.Controllers
{
    [RoutePrefix("api/listToSales")]
    [Authorize]
    public class ListToSalesController : ApiController
    {
        private readonly IListToSalesRepository listToSalesRepository;
        private  readonly LoadRepository loadRepository = new LoadRepository();

        public ListToSalesController(IListToSalesRepository listToSalesRepository)
        {
            this.listToSalesRepository = listToSalesRepository;
        }

        [Route("paged")]
        [HttpPost]
        public ListToSalesSearchResponseViewModel GetPagedListToSales(ListToSalesSeachRequestViewModel listToSalesSeach)
        {
            if (listToSalesSeach.Validate)
                return loadRepository.GetListToSalesToValidate(listToSalesSeach);
            return listToSalesRepository.GetPagedListToSales(listToSalesSeach);
        }

        [Route("saveVersion")]
        [HttpPost]
        public void SaveVersion(ListToSalesSaveModel save)
        {
            listToSalesRepository.SaveVersion(save.ListToSales);
        }

        [Route("excel")]
        public HttpResponseMessage GetExcel([FromUri]string token)
        {
            return listToSalesRepository.GetExcel(token);
        }
    }
}