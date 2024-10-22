using System.Collections.Generic;
using PriceCare.Web.Models;

namespace PriceCare.Web.Models
{
    public class EventSearchResponseViewModel
    {
        public IEnumerable<DataViewModel> Events { get; set; }
    }
}