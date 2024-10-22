using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using PriceCare.Web.Helpers;
using PriceCare.Web.Math;
using PriceCare.Web.Math.Utils;
using PriceCare.Web.Models;

namespace PriceCare.Web.Controllers
{
    [RoutePrefix("api/analyzer")]
    [Authorize]
    public class AnalyzerController : ApiController
    {
        private readonly Analyzer analyzer;

        public AnalyzerController()
        {
            analyzer = new Analyzer();
        }

        [HttpPost]
        [Route("data")]
        public AnalyzerView GetPrices(AnalyzerViewModel analyzerViewModel)
        {
            var analyzerView = analyzer.GetAnalyzerData(analyzerViewModel.SimulationId, analyzerViewModel.GeographyIds, analyzerViewModel.ProductId, analyzerViewModel.DataTypeId);
            return analyzerView;
        }

        [HttpPost]
        [Route("salesImpact")]
        public AnalyzerView GetSalesImpact(SalesImpactViewModel analyzerViewModel)
        {
            var events = analyzerViewModel.Events;
            if (events == null || !events.Any())
            {
                var analyzerView = analyzer.GetAnalyzerData(analyzerViewModel.SimulationId, analyzerViewModel.GeographyIds, analyzerViewModel.ProductId, (int) DataTypes.Price);
                events = analyzerView.Focus.Where(f => f.EventTypeId != (int)EventTypes.NotSpecified && f.EventTypeId != (int)EventTypes.NoEvent).ToList();
            }
            var salesImpact = analyzer.EvaluateSalesImpact(analyzerViewModel.SimulationId, analyzerViewModel.ProductId, events);
            return salesImpact;
        }

        [Route("excelSalesImpact")]
        public HttpResponseMessage GetExcel([FromUri]string token)
        {
            return analyzer.GetSalesImpactExcel(token);
        }

    }

    public class AnalyzerViewModel
    {
        public int SimulationId { get; set; }
        public List<int> GeographyIds { get; set; }
        public int ProductId { get; set; }
        public int DataTypeId { get; set; }
    }

    public class SalesImpactViewModel : AnalyzerViewModel
    {
        public List<DataViewModel> Events { get; set; }
    }
}