using System.Threading.Tasks;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public interface IInvitationRepository
    {
        Task<bool> CreateAsync(InvitationInfoViewModel model);
    }
}