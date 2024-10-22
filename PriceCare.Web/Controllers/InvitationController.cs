using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using PriceCare.Web.Models;
using PriceCare.Web.Repository;

namespace PriceCare.Web.Controllers
{
    [RoutePrefix("api/invitation")]
    [Authorize]
    public class InvitationController : ApiController
    {
        private readonly IInvitationRepository invitationRepository;
        private readonly IAccountRepository accountRepository;

        public InvitationController(IInvitationRepository invitationRepository, IAccountRepository accountRepository)
        {
            this.invitationRepository = invitationRepository;
            this.accountRepository = accountRepository;
        }

        //public InvitationController()
        //{
        //    accountRepository = new AccountRepository();
        //    invitationRepository = new InvitationRepository(accountRepository);
        //}

        [Route("roles")]
        public IEnumerable<RoleInfoViewModel> GetRoles()
        {
            return accountRepository.GetAvailableRoles();
        }

        [Route("isEmailUnique")]
        [HttpPost]
        public bool IsEmailUnique(EmailViewModel model)
        {
            return accountRepository.IsEmailUnique(model.Email);
        }

        [Route("create")]
        [HttpPost]
        public async Task<bool> Create(InvitationInfoViewModel model)
        {
            foreach (var key in ModelState.Keys)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState[key].Value.ToString()));
            }

            return await invitationRepository.CreateAsync(model);
        }

        [Route("createForAnyUser")]
        [HttpPost]
        public async Task<bool> CreateForAnyUser(InvitationInfoViewModel model)
        {
            foreach (var key in ModelState.Keys)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState[key].Value.ToString()));
            }

            return await invitationRepository.CreateAsync(model);
        }

        //[System.Web.Http.Route("request")]
        //[System.Web.Http.AllowAnonymous]
        //[System.Web.Http.HttpPost]
        //public async Task<bool> RequestAccount(RequestAccountViewModel model)
        //{
        //    foreach (var key in ModelState.Keys)
        //    {
        //        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState[key].Value.ToString()));
        //    }

        //    return await invitationRepository.RequestAsync(model);
        //}
    }
}