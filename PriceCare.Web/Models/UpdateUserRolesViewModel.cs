using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class UpdateUserRolesViewModel
    {
        public string UserId { get; set; }
        public IEnumerable<RoleInfoViewModel> Roles { get; set; }
    }
}