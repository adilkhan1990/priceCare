using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using PriceCare.Web.Models;
using PriceCare.Web.Repository;

namespace PriceCare.Web.Controllers
{
    [RoutePrefix("api/requestaccess")]
    [Authorize]
    public class RequestAccessController : ApiController
    {
        private readonly IRequestAccessRepository requestAccessRepository;

        public RequestAccessController(IRequestAccessRepository requestAccessRepository)
        {
            this.requestAccessRepository = requestAccessRepository;
        }

        [Route("paged")]
        [HttpPost]
        public RequestAccessSearchResponseViewModel GetPaged(RequestAccessSearchRequestViewModel requestAccessSearch)
        {
            return requestAccessRepository.GetPaged(requestAccessSearch);
        }

        [Route("changeStatus")]
        [HttpPost]
        public Task<bool> ChangeStatus(RequestAccessChangeStatusViewModel requestAccessChangeStatus)
        {
            return requestAccessRepository.ChangeStatus(requestAccessChangeStatus);
        }

        [Route("status")]
        public IEnumerable<RequestAccessStatusViewModel> GetRequestStatus()
        {
            return requestAccessRepository.GetRequestAccessStatus();
        }
    }
}