using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.ModelBinding;
using System.Web.Http.Routing;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using PriceCare.Web.Constants;
using PriceCare.Web.Exceptions;
using PriceCare.Web.Helpers;
using PriceCare.Web.Models;


namespace PriceCare.Web.Repository
{

    public class AccountRepository : IAccountRepository
    {
        private readonly string connectionString = DataBaseConstants.ConnectionString;

        private ApplicationUserManager userManager;
        private ApplicationUserManager UserManager
        {
            get
            {
                if (userManager == null)
                {
                    HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>().Configuration.LazyLoadingEnabled = true;
                    userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                }
                return userManager;
            }
        }

        public IIdentity GetUserIdentity()
        {
            return HttpContext.Current.User.Identity;
        }
        public ClaimsIdentity GetUserClaimsIdentity()
        {
            return (ClaimsIdentity)GetUserIdentity();
        }
        public int GetUserAccountId()
        {
            var identity = GetUserClaimsIdentity();

            IEnumerable<Claim> claims = identity.Claims;

            var accountClaim = claims.FirstOrDefault(claim => claim.Type == ClaimsConstants.Account);
            
            if(accountClaim == null)
                throw new NoAccessException("You don't have an account");

            var accoundId = int.Parse(accountClaim.Value);

            return accoundId;
        }
        public void LockUser(UserLockViewModel userLock)
        {
            var userIdConnected = GetUserId();

            var user = UserManager.Users.FirstOrDefault(u => u.Id == userLock.UserId);

            if (user == null)
                throw new ItemNotFoundException("The user specified doesn't exist");

            if (userIdConnected == userLock.UserId)
                throw new ArgumentException("You can't lock yourself");

            var isUserLocked = user.LockoutEndDateUtc != null &&
                   DateTime.Compare(DateTime.Now, (DateTime)user.LockoutEndDateUtc) < 0;

            var lockoutEndDateUtc = isUserLocked ? null : (DateTime?)DateTime.MaxValue;

            user.LockoutEndDateUtc = lockoutEndDateUtc;

            UserManager.Update(user);
        }
        public void DeleteUser(string id)
        {
            var userIdConnected = GetUserId();

            var user = UserManager.Users.FirstOrDefault(u => u.Id == id);

            if(user == null)
                throw new ArgumentException("This user does not exist");

            if(userIdConnected == id)
                throw new ArgumentException("You can't delete yourself");

            user.IsDeleted = true;
            user.DeletedBy = userIdConnected;
            user.DateDeleted = DateTime.Now;
            user.LockoutEndDateUtc = null;

            UserManager.Update(user);
        }

        public void ActivateUser(string id)
        {
            var userIdConnected = GetUserId();

            var user = UserManager.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
                throw new ArgumentException("This user does not exist");

            if (userIdConnected == id)
                throw new ArgumentException("You can't activate yourself");

            user.IsDeleted = false;
            user.DeletedBy = null;
            user.DateDeleted = null;
            user.LockoutEndDateUtc = null;

            UserManager.Update(user);

        }

        public List<UserInfoViewModel> GetUsers()
        {
            var context = new ApplicationDbContext();
            var result = new List<UserInfoViewModel>();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var users = UserManager.Users.Where(u => !u.IsDeleted).OrderBy(u => u.UserName).ToList();

            foreach (var u in users)
            {
                var roleNames = new List<string>();
                foreach (var userRole in u.Roles)
                {
                    var roleName = roleManager.Roles.FirstOrDefault(r => r.Id == userRole.RoleId);
                    if (roleName != null)
                    {
                        roleNames.Add(roleName.Name);
                    }
                }

                var userIsLocked = u.LockoutEndDateUtc != null &&
                                   DateTime.Compare(DateTime.Now, (DateTime)u.LockoutEndDateUtc) < 0;

                var userViewModel = new UserInfoViewModel
                {
                    Id = u.Id,
                    Username = u.UserName,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Roles = roleNames,
                    IsUserLocked = userIsLocked,
                    LastConnectionDate = u.LastConnectionDate
                };
                result.Add(userViewModel);
            }

            return result;
        }

