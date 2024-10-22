using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Models
{
    public class VolumeDataTableViewModel
    {
        public List<string> Columns { get; set; } 
        public List<List<string>> Rows { get; set; } 
    }
}