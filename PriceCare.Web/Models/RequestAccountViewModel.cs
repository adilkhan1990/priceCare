using System.ComponentModel.DataAnnotations;

namespace PriceCare.Web.Models
{
    public class RequestAccountViewModel
    {
        [Required(ErrorMessage="Required")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Reason")]
        public string Reason { get; set; }
    }
}