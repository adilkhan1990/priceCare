using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using Newtonsoft.Json;
using PriceCare.Central;
using PriceCare.Web.Constants;
using PriceCare.Web.Helpers;
using PriceCare.Web.Math.Utils;
using PriceCare.Web.Models;
using PriceCare.Web.Repository;
using System.Collections.Generic;

namespace PriceCare.Web.Math
{
    public class Analyzer
    {
        private readonly DataRepository dataRepository = new DataRepository();
        private readonly CacheRepository cacheRepository = new CacheRepository();
        private readonly SaveRepository saveRepository = new SaveRepository();
        private readonly Forecast forecastRepository = new Forecast();
        private readonly ListToSalesRepository listToSalesRepository = new ListToSalesRepository();
        private readonly ListToSalesImpactRepository listToSalesImpactRepository = new ListToSalesImpactRepository();
        public AnalyzerView GetAnalyzerData(int simulationId, List<int> geographyId, int productId, int dataTypeId)
        {
            var result = new AnalyzerView();
            var data = new List<DataViewModel>();
            NetData netData;
            switch (dataTypeId)
            {
                case (int)DataTypes.Price:
                case (int)DataTypes.Volume:
                    data = forecastRepository.GetProductForSimulation(
                            simulationId,
                            new List<int>(),
                            new List<int> { productId },
                            new List<int> { dataTypeId }, true).ToList();
                    break;
                case (int)DataTypes.NetPrice:
                    netData = NetSimulationForecast(simulationId, new List<int>(), new List<int> { productId });
                    data = netData.Price.ToList();
                    break;
                case (int)DataTypes.Sales:
                    netData = NetSimulationForecast(simulationId, new List<int>(), new List<int> { productId });
                    data = netData.Sales.ToList();
                    break;
            }

            var basket = cacheRepository.GetBasketCache(simulationId, productId).ToList();
            var referencedBasket = basket.Where(b => geographyId.Contains(b.GeographyId)).Select(b => b.ReferencedGeographyId).Distinct();
            var order1Basket = basket.Where(b => geographyId.Contains(b.ReferencedGeographyId)).Select(b => b.GeographyId).Distinct().ToList();
            var orderNBasket = GetOrderNbasket(basket, order1Basket, geographyId);

            result.Focus = data.Where(d => geographyId.Contains(d.GeographyId)).ToList();
            result.Referenced = data.Where(d => referencedBasket.Contains(d.GeographyId) && d.IsPublic).ToList();
            result.ImpactedOrder1 = data.Where(d => order1Basket.Contains(d.GeographyId)).ToList();
            result.ImpactedOrderN = data.Where(d => orderNBasket.Contains(d.GeographyId)).ToList();
            return result;
        }
        private static IEnumerable<DateTime> CreateImpactTime(int startYear, int endYear)
        {
            var result = new List<DateTime>();
            var startTime = new DateTime(startYear, 1, 1);
            var endTime = new DateTime(endYear, 1, 1);
            for (var newYear = startTime; newYear <= endTime; newYear = newYear.AddYears(1))
            {
                result.Add(newYear);
            }
            return result;
        }

