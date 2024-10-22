using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Models
{
    public class LoadViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public string UserName { get; set; }        
    }
}