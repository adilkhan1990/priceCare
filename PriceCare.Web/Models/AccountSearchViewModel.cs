using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Models
{
    public class AccountSearchViewModel
    {
        public string SearchText { get; set; }
        public int? PageNumber { get; set; }
        public int? ItemsPerPage { get; set; }
    }
}