        private static IEnumerable<DataViewModel> ConsolidateYearlyData(IEnumerable<DataViewModel> data, IEnumerable<DateTime> time)
        {
            return data.Join(time, d => d.DataTime.Year, t => t.Year, (d, t) => new { d, t })
                .GroupBy(dt => new { dt.d.GeographyId, dt.d.ProductId, dt.d.SegmentId, dt.d.PriceTypeId, dt.d.CurrencySpotId, dt.d.UsdBudget, dt.d.EurBudget, dt.d.UsdSpot, dt.d.EurSpot, dt.t.Date })
                .Select(dtg => new DataViewModel
                {
                    GeographyId = dtg.Key.GeographyId,
                    ProductId = dtg.Key.ProductId,
                    SegmentId = dtg.Key.SegmentId,
                    DataTypeId = (int)DataTypes.Sales,
                    PriceTypeId = ApplicationConstants.NotSpecifiedPrice,
                    DataTime = dtg.Key.Date,
                    Value = dtg.Sum(v => v.d.Value),
                    UsdBudget = dtg.Key.UsdBudget,
                    EurBudget = dtg.Key.EurBudget,
                    CurrencySpotId = dtg.Key.CurrencySpotId,
                    CurrencySpotVersionId = ApplicationConstants.DefaultCurrencySpotVersionId,
                    EventTypeId = (int)EventTypes.NoEvent,
                    UnitTypeId = ApplicationConstants.CurrencyUnitType,
                    Description = null,
                    UsdSpot = dtg.Key.UsdSpot,
                    EurSpot = dtg.Key.EurSpot,
                    PercentageVariation = 0,
                    Reviewed = true
                }).ToList();
        }
        private static List<DataViewModel> SalesDifference(List<DataViewModel> data0, IEnumerable<DataViewModel> data1)
        {
            var startYear = data0.Min(d => d.DataTime).Year;
            var endYear = data0.Max(d => d.DataTime).Year;
            var impactTime = CreateImpactTime(startYear, endYear).ToList();

            var data0Y = ConsolidateYearlyData(data0, impactTime).ToList();
            var data1Y = ConsolidateYearlyData(data1, impactTime).ToList();

            var data0T = data0Y.ToLookup(d => d, new DataPermIdentityEqualityComparer());
            var data1T = data1Y.ToLookup(d => d, new DataPermIdentityEqualityComparer());

            var index = data0Y.Distinct(new DataPermIdentityEqualityComparer());

            foreach (var i in index)
            {
                var d0 = data0T[i].First();
                var d1 = data1T[i].First();
                d0.Value = d0.Value - d1.Value;
                d0.PercentageVariation = d0.Value != 0 ? (d0.Value - d1.Value) / d0.Value : 0;
            }
            return data0Y;
        }
        public AnalyzerView EvaluateSalesImpact(int simulationId, int productId, List<DataViewModel> events)
        {
            var result = new AnalyzerView();
            var simulationInfo = cacheRepository.GetSimulation(simulationId);
            var dataVersionId = simulationInfo.DataVersionId;
            var startTime = simulationInfo.StartTime;
            var simulationDuration = simulationInfo.Duration;

            var geographyId = events.Select(e => e.GeographyId).Distinct().ToList();

            var data0 = forecastRepository.GetProductForSimulation(simulationId, new List<int>(),
                new List<int> { productId }, new List<int>());

            var priceData0 = data0.Where(d => d.DataTypeId == (int)DataTypes.Price).ToList(); 
            var priceData = priceData0.Where(d => d.DataTime <= startTime).ToList();
            var volumeData = data0.Where(d => d.DataTypeId == (int)DataTypes.Volume).ToList();
            var eventData = data0.Where(d => d.DataTypeId == (int)DataTypes.Event).ToList();
            var rule = cacheRepository.GetRule(simulationId);

            var netData0 = CalculateNetData(startTime, simulationDuration, dataVersionId, new List<int>(),
                new List<int> { productId }, priceData0, volumeData);

            var eventDataT = eventData.ToLookup(e => e, new GeographyDataTimeIdentityComparer());
            var indexEvent = events.Distinct(new GeographyDataTimeIdentityComparer());

            foreach (var eRemove in indexEvent.Select(e => eventDataT[e].First()))
            {
                eventData.Remove(eRemove);
            }

            var basket = cacheRepository.GetBasketCache(simulationId, productId).ToList();
            var order1Basket = basket.Where(b => geographyId.Contains(b.ReferencedGeographyId)).Select(b => b.GeographyId).Distinct().ToList();
            var orderNBasket = GetOrderNbasket(basket, order1Basket, geographyId);


            var forecast = forecastRepository.RunSimulation(priceData, volumeData, eventData, rule, simulationInfo.Duration, simulationInfo.SimulationCurrencyId, startTime);
            var priceForecast = forecast.Where(f => f.DataTypeId == (int)DataTypes.Price).ToList();
            var volumeForecast = forecast.Where(f => f.DataTypeId == (int)DataTypes.Volume).ToList();

            var netData = CalculateNetData(startTime, simulationDuration, dataVersionId, new List<int>(),
                new List<int> { productId }, priceForecast, volumeForecast);

            var netDifference = SalesDifference(netData0.Sales, netData.Sales);

            result.Focus = netDifference.Where(d => geographyId.Contains(d.GeographyId)).ToList();
            result.ImpactedOrder1 = netDifference.Where(d => order1Basket.Contains(d.GeographyId)).ToList();
            result.ImpactedOrderN = netDifference.Where(d => orderNBasket.Contains(d.GeographyId)).ToList();

            return result;
        }

