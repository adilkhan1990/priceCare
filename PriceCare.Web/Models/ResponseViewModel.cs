using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class ResponseViewModel
    {
        public ResponseViewModel()
        {
            Messages = new List<string>();
            SourceErrors = new List<object>();
        }
        public List<string> Messages { get; set; }
        public IEnumerable<object> SourceErrors { get; set; } 
        public bool IsError { get; set; }
    }
}