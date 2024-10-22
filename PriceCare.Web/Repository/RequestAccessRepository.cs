using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PriceCare.Central;
using PriceCare.Web.Constants;
using PriceCare.Web.Models;
using WebGrease.Css.Extensions;

namespace PriceCare.Web.Repository
{
    public class RequestAccessRepository : IRequestAccessRepository
    {
        private readonly IAccountRepository accountRepository = new AccountRepository();
        private readonly IInvitationRepository invitationRepository = new InvitationRepository();

        public void Create(RequestAccountViewModel requestAccount)
        {
            using (var context = new PriceCareCentral())
            {
                if (!context.RequestAccesses.Any(ra => ra.Email.ToLower() == requestAccount.Email.ToLower() && ra.Status == (int)RequestAccessStatus.New))
                {
                    var request = new RequestAccess
                    {
                        Date = DateTime.Now,
                        Email = requestAccount.Email,
                        Reason = requestAccount.Reason,
                        Status = (int)RequestAccessStatus.New
                    };

                    context.RequestAccesses.Add(request);
                    context.SaveChanges();
                }
            }
        }

        public IEnumerable<RequestAccessStatusViewModel> GetRequestAccessStatus()
        {
            var listStatus = new List<RequestAccessStatusViewModel>();
            listStatus.Add( new RequestAccessStatusViewModel{Id=1,Name = "New"});
            listStatus.Add( new RequestAccessStatusViewModel{Id=2,Name = "Accepted"});
            listStatus.Add( new RequestAccessStatusViewModel{Id=3,Name = "Rejected"});
            return listStatus;
        }

        public RequestAccessSearchResponseViewModel GetPaged(RequestAccessSearchRequestViewModel requestAccessSearch)
        {
            using (var context = new PriceCareCentral())
            {
                var result = context.RequestAccesses
                    .Select(ra => new RequestAccessInfoViewModel 
                    {
                        Id = ra.Id,
                        Date = ra.Date,
                        DateStatusChanged = ra.DateStatusChanged,
                        Email = ra.Email,
                        Reason = ra.Reason,
                        Status = ra.Status,
                        UserStatusChanged = ra.UserStatusChanged,
                    }).OrderBy(ra => ra.Date).ToList();

                var listUsers = new List<UserInfoViewModel>();

                for (int i = 0; i < result.Count(); i++)
                {
                    if (result[i].UserStatusChanged != null)
                    {
                        var userInfo = listUsers.FirstOrDefault(u => u.Id == result[i].UserStatusChanged);
                        if (userInfo == null)
                        {
                            userInfo = accountRepository.GetUserInfo(result[i].UserStatusChanged);
                            listUsers.Add(userInfo);
                        }

                        result[i].UserNameStatusChanged = userInfo.FirstName+", "+userInfo.LastName;
                    }
                }

                if (requestAccessSearch.Status != 0)
                {
                    result = result.Where(ra => ra.Status == requestAccessSearch.Status).ToList();
                }

                var totalRequestAccesses = result.Count();

                result = result.OrderBy(r => r.Email).ToList();

                result = result
                    .Skip(requestAccessSearch.PageNumber*requestAccessSearch.ItemsPerPage)
                    .Take(requestAccessSearch.ItemsPerPage).ToList();


                var viewModel = new RequestAccessSearchResponseViewModel
                {
                    RequestAccesses = result.ToList(), // to avoid DbContext disposed error
                    IsLastPage = (result.Count() + (requestAccessSearch.PageNumber * requestAccessSearch.ItemsPerPage)) >= totalRequestAccesses,
                    PageNumber = ++requestAccessSearch.PageNumber,
                    TotalRequestAccess = totalRequestAccesses
                };
                return viewModel;

            }
        }

        public async Task<bool> ChangeStatus(RequestAccessChangeStatusViewModel requestAccessChangeStatus)
        {
            var user = accountRepository.GetUserInfo();
            var role = accountRepository.GetAllRoles().FirstOrDefault(r => r.Name == RoleConstants.Viewer);
            bool result = false;
            if(role == null)
                throw new ArgumentException("The role viewer does not exist");

            using (var context = new PriceCareCentral())
            {
                var requestAccess = context.RequestAccesses.FirstOrDefault(ra => ra.Id == requestAccessChangeStatus.Id);
                if (requestAccess == null)
                    throw new ArgumentException("The request access does not exist");

                if (requestAccessChangeStatus.Status != (int)RequestAccessStatus.Accepted && 
                        requestAccessChangeStatus.Status != (int)RequestAccessStatus.Rejected)
                    throw new ArgumentException("The status is out of range, it only accepts [Accepted,Rejected]");

                if (requestAccess.Status != requestAccessChangeStatus.Status)
                {
                    switch (requestAccessChangeStatus.Status)
                    {
                        case (int)RequestAccessStatus.Accepted:
                            requestAccess.Status = (int)RequestAccessStatus.Accepted;
                            requestAccess.UserStatusChanged = user.Id;
                            requestAccess.DateStatusChanged = DateTime.Now;
                            var invitation = new InvitationInfoViewModel
                            {
                                Email = requestAccess.Email,
                                Roles = new List<RoleInfoViewModel> { role }
                            };
                            result = await invitationRepository.CreateAsync(invitation);
                            break;
                        case (int)RequestAccessStatus.Rejected:
                            requestAccess.Status = (int)RequestAccessStatus.Rejected;
                            requestAccess.UserStatusChanged = user.Id;
                            requestAccess.DateStatusChanged = DateTime.Now;
                            break;
                    }
                    context.SaveChanges();
                }
            }
            return result;
        }
    }
}