        public UserSearchResponseViewModel GetUsers(UserSearchRequestViewModel userSearch)
        {
            var context = new ApplicationDbContext();
            var userList = new List<UserInfoViewModel>();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            var users = UserManager.Users.OrderBy(u => u.UserName).ToList();

            if (!string.IsNullOrEmpty(userSearch.SearchText))
            {
                users = users.Where(u =>
                    u.Email.IndexOf(userSearch.SearchText, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                    u.UserName.IndexOf(userSearch.SearchText, StringComparison.CurrentCultureIgnoreCase) >= 0).ToList();
            }

            if (userSearch.RoleId != "0")
            {
                users = users.Where(u => u.Roles.Any(r => r.RoleId == userSearch.RoleId)).ToList();
            }

            switch (userSearch.Status)
            {
                case (int)UserStatus.AllStatus:
                    break;
                case (int) UserStatus.Deleted:
                    users = users.Where(u => u.IsDeleted).ToList();
                    break;
                case (int) UserStatus.Locked:
                    users = users.Where(u => u.LockoutEndDateUtc != null &&
                                             DateTime.Compare(DateTime.Now, (DateTime) u.LockoutEndDateUtc) < 0)
                        .ToList();
                    break;
                case (int) UserStatus.Active:
                    users = users.Where(u => !u.IsDeleted && u.LockoutEndDateUtc == null).ToList();
                    break;
            }

            var totalUsers = users.Count;

            // pagination
            users = users
                .Skip(userSearch.PageNumber*userSearch.ItemsPerPage)
                .Take(userSearch.ItemsPerPage)
                .ToList();

            foreach (var u in users)
            {
                var roleNames = new List<string>();
                foreach (var userRole in u.Roles)
                {
                    var roleName = roleManager.Roles.FirstOrDefault(r => r.Id == userRole.RoleId);
                    if (roleName != null)
                    {
                        roleNames.Add(roleName.Name);
                    }
                }

                var userIsLocked = u.LockoutEndDateUtc != null &&
                                   DateTime.Compare(DateTime.Now, (DateTime) u.LockoutEndDateUtc) < 0;

                var userViewModel = new UserInfoViewModel
                {
                    Id = u.Id,
                    Username = u.UserName,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Roles = roleNames,
                    IsUserLocked = userIsLocked,
                    IsDeleted = u.IsDeleted,
                    LastConnectionDate = u.LastConnectionDate
                };
                userList.Add(userViewModel);
            }


            var viewModel = new UserSearchResponseViewModel
            {
                Users = userList,
                IsLastPage = (userList.Count + (userSearch.PageNumber * userSearch.ItemsPerPage)) >= totalUsers,
                Page = ++userSearch.PageNumber,
                TotalUsers = totalUsers
            };

            return viewModel;
        }

        public List<SmallUserInfoViewModel> GetUserMapping()
        {
            var users = UserManager.Users.Select(u=>new SmallUserInfoViewModel
            {
                Id = u.Id,
                Username = u.UserName,
                FirstName = u.FirstName,
                LastName = u.LastName
            }).ToList();

            return users;
        }

        public IEnumerable<string> GetCurrentUserRoles()
        {
            var context = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            var user = UserManager.FindById(GetUserId());

            var roleUser = new List<string>();

            foreach (string id in user.Roles.Select(r => r.RoleId).ToList())
            {
                var role = roleManager.Roles.FirstOrDefault(r => r.Id == id);
                if (role != null)
                    roleUser.Add(role.Name);
            }
            return roleUser;
        }
        public IEnumerable<RoleInfoViewModel> GetAllRoles()
        {
            var context = new ApplicationDbContext();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            return roleManager.Roles.Select(r => new RoleInfoViewModel
            {
                Id = r.Id,
                Name = r.Name,
                Count = r.Users.Count()
            }).ToList();
        }

        public RoleUserViewModels GetRoleById(string id)
        {
            var context = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            var role = roleManager.Roles.FirstOrDefault(r => r.Id == id);

            if (role == null)
                throw new ItemNotFoundException("The role specified does not exist");
            
            var users = UserManager.Users
                .Where(u => !u.IsDeleted  && u.Roles
                    .Select(r => r.RoleId)
                    .Contains(role.Id))
                .Select(u => new UserInfoViewModel { Id = u.Id, Username = u.UserName, FirstName = u.FirstName, LastName = u.LastName })
                .ToList();

            return new RoleUserViewModels
            {
                Id = id,
                Name = role.Name,
                Users = users
            };
        }
        public IEnumerable<RoleInfoViewModel> GetAvailableRoles()
        {
            return GetAvailableRoles(GetUserId());
        }
        public IEnumerable<RoleInfoViewModel> GetAvailableRoles(string id)
        {
            return GetAvailableRoles(GetCurrentUserRoles(), GetAllRoles());
        }
        public IEnumerable<RoleInfoViewModel> GetAvailableRoles(IEnumerable<string> roleUser, IEnumerable<RoleInfoViewModel> allRoles)
        {
            //if (roleUser.Contains("Admin"))
            //{
            //    return allRoles;
            //}
            //if (roleUser.Contains("Support"))
            //{
            //    return allRoles.Where(r => r.Name == "Support" || r.Name == "Manager" || r.Name == "User").ToList();
            //}
            //if (roleUser.Contains("Manager"))
            //{
            //    return allRoles.Where(r => r.Name == "Manager" || r.Name == "User").ToList();
            //}
            //if (roleUser.Contains("User"))
            //{
            //    return allRoles.Where(r => r.Name == "User").ToList();
            //}
            return allRoles;
        }
        public string GetUserId()
        {
            return GetUserIdentity().GetUserId();
        }
        public bool IsEmailUnique(string email)
        {
            return UserManager.Users.Count(u => u.Email == email) == 0;
        }

        public bool IsEmailUnique(UserInfoViewModel user)
        {
            return !UserManager.Users.Any(u => u.Id != user.Id && u.Email == user.Email);
        }
        public bool IsUsernameUnique(string username)
        {
            return UserManager.Users.Count(user => user.UserName == username) == 0;
        }

        public ResponseViewModel CreateUserAndAssignRoles(RegisterViewModel model, ModelStateDictionary modelState, UrlHelper urlGenerator)
        {
            var responseModel = new ResponseViewModel();

            if (!modelState.IsValid)
            {
                var sourceError = new List<object>();
                var values = modelState.Values.ToList();
                var keys = modelState.Keys.ToList();
                for (var i = 0; i < values.Count; i++)
                {
                    sourceError.Add(new { Message = values[i].Errors[0].ErrorMessage, Source = keys[i].Substring(9) });
                }
                responseModel.SourceErrors = sourceError;
                responseModel.IsError = true;
            }

            BackendCheckIntegrity(model, responseModel);

            if (responseModel.IsError)
                return responseModel;

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                EmailConfirmed = true
            };

            IdentityResult result = UserManager.Create(user, model.Password);
            if (result.Succeeded)
            {
                foreach (var role in model.Roles)
                {
                    UserManager.AddToRole(user.Id, role.Name);
                }

                var url = urlGenerator.Link("isDefault", new { action = "Login", controller = "Account" });
                var mail =
                    String.Format(
                        "A new account was created for you.<br/>Username: {0}<br/>Password: {1}<br/>Website: <a href=\"{2}\">Login</a>",
                        user.UserName, model.Password, url);
                UserManager.SendEmail(user.Id, "Account Pricecare", mail);
            }
            else
            {
                var messages = new List<object>();
                foreach (var error in result.Errors)
                {
                    messages.Add(new { Message = error, Source = "Password" });
                }
                responseModel.SourceErrors = responseModel.SourceErrors.Concat(messages).ToList(); 
            }

            responseModel.IsError = responseModel.SourceErrors.Any();

            return responseModel;
        }