        public NetData CalculateNetData(DateTime startTime, int simulationDuration, int dataVersionId, List<int> geographyId, List<int> productId, List<DataViewModel> priceData, List<DataViewModel> volumeData)
        {
            var result = new NetData
            {
                Price = new List<DataViewModel>(),
                Sales = new List<DataViewModel>()
            };
            var impactTime = dataRepository.CreateDataTimes(startTime, simulationDuration).ToList();
            var productPrice = priceData.ToLookup(d => d, new ProductIdentityComparer());
            var productVolume = volumeData.ToLookup(d => d, new ProductIdentityComparer());

            var listToSalesData = listToSalesRepository.GetVersionListToSales(dataVersionId, geographyId, productId).ToList();
            var listToSalesImpactData = listToSalesImpactRepository.GetVersionListToSalesImpact(dataVersionId, geographyId, productId).ToList();
            var productListToSales = listToSalesData.ToLookup(l2S => l2S, new ProductIdentityComparer());
            var productListToSalesImpact = listToSalesImpactData.ToLookup(l2S => l2S, new ProductIdentityComparer());

            var indexProduct = priceData.Distinct(new ProductIdentityComparer());

            foreach (var net in from i in indexProduct
                                let p = i.ProductId
                                let price = productPrice[i].ToList()
                                let volume = productVolume[i].ToList()
                                let listToSales = productListToSales[i].ToList()
                                let listToSalesImpact = productListToSalesImpact[i].ToList()
                                select CalculateNet(price, volume, listToSales, listToSalesImpact, impactTime, startTime, p))
            {
                result.Price.AddRange(net.Price);
                result.Sales.AddRange(net.Sales);
            }
            return result;
        }
        public NetData NetSimulationForecast(int simulationId, List<int> geographyId, List<int> productId)
        {   
            var simulationInfo = cacheRepository.GetSimulation(simulationId);
            var dataVersionId = simulationInfo.DataVersionId;
            var startTime = simulationInfo.StartTime;
            var simulationDuration = simulationInfo.Duration;

            var data = forecastRepository.GetProductForSimulation(simulationId, geographyId, productId, new List<int> { (int)DataTypes.Price, (int)DataTypes.Volume }).ToList();
            var priceData = data.Where(d => d.DataTypeId == (int)DataTypes.Price).ToList();
            var volumeData = data.Where(d => d.DataTypeId == (int)DataTypes.Volume).ToList();

            return CalculateNetData(startTime, simulationDuration, dataVersionId, geographyId, productId, priceData, volumeData);
        }
        public NetData NetSaveForecast(int saveId, List<int> geographyId, List<int> productId)
        {
            var simulationInfo = saveRepository.GetSimulationInfo(saveId);

            var dataVersionId = simulationInfo.DataVersionId;
            var currencyBudgetVersionId = simulationInfo.CurrencyBudgetVersionId;
            var assumptionsSaveId = simulationInfo.AssumptionsSaveId;
            var simulationDuration = simulationInfo.Duration;
            var startTime = simulationInfo.StartTime;
            var startYear = startTime.Year - 1;
            var endYear = startYear + simulationDuration + 1;

            var priceData =
                dataRepository.GetSaveData(geographyId, productId, (int)DataTypes.Price, saveId, dataVersionId, currencyBudgetVersionId,
                    assumptionsSaveId, startYear, endYear).ToList();
            var volumeData =
                dataRepository.GetSaveData(geographyId, productId, (int)DataTypes.Volume, saveId, dataVersionId, currencyBudgetVersionId,
                    assumptionsSaveId, startYear, endYear).ToList();

            return CalculateNetData(startTime, simulationDuration, dataVersionId, geographyId, productId, priceData, volumeData);
        }

