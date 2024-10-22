namespace PriceCare.Web.Models
{
    public class DimensionDictionaryModel
    {
        public string Dimension { get; set; }
        public int DimensionId { get; set; }
        public int SystemId { get; set; }
        public string Name { get; set; }
        public string NewName { get; set; }
        public int NewDimensionId { get; set; }

    }
}