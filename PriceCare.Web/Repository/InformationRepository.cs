using System.Collections.Generic;
using PriceCare.Web.Constants;
using PriceCare.Web.Helpers;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public class InformationRepository : IInformationRepository
    {
        public IEnumerable<GeneralInformationsViewModel> GetGeneralInformations()
        {
            const string queryString = "SELECT ContactPerson, ContactMail, TechnicalSupportMail " +
                                       "FROM GeneralInformation";

            var result = RequestHelper.ExecuteQuery<GeneralInformationsViewModel>(DataBaseConstants.ConnectionString,queryString, row => new GeneralInformationsViewModel
            {
                AmgenSupportContactEmail = (string)row["ContactMail"],
                AmgenSupportContactName = (string)row["ContactPerson"],
                TechnicalSupportEmail = (string)row["TechnicalSupportMail"]
            });
            return result;
        }

        public void UpdateGeneralInformations(GeneralInformationsViewModel model)
        {
            const string queryString = "UPDATE GeneralInformation " +
                                       "SET ContactPerson=@person, ContactMail=@mail, TechnicalSupportMail=@technical";

            var dictionary = new Dictionary<string, object>
            {
                {"person", model.AmgenSupportContactName},
                {"mail", model.AmgenSupportContactEmail},
                {"technical", model.TechnicalSupportEmail}
            };

            RequestHelper.ExecuteQuery(DataBaseConstants.ConnectionString, queryString, null, dictionary);
        }
    }
}