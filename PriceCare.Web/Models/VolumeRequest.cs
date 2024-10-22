using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class VolumeRequest : RequestExcelDownload
    {
        public List<int> GeographyIds { get; set; }
        public List<int> PriceTypesId { get; set; }
        public int? ProductId { get; set; }
        public int VersionId { get; set; }        
        public int Months { get; set; }
        public int SimulationId { get; set; }
        public bool Validate { get; set; }
        public int SaveId { get; set; }
        public int? CompareToVersionId { get; set; }
    }
}