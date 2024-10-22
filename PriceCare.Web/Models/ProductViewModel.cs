namespace PriceCare.Web.Models
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string ExportName { get; set; }
        public string BaseConsolidationUnit { get; set; }
        public string DisplayConsolidationUnit { get; set; }
        public bool Active { get; set; }
        public int UnitTypeId { get; set; }
        public double FactorScreen { get; set; }
        public double Factor { get; set; }
        public string Status { get; set; }
        public string DisplayName { get; set; }
        public int BaseConsolidationUnitId { get; set; }
        public string Tag { get; set; }
    }
}