        private static List<ListToSalesViewModel> SetListToSalesAsList(IEnumerable<GeographyProductSegmentIdentifier> gps)
        {
            return gps.Select(lP => new ListToSalesViewModel
                {
                    GeographyId = lP.GeographyId,
                    ProductId = lP.ProductId,
                    CurrencySpotId = ApplicationConstants.DefaultCurrencyId,
                    CurrencySpotVersionId = ApplicationConstants.DefaultCurrencySpotVersionId,
                    SegmentId = lP.SegmentId,
                    Asp = 0,
                    MarketPercentage = 1,
                    ImpactPercentage = 1,
                }).ToList();
        }
        private static List<ListToSalesImpactViewModel> SetDefaultListToSalesImpact(IEnumerable<GeographyProductSegmentIdentifier> gps)
        {
            return gps.Select(lP => new ListToSalesImpactViewModel
            {
                GeographyId = lP.GeographyId,
                ProductId = lP.ProductId,
                SegmentId = lP.SegmentId,
                ImpactDelay = 1,
                ImpactPercentage = 1
            }).ToList();
        }
        private NetData CalculateNet(List<DataViewModel> listPrice, IEnumerable<DataViewModel> volume, List<ListToSalesViewModel> listToSalesL, List<ListToSalesImpactViewModel> listToSalesImpactL, List<DateTime> impactTime, DateTime startTime, int productId)
        {
            var result = new NetData();
            if (listToSalesL.Count == 0)
            {
                var gps = listPrice.Select(lP => new
                {
                    lP.GeographyId,
                    lP.ProductId,
                    lP.SegmentId
                }).Distinct().Select(dLp => new GeographyProductSegmentIdentifier
                {
                    GeographyId = dLp.GeographyId,
                    ProductId = dLp.ProductId,
                    SegmentId = dLp.SegmentId
                }).ToList();
                listToSalesL = SetListToSalesAsList(gps);
                listToSalesImpactL = SetDefaultListToSalesImpact(gps);
            }

            var listToSales = listToSalesL.ToLookup(l2S => l2S, new GeographyProductIdentityComparer());
            var listToSalesImpact = listToSalesImpactL.ToLookup(l2S => l2S, new GeographyProductIdentityComparer());

            var netPriceL = InitializeNet(impactTime, listToSalesL, (int)DataTypes.NetPrice).ToList();
            var netPrice = netPriceL.ToLookup(nP => nP, new GeographySegmentDataTimeIdentityComparer());

            var launches =
                listPrice.Where(lP => lP.DataTime > startTime && lP.Reviewed && lP.Value != null && 
                    (lP.EventTypeId == (int)EventTypes.LaunchTarget ||
                    lP.EventTypeId == (int)EventTypes.LaunchDefaultRule ||
                    lP.EventTypeId == (int)EventTypes.LaunchLaunchRule));

            foreach (var l in launches)
            {
                var g = l.GeographyId;
                var t = l.DataTime;
                var s = l.SegmentId;
                foreach (var tDelay in impactTime.Where(i => i.Date >= t).OrderBy(i => i.Date))
                {
                    netPrice[new GeographySegmentDataTimeIdentifier(g, s, tDelay)].First().Value = l.Value;
                }
            }
            var listPriceChanges =
                listPrice.Where(lP => lP.DataTime > startTime && lP.Reviewed && lP.PercentageVariation != 0 && lP.PercentageVariation != null);
            foreach (var listPriceChange in listPriceChanges)
            {
                var g = listPriceChange.GeographyId;
                var t = listPriceChange.DataTime;
                var dValue = listPriceChange.PercentageVariation;
                var l2S = listToSales[new GeographyProductIdentifier(g, productId)].ToList();
                var l2SImpact = listToSalesImpact[new GeographyProductIdentifier(g, productId)].ToList();
                var segments = l2S.Count != 0 ? l2S.Select(l => l.SegmentId).Distinct() : new List<int> {ApplicationConstants.DefaultSegment};
                foreach (var segment in segments)
                {
                    var netP = netPrice[new GeographySegmentDataTimeIdentifier(g, segment, t)].Select(nP => new { nP.Value }).First().Value;
                    var segmentL2S = l2S.First(l => l.SegmentId == segment);
                    var segmentL2SImpact = l2SImpact.Where(l => l.SegmentId == segment).ToList();
                    var impactPercentage = segmentL2S.ImpactPercentage;
                    var impact = (double)0;
                    foreach (var impactDelay in segmentL2SImpact.OrderBy(l => l.ImpactDelay))
                    {
                        var delay = impactDelay.ImpactDelay;
                        impact = impact + impactDelay.ImpactPercentage;
                        foreach (var tDelay in impactTime.Where(i => i.Date >= t.AddMonths(delay - 1)).OrderBy(i => i.Date))
                        {
                            netPrice[new GeographySegmentDataTimeIdentifier(g, segment, tDelay)].First().Value = netP * (1 + dValue * impactPercentage * impact);
                        }
                    }
                }
            }
            var index = netPriceL.Distinct(new GeographySegmentDataTimeIdentityComparer());
            var netPriceByIndex = netPriceL.ToLookup(nP => nP, new GeographySegmentDataTimeIdentityComparer());
            var volumeByIndex = volume.ToLookup(nP => nP, new GeographyDataTimeIdentityComparer());
            var listToSalesByIndex = listToSalesL.ToLookup(l2S => l2S, new GeographyProductSegmentIdentityComparer());

            var sales = (from i in index
                         let g = i.GeographyId
                         let s = i.SegmentId
                         let t = i.DataTime
                         let netP = netPriceByIndex[i].First()
                         let l2S = listToSalesByIndex[new GeographyProductSegmentIdentifier(g, productId, s)].First()
                         let vol = volumeByIndex[new GeographyDataTimeIdentifier(g, t)].FirstOrDefault()
                         select new DataViewModel
                         {
                             GeographyId = g,
                             ProductId = productId,
                             CurrencySpotId = netP.CurrencySpotId,
                             CurrencySpotVersionId = netP.CurrencySpotVersionId,
                             PriceTypeId = ApplicationConstants.NotSpecifiedPrice,
                             DataTypeId = (int)DataTypes.Sales,
                             EventTypeId = (int)EventTypes.NoEvent,
                             UnitTypeId = ApplicationConstants.CurrencyUnitType,
                             SegmentId = s,
                             DataTime = t,
                             Value = vol != null ? netP.Value * vol.Value * l2S.MarketPercentage : 0,
                             Description = null,
                             UsdSpot = netP.UsdSpot,
                             EurSpot = netP.EurSpot,
                             UsdBudget = netP.UsdBudget,
                             EurBudget = netP.EurBudget,
                             PercentageVariation = 0,
                             Reviewed = true
                         }).ToList();
            result.Price = netPriceL;
            result.Sales = sales;
            return result;
        }
        private IEnumerable<DataViewModel> InitializeNet(IEnumerable<DateTime> timeImpact, IEnumerable<ListToSalesViewModel> listToSales, int dataType)
        {
            var usdRates = dataRepository.GetLatestUsdRates();
            return timeImpact
                .Join(listToSales, tS => 1, d => 1, (tS, d) => new { tS, d })
                .Select(f => new DataViewModel
                {
                    GeographyId = f.d.GeographyId,
                    ProductId = f.d.ProductId,
                    CurrencySpotId = f.d.CurrencySpotId,
                    CurrencySpotVersionId = f.d.CurrencySpotVersionId,
                    PriceTypeId = ApplicationConstants.NotSpecifiedPrice,
                    DataTypeId = dataType,
                    EventTypeId = (int)EventTypes.NoEvent,
                    UnitTypeId = ApplicationConstants.CurrencyUnitType,
                    SegmentId = f.d.SegmentId,
                    DataTime = f.tS.Date,
                    Value = f.d.Asp,
                    Description = null,
                    UsdSpot = usdRates.UsdSpot,
                    EurSpot = usdRates.EurSpot,
                    UsdBudget = usdRates.UsdBudget,
                    EurBudget = usdRates.EurBudget,
                    PercentageVariation = 0,
                    Reviewed = true
                }).ToList();
        }
        private static List<int> GetOrderNbasket(IEnumerable<BasketCache> refBasket, IEnumerable<int> previousOrder, IEnumerable<int> focusBasket)
        {
            var basket = refBasket as IList<BasketCache> ?? refBasket.ToList();
            var previous = previousOrder as IList<int> ?? previousOrder.ToList();
            var focus = focusBasket as IList<int> ?? focusBasket.ToList();
            var next = basket.Where(b => previous.Contains(b.ReferencedGeographyId)).Select(b => b.GeographyId).Distinct().Except(previous).Except(focus).ToList();
            var loop = new List<int>();
            while (next.Except(loop).Any())
            {
                loop = next;
                next = basket.Where(b => loop.Contains(b.ReferencedGeographyId)).Select(b => b.GeographyId).Distinct().Concat(loop).Distinct().Except(previous).Except(focus).ToList();
            }
            return next;
        }

