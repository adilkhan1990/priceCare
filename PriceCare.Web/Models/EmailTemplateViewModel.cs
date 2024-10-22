using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Models
{
    public class EmailTemplateViewModel
    {
        public EmailType EmailType { get; set; }
        public EmailViewType EmailViewType { get; set; }
        public bool IsDefault { get; set; }
        public string Content { get; set; }
    }
}