        private ResponseViewModel BackendCheckIntegrity(RegisterViewModel model, ResponseViewModel response)
        {
            var messages = new List<object>();

            if (!IsUsernameUnique(model.Username))
               messages.Add(new{Message="The user name is already taken, please choose another one", Source="Username"});
            
            if (!IsEmailUnique(model.Email))
                messages.Add(new { Message = "The email is already taken, please choose another one", Source = "Email" });
            
            if (model.Roles.Count == 0)
                messages.Add(new {Message = "Choose at least one role", Source = "Roles"});

            response.SourceErrors = response.SourceErrors.Concat(messages).ToList();
            response.IsError = response.SourceErrors.Any();
            return response;
        }

        public bool CreateUser(RegisterViewModel model)
        {
            if (!IsUsernameUnique(model.Username))
            {
                throw new ArgumentException("The user name is already taken, please choose another one");
            }
            if (!IsEmailUnique(model.Email))
            {
                throw new ArgumentException("The email is already taken, please choose another one");
            }

            var userConnected = UserManager.FindById(GetUserId());
            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,                
            };

            IdentityResult result = UserManager.Create(user, model.Password);
            if (result.Succeeded)
            {
                UserManager.AddToRole(user.Id, "User");

                var identity = (ClaimsIdentity)GetUserIdentity();
                IEnumerable<Claim> claims = identity.Claims;

                var accountClaim = claims.FirstOrDefault(cl => cl.Type == ClaimsConstants.Account);
                if (accountClaim == null)
                    throw new NoAccessException("You don't have any claims...");

                var claim = new Claim(ClaimsConstants.Account, accountClaim.Value);

                UserManager.AddClaim(user.Id, claim);
            }
            else
            {
                throw new NoAccessException("An error occured when creating the new user...");
            }

