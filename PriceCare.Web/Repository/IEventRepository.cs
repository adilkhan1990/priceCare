using System.Collections.Generic;
using System.Net.Http;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public interface IEventRepository
    {
        IEnumerable<DataViewModel> GetEvents(EventSearchRequestViewModel eventTypeSearchRequest, bool fillData = true);
        IEnumerable<DataViewModel> GetEventsForecast(EventSearchRequestViewModel eventTypeSearchRequest);
    }
}
