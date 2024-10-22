using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Models
{
    public class LoadSearchRequest
    {
        public int StatusId { get; set; }
        public int PageNumber { get; set; }
        public int ItemsPerPage { get; set; }        
    }
}