            return true;
        }
        public UserInfoViewModel GetUserInfo(string id)
        {
            var context = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            var user = UserManager.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
                throw new ItemNotFoundException("The user specified doesn't exist...");

            var roleNames = new List<string>();
            foreach (var userRole in user.Roles)
            {
                var roleName = roleManager.Roles.FirstOrDefault(r => r.Id == userRole.RoleId);
                if (roleName != null)
                {
                    roleNames.Add(roleName.Name);
                }
            }

            var userIsLocked = user.LockoutEndDateUtc != null &&
                                  DateTime.Compare(DateTime.Now, (DateTime)user.LockoutEndDateUtc) < 0;

            var userViewModel = new UserInfoViewModel
            {
                Id = id,
                Username = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roleNames,
                IsUserLocked = userIsLocked,                
                Email = user.Email,
                LastConnectionDate = user.LastConnectionDate
            };

            return userViewModel;
        }
        public UserInfoViewModel GetUserInfo()
        {
            return GetUserInfo(GetUserId());
        }
        public IEnumerable<IdentityUserClaim> GetClaims(string id)
        {
            var user = UserManager.Users.FirstOrDefault(u => !u.IsDeleted && u.Id == id);

            if (user == null)
                throw new ItemNotFoundException("The user specified doesn't exist...");

            return user.Claims.AsEnumerable();
        }        
        public void RemoveRoleForUser(string userid, string roleid)
        {
            var context = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            var factoryErrorMsgs = new StringBuilder();

            if (UserManager.Users.FirstOrDefault(u => u.Id == userid) == null)
            {
                factoryErrorMsgs.Append("The user specified doesn't exist...");
            }

            if (roleManager.Roles.FirstOrDefault(r => r.Id == roleid) == null)
            {
                factoryErrorMsgs.Append("The role specified doesn't exist...");
            }

            if (factoryErrorMsgs.Length > 0)
                throw new ArgumentException(factoryErrorMsgs.ToString());

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("RoleId", roleid),
                new SqlParameter("UserId", userid)
            };

            var arrayParams = parameters.ToArray();

