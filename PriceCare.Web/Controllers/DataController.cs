using System.Collections.Generic;
using System.Web.Http;
using PriceCare.Web.Math;
using PriceCare.Web.Models;
using PriceCare.Web.Repository;

namespace PriceCare.Web.Controllers
{
    [RoutePrefix("api/data")]
    [Authorize]
    public class DataController : ApiController
    {
        private readonly IDataRepository dataRepository = new DataRepository();
        private readonly CacheRepository cacheRepository = new CacheRepository();
        private readonly Forecast forecastRepository = new Forecast();

        [Route("save")]
        [HttpPost]
        public void SaveData(List<DataViewModel> data)
        {
            dataRepository.SaveVersionData(data);
        }


        [Route("cache")]
        [HttpPost]
        public List<DataViewModel> GetDataCache(DataSearchRequestViewModel dataSearch)
        {
            var simulationId = cacheRepository.GetFirstSimulation();
            if(simulationId == -1)
                return new List<DataViewModel>();
            return forecastRepository.GetProductForSimulation(simulationId, dataSearch.GeographyId, dataSearch.ProductId, dataSearch.DataTypeId);
        }
    }
}