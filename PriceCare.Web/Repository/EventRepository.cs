using System.Collections.Generic;
using System.Linq;
using PriceCare.Web.Constants;
using PriceCare.Web.Math;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public class EventRepository : IEventRepository
    {
        private readonly DataRepository dataRepository = new DataRepository();
        private readonly Forecast forecastRepository = new Forecast();

        public IEnumerable<DataViewModel> GetEvents(EventSearchRequestViewModel eventTypeSearchRequest, bool fillData = true)
        {
            IEnumerable<DataViewModel> events;
            if (eventTypeSearchRequest.VersionId.HasValue)
            {
                events = dataRepository.GetVersionData(
                    eventTypeSearchRequest.ScenarioTypeId,
                    eventTypeSearchRequest.VersionId.Value,
                    0,
                    eventTypeSearchRequest.GeographyId,
                    eventTypeSearchRequest.ProductId,
                    new List<int> {(int) DataTypes.Event}).OrderBy(e => e.DataTime).ToList();
            }
            else
            {
                events = dataRepository.GetLatestData(
                    eventTypeSearchRequest.ScenarioTypeId,
                    eventTypeSearchRequest.GeographyId,
                    eventTypeSearchRequest.ProductId,
                    new List<int> { (int)DataTypes.Event }, fillData).ToList();
            }
            eventTypeSearchRequest.EventTypeId.Add((int)EventTypes.NoEvent);
            eventTypeSearchRequest.EventTypeId.Add((int)EventTypes.NotSpecified);
            //foreach (var evt in events)
            //{
            //    if(!eventTypeSearchRequest.EventTypeId.Contains(evt.EventTypeId))
            //    {
            //        evt.Value = null;
            //        evt.EventTypeId = (int)EventTypes.NoEvent;
            //        evt.SegmentId = ApplicationConstants.DefaultSegment;
            //    }
            //}

            return events;
        }

        public IEnumerable<DataViewModel> GetEventsForecast(EventSearchRequestViewModel eventTypeSearchRequest)
        {
            var events = forecastRepository.GetProductForSimulation(eventTypeSearchRequest.SimulationId, eventTypeSearchRequest.GeographyId, 
                eventTypeSearchRequest.ProductId, new List<int>{(int)DataTypes.Event});

            eventTypeSearchRequest.EventTypeId.Add((int)EventTypes.NoEvent);
            eventTypeSearchRequest.EventTypeId.Add((int)EventTypes.NotSpecified);
            foreach (var evt in events)
            {
                if (!eventTypeSearchRequest.EventTypeId.Contains(evt.EventTypeId))
                {
                    evt.Value = null;
                    evt.EventTypeId = (int)EventTypes.NoEvent;
                    evt.SegmentId = ApplicationConstants.DefaultSegment;
                }
            }

            return events;
        }
    }
}