using System.Collections.Generic;
using System.Linq;
using PriceCare.Web.Constants;
using PriceCare.Web.Math;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public class VolumeRepository : IVolumeRepository
    {
        private readonly DataRepository dataRepository = new DataRepository();
        private readonly Forecast forecastRepository = new Forecast();

        public List<DataViewModel> GetDataCache(DataSearchRequestViewModel model)
        {
            return forecastRepository.GetProductForSimulation(model.SimulationId, model.GeographyId, model.ProductId,
                new List<int>{(int) DataTypes.Volume});
        }
        public object GetVolumes(VolumeRequest model, bool fillData = true)
        {
            IEnumerable<DataViewModel> data;
            if (model.VersionId != 0)
            {
                data = dataRepository.GetVersionData(ApplicationConstants.ImportedDataSaveId, model.VersionId, 0, model.GeographyIds,
                    model.ProductId.HasValue ? new List<int> { model.ProductId.Value } : new List<int>(),
                    new List<int> { (int)DataTypes.Volume });
            }
            else
            {
                data = dataRepository.GetLatestData(
                ApplicationConstants.ImportedDataSaveId,
                model.GeographyIds,
                model.ProductId.HasValue ? new List<int> { model.ProductId.Value } : new List<int>(),
                new List<int> { (int)DataTypes.Volume }, fillData).ToList();
            }

            return data;
        }

        public object GetForecastVolumes(VolumeRequest model)
        {
            var cr = new CacheRepository();

            var productsId = new List<int>();
            var dataTypesId = new List<int> { 42 };

            if (model.ProductId.HasValue)
                productsId.Add(model.ProductId.Value);

            var simulationId = cr.GetFirstSimulation();
            IEnumerable<DataViewModel> data = forecastRepository.GetProductForSimulation(simulationId, model.GeographyIds, productsId, dataTypesId);

            return data;
        }

        public object GetVersions(VolumeRequest model)
        {
            var dr = new VersionRepository();
            var data = dr.GetDataVersion(model.GeographyIds ?? new List<int>(),
                model.ProductId.HasValue ? new List<int> { model.ProductId.Value } : new List<int>(), new List<int> { 42 });
            return data;
        }

    }
}