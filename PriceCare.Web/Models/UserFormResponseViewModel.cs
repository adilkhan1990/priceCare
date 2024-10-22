namespace PriceCare.Web.Models
{
    public class UserFormResponseViewModel
    {
        public UserFormResponseViewModel()
        {
            EmailEmpty = true;
            FirstNameEmpty = true;
            LastNameEmpty = true;
        }
        public bool EmailEmpty { get; set; }
        public bool EmailFormat { get; set; }
        public bool EmailUnique { get; set; }
        public bool FirstNameEmpty { get; set; }
        public bool LastNameEmpty { get; set; }
    }
}