            context.Database.ExecuteSqlCommand("Delete From AspNetUserRoles WHERE RoleId = @RoleId AND UserId = @UserId", arrayParams);
        }       
              
        public UserInfoViewModel PostUser(UserInfoViewModel model)
        {
            var context = new ApplicationDbContext();

            var result = IsValid(new UserFormRequestViewModel { User = model, CreatePurpose = false });
            if(result.FirstNameEmpty)
                throw new ArgumentException("The first name is required");
            if(result.LastNameEmpty)
                throw new ArgumentException("The last name is required");
            if(result.EmailEmpty)
                throw new ArgumentException("The mail is required");
            if (!result.EmailFormat) 
                throw new ArgumentException("The format is not correct");
            if (!result.EmailUnique)
                throw new ArgumentException("The email is already exists");

            var user = UserManager.FindById(model.Id);

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.UserName = model.Username;
            user.Email = model.Email;

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("id", user.Id),
                new SqlParameter("firstname", user.FirstName),
                new SqlParameter("lastname", user.LastName),                
                new SqlParameter("email", user.Email),
            };

            var arrayParams = parameters.ToArray();

            context.Database.ExecuteSqlCommand("UPDATE AspNetUsers " +
                                               "SET Email = @email, FirstName = @firstname, LastName = @lastname " +
                                               "WHERE Id = @id", arrayParams);

            return model;
        }

        public UserInfoViewModel PostAnyUser(UserInfoViewModel model)
        {
            var context = new ApplicationDbContext();
            var factoryErrorMsgs = new StringBuilder();

            if (string.IsNullOrEmpty(model.FirstName))
            {
                factoryErrorMsgs.Append("The first name is required.");
            }

            if (string.IsNullOrEmpty(model.LastName))
            {
                factoryErrorMsgs.Append("The last name is required.");
            }

            if (string.IsNullOrEmpty(model.Email))
            {
                factoryErrorMsgs.Append("The email is required.");
            }
            if (factoryErrorMsgs.Length > 0)
                throw new ArgumentException(factoryErrorMsgs.ToString());

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("id", model.Id),
                new SqlParameter("firstname", model.FirstName),
                new SqlParameter("lastname", model.LastName),                
                new SqlParameter("email", model.Email),
            };

            var arrayParams = parameters.ToArray();

            context.Database.ExecuteSqlCommand("UPDATE AspNetUsers " +
                                               "SET Email = @email, FirstName = @firstname, LastName = @lastname " +
                                               "WHERE Id = @id", arrayParams);

            return model;
        }

        public ResponseViewModel PostPassword(ManageUserViewModel model, string userName)
        {
            try
            {
                var user = UserManager.FindByName(userName);

                IdentityResult result = UserManager.ChangePassword(user.Id, model.OldPassword, model.ConfirmPassword);
                if (result.Succeeded)
                {
                    return new ResponseViewModel { Messages = new List<string> { "Reset password successful." }, IsError = false };
                }

                return new ResponseViewModel { Messages = result.Errors.ToList(), IsError = true };
            }
            catch (Exception ex)
            {
                return new ResponseViewModel { Messages = new List<string> { ex.Message }, IsError = true };
            }
        }

        public async Task<string> ResetUserPassword(string userId, UrlHelper url)
        {
            var user = await UserManager.FindByIdAsync(userId);

            if (user == null || !user.IsDeleted)
            {
                throw new ItemNotFoundException("The user either does not exist or is not confirmed.");
            }

            try
            {
                string code = await UserManager.GeneratePasswordResetTokenAsync(userId);
                var callbackUrl = url.Link("isDefault", new { action = "ResetPassword", controller = "Account", userId = user.Id, code });
                await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");

                return "OK";
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }


        public bool IsEmailUniqueForInvitation(string email)
        {
            //using (var context = new EnergyImpactCentral())
            //{
            return !UserManager.Users.Any(u => u.Email == email);
                // && context.Invitation.Count(i => i.Email == email && i.UsedOn == null) == 0
            //}
        }

        public void ChangeEmail(UserInfoViewModel userInfo)
        {
            var user = UserManager.Users.FirstOrDefault(u => u.Id == userInfo.Id);
            if(user == null)
                throw new ArgumentException("The user does not exist");
            if(!IsEmailUnique(userInfo))
                throw new ArgumentException("The email exist already");
            user.Email = userInfo.Email;
            UserManager.Update(user);
        }

        public bool SaveSettings(string userId, int defaultRegionId, int defaultCountryId, int defaultProductId)
        {
            if (UserExists(userId))
            {
                const string query = "UPDATE UserSettings " +
                                     "SET DefaultRegionID = @defaultRegionId, DefaultCountryId = @defaultCountryId, DefaultProductId = @defaultProductId " +
                                     "WHERE UserId = @userId";
                var dictionary = new Dictionary<string, object>
                {
                    {"userId", userId},
                    {"defaultRegionId", defaultRegionId},
                    {"defaultCountryId", defaultCountryId},
                    {"defaultProductId", defaultProductId}
                };
                var count = RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, dictionary);
                return count == 1;
            }
            else
            {
                const string query = "INSERT INTO UserSettings VALUES (@userId, @defaultRegionId, @defaultCountryId, @defaultProductId)";

                var dictionary = new Dictionary<string, object>
                {
                    {"userId", userId},
                    {"defaultRegionId", defaultRegionId},
                    {"defaultCountryId", defaultCountryId},
                    {"defaultProductId", defaultProductId}
                };
                var count = RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, dictionary);
                return count == 1;
            }
        }

        public UserFormResponseViewModel IsValid(UserFormRequestViewModel form)
        {
            var result = new UserFormResponseViewModel();
            result.EmailEmpty = string.IsNullOrEmpty(form.User.Email);
            if (!result.EmailEmpty)
            {
                result.EmailFormat = EmailService.IsEmailValid(form.User.Email);
                if (result.EmailFormat)
                {
                    result.EmailUnique = (form.CreatePurpose)
                    ? UserManager.Users.Count(c => c.Email.ToLower() == form.User.Email.Trim().ToLower()) == 0
                    : UserManager.Users.Count(
                        c => c.Email.ToLower() == form.User.Email.Trim().ToLower() && c.Id != form.User.Id) == 0;
                }
            }
            result.FirstNameEmpty = string.IsNullOrEmpty(form.User.FirstName);
            result.LastNameEmpty = string.IsNullOrEmpty(form.User.LastName);

            return result;
        }

        public UserSettingsViewModel GetUserSettings(string userId)
        {
            const string query = "SELECT DefaultRegionId, DefaultCountryId, DefaultProductId " +
                                 "FROM UserSettings " +
                                 "WHERE userId=@userId";

            var dictionary = new Dictionary<string, object>
            {
                {"userId", userId}
            };


            var settings =
                RequestHelper.ExecuteQuery(DataBaseConstants.ConnectionString, query, row => new UserSettingsViewModel
                {
                    DefaultRegionId = (int) row["DefaultRegionId"],
                    DefaultCountryId = (int) row["DefaultCountryId"],
                    DefaultProductId = (int) row["DefaultProductId"]
                }, dictionary).ToList();

            if (settings.Count == 0)
            {
                return new UserSettingsViewModel(); //default value = 0
            }
            return settings.First();
        }

        public void UpdateRolesForUser(UpdateUserRolesViewModel updateUser)
        {
            var context = new ApplicationDbContext();

            if(!updateUser.Roles.Any())
                throw new ArgumentException("No roles given");

            var user = GetUserInfo();
            if (updateUser.UserId == user.Id)
            {
                if( (user.Roles.Any(r => r == RoleConstants.SuperAdmin) && !updateUser.Roles.Any(r => r.Name == RoleConstants.SuperAdmin) )
                    || (user.Roles.Any(r => r == RoleConstants.SystemAdministrator) && !updateUser.Roles.Any(r => r.Name == RoleConstants.SystemAdministrator)) )
                    throw new ArgumentException("You can't remove the roles "+RoleConstants.SuperAdmin+", "+RoleConstants.SystemAdministrator+" for yourself");

            }
            var parameters = new List<SqlParameter>
            {
               new SqlParameter("UserId", updateUser.UserId)
            };

            var arrayParams = parameters.ToArray();
            context.Database.ExecuteSqlCommand("DELETE FROM AspNetUserRoles WHERE UserId = @UserId", arrayParams);

            var query = "INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES ";
            
            foreach (var role in updateUser.Roles)
            {
                query += " ('"+updateUser.UserId+"', '"+role.Id+"'),";
            }
            query = query.Substring(0, query.Length - 1);
            context.Database.ExecuteSqlCommand(query);

        }

        private bool UserExists(string userId)
        {
            const string query = "SELECT UserId FROM UserSettings WHERE UserId = @userId";

            var dictionary = new Dictionary<string, object>
            {
                {"userId", userId}
            };

            var result = RequestHelper.ExecuteQuery(DataBaseConstants.ConnectionString, query, row => new UserSettingsViewModel
            {
                UserId = Guid.Parse(row["UserId"].ToString())
            } , dictionary).ToList(); //-1 if user not found
            return result.Count == 1; 
        }

        public void RequestResetPassword(ForgotPasswordViewModel model)
        {
            var user = UserManager.FindByEmail(model.Email);
            string code = UserManager.GeneratePasswordResetToken(user.Id);

            var urlHelper = new System.Web.Mvc.UrlHelper(HttpContext.Current.Request.RequestContext);

            var callbackUrl = urlHelper.Action("ResetPassword", "Account", new { code }, HttpContext.Current.Request.Url.Scheme);

            var emailService = new EmailService();
            emailService.Send(new IdentityMessage
            {
                Destination = model.Email,
                Subject = "Price Care Reset Password",
                Body =
                    "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a> to use our registration form."
            });
        }
    }
}
