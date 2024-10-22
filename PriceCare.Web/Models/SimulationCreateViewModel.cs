namespace PriceCare.Web.Models
{
    public class SimulationCreateViewModel
    {
        public int AssumptionsSaveId { get; set; }
        public int SimulationDuration { get; set; }
        public int SimulationCurrencyId { get; set; }
        public int ProductId { get; set; }
        public bool IsLaunch { get; set; }
    }
}