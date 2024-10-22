using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using PriceCare.Web.Models;
using PriceCare.Web.Repository;

namespace PriceCare.Web.Controllers
{
    [RoutePrefix("api/user")]
    [Authorize]
    public class UserController : ApiController
    {

        private readonly IAccountRepository accountRepository;
        public UserController(IAccountRepository repository)
        {
            accountRepository = repository;
        }

        [Route("info")]
        public UserInfoViewModel GetUserInfo()
        {
            return accountRepository.GetUserInfo();
        }

        [Route("users")]
        [Authorize(Roles = "Super Admin, System Admin")]
        [HttpPost]
        public UserSearchResponseViewModel GetUsers(UserSearchRequestViewModel model)
        {
            return accountRepository.GetUsers(model);
        }

        [Route("usermapping")]
        [Authorize]
        [HttpGet]
        public List<SmallUserInfoViewModel> GetUserMapping()
        {
            return accountRepository.GetUserMapping();
        }

        [Route("lock")]
        [Authorize(Roles = "Super Admin, System Admin")]
        [HttpPost]
        public void Lock(UserLockViewModel userLock)
        {
            accountRepository.LockUser(userLock);
        }

        [Route("delete")]
        [Authorize(Roles = "Super Admin, System Admin")]
        [HttpPost]
        public void Delete(UserInfoViewModel userLock)
        {
            accountRepository.DeleteUser(userLock.Id);
        }

        [Route("activate")]
        [Authorize(Roles = "Super Admin, System Admin")]
        [HttpPost]
        public void Activate(UserInfoViewModel userLock)
        {
            accountRepository.ActivateUser(userLock.Id);
        }

        [Route("changeEmail")]
        [Authorize(Roles = "Super Admin, System Admin")]
        [HttpPost]
        public void ChangeEmail(UserInfoViewModel userInfo)
        {
            accountRepository.ChangeEmail(userInfo);
        }

        [Route("password")]
        public ResponseViewModel PostPassword(ManageUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                return accountRepository.PostPassword(model, User.Identity.GetUserName());
            }
            var messages = new List<object>();
            var values = ModelState.Values.ToList();
            var keys = ModelState.Keys.ToList();
            for (var i = 0; i < keys.Count; i++)
            {
                messages.Add(new { Message = values[i].Errors[0].ErrorMessage, Source = keys[i].Substring(6) });
            }
            return new ResponseViewModel {SourceErrors = messages, IsError = true};
        }

        [Route("save")]
        [HttpPost]
        public UserInfoViewModel PostUser(UserInfoViewModel model)
        {
            model.Id = User.Identity.GetUserId();
            return accountRepository.PostUser(model);
        }

        [Route("isEmailUnique")]
        [Authorize(Roles = "Super Admin, System Admin")]
        [HttpPost]
        public bool IsEMailUnique(UserInfoViewModel userInfo)
        {
            return accountRepository.IsEmailUnique(userInfo);
        }

        [Route("saveSettings")]
        [HttpPost]
        public bool SaveSettings(SaveSettingsRequest model)
        {
            string userId = User.Identity.GetUserId();
            return accountRepository.SaveSettings(userId, model.DefaultRegionId, model.DefaultCountryId, model.DefaultProductId);
        }

        [Route("settings")]
        public UserSettingsViewModel GetUserSettings()
        {
            var userId = User.Identity.GetUserId();
            return accountRepository.GetUserSettings(userId);
        }

        [Route("roles")]
        public IEnumerable<RoleInfoViewModel> GetAllRoles()
        {
            return accountRepository.GetAllRoles();
        }

        [Route("create")]
        public ResponseViewModel CreateUserAndAssignRoles(RegisterViewModel register)
        {
            return accountRepository.CreateUserAndAssignRoles(register, ModelState, Url);
        }

        [Route("updateRoles")]
        [HttpPost]
        public void UpdateRolesForUser(UpdateUserRolesViewModel userRoles)
        {
            accountRepository.UpdateRolesForUser(userRoles);
        }

        [Route("requestResetPassword")]
        [HttpPost]
        public void RequestResetPassword(ForgotPasswordViewModel passwordModel)
        {
            accountRepository.RequestResetPassword(passwordModel);
        }

        [Route("isValid")]
        [HttpPost]
        public UserFormResponseViewModel IsValid(UserFormRequestViewModel model)
        {
            return accountRepository.IsValid(model);
        }
    }
}