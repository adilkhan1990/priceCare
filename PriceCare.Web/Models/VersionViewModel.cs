using System;

namespace PriceCare.Web.Models
{
    public class VersionViewModel
    {
        public int VersionId { get; set; }
        public string Information { get; set; }
        public DateTime VersionTime { get; set; }
        public string VersionTimeAsString { get; set; }
        public string UserName { get; set; }
        public string VersionData { get; set; }  
    }
}