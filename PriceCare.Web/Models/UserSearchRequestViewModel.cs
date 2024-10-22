using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class UserSearchRequestViewModel
    {
        public int Status { get; set; }        
        public string SearchText { get; set; }
        public int PageNumber { get; set; }
        public int ItemsPerPage { get; set; }
        public string RoleId { get; set; }
    }

    public enum UserStatus
    {
        AllStatus = 0,
        Deleted = 1,
        Locked = 2,
        Active = 3
    }
}