using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using PriceCare.Web.Math;
using PriceCare.Web.Models;
using PriceCare.Web.Repository;
using System.Net.Http;
using PriceCare.Web.Helpers;

namespace PriceCare.Web.Controllers
{
    [RoutePrefix("api/price")]
    [Authorize]
    public class PriceController : ApiController
    {
        private readonly IPriceRepository priceRepository;
        private readonly ILoadRepository loadRepository;
        private readonly Excel excelRepository = new Excel();

        public PriceController(IPriceRepository priceRepository, ILoadRepository loadRepository)
        {
            this.priceRepository = priceRepository;
            this.loadRepository = loadRepository;
        }

        [Route("paged")]
        [HttpPost]
        public object GetPrices(PriceRequestViewModel model)
        {
            if (model.Validate)
            {
                return loadRepository.GetPriceToValidate(model);
            }
            else if (model.CompareTo.HasValue)
            {
                var forecast = new Forecast();
                var data0 = (IEnumerable<DataViewModel>) priceRepository.GetPrices(model);
                var data1 = (IEnumerable<DataViewModel>) priceRepository.GetPrices(new PriceRequestViewModel
                {
                    AllCountries = model.AllCountries,
                    AllEvents = model.AllEvents,
                    AllProducts = model.AllProducts,
                    ProductId = model.ProductId,
                    CompareTo = model.CompareTo,
                    CompareToSimulation = model.CompareToSimulation,
                    DatabaseLike = model.DatabaseLike,
                    GeographyIds = model.GeographyIds,
                    SaveId = model.SaveId,
                    SimulationId = model.SimulationId,
                    UserId = model.UserId,
                    Validate = model.Validate,
                    VersionId = model.CompareTo //compare to versionId
                });
                return forecast.SimulationDifference(data0.ToList(), data1.ToList());
            }
            else
            {
                return priceRepository.GetPrices(model);
            }                           
        }

        [Route("versions")]
        [HttpPost]
        public object GetVersions(PriceRequestViewModel model)
        {
            return priceRepository.GetVersions(model);
        }

        [Route("forecast")]
        [HttpPost]
        public List<DataViewModel> GetDataCache(PriceRequestViewModel dataSearch)
        {
           
            dataSearch.UserId = User.Identity.GetUserId();
            if(dataSearch.CompareTo.HasValue)
            {
                Forecast foreCast = new Forecast();
                return foreCast.CompareSimulation(dataSearch.SimulationId, 
                    dataSearch.CompareTo.Value, dataSearch.ProductId, dataSearch.GeographyIds ,(int)DataTypes.Price);
            }
                
            return priceRepository.GetDataCache(dataSearch).OrderBy(d => d.DataTime).ToList();
        }

        [Route("excel")]
        public HttpResponseMessage GetExcel([FromUri]string token)
        {
            return excelRepository.GetExcel(token, true);
        }

        [Route("excelForecast")]
        public HttpResponseMessage GetExcelForecast([FromUri]string token)
        {
            return excelRepository.GetExcel(token, false);
        }
        [Route("excelReviewedForecast")]
        public HttpResponseMessage GetExcelReviewedForecast([FromUri]string token)
        {
            return excelRepository.GetExcelForReviewedPriceForecast(token);
        }

        [Route("excelReviewedChanges")]
        public HttpResponseMessage GetExcelReviewedChanges([FromUri]string token)
        {
            return excelRepository.GetExcelForReviewedPriceChanges(token);
        }

        [Route("excelAspForecast")]
        public HttpResponseMessage GetExcelForAspForecast([FromUri]string token)
        {
            return excelRepository.GetAspForecastExcel(token);
        }
        
    }
}