using System;

namespace PriceCare.Web.Models
{
    public class SaveViewModel
    {
        public int Id { get; set; }
        public int SaveId { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public int SaveTypeId { get; set; }
        public DateTime SaveTime { get; set; }
        public bool IsPublic { get; set; }
        public bool IsApproved { get; set; }
        public bool IsReference { get; set; }
        public bool IsBudget { get; set; }
        public string UserId { get; set; }
        public int DataVersionId { get; set; }
        public int CurrencyBudgetVersionId { get; set; }
        public int AssumptionsSaveId { get; set; }
        public bool Active { get; set; }
        public int SimulationCurrencyId { get; set; }
        public bool Edited { get; set; }
        public bool OverrideValue { get; set; }
        public bool IsLaunch { get; set; }
        public string UserName { get; set; }
    }

    public class SaveVersionViewModel
    {
        public int SaveId { get; set; }
        public int SubSaveId { get; set; }
        public int SubSaveTypeId { get; set; }
        public int DataVersionId { get; set; }
        public int VersionId { get; set; }
    }
}