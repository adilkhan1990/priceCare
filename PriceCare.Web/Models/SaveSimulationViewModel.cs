using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Models
{
    public class SaveSimulationViewModel
    {
        public SaveViewModel Save { get; set; }
        public int SimulationId { get; set; }
    }
}