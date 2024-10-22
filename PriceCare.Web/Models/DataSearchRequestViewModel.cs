using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class DataSearchRequestViewModel
    {
        public int SaveId { get; set; }
        public int SimulationId { get; set; }
        public List<int> GeographyId { get; set; }
        public List<int> ProductId { get; set; }
        public List<int> DataTypeId { get; set; }
        public int? CompareToSaveId { get; set; }
        public bool IsCompareMode { get; set; }
        

    }
}