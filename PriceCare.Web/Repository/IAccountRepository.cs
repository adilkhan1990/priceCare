using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;
using System.Web.Http.Routing;
using Microsoft.AspNet.Identity.EntityFramework;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public interface IAccountRepository
    {
        IIdentity GetUserIdentity();
        ClaimsIdentity GetUserClaimsIdentity();
        int GetUserAccountId();
        string GetUserId();
        bool IsEmailUnique(string email);
        bool IsEmailUnique(UserInfoViewModel user);
        bool IsUsernameUnique(string username);
        bool CreateUser(RegisterViewModel model);
        ResponseViewModel CreateUserAndAssignRoles(RegisterViewModel model, ModelStateDictionary modelState, UrlHelper url);
        List<UserInfoViewModel> GetUsers();
        UserSearchResponseViewModel GetUsers(UserSearchRequestViewModel userSearch);
        List<SmallUserInfoViewModel> GetUserMapping();  
        IEnumerable<string> GetCurrentUserRoles();
        IEnumerable<RoleInfoViewModel> GetAllRoles();
        RoleUserViewModels GetRoleById(string id);
        IEnumerable<RoleInfoViewModel> GetAvailableRoles();
        IEnumerable<RoleInfoViewModel> GetAvailableRoles(string id);
        IEnumerable<RoleInfoViewModel> GetAvailableRoles(IEnumerable<string> roleUser, IEnumerable<RoleInfoViewModel> allRoles);
        UserInfoViewModel GetUserInfo();
        UserInfoViewModel GetUserInfo(string id);
        IEnumerable<IdentityUserClaim> GetClaims(string id);
        void RemoveRoleForUser(string userid, string roleid);
        UserInfoViewModel PostUser(UserInfoViewModel model);
        UserInfoViewModel PostAnyUser(UserInfoViewModel model);
        ResponseViewModel PostPassword(ManageUserViewModel model, string userName);
        Task<string> ResetUserPassword(string userId, UrlHelper url);
        bool IsEmailUniqueForInvitation(string email);        
        void LockUser(UserLockViewModel userLock);
        void DeleteUser(string id);
        void ActivateUser(string id);
        void ChangeEmail(UserInfoViewModel userInfo);
        bool SaveSettings(string userId, int defaultRegionId, int defaultCountryId, int defaultProductId);
        UserSettingsViewModel GetUserSettings(string userId);
        void UpdateRolesForUser(UpdateUserRolesViewModel updateUser);

        void RequestResetPassword(ForgotPasswordViewModel model);

        /// <summary>
        /// Checks if a user is valid
        ///     - Check if the mail is unique
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        UserFormResponseViewModel IsValid(UserFormRequestViewModel form);
    }
}