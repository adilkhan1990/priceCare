namespace PriceCare.Web.Models
{
    public class CurrencyViewModel
    {
        public CurrencyViewModel()
        {
            Active = true;
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Iso { get; set; }
        public double UsdRate { get; set; }
        public double EurRate { get; set; }
        public bool Active { get; set; }
        public string Tag { get; set; }
        public object OldValue { get; set; }
        public RateType RateType { get; set; }
    }

    public class CurrencyExportViewModel : CurrencyViewModel
    {
        public double? UsdSpot { get; set; }
        public double? EurSpot { get; set; }
        public double? UsdBudget { get; set; }
        public double? EurBudget { get; set; }
    }

    public enum RateType
    {
        Budget = 0,
        Spot = 1
    }

    public enum SimulationRate
    {
        UsdSpot = 4,
        EurSpot = 2,
        UsdBudget = 5,
        EurBudget = 3,

    }

}