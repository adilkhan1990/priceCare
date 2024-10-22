using System.Collections.Generic;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public interface IDimensionRepository
    {
        IEnumerable<DimensionViewModel> GetMathType();
        IEnumerable<DimensionViewModel> GetGeographyType();
        IEnumerable<DimensionViewModel> GetGprmRuleType();
        IEnumerable<DimensionViewModel> GetEventType();
        IEnumerable<DimensionViewModel> GetUnitType();
        IEnumerable<DimensionViewModel> GetFormulationType();
        IEnumerable<DimensionViewModel> GetSystemType();
        IEnumerable<DimensionViewModel> GetDataType();
    }
}