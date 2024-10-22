using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class UpdateSimulationViewModel
    {
        public List<DataViewModel> UpdatedData { get; set; }
        public int SimulationId { get; set; }
    }
}