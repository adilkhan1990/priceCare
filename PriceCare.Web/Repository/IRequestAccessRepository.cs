using System.Collections.Generic;
using System.Threading.Tasks;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public interface IRequestAccessRepository
    {
        void Create(RequestAccountViewModel requestAccount);
        IEnumerable<RequestAccessStatusViewModel> GetRequestAccessStatus();
        RequestAccessSearchResponseViewModel GetPaged(RequestAccessSearchRequestViewModel requestAccessSearch);
        Task<bool> ChangeStatus(RequestAccessChangeStatusViewModel requestAccessChangeStatus);
    }
}