        #region Export Excel

        #region Sales Impact
        public HttpResponseMessage GetSalesImpactExcel(string token)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            ExcelDownloadBuffer buffer;
            using (var context = new PriceCareCentral())
            {
                buffer = context.ExcelDownloadBuffers.FirstOrDefault(edb => edb.Token == token);
                if (buffer == null)
                    throw new ArgumentException("The token does not exist");
                context.ExcelDownloadBuffers.Remove(buffer);
                context.SaveChanges();
            }
            var filter = JsonConvert.DeserializeObject<SalesImpactSearchViewModel>(buffer.FilterJson);

            var fileName = filter.DatabaseLike ? ExcelFileNameConstants.SalesImpact : ExcelTemplateNameConstants.SalesImpact;
            var extention = filter.DatabaseLike ? "xlsx" : "xlsm";

            string templateFilePath = HttpContext.Current.Server.MapPath(@"~\App_Data\"+fileName);

            var memoryStream = FillExcelFile(templateFilePath, filter);

            response.Content = new StreamContent(memoryStream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(templateFilePath));
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "salesImpact_" + DateTime.Now.Ticks + "." + extention
            };

            return response;
        }

        private MemoryStream FillExcelFile(string templateFilePath, SalesImpactSearchViewModel filter)
        {
            var fileXls = File.ReadAllBytes(templateFilePath);

            List<SalesImpactViewModel> datas = new List<SalesImpactViewModel>();

            if (filter.AllProducts && filter.AllEvents)
            {
                datas.AddRange(GetSalesImpactExport(filter.SimulationId));
            }
            else if (filter.AllEvents)
            {
                foreach (var productId in filter.Products)
                {
                    datas.AddRange(GetSalesImpactExport(filter.SimulationId, productId));
                }
            }
            else
            {
                foreach (var productId in filter.Products)
                {
                    foreach (var eventTmp in filter.Events)
                    {
                        var salesImpact = GetSalesImpactExport(filter.SimulationId, productId,
                            new List<DataViewModel> {eventTmp});
                        datas.AddRange(salesImpact);
                    }
                }
            }

            var sheetDatas = new Dictionary<string, List<List<string>>>();
            var data = FillDataForSalesImpact(datas);
            sheetDatas.Add(data.Name, data.Datas);
            if (!filter.DatabaseLike)
            {
                var minDate = datas.Any() ? datas.Min(d => d.EventDateTime)  : DateTime.MinValue;
                var metadata = FillMetaDataForSalesImpact(filter, minDate);
                sheetDatas.Add(metadata.Name, metadata.Datas);
            }
            
            var memoryStream = ClosedXmlHelper.AppendXlsxSheetsHavingHeaders(fileXls, sheetDatas);

            return memoryStream;
        }

