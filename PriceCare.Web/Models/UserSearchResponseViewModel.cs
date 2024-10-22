using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class UserSearchResponseViewModel
    {
        public IEnumerable<UserInfoViewModel> Users { get; set; }

        public bool IsLastPage { get; set; }

        public int Page { get; set; }

        public int TotalUsers { get; set; }
    }
}