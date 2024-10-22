using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using PriceCare.Central;
using PriceCare.Web.Exceptions;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public class InvitationRepository : IInvitationRepository
    {
        private readonly IAccountRepository accountRepository = new AccountRepository();
        //private readonly IRequestAccessRepository requestAccessRepository = new RequestAccessRepository();

        public async Task<bool> CreateAsync(InvitationInfoViewModel model)
        {
            using (var context = new PriceCareCentral())
            {
                if (!accountRepository.IsEmailUnique(model.Email))
                {
                    throw new NoAccessException("This mail exists already");
                }

                var userId = accountRepository.GetUserId();

                var invitation = new Invitation
                {
                    CreatedOn = DateTime.Now,
                    Email = model.Email,
                    Token = Guid.NewGuid().ToString(),
                    UserId = userId
                };

                context.Invitations.Add(invitation);

                context.SaveChanges();

                foreach (var role in model.Roles)
                {
                    invitation.InvitationRoles.Add(new InvitationRole{InvitationId = invitation.Id, RoleId = role.Id});                    
                }

                context.SaveChanges();

                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);

                var callbackUrl = urlHelper.Action("Register", "Account", new { token = invitation.Token }, HttpContext.Current.Request.Url.Scheme);

                var emailService = new EmailService();
                await emailService.SendAsync(new IdentityMessage
                {
                    Destination = invitation.Email,
                    Subject = "Price Care Invitation",
                    Body =
                        "You have been invited to use Price Care. Please click <a href=\"" + callbackUrl +
                        "\">here</a> to use our registration form."
                });
            }

            return true;
        }
    }
}