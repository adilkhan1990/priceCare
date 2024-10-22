using System.Collections.Generic;
using System.Web.Http;
using PriceCare.Web.Models;
using PriceCare.Web.Repository;

namespace PriceCare.Web.Controllers
{
    [RoutePrefix("api/informations")]
    public class InformationController : ApiController
    {
        private readonly IInformationRepository informationRepository;

        public InformationController()
        {
            informationRepository = new InformationRepository();
        }

        [Route("general")]
        public IEnumerable<GeneralInformationsViewModel> GetGeneralInformations()
        {
            return informationRepository.GetGeneralInformations();
        }

        [Route("update")]
        [HttpPost]
        public void UpdateGeneralInformations(GeneralInformationsViewModel model)
        {
            informationRepository.UpdateGeneralInformations(model);
        }

    }
}