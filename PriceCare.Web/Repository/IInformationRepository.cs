using System.Collections.Generic;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public interface IInformationRepository
    {
        IEnumerable<GeneralInformationsViewModel> GetGeneralInformations();
        void UpdateGeneralInformations(GeneralInformationsViewModel model);
    }
}