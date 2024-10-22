using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using PriceCare.Web.Helpers;
using PriceCare.Web.Math;
using PriceCare.Web.Models;
using PriceCare.Web.Repository;

namespace PriceCare.Web.Controllers
{
    [RoutePrefix("api/volume")]
    [Authorize]
    public class VolumeController : ApiController
    {
        private readonly IVolumeRepository volumeRepository;
        private readonly ILoadRepository loadRepository;
        private readonly Excel excelRepository = new Excel();

        public VolumeController(IVolumeRepository volumeRepository, ILoadRepository loadRepository)
        {
            this.volumeRepository = volumeRepository;
            this.loadRepository = loadRepository;
        }

        [Route("paged")]
        [HttpPost]
        public object GetVolumes(VolumeRequest model)
        {
            if (model.Validate)
            {
                return loadRepository.GetVolumeToValidate(model);
            }
            else if (model.CompareToVersionId.HasValue)
            {
                var forecast = new Forecast();
                var data0 = (IEnumerable<DataViewModel>)volumeRepository.GetVolumes(model);
                var data1 = (IEnumerable<DataViewModel>)volumeRepository.GetVolumes(new VolumeRequest
                {
                    ProductId = model.ProductId,
                    AllEvents = model.AllEvents,
                    AllProducts = model.AllProducts,
                    SimulationId = model.SimulationId,
                    AllCountries = model.AllCountries,
                    DatabaseLike = model.DatabaseLike,
                    GeographyIds = model.GeographyIds,
                    SaveId = model.SaveId,
                    Validate = model.Validate,
                    VersionId = model.CompareToVersionId.Value,
                    Months = model.Months,
                    PriceTypesId = model.PriceTypesId
                });
                return forecast.SimulationDifference(data0.ToList(), data1.ToList());
            }
            return volumeRepository.GetVolumes(model);
        }

        [Route("pagedForecast")]
        [HttpPost]
        public List<DataViewModel> GetForecastVolumes(DataSearchRequestViewModel model)//TODO: get productId for call
        {                       
            if (model.CompareToSaveId.HasValue)
            {
                var forecast = new Forecast();
                return forecast.CompareSimulation(model.SimulationId, model.CompareToSaveId.Value, model.ProductId.First(), model.GeographyId,
                    (int) DataTypes.Volume);
            }

            return volumeRepository.GetDataCache(model);
        }

        [Route("versions")]
        [HttpPost]
        public object GetVersions(VolumeRequest model)
        {
            return volumeRepository.GetVersions(model);
        }

        [Route("excelForecast")]
        public HttpResponseMessage GetExcelForecast([FromUri]string token)
        {
            return excelRepository.GetExcel(token, false);
        }

        [Route("excel")]
        public HttpResponseMessage GetExcel([FromUri]string token)
        {
            return excelRepository.GetExcel(token, true);
        }

        [Route("forecast")]
        [HttpPost]
        public object GetDataCache(DataSearchRequestViewModel model)
        {
            return volumeRepository.GetDataCache(model);
        }

        [Route("launch/excel")]
        public HttpResponseMessage GetExcelForLaunch([FromUri]string token)
        {
            return excelRepository.GetExcel(token, false);
        }

        [Route("postFilterExcel")]
        [HttpPost]
        public object PostFilterExcel(VolumeRequest priceRequest)
        {
            return ExcelDownloadBufferHelper.PostFilterExcel(priceRequest);
        }
    }
}