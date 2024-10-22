using System.Collections.Generic;
using System.Web.Http;
using PriceCare.Web.Models;
using PriceCare.Web.Repository;

namespace PriceCare.Web.Controllers
{
    [RoutePrefix("api/dimension")]
    public class DimensionController : ApiController
    {
        private readonly  DimensionRepository dimensionRepository = new DimensionRepository();

        [Route("events")]
        public IEnumerable<DimensionViewModel> GetEventTypes()
        {
            return dimensionRepository.GetEventType();
        }

        [Route("formulations")]
        public IEnumerable<DimensionViewModel> GetFormulationTypes()
        {
            return dimensionRepository.GetFormulationType();
        }

        [Route("units")]
        public IEnumerable<UnitViewModel> GetUnits()
        {
            return dimensionRepository.GetUnits();
        }
    }

}