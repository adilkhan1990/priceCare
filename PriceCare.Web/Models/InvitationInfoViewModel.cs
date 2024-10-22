using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PriceCare.Web.Models
{
    public class InvitationInfoViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        public List<RoleInfoViewModel> Roles { get; set; }

        public int AccountId { get; set; }

    }
}