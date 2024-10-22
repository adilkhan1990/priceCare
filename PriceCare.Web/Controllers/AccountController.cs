using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PriceCare.Web.Attributes;
using PriceCare.Web.Repository;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using PriceCare.Web.Models;
using PriceCare.Central;

namespace PriceCare.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IAccountRepository accountRepository;
        private readonly IInvitationRepository invitationRepository;
        private readonly IRequestAccessRepository requestAccessRepository;
        public AccountController(ApplicationUserManager userManager, IAccountRepository accountRepository, IInvitationRepository invitationRepository, IRequestAccessRepository requestAccessRepository)            
        {
            UserManager = userManager;
            this.accountRepository = accountRepository;
            this.invitationRepository = invitationRepository;
            this.requestAccessRepository = requestAccessRepository;
        }

        public ApplicationUserManager UserManager { get; private set; }

        [AllowAnonymous]
        public PartialViewResult GetLeftPanel()
        {
            var model = GetInfos();
            return PartialView(model);            
        }

        //
        // GET: /Account/Login
        [System.Web.Mvc.AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            return View();                                                 
        }

        //
        // POST: /Account/Login
        [System.Web.Mvc.HttpPost]
        [System.Web.Mvc.AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = UserManager.FindByName(model.Username);
                if (user != null)
                {
                    if (user.IsDeleted)
                    {
                        var errorMsg = "Your account has been deleted";
                        ModelState.AddModelError("Username", errorMsg);
                        return View(model);
                    }
                    if (!user.EmailConfirmed)
                    {
                        var errorMsg = "You did not confirm your email address yet. Please check your emails for the confirmation message";
                        ModelState.AddModelError("Username", errorMsg);
                        return View(model);
                    }
                    var validCredentials = await UserManager.FindAsync(model.Username, model.Password);

                    // When a user is lockedout, this check is done to ensure that even if the credentials are valid
                    // the user can not login until the lockout duration has passed
                    if (await UserManager.IsLockedOutAsync(user.Id))
                    {
                        var userDate = (DateTime)user.LockoutEndDateUtc;
                        // If he was blocked by an admin
                        if (userDate.Year == DateTime.MaxValue.Year)
                        {
                            var errorMsg = "Your account has been locked";
                            ModelState.AddModelError("Username", errorMsg);
                        }
                        else // if he was blocked because he failed to log in x times
                        {
                            var errorMsg = "Your account has been locked out for {0} minutes due to multiple failed login attempts";
                            ModelState.AddModelError("Username", string.Format(errorMsg, UserManager.DefaultAccountLockoutTimeSpan.Minutes));
                        }
                    }
                    // if user is subject to lockouts and the credentials are invalid
                    // record the failure and check if user is lockedout and display message, otherwise,
                    // display the number of attempts remaining before lockout
                    else if (await UserManager.GetLockoutEnabledAsync(user.Id) && validCredentials == null)
                    {
                        // Record the failure which also may cause the user to be locked out
                        await UserManager.AccessFailedAsync(user.Id);

                        string message;

                        if (await UserManager.IsLockedOutAsync(user.Id))
                        {
                            var userDate = (DateTime)user.LockoutEndDateUtc;
                            // If he was blocked by an admin
                            if (userDate.Year == DateTime.MaxValue.Year)
                            {
                                message = "Your account has been locked";
                            }
                            else // if he was blocked because he failed to log in x times
                            {
                                var errorMsg = "Your account has been locked out for {0} minutes due to multiple failed login attempts";
                                message = string.Format(errorMsg, UserManager.DefaultAccountLockoutTimeSpan.Minutes);
                            }
                        }
                        else
                        {
                            int accessFailedCount = await UserManager.GetAccessFailedCountAsync(user.Id);

                            int attemptsLeft =
                                Convert.ToInt32(UserManager.MaxFailedAccessAttemptsBeforeLockout) - accessFailedCount;
                            var errorMsg = "Invalid credentials. You have {0} more attempt(s) before your account gets locked out";
                            message = string.Format(errorMsg, attemptsLeft);

                        }

                        ModelState.AddModelError("Username", message);
                    }
                    else
                    {
                        // If the lock is not enabled but the credentials are not good
                        if (validCredentials == null)
                        {
                            var errorMsg = "Invalid username or password";
                            ModelState.AddModelError("Username", errorMsg);
                        }
                        else
                        {

                            await SignInAsync(user, model.RememberMe);

                            user.LastConnectionDate = DateTime.Now;

                            await UserManager.UpdateAsync(user);

                            // When token is verified correctly, clear the access failed count used for lockout
                            await UserManager.ResetAccessFailedCountAsync(user.Id);

                            return RedirectToLocal(returnUrl);
                        }
                    }

                }
                else
                {
                    var errorMsg = "Invalid username or password";
                    ModelState.AddModelError("Username", errorMsg);
                }
            }

            // If we got this far, something failed, redisplay form            
            return View(model);                        
        }

        //
        // GET: /Account/Register
        [System.Web.Mvc.AllowAnonymous]
        public ActionResult Register(string token)
        {
            using (var entities = new PriceCareCentral())
            {
                var infos = entities.GeneralInformations.First();
                if (token != null && !string.IsNullOrWhiteSpace(token))
                {
                    var invitation = entities.Invitations.SingleOrDefault(i => i.Token == token);
                    if (invitation != null)
                    {
                        if (invitation.UsedOn == null)
                        {
                            return View(new RegisterViewModel
                            {
                                Invitation = new InvitationViewModel
                                {
                                    Token = token,
                                    State = TokenState.Valid,
                                    CreatedOn = invitation.CreatedOn,
                                    InvitedBy = GetUserFullName(UserManager.FindById(invitation.UserId))
                                },
                                Email = invitation.Email
                            });
                        }

                        return View(new RegisterViewModel
                        {
                            Invitation = new InvitationViewModel
                            {
                                Token = token,
                                State = TokenState.AlreadyUsed
                            }
                        });
                    }

                    return View(new RegisterViewModel
                    {
                        Invitation = new InvitationViewModel
                        {
                            Token = token,
                            State = TokenState.Invalid
                        }
                    });

                }
                return View();
            }                        
        }
        
        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            //TODO Remove the invitation validation when we open public

            if (!accountRepository.IsEmailUniqueForInvitation(model.Email)) // don't test emails on invitation
            {
                ModelState.AddModelError("Email", "This email exists already");
            }

            if (ModelState.IsValid && model.Invitation != null && !string.IsNullOrWhiteSpace(model.Invitation.Token))
            {
                var user = new ApplicationUser() { UserName = model.Username, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName };

                IdentityResult result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (model.Invitation != null && !string.IsNullOrWhiteSpace(model.Invitation.Token))
                    {
                        using (var entities = new PriceCareCentral())
                        {
                            var invitation = entities.Invitations.SingleOrDefault(i => i.Token == model.Invitation.Token);

                            if (invitation == null)
                            {
                                model.Invitation.State = TokenState.Invalid;
                                return View(model);
                            }

                            if (invitation.UsedOn != null)
                            {
                                model.Invitation.State = TokenState.AlreadyUsed;
                                return View(model);
                            }

                            invitation.UsedOn = DateTime.Now;
                            entities.SaveChanges();
                            var invitationRoles = entities.InvitationRoles.Where(ir => ir.InvitationId == invitation.Id);
                            foreach (var invitationRole in invitationRoles)
                            {
                                var role = accountRepository.GetRoleById(invitationRole.RoleId);
                                await UserManager.AddToRoleAsync(user.Id, role.Name);
                            }
                        }
                    }
                    else
                    {
                        await UserManager.AddToRoleAsync(user.Id, "Viewer");

                    }

                    // TODO: VALIDATE
                    //await SignInAsync(user, isPersistent: false);

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return View("DisplayEmail");
                    //return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("Username", error);
                    }
                }
            }            
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult RequestAccess()
        {            
            return View();                                               
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        [HttpPost]
        public ActionResult RequestAccess(RequestAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                requestAccessRepository.Create(model);
                return RedirectToAction("Login");
            }
            return View(model);                        
        }

        //
        // GET: /Account/ConfirmEmail
        [System.Web.Mvc.AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }

            IdentityResult result = await UserManager.ConfirmEmailAsync(userId, code);
            if (result.Succeeded)
            {                
                return View("ConfirmEmail");                                
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ForgotPassword
        [System.Web.Mvc.AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();                        
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);
                //if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                if (user == null)
                {
                    ModelState.AddModelError("Email", "The user either does not exist or is not confirmed.");
                    return View();
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");

                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new {userId = user.Id, code},
                    Request.Url.Scheme);
                await
                    UserManager.SendEmailAsync(user.Id, "Reset Password",
                        "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form             
            return View(model);                        
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [System.Web.Mvc.AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();                        
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {            
            if (code == null)
            {
                return View("Error");
            }
            return View();                        
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("Email", "No user found.");
                    return View();
                }
                IdentityResult result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("ResetPasswordConfirmation");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("Password", error);
                }
                return View();
            }

            // If we got this far, something failed, redisplay form            
            return View(model);                        
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        
        private string GetUserFullName(ApplicationUser user)
        {
            var fullName = "";

            if (user.FirstName != null)
            {
                fullName += user.FirstName;
            }

            if (user.LastName != null)
            {
                if (!string.IsNullOrWhiteSpace(fullName))
                {
                    fullName += " ";
                }

                fullName += user.LastName;
            }

            return fullName;
        }

        ////
        //// POST: /Account/Disassociate
        //[System.Web.Mvc.HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        //{
        //    ManageMessageId? message = null;
        //    IdentityResult result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
        //    if (result.Succeeded)
        //    {
        //        var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
        //        await SignInAsync(user, isPersistent: false);
        //        message = ManageMessageId.RemoveLoginSuccess;
        //    }
        //    else
        //    {
        //        message = ManageMessageId.Error;
        //    }
        //    return RedirectToAction("Manage", new { Message = message });
        //}

        //
        //// GET: /Account/Manage
        //public ActionResult Manage(ManageMessageId? message)
        //{
        //    ViewBag.StatusMessage =
        //        message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
        //        : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
        //        : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
        //        : message == ManageMessageId.Error ? "An error has occurred."
        //        : "";
        //    ViewBag.HasLocalPassword = HasPassword();
        //    ViewBag.ReturnUrl = Url.Action("Manage");
        //    return View();
        //}

        ////
        //// POST: /Account/Manage
        //[System.Web.Mvc.HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Manage(ManageUserViewModel model)
        //{
        //    bool hasPassword = HasPassword();
        //    ViewBag.HasLocalPassword = hasPassword;
        //    ViewBag.ReturnUrl = Url.Action("Manage");
        //    if (hasPassword)
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
        //            if (result.Succeeded)
        //            {
        //                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
        //                await SignInAsync(user, isPersistent: false);
        //                return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
        //            }
        //            else
        //            {
        //                AddErrors(result);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        // User does not have a password so remove any validation errors caused by a missing OldPassword field
        //        ModelState state = ModelState["OldPassword"];
        //        if (state != null)
        //        {
        //            state.Errors.Clear();
        //        }

        //        if (ModelState.IsValid)
        //        {
        //            IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
        //            if (result.Succeeded)
        //            {
        //                return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
        //            }
        //            else
        //            {
        //                AddErrors(result);
        //            }
        //        }
        //    }

        //    // If we got this far, something failed, redisplay form
        //    return View(model);
        //}

        //
        // POST: /Account/ExternalLogin
        [System.Web.Mvc.HttpPost]
        [System.Web.Mvc.AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback
        [System.Web.Mvc.AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var user = await UserManager.FindAsync(loginInfo.Login);
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // If the user does not have an account, then prompt the user to create an account
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        ////
        //// POST: /Account/LinkLogin
        //[System.Web.Mvc.HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult LinkLogin(string provider)
        //{
        //    // Request a redirect to the external login provider to link a login for the current user
        //    return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Account"), User.Identity.GetUserId());
        //}

        //
        // GET: /Account/LinkLoginCallback
        //public async Task<ActionResult> LinkLoginCallback()
        //{
        //    var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
        //    if (loginInfo == null)
        //    {
        //        return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
        //    }
        //    IdentityResult result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
        //    if (result.Succeeded)
        //    {
        //        return RedirectToAction("Manage");
        //    }
        //    return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
        //}

        ////
        //// POST: /Account/ExternalLoginConfirmation
        //[System.Web.Mvc.HttpPost]
        //[System.Web.Mvc.AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        //{
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        return RedirectToAction("Manage");
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        // Get the information about the user from the external login provider
        //        var info = await AuthenticationManager.GetExternalLoginInfoAsync();
        //        if (info == null)
        //        {
        //            return View("ExternalLoginFailure");
        //        }
        //        var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };
        //        IdentityResult result = await UserManager.CreateAsync(user);
        //        if (result.Succeeded)
        //        {
        //            result = await UserManager.AddLoginAsync(user.Id, info.Login);
        //            if (result.Succeeded)
        //            {
        //                await SignInAsync(user, isPersistent: false);

        //                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
        //                // Send an email with this link
        //                // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
        //                // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
        //                // SendEmail(user.Email, callbackUrl, "Confirm your account", "Please confirm your account by clicking this link");

        //                return RedirectToLocal(returnUrl);
        //            }
        //        }
        //        AddErrors(result);
        //    }

        //    ViewBag.ReturnUrl = returnUrl;
        //    return View(model);
        //}

        //
        // POST: /Account/LogOff
        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgery]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Login", "Account");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [System.Web.Mvc.AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult RemoveAccountList()
        {
            var linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId());
            ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
            return (ActionResult)PartialView("_RemoveAccountPartial", linkedAccounts);
        }

        private GeneralInformationsViewModel GetInfos()
        {
            using (var entities = new PriceCareCentral())
            {
                var infos = entities.GeneralInformations.First();
                var model = new GeneralInformationsViewModel
                {
                    AmgenSupportContactEmail = infos.ContactMail,
                    TechnicalSupportEmail = infos.TechnicalSupportMail
                };
                return model;
            }           
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }
            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, await user.GenerateUserIdentityAsync(UserManager));
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                var source = (error.ToLower().Contains("name") ? "Username" : "");
                ModelState.AddModelError(source, error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private void SendEmail(string email, string callbackUrl, string subject, string message)
        {
            // For information on sending mail, please visit http://go.microsoft.com/fwlink/?LinkID=320771
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}