        private SheetData FillMetaDataForSalesImpact(SalesImpactSearchViewModel filter, DateTime startYear)
        {
            var allData = new List<List<string>>();
            allData.Add(new List<string> { "SaveId", filter.SaveId == 0 ? "1" : filter.SaveId.ToString() });
            allData.Add(new List<string> { "Start year", startYear.Year.ToString()});
            var simulationInfo = new CacheRepository().GetSimulation(filter.SimulationId);
            allData.Add(new List<string> { "Simulation", simulationInfo.StartTime.Date.ToString() });
            allData.Add(new List<string> { "SimulationId", simulationInfo.Id.ToString() });
            return new SheetData { Name = "MetaData", Datas = allData };
        }
        private SheetData FillDataForSalesImpact(List<SalesImpactViewModel> datas)
        {
            var allData = new List<List<string>>();
            var headers = new List<string>
            {
                "Event",
                "Geography",
                "Product",
                "EventDatetime",
                "Year",
                "Month",
                "EventType",
                "Value",
                "Geography",
                "ImpactDateTime",
                "Impact"
            };

            allData.Add(headers);

            allData.AddRange(datas.Select(c => new List<string>
            {
                c.EventId.ToString(),
                c.Geography,
                c.Product,
                c.EventDateTime.Date.ToString(),
                c.Year.ToString(),
                c.Month.ToString(),
                c.EventType.ToString(),
                c.Value.ToString(),
                c.ImpactedGeography,
                c.ImpactedDateTime.Date.ToString(),
                c.Impact.ToString()
            }));

            return new SheetData { Name = "Data", Datas = allData };
        }

