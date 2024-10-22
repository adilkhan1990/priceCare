using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class TypeAndSimulationsViewModel
    {
        public string Name { get; set; }
        public List<SaveViewModel> Simulations { get; set; }
    }
}