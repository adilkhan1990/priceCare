namespace PriceCare.Web.Models
{
    public class ProductUnitViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int UnitId { get; set; }
        public int FactorScreen { get; set; }
        public bool Active { get; set; }
        public bool IsDefault { get; set; }
    }
}