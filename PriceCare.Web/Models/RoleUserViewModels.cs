using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Models
{
    public class RoleUserViewModels
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<UserInfoViewModel> Users { get; set; }
    }
}