        public List<SalesImpactViewModel> GetSalesImpactExport(int simulationId, int productId, List<DataViewModel> events)
        {
            var simulationInfo = cacheRepository.GetSimulation(simulationId);
            var startTime = simulationInfo.StartTime;

            var data = forecastRepository.GetProductForSimulation(simulationId, new List<int>(), new List<int> { productId }, 
                new List<int>
                {
                    (int)DataTypes.Price,
                    (int)DataTypes.Volume,
                    (int)DataTypes.Event
                }).ToList();

            var priceData = data.Where(d => d.DataTypeId == (int)DataTypes.Price && d.DataTime <= startTime).ToList();
            var volumeData = data.Where(d => d.DataTypeId == (int)DataTypes.Volume).ToList();
            var eventData = data.Where(d => 
                d.DataTypeId == (int)DataTypes.Event &&
                d.EventTypeId != (int)EventTypes.NoEvent && 
                d.EventTypeId != (int)EventTypes.NotSpecified).ToList();
                
            var rule = cacheRepository.GetRule(simulationId, productId);
            return GetProductSalesImpact(simulationInfo, priceData, volumeData, eventData, productId, rule, events).ToList();
        }
        public List<SalesImpactViewModel> GetSalesImpactExport(int simulationId, int productId)
        {
            var simulationInfo = cacheRepository.GetSimulation(simulationId);
            var startTime = simulationInfo.StartTime;

            var data = forecastRepository.GetProductForSimulation(simulationId, new List<int>(), new List<int> { productId },
                new List<int>
                {
                    (int)DataTypes.Price,
                    (int)DataTypes.Volume,
                    (int)DataTypes.Event
                }).ToList();

            var priceData = data.Where(d => d.DataTypeId == (int)DataTypes.Price && d.DataTime <= startTime).ToList();
            var volumeData = data.Where(d => d.DataTypeId == (int)DataTypes.Volume).ToList();
            var eventData = data.Where(d =>
                d.DataTypeId == (int)DataTypes.Event &&
                d.EventTypeId != (int)EventTypes.NoEvent &&
                d.EventTypeId != (int)EventTypes.NotSpecified).ToList();

            var rule = cacheRepository.GetRule(simulationId, productId);

            return GetProductSalesImpact(simulationInfo, priceData, volumeData, eventData, productId, rule).ToList();
        }

