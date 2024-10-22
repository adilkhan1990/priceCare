using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class RequestExcelDownload
    {
        public bool AllCountries { get; set; }
        public bool AllProducts { get; set; }
        public bool AllEvents { get; set; }
        public bool DatabaseLike { get; set; }
        public int Duration { get; set; }
    }
}