using System;
using System.Collections.Generic;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using PriceCare.Web.Helpers;
using PriceCare.Web.Math;
using PriceCare.Web.Models;
using PriceCare.Web.Repository;
using System.Net.Http;

namespace PriceCare.Web.Controllers
{
    [RoutePrefix("api/simulation")]
    [Authorize]
    public class SimulationController : ApiController
    {
        private readonly SaveRepository saveRepository;
        private readonly Forecast forecastRepository;
        private readonly Excel excelRepository = new Excel();

        public SimulationController()
        {
            saveRepository = new SaveRepository();
            forecastRepository = new Forecast();
        }

        [Route("all")]        
        public List<TypeAndSimulationsViewModel> GetSimulations()
        {
            var userId = User.Identity.GetUserId();
            return saveRepository.GetSimulations(userId);
        }
        [Route("launch")]
        public List<TypeAndSimulationsViewModel> GetLaunchSimulations()
        {
            var userId = User.Identity.GetUserId();
            return saveRepository.GetLaunchSimulations(userId);
        }

        [Route("{simulationType}/{isActive}")]
        public List<SaveViewModel> GetSimulations(int simulationType, bool isActive)
        {
            var userId = User.Identity.GetUserId();
            return saveRepository.GetSimulations(userId, simulationType, isActive);
        } 

        [Route("load/{saveId:int}/{productId:int}")]
        [HttpGet]
        public CacheViewModel Load(int saveId, int productId)//TODO: get parameter value
        {
            var userId = User.Identity.GetUserId();
            var forecast = new Forecast();
            return forecast.LoadSimulation(userId, saveId, productId);
        }

        [Route("save")]
        [HttpPost]
        public object SaveSimulation(SaveSimulationViewModel model)
        {
            model.Save.UserId = User.Identity.GetUserId();
            model.Save.UserName = User.Identity.GetUserName();
            if (model.Save.IsBudget)
                forecastRepository.PublishForecast(model);
            return saveRepository.SaveCache(model);
        }

        [Route("assumptionsScenarios")]
        public IEnumerable<SaveViewModel> GetAssumptionsScenarios()
        {
            return saveRepository.GetAssumptionsScenarios();
        }

        [Route("create")]
        [HttpPost]
        public object CreateSimulation(SimulationCreateViewModel create)
        {
            return forecastRepository.CreateSimulation(User.Identity.GetUserId(), create.AssumptionsSaveId,create.SimulationDuration, create.SimulationCurrencyId, create.ProductId, create.IsLaunch);
        }

        [Route("first")]
        public CacheViewModel GetFirstSimulation()
        {
            return new CacheRepository().GetFirstSimulationAndSaveId();
        }

        [Route("update")]        
        [HttpPost]
        public bool DeleteSimulation(SaveViewModel model)
        {
            return saveRepository.UpdateSimulation(model);
        }

        [Route("updateSimulation")]
        [HttpPost]
        public object UpdateSimulation(UpdateSimulationViewModel model)
        {
            var forecast = new Forecast();
            forecast.UpdateSimulation(model.UpdatedData, null, model.SimulationId);
            return true;
        }
        [Route("launch/excel")]
        public HttpResponseMessage GetExcelForLaunchIrpAssumtions([FromUri]string token)
        {
            return excelRepository.GetLaunchExcel(token);
        }
        [Route("postFilterExcel")]
        [HttpPost]
        public object PostFilterExcel(EventSearchRequestViewModel eventTypeSearch)
        {
            return ExcelDownloadBufferHelper.PostFilterExcel(eventTypeSearch);
        }

        [Route("launch/checkScenarioLoad")]
        [HttpPost]
        public Tuple<bool, bool, bool> CheckSceanrioLoad()
        {
            var userId = User.Identity.GetUserId();
            return new CacheRepository().CheckLaunchScenario(userId);
        }

        [Route("createAssumptionsScenario")]
        [HttpPost]
        public int CreateAssumptionsScenario(SaveViewModel save)
        {
            return new SaveRepository().CreateAssumptionsScenario(save);
        }

        [Route("isValid")]
        [HttpPost]
        public SaveValidationResponseViewModel IsValid(SaveViewModel saveInfo)
        {
            return new SaveRepository().IsValid(saveInfo);
        }

    }
}