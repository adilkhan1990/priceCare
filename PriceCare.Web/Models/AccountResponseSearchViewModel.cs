using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Models
{
    public class AccountResponseSearchViewModel
    {
        public List<AccountInfoViewModel> Accounts { get; set; }
        public int? PageNumber { get; set; }
        public bool IsLastPage { get; set; }
    }
}