        public List<SalesImpactViewModel> GetSalesImpactExport(int simulationId)
        {
            var result = new List<SalesImpactViewModel>();
            var simulationInfo = cacheRepository.GetSimulation(simulationId);
            var startTime = simulationInfo.StartTime;

            var allProducts = new ProductRepository().GetAllProducts().Select(gP => gP.Id);
            var priceData = new List<DataViewModel>();
            var volumeData = new List<DataViewModel>();
            var eventData = new List<DataViewModel>();

            foreach (var product in allProducts)
            {
                var data = forecastRepository.GetProductForSimulation(simulationId, new List<int>(), new List<int> { product },
                new List<int>
                {
                    (int)DataTypes.Price,
                    (int)DataTypes.Volume,
                    (int)DataTypes.Event
                }).ToList();

                priceData.AddRange(data.Where(d => d.DataTypeId == (int)DataTypes.Price && d.DataTime <= startTime).ToList());
                volumeData.AddRange(data.Where(d => d.DataTypeId == (int)DataTypes.Volume).ToList());
                eventData.AddRange(data.Where(d =>
                    d.DataTypeId == (int)DataTypes.Event &&
                    d.EventTypeId != (int)EventTypes.NoEvent &&
                    d.EventTypeId != (int)EventTypes.NotSpecified).ToList());
            }

            var rule = cacheRepository.GetRule(simulationId);

            var productsIndex = priceData.Distinct(new ProductIdentityComparer());
            var productPrice = priceData.ToLookup(d => d, new ProductIdentityComparer());
            var productVolume = volumeData.ToLookup(d => d, new ProductIdentityComparer());
            var productEvent = eventData.ToLookup(d => d, new ProductIdentityComparer());

            foreach (var p in productsIndex)
            {
                result.AddRange(GetProductSalesImpact(simulationInfo, productPrice[p].ToList(), productVolume[p].ToList(), productEvent[p].ToList(), p.ProductId, rule));
            }

            return result;
        }
        private List<SalesImpactViewModel> GetProductSalesImpact(CacheViewModel simulationInfo, List<DataViewModel> price, List<DataViewModel> volume, List<DataViewModel> events, int productId, List<RuleViewModel> rule, List<DataViewModel> eventForImpact = null)
        {
            var allCountries = new CountryRepository().GetCountryExport().ToDictionary(x => x.Id, x => x.Name);
            var allProducts = new ProductRepository().GetProductExport().ToDictionary(x => x.Id, x => x.Name);
            var allEventTypes = new DimensionRepository().GetEventType().ToDictionary(x => x.Id, x => x.Name);

            var result = new List<SalesImpactViewModel>();
            var simulationId = simulationInfo.Id;
            var startTime = simulationInfo.StartTime;
            var simulationDuration = simulationInfo.Duration;
            var dataVersionId = simulationInfo.DataVersionId;
            var netData0 = NetSimulationForecast(simulationId, new List<int>(), new List<int> { productId });
            
            IEnumerable<IGeographyDataTimeIdentifier> indexEvent;
            if (eventForImpact == null)
            {
                indexEvent = events.Distinct(new GeographyDataTimeIdentityComparer());
            }
            else
            {
                indexEvent = eventForImpact.Distinct(new GeographyDataTimeIdentityComparer());
            }

            foreach (var i in indexEvent)
            {
                var eventi = CopyData(events).ToList();
                var eventiT = eventi.ToLookup(e => e, new GeographyDataTimeIdentityComparer());
                var impactEvent = eventiT[i].First();
                eventi.Remove(impactEvent);
                var forecast = forecastRepository.RunSimulation(price, volume, eventi, rule, simulationInfo.Duration, simulationInfo.SimulationCurrencyId, startTime);
                var priceForecast = forecast.Where(f => f.DataTypeId == (int)DataTypes.Price).ToList();
                
                var volumeForecast = forecast.Where(f => f.DataTypeId == (int)DataTypes.Volume).ToList();
                var netData = CalculateNetData(startTime, simulationDuration, dataVersionId, new List<int>(), new List<int> { productId }, priceForecast, volumeForecast);
                var netDifference = SalesDifference(netData0.Sales, netData.Sales).Where(sD => System.Math.Abs(sD.PercentageVariation ?? 0) > ApplicationConstants.DoubleComparisonThreshold).ToList();
                if (netDifference.Count != 0)
                    result.AddRange(netDifference.Select(nD => new SalesImpactViewModel
                    {
                        EventId = impactEvent.PermId,
                        Geography = allCountries[impactEvent.GeographyId],
                        Product = allProducts[impactEvent.ProductId],
                        EventDateTime = impactEvent.DataTime,
                        Year = nD.DataTime.Year,
                        Month = nD.DataTime.Month,
                        EventType = allEventTypes[impactEvent.EventTypeId],
                        Value = impactEvent.Value ?? 0,
                        ImpactedGeography = allCountries[nD.GeographyId],
                        ImpactedDateTime = nD.DataTime,
                        Impact = nD.Value ?? 0
                    }).OrderBy(nD => nD.ImpactedGeography).ThenBy(nD => nD.ImpactedDateTime));
            }
            return result;
        }

        private static IEnumerable<DataViewModel> CopyData(IEnumerable<DataViewModel> data0)
        {
            return data0.Select(d => new DataViewModel
            {
                GeographyId = d.GeographyId,
                ProductId = d.ProductId,
                CurrencySpotId = d.CurrencySpotId,
                CurrencySpotVersionId = d.CurrencySpotVersionId,
                PriceTypeId = d.PriceTypeId,
                DataTypeId = d.DataTypeId,
                EventTypeId = d.EventTypeId,
                UnitTypeId = d.UnitTypeId,
                SegmentId = d.SegmentId,
                DataTime = d.DataTime,
                Value = d.Value,
                Description = d.Description,
                UsdSpot = d.UsdSpot,
                EurSpot = d.EurSpot,
                UsdBudget = d.UsdBudget,
                EurBudget = d.EurBudget,
                PercentageVariation = d.PercentageVariation,
                Active = d.Active,
                Edited = d.Edited,
                Reviewed = d.Reviewed,
                Tag = d.Tag,
                OldValue = d.OldValue,
                PercentageVariationFromX = d.PercentageVariationFromX
            });
        }


        #endregion 

        #endregion
    }
}