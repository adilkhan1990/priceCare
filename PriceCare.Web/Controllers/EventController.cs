using System.Net.Http;
using System.Collections.Generic;
using System.Web.Http;
using PriceCare.Web.Constants;
using PriceCare.Web.Helpers;
using PriceCare.Web.Math;
using PriceCare.Web.Models;
using PriceCare.Web.Repository;
using Microsoft.AspNet.Identity;

namespace PriceCare.Web.Controllers
{
    [RoutePrefix("api/event")]
    public class EventController : ApiController
    {
        private readonly IEventRepository eventRepository;
        private readonly ILoadRepository loadRepository;
        private readonly Excel excelRepository = new Excel();

        public EventController(IEventRepository eventRepository, ILoadRepository loadRepository)
        {
            this.eventRepository = eventRepository;
            this.loadRepository = loadRepository;
        }

        [Route("paged")]
        [HttpPost]
        public IEnumerable<DataViewModel> GetEventDatas(EventSearchRequestViewModel eventTypeSearch)
        {
            if (eventTypeSearch.Validate)
                return loadRepository.GetEventToValidate(eventTypeSearch);
            return eventRepository.GetEvents(eventTypeSearch);
        }

        [Route("forecast")]
        [HttpPost]
        public IEnumerable<DataViewModel> GetDataCache(EventSearchRequestViewModel dataSearch)
        {
            dataSearch.UserId = User.Identity.GetUserId();
            return eventRepository.GetEventsForecast(dataSearch);
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
        [Route("launch/excel/scenario")]
        public HttpResponseMessage GetExcelForLaunchScenario([FromUri]string token)
        {
            return excelRepository.GetExcel(token, true, ApplicationConstants.LaunchOptionScenario);
        }

        [Route("launch/excel/assumptions")]
        public HttpResponseMessage GetExcelForLaunchIrpAssumtions([FromUri]string token)
        {
            return excelRepository.GetExcel(token, true, ApplicationConstants.LaunchOptionAssumptions);
        }

        [Route("postFilterExcel")]
        [HttpPost]
        public object PostFilterExcel(EventSearchRequestViewModel eventTypeSearch)
        {
            return ExcelDownloadBufferHelper.PostFilterExcel(eventTypeSearch);
        }
    }
}