using System.Collections.Generic;
using PriceCare.Web.Constants;

namespace PriceCare.Web.Models
{
    public class EventSearchRequestViewModel : RequestExcelDownload
    {
        public EventSearchRequestViewModel()
        {
            SystemId = ApplicationConstants.IrpLoadSystem;
        }
        public List<int> GeographyId { get; set; }
        public List<int> ProductId { get; set; }
        public List<int> EventTypeId { get; set; }
        public int? VersionId { get; set; }
        public int SimulationId { get; set; }
        public int SaveId { get; set; }
        public bool Validate { get; set; }
        public int ScenarioTypeId { get; set; }
        public string UserId { get; set; }
        public int SystemId { get; set; }
    }
}