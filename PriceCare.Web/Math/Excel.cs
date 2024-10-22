using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using Newtonsoft.Json;
using PriceCare.Central;
using PriceCare.Web.Constants;
using PriceCare.Web.Helpers;
using PriceCare.Web.Models;
using PriceCare.Web.Repository;
using System.Collections.Generic;

namespace PriceCare.Web.Math
{
    public class Excel
    {
        private readonly DataRepository dataRepository = new DataRepository();
        private readonly VersionRepository versionRepository = new VersionRepository();
        private readonly CacheRepository cacheRepository = new CacheRepository();
        private readonly CountryRepository countryRepository = new CountryRepository();
        private readonly Forecast forecastRepository = new Forecast();
        private readonly Analyzer analyserRepository = new Analyzer();
        public IEnumerable<DataExportViewModel> GetProductPriceChangesForExport(int simulationId, List<int> geographyId, List<int> productId)
        {
            var data = new List<DataViewModel>();
            foreach (var p in productId)
            {
                data.AddRange(forecastRepository.GetProductForSimulation(simulationId, geographyId, new List<int> { p }, new List<int> { (int)DataTypes.Price })
                    .Where(pS => System.Math.Abs(pS.PercentageVariation ?? 0) > ApplicationConstants.DoubleComparisonThreshold && pS.Reviewed));
            }
            var result = ProcessDataForExport(data, new List<int> { (int)DataTypes.Price });
            result = result.OrderBy(r => r.GeographyName)
                            .ThenBy(r => r.ProductName)
                            .ThenBy(r => r.PriceType)
                            .ThenBy(r => r.DataTime).ToList();
            return result;
        }
        public List<DataViewModel> GetProductsForSimulationExport(int simulationId, List<int> geographyId, List<int> productId, List<int> dataTypeId)
        {
            var tmpSimulation = new List<DataViewModel>();
            var dataType = dataTypeId[0];
            foreach (var product in productId)
            {
                var rows =
                    forecastRepository.GetProductForSimulation(simulationId, geographyId, new List<int> { product }, dataTypeId, true).ToList();

                if (dataType == (int)DataTypes.Event)
                {
                    rows = rows.Where(
                        p =>
                            p.EventTypeId != (int)EventTypes.NotSpecified &&
                            p.EventTypeId != (int)EventTypes.NoEvent).ToList();
                }
                else
                {
                    rows = rows.Where(p => p.Value != null).ToList();
                }

                tmpSimulation.AddRange(rows);
            }
            var existingGeographyId = tmpSimulation.Select(d => d.GeographyId).Distinct().ToList();
            tmpSimulation.AddRange(dataRepository.FillEmptyData(existingGeographyId, geographyId, productId, dataTypeId.First()));
            return tmpSimulation;
        }
        public HttpResponseMessage GetLaunchExcel(string token)
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
            var filter = JsonConvert.DeserializeObject<ExcelSearchRequestViewModel>(buffer.FilterJson);
            const string fileName = ExcelTemplateNameConstants.LaunchScenario;
            const string extention = "xlsm";
            const string FileName = "LaunchScenario_";

            string templateFilePath = HttpContext.Current.Server.MapPath(@"~\App_Data\" + fileName);

            var memoryStream = FillLaunchExcelFile(templateFilePath, filter);

            response.Content = new StreamContent(memoryStream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(templateFilePath));
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = FileName + DateTime.Now.Ticks + "." + extention
            };

            return response;
        }
        public HttpResponseMessage GetExcel(string token, bool forData, int launchOption = 1)
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
            var filter = JsonConvert.DeserializeObject<ExcelSearchRequestViewModel>(buffer.FilterJson);

            string fileName;
            string FileName;
            switch (filter.DataTypeId)
            {
                case (int)DataTypes.Price:
                    fileName = forData
                        ? (filter.DatabaseLike ? ExcelFileNameConstants.DataPrice : ExcelTemplateNameConstants.DataPrice)
                        : (filter.DatabaseLike ? ExcelFileNameConstants.ForecastPrice : ExcelTemplateNameConstants.ForecastPrice);
                    FileName = "Price_";
                    break;
                case (int)DataTypes.Volume:
                    fileName = forData
                        ? (filter.DatabaseLike ? ExcelFileNameConstants.DataVolume : ExcelTemplateNameConstants.DataVolume)
                        : (filter.DatabaseLike ? ExcelFileNameConstants.ForecastVolume : ExcelTemplateNameConstants.ForecastVolume);
                    FileName = "Volume_";
                    break;
                case (int)DataTypes.Event:
                    fileName = forData
                        ? (filter.DatabaseLike ? ExcelFileNameConstants.DataEvent : ExcelTemplateNameConstants.DataEvent)
                        : (filter.DatabaseLike ? ExcelFileNameConstants.ForecastEvent : ExcelTemplateNameConstants.ForecastEvent);
                    FileName = "Event_";
                    break;
                default:
                    return null;
            }

            var extention = filter.DatabaseLike ? "xlsx" : "xlsm";

            string templateFilePath = HttpContext.Current.Server.MapPath(@"~\App_Data\" + fileName);

            var memoryStream = FillExcelFile(templateFilePath, filter, forData, launchOption);

            response.Content = new StreamContent(memoryStream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(templateFilePath));
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = FileName + DateTime.Now.Ticks + "." + extention
            };

            return response;
        }
        private MemoryStream FillExcelFile(string templateFilePath, ExcelSearchRequestViewModel filter, bool forData, int launchOption)
        {
            DateTime minDate;
            DateTime maxDate;
            
            var fileXls = File.ReadAllBytes(templateFilePath);

            var products = filter.AllProducts ? new ProductRepository().GetAllProducts().Select(p => p.Id).ToList() : filter.ProductId;
            var countries = filter.AllCountries
                ? new CountryRepository().GetAllCountries().Select(c => c.Id).ToList()
                : filter.GeographyId;

            IEnumerable<DataViewModel> exportData;
            if (forData)
            {
                if (filter.VersionId.HasValue)
                {
                    exportData = dataRepository.GetVersionData(
                        filter.ScenarioTypeId,
                        (int)filter.VersionId,
                        0,
                        countries,
                        products,
                        new List<int> { filter.DataTypeId },
                        false).ToList();
                }
                else
                {
                    exportData = dataRepository.GetLatestData(
                        filter.ScenarioTypeId,
                        countries,
                        products,
                        new List<int> { filter.DataTypeId },
                        false).ToList();
                }
                minDate = new DateTime(DateTime.Now.Year - 1,1,1);
                maxDate = new DateTime(DateTime.Now.Year + ApplicationConstants.MaximumSimulationDuration, 12, 1);
            }
            else
            {
                exportData = GetProductsForSimulationExport(filter.SimulationId, countries, products, new List<int> { filter.DataTypeId }).ToList();
                var simulationInfo = cacheRepository.GetSimulation(filter.SimulationId);
                minDate = new DateTime(simulationInfo.StartTime.Year - 1, 1, 1);
                maxDate = new DateTime(simulationInfo.StartTime.Year + simulationInfo.Duration + 1, 12, 1);
            }

            switch (launchOption)
            {
                case ApplicationConstants.LaunchOptionScenario:
                    exportData.ForEach(e =>
                    {
                        e.EventTypeId = (int)EventTypes.LaunchTarget;
                        e.Value = 0;
                    });
                    break;
                case ApplicationConstants.LaunchOptionAssumptions:
                    exportData.ForEach(e => e.GeographyId = ApplicationConstants.NotSpecifiedProduct);
                    cacheRepository.CacheData(exportData.Where(e => e.EventTypeId != (int)EventTypes.NotSpecified && e.EventTypeId != (int)EventTypes.NoEvent).ToList(), filter.SimulationId, filter.UserId);
                    break;
            }

            var sheetDatas = new Dictionary<string, List<List<string>>>();



            var data = filter.DataTypeId == (int)DataTypes.Event
                ? FillDataForEvent(ProcessDataForExport(exportData, new List<int> { filter.DataTypeId }))
                : FillDataForPriceAndVolume(ProcessDataForExport(exportData, new List<int> { filter.DataTypeId }));
            sheetDatas.Add(data.Name, data.Datas);
            if (!filter.DatabaseLike)
            {
                if (exportData.Count() != 0)
                {
                    minDate = exportData.Min(d => d.DataTime);
                    maxDate = exportData.Max(d => d.DataTime);
                }

                SheetData metadata;
                switch (filter.DataTypeId)
                {
                    case (int)DataTypes.Price:
                        metadata = FillMetaDataPrice(filter.SaveId, filter.SimulationId, filter.VersionId == null ? 0 : (int)filter.VersionId, minDate, maxDate, forData);
                        break;
                    case (int)DataTypes.Volume:
                        metadata = FillMetaDataVolume(filter.SaveId, filter.SimulationId, filter.VersionId == null ? 0 : (int)filter.VersionId, minDate, maxDate, forData);
                        break;
                    default:
                        metadata = FillMetaDataEvent(filter.SaveId, filter.SimulationId,
                            filter.VersionId == null ? 0 : (int) filter.VersionId, forData);
                        break;
                }
                
                sheetDatas.Add(metadata.Name, metadata.Datas);
                var listData = FillListData();
                sheetDatas.Add(listData.Name, listData.Datas);
                if (filter.DataTypeId != (int) DataTypes.Price || forData)
                {
                    var setup = FillSetup("ExcelUrlDataExport");
                    sheetDatas.Add(setup.Name, setup.Datas);
                }
            }

            var memoryStream = ClosedXmlHelper.AppendXlsxSheetsHavingHeaders(fileXls, sheetDatas);

            return memoryStream;
        }
        private MemoryStream FillLaunchExcelFile(string templateFilePath, ExcelSearchRequestViewModel filter)
        {
            var fileXls = File.ReadAllBytes(templateFilePath);

            var products = filter.ProductId;
            var countries = new CountryRepository().GetAllCountries().Select(c => c.Id).ToList();

            var versionId = filter.VersionId ?? 0;
            
            var launchScenario = dataRepository.FillEmptyData(new List<int>(), countries, new List<int> {ApplicationConstants.NotSpecifiedProduct}, (int) DataTypes.Event).ToList();
            launchScenario.ForEach(e =>
            {
                e.EventTypeId = (int)EventTypes.LaunchTarget;
                e.Value = 0;
            });

            var startYear = DateTime.Now.Year;
            var endYear = startYear + filter.Duration;

            var launchAssumptions = dataRepository.GetVersionData(new List<int>(), products, (int)DataTypes.Event, filter.ScenarioTypeId, versionId, versionId, startYear, endYear).ToList();
            launchAssumptions.ForEach(e => e.ProductId = ApplicationConstants.NotSpecifiedProduct);
            cacheRepository.CacheData(launchAssumptions
                .Where(e => 
                    e.EventTypeId != (int)EventTypes.NotSpecified && 
                    e.EventTypeId != (int)EventTypes.NoEvent &&
                    e.EventTypeId != (int)EventTypes.LaunchTarget &&
                    e.EventTypeId != (int)EventTypes.LaunchDefaultRule &&
                    e.EventTypeId != (int)EventTypes.LaunchLaunchRule).ToList(), filter.SimulationId, filter.UserId);

            var volumeForecast = dataRepository.FillEmptyData(new List<int>(), countries, new List<int> { ApplicationConstants.NotSpecifiedProduct }, (int)DataTypes.Volume).ToList();
            var sheetDatas = new Dictionary<string, List<List<string>>>();
            
            var launchScenarioTab =
                FillDataForEvent(ProcessDataForExport(launchScenario, new List<int> {(int) DataTypes.Event}), "Launches");
            sheetDatas.Add(launchScenarioTab.Name, launchScenarioTab.Datas);

            var launchAssumptionsTab =
                FillDataForEvent(ProcessDataForExport(launchAssumptions, new List<int> { (int)DataTypes.Event }), "Events");
            sheetDatas.Add(launchAssumptionsTab.Name, launchAssumptionsTab.Datas);

            var volumeForecastTab = FillDataForPriceAndVolume(ProcessDataForExport(volumeForecast, new List<int> { (int)DataTypes.Volume }), "Volume");
            sheetDatas.Add(volumeForecastTab.Name, volumeForecastTab.Datas);

            var metaDataEvent = FillMetaDataEvent(filter.SaveId, filter.SimulationId, versionId, false, "MetaEvents");
            sheetDatas.Add(metaDataEvent.Name, metaDataEvent.Datas);

            var minDate = volumeForecast.Min(d => d.DataTime);
            var maxDate = volumeForecast.Max(d => d.DataTime);
            var metaDataVolume = FillMetaDataVolume(filter.SaveId, filter.SimulationId, versionId, minDate, maxDate, false, "MetaVolume");
            sheetDatas.Add(metaDataVolume.Name, metaDataVolume.Datas);

            var listData = FillListData();
            sheetDatas.Add(listData.Name, listData.Datas);

            var setup = FillSetup("ExcelUrlDataExport");
            sheetDatas.Add(setup.Name, setup.Datas);

            var memoryStream = ClosedXmlHelper.AppendXlsxSheetsHavingHeaders(fileXls, sheetDatas);

            return memoryStream;
        }
        public SheetData FillSetup(string keyUrl)
        {
            var allData = new List<List<string>>();
            var accountRepository = new AccountRepository();
            var login = accountRepository.GetUserInfo().Username;
            var url = ConfigurationManager.AppSettings[keyUrl];

            allData.Add(new List<string> { "Data export sheet", "Export", "MetaData" });
            allData.Add(new List<string> { "Header row", "1" });
            allData.Add(new List<string> { "First data column", "1" });
            allData.Add(new List<string> { "Last data column", "13" });
            allData.Add(new List<string> { "First data row", "2" });
            allData.Add(new List<string> { "Last data row", "5" });
            allData.Add(new List<string> { "Model", "ExcelDataViewModel", "ExcelData" });
            allData.Add(new List<string> { "Url", url });
            allData.Add(new List<string> { "First metadata row", "1" });
            allData.Add(new List<string> { "Last metadata row", "3" });
            allData.Add(new List<string> { "Login", login });
            allData.Add(new List<string> { "Was open", "FALSE" });
            allData.Add(new List<string> { "Was pushed", "FALSE" });

            return new SheetData { Name = "Setup", Datas = allData };
        }
        private SheetData FillMetaDataEvent(int saveId, int simulationid, int versionId, bool forData, string tab = null)
        {
            var allData = new List<List<string>>();

            allData.Add(new List<string> { "SaveId", saveId == 0 ? "1" : saveId.ToString() });
            allData.Add(new List<string> { "DeactivatePreviousVersion", "false" });
            allData.Add(new List<string> { "SimulationId", simulationid.ToString() });

            if (forData)
            {
                var versionInfos = versionRepository.GetVersionInfos(versionId);
                allData.Add(new List<string> { "VersionDate", versionInfos.VersionTimeAsString });
            }

            return new SheetData { Name = tab ?? "MetaData", Datas = allData };
        }
        private SheetData FillMetaDataVolume(int saveId, int simulationid, int versionId, DateTime minDate, DateTime maxDate, bool forData, string tab = null)
        {
            var allData = new List<List<string>>();
            if (forData)
            {
                allData.Add(new List<string> { "SaveId", saveId == 0 ? "1" : saveId.ToString() });
                allData.Add(new List<string> { "DeactivatePreviousVersion", "false" });
                allData.Add(new List<string> { "SimulationId", simulationid.ToString() });
                var versionInfos = versionRepository.GetVersionInfos(versionId);
                allData.Add(new List<string> { "VersionDate", versionInfos.VersionTimeAsString });
                allData.Add(new List<string> { "StartTime", minDate.Date.ToString() });
                allData.Add(new List<string> { "EndTime", maxDate.Date.ToString() });
            }
            else
            {
                allData.Add(new List<string> { "SaveId", saveId.ToString() });
                allData.Add(new List<string> { "DeactivatePreviousVersion", "false" });
                allData.Add(new List<string> { "SimulationId", simulationid.ToString() });
                var simulationInfo = new CacheRepository().GetSimulation(simulationid);
                allData.Add(new List<string> { "Scenario Date", simulationInfo.StartTime.ToString() });
                allData.Add(new List<string> { "StartTime", minDate.Date.ToString() });
                allData.Add(new List<string> { "EndTime", maxDate.Date.ToString() });
            }
            return new SheetData { Name = tab ?? "MetaData", Datas = allData };
        }
        private SheetData FillMetaDataPrice(int saveId, int simulationid, int versionId, DateTime minDate, DateTime maxDate, bool forData, string tab = null)
        {
            var allData = new List<List<string>>();

            if (forData)
            {
                allData.Add(new List<string> { "SaveId", saveId == 0 ? "1" : saveId.ToString() });
                allData.Add(new List<string> { "DeactivatePreviousVersion", "false" });

                allData.Add(new List<string> { "SimulationId", simulationid.ToString() });

                var versionInfos = versionRepository.GetVersionInfos(versionId);
                allData.Add(new List<string> { "VersionDate", versionInfos.VersionTimeAsString });

                allData.Add(new List<string> { "StartTime", minDate.Date.ToString() });
                allData.Add(new List<string> { "EndTime", maxDate.Date.ToString() });
            }
            else
            {
                allData.Add(new List<string> { "SaveId", saveId.ToString() });
                allData.Add(new List<string> { "DeactivatePreviousVersion", "false" });

                var simulation = new CacheRepository().GetSimulation(simulationid);
                allData.Add(new List<string> { "SimulationDate", simulation.StartTime.Date.ToString() });

                allData.Add(new List<string> { "StartTime", minDate.Date.ToString() });
                allData.Add(new List<string> { "EndTime", maxDate.Date.ToString() });
            }

            return new SheetData { Name = tab ?? "MetaData", Datas = allData };
        }
        public SheetData FillListData()
        {
            var allData = new List<List<string>>();

            allData.Add(new List<string>
            {
                "Name",
                "Id",
                "Name",
                "ShortName",
                "ProductId",
                "Name",
                "GeographyId"
            });

            var countries = new CountryRepository().GetCountryExport().ToList();
            var products = new ProductRepository().GetProductExport().ToList();
            var eventTypes = new DimensionRepository().GetEventType().ToList();

            for (var i = 0; i < countries.Count() || i < products.Count() || i < eventTypes.Count(); i++)
            {
                var list = new List<string>();
                list.Add((i < eventTypes.Count()) ? eventTypes[i].Name : "");
                list.Add((i < eventTypes.Count()) ? eventTypes[i].Id.ToString() : "");
                list.Add((i < products.Count()) ? products[i].Name : "");
                list.Add((i < products.Count()) ? products[i].ShortName : "");
                list.Add((i < products.Count()) ? products[i].Id.ToString() : "");
                list.Add((i < countries.Count()) ? countries[i].Name : "");
                list.Add((i < countries.Count()) ? countries[i].Id.ToString() : "");

                allData.Add(list);
            }

            return new SheetData { Name = "Lists", Datas = allData };
        }
        public IEnumerable<DataExportViewModel> ProcessDataForExport(IEnumerable<DataViewModel> datas, List<int> dataTypeId)
        {
            var allCountries = countryRepository.GetCountryExport().ToDictionary(x => x.Id, x => x.Name);
            var allProducts = new ProductRepository().GetProductExport().ToDictionary(x => x.Id, x => x.Name);
            var allEventTypes = new DimensionRepository().GetEventType().ToDictionary(x => x.Id, x => x.Name);
            var allPriceTypes = new PriceTypeRepository().GetAllPriceTypes(0, null).ToDictionary(x => x.Id, x => x.ShortName);

            var result = datas.Select(d => new DataExportViewModel
            {
                Active = d.Active,
                CurrencySpotId = d.CurrencySpotId,
                CurrencySpotVersionId = d.CurrencySpotVersionId,
                DataTime = d.DataTime,
                DataTypeId = d.DataTypeId,
                Description = d.Description,
                Edited = d.Edited,
                EurBudget = d.EurBudget,
                EurSpot = d.EurSpot,
                EventName = allEventTypes[d.EventTypeId],
                UsdBudget = d.UsdBudget,
                UsdSpot = d.UsdSpot,
                EventTypeId = d.EventTypeId,
                UnitTypeId = d.UnitTypeId,
                PriceTypeId = d.PriceTypeId,
                PriceType = allPriceTypes[d.PriceTypeId],
                GeographyId = d.GeographyId,
                GeographyName = allCountries[d.GeographyId],
                ProductId = d.ProductId,
                ProductName = allProducts[d.ProductId],
                SegmentId = d.SegmentId,
                Month = d.DataTime.Month,
                Year = d.DataTime.Year,
                Value = d.Value,
                PercentageVariation = d.PercentageVariation,
                PercentageVariationFromX = d.PercentageVariationFromX
            });

            var dataType = dataTypeId[0];
            switch (dataType) // order by
            {
                case (int)DataTypes.Event:
                    result = result.OrderBy(r => r.GeographyName)
                            .ThenBy(r => r.ProductName)
                            .ThenBy(r => r.EventName)
                            .ThenBy(r => r.DataTime);
                    break;
                default: // Price + Volume
                    result = result.OrderBy(r => r.GeographyName)
                            .ThenBy(r => r.ProductName)
                            .ThenBy(r => r.PriceType)
                            .ThenBy(r => r.DataTime);
                    break;
            }

            return result;
        }
        public SheetData FillDataForEvent(IEnumerable<DataExportViewModel> datas, string tab = null)
        {
            var allData = new List<List<string>>();
            var headers = new List<string>
            {
                "Geography Id",
                "Name",
                "Product Id",
                "Name",
                "EventTypeId",
                "EventName",
                "DataTime",
                "Year",
                "Month",
                "Value",
                "CurrencySpotId",
                "CurrencySpotVersionId",
                "DataTypeId",
                "PriceTypeId"
            };

            allData.Add(headers);

            allData.AddRange(datas.Select(l => new List<string>
            {       
                l.GeographyId.ToString(),
                l.GeographyName,
                l.ProductId.ToString(),
                l.ProductName,
                l.EventTypeId.ToString(),
                l.EventName,
                l.DataTime.Date.ToString(),
                l.Year.ToString(),
                l.Month.ToString(),
                l.Value.ToString(),
                l.CurrencySpotId.ToString(),
                l.CurrencySpotVersionId.ToString(),
                l.DataTypeId.ToString(),
                l.PriceTypeId.ToString()
            }));

            return new SheetData { Name = tab ?? "Data", Datas = allData };
        }
        public SheetData FillDataForPriceAndVolume(IEnumerable<DataExportViewModel> datas, string tab = null)
        {
            var allData = new List<List<string>>();
            var headers = new List<string>
            {
                "Perm Id",
                "Geography Id",
                "Name",
                "Product Id",
                "Name",
                "CurrencySpotId",
                "CurrencySpotVersionId",
                "PriceTypeId",
                "PriceTypeName",
                "DataTypeId",
                "EventTypeId",
                "EventName",
                "UnitTypeId",
                "SegmentId",
                "DataTime",
                "Year",
                "Month",
                "Value",
                "Description",
                "UsdSpot",
                "EurSpot",
                "UsdBudget",
                "EurBudget"
            };

            allData.Add(headers);

            allData.AddRange(datas.Select(l => new List<string>
            {       
                l.PermId.ToString(),
                l.GeographyId.ToString(),
                l.GeographyName,
                l.ProductId.ToString(),
                l.ProductName,
                l.CurrencySpotId.ToString(),
                l.CurrencySpotVersionId.ToString(),
                l.PriceTypeId.ToString(),
                l.PriceType,
                l.DataTypeId.ToString(),
                l.EventTypeId.ToString(),
                l.EventName,
                l.UnitTypeId.ToString(),
                l.SegmentId.ToString(),
                l.DataTime.Date.ToString(),
                l.Year.ToString(),
                l.Month.ToString(),
                l.Value.ToString(),
                l.Description,
                l.UsdSpot.ToString(),
                l.EurSpot.ToString(),
                l.UsdBudget.ToString(),
                l.EurBudget.ToString()
            }).ToList());

            return new SheetData { Name = tab ?? "Data", Datas = allData };
        }

        public HttpResponseMessage GetExcelForReviewedPriceForecast(string token)
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
            var filter = JsonConvert.DeserializeObject<PriceRequestViewModel>(buffer.FilterJson);

            var fileName = filter.DatabaseLike ? ExcelFileNameConstants.ReviewedPriceForecast : ExcelTemplateNameConstants.ReviewedPriceForecast;
            var extention = filter.DatabaseLike ? "xlsx" : "xlsm";

            string templateFilePath = HttpContext.Current.Server.MapPath(@"~\App_Data\" + fileName);

            var memoryStream = FillExcelFileForReviewedPriceForecast(templateFilePath, filter);

            response.Content = new StreamContent(memoryStream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(templateFilePath));
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "priceReviewed_" + DateTime.Now.Ticks + "." + extention
            };

            return response;
        }
        private MemoryStream FillExcelFileForReviewedPriceForecast(string templateFilePath, PriceRequestViewModel filter)
        {
            var fileXls = File.ReadAllBytes(templateFilePath);

            var products = (filter.AllProducts || filter.ProductId == 0)
                ? new ProductRepository().GetAllProducts().Select(p => p.Id).ToList()
                : new List<int> { filter.ProductId };

            var countries = (filter.AllCountries)
                ? new CountryRepository().GetAllCountries().Select(c => c.Id).ToList()
                : filter.GeographyIds;

            var prices = GetProductsForSimulationExport(filter.SimulationId, countries, products,
                new List<int> { (int)DataTypes.Price }).Where(d => d.IsPublic);

            var pricesReviewed = prices.Where(p => p.Reviewed).ToList();

            var sheetDatas = new Dictionary<string, List<List<string>>>();

            var data = FillDataForPriceAndVolume(ProcessDataForExport(pricesReviewed, new List<int> { (int)DataTypes.Price }));
            sheetDatas.Add(data.Name, data.Datas);
            if (!filter.DatabaseLike)
            {
                DateTime minDate = pricesReviewed.Any() ? pricesReviewed.Min(d => d.DataTime) : DateTime.MinValue;
                DateTime maxDate = pricesReviewed.Any() ? pricesReviewed.Max(d => d.DataTime) : DateTime.MaxValue;
                var metadata = FillMetaDataForReviewedPriceForecast(filter.SaveId, minDate, maxDate);
                sheetDatas.Add(metadata.Name, metadata.Datas);
            }

            var memoryStream = ClosedXmlHelper.AppendXlsxSheetsHavingHeaders(fileXls, sheetDatas);
            return memoryStream;
        }
        private SheetData FillMetaDataForReviewedPriceForecast(int saveId, DateTime minDate, DateTime maxDate)
        {
            var allData = new List<List<string>>();
            allData.Add(new List<string> { "SaveId", saveId == 0 ? "1" : saveId.ToString() });
            allData.Add(new List<string> { "StartTime", minDate.Date.ToString() });
            allData.Add(new List<string> { "EndTime", maxDate.Date.ToString() });
            return new SheetData { Name = "MetaData", Datas = allData };
        }

        public HttpResponseMessage GetExcelForReviewedPriceChanges(string token)
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
            var filter = JsonConvert.DeserializeObject<PriceRequestViewModel>(buffer.FilterJson);

            var fileName = filter.DatabaseLike ? ExcelFileNameConstants.ReviewedPriceChanges : ExcelTemplateNameConstants.ReviewedPriceChanges;
            var extention = filter.DatabaseLike ? "xlsx" : "xlsm";

            string templateFilePath = HttpContext.Current.Server.MapPath(@"~\App_Data\" + fileName);

            var memoryStream = FillExcelFileForReviewedPriceChanges(templateFilePath, filter);

            response.Content = new StreamContent(memoryStream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(templateFilePath));
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "priceChanges_" + DateTime.Now.Ticks + "." + extention
            };

            return response;
        }
        private MemoryStream FillExcelFileForReviewedPriceChanges(string templateFilePath, PriceRequestViewModel filter)
        {
            var fileXls = File.ReadAllBytes(templateFilePath);

            var products = (filter.AllProducts || filter.ProductId == 0)
                ? new ProductRepository().GetAllProducts().Select(p => p.Id).ToList()
                : new List<int> { filter.ProductId };

            var countries = (filter.AllCountries)
                ? new CountryRepository().GetAllCountries().Select(c => c.Id).ToList()
                : filter.GeographyIds;

            var prices = GetProductPriceChangesForExport(filter.SimulationId, countries, products);

            var sheetDatas = new Dictionary<string, List<List<string>>>();

            var data = FillDataForPriceChanges(prices);
            sheetDatas.Add(data.Name, data.Datas);
            if (!filter.DatabaseLike)
            {
                var metadata = FillMetaDataForReviewedPriceChanges(filter.SimulationId);
                sheetDatas.Add(metadata.Name, metadata.Datas);
            }

            var memoryStream = ClosedXmlHelper.AppendXlsxSheetsHavingHeaders(fileXls, sheetDatas);
            return memoryStream;
        }


        private SheetData FillMetaDataForReviewedPriceChanges(int simulationId)
        {
            var allData = new List<List<string>>();
            var simulation = new CacheRepository().GetSimulation(simulationId);
            allData.Add(new List<string> { "Simulation start date", simulation.StartTime.Date.ToString() });
            allData.Add(new List<string> { "Simulation end date", simulation.StartTime.AddYears(simulation.Duration).Date.ToString() });
            return new SheetData { Name = "MetaData", Datas = allData };
        }


        public SheetData FillDataForPriceChanges(IEnumerable<DataExportViewModel> datas)
        {
            var allData = new List<List<string>>();
            var headers = new List<string>
            {
                "Perm Id",
                "Geography Id",
                "Name",
                "Product Id",
                "Name",
                "CurrencySpotId",
                "CurrencySpotVersionId",
                "PriceTypeId",
                "PriceTypeName",
                "DataTypeId",
                "EventTypeId",
                "EventName",
                "UnitTypeId",
                "SegmentId",
                "DataTime",
                "Year",
                "Month",
                "Value",
                "Description",
                "UsdSpot",
                "EurSpot",
                "UsdBudget",
                "EurBudget",
                "PercentageVariation"
            };

            allData.Add(headers);

            allData.AddRange(datas.Select(l => new List<string>
            {       
                l.PermId.ToString(),
                l.GeographyId.ToString(),
                l.GeographyName,
                l.ProductId.ToString(),
                l.ProductName,
                l.CurrencySpotId.ToString(),
                l.CurrencySpotVersionId.ToString(),
                l.PriceTypeId.ToString(),
                l.PriceType,
                l.DataTypeId.ToString(),
                l.EventTypeId.ToString(),
                l.EventName,
                l.UnitTypeId.ToString(),
                l.SegmentId.ToString(),
                l.DataTime.Date.ToString(),
                l.Year.ToString(),
                l.Month.ToString(),
                l.Value.ToString(),
                l.Description,
                l.UsdSpot.ToString(),
                l.EurSpot.ToString(),
                l.UsdBudget.ToString(),
                l.EurBudget.ToString(),
                l.PercentageVariation.ToString() ?? ""
            }).ToList());

            return new SheetData { Name = "Data", Datas = allData };
        }

        public HttpResponseMessage GetAspForecastExcel(string token)
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
            var filter = JsonConvert.DeserializeObject<AspForecastSearchViewModel>(buffer.FilterJson);

            var fileName = filter.DatabaseLike ? ExcelFileNameConstants.AspForecast : ExcelTemplateNameConstants.AspForecast;
            var extention = filter.DatabaseLike ? "xlsx" : "xlsm";

            string templateFilePath = HttpContext.Current.Server.MapPath(@"~\App_Data\" + fileName);

            var memoryStream = FillExcelFile(templateFilePath, filter);

            response.Content = new StreamContent(memoryStream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(templateFilePath));
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "aspForecast_" + DateTime.Now.Ticks + "." + extention
            };

            return response;
        }

        private MemoryStream FillExcelFile(string templateFilePath, AspForecastSearchViewModel filter)
        {
            var fileXls = File.ReadAllBytes(templateFilePath);

            var countries = filter.AllCountries
                ? new CountryRepository().GetAllCountries().Select(c => c.Id).ToList()
                : filter.Countries.ToList();
            var products = filter.AllProducts
                ? new ProductRepository().GetAllProducts().Select(p => p.Id).ToList()
                : filter.Products.ToList();

            var datas = new Tuple<List<DataExportViewModel>, List<DataExportViewModel>>(new List<DataExportViewModel>(), new List<DataExportViewModel>());

            foreach (var data in products.Select(p => GetNetExport(filter.SimulationId, countries, new List<int>{ p })))
            {
                datas.Item1.AddRange(data.Item1);
                datas.Item2.AddRange(data.Item2);
            }
            
            var sheetDatas = new Dictionary<string, List<List<string>>>();
            var dataPrice = FillDataForAspForecast(datas.Item1, "DataPrice");
            sheetDatas.Add(dataPrice.Name, dataPrice.Datas);
            var dataSale = FillDataForAspForecast(datas.Item2, "DataSale");
            sheetDatas.Add(dataSale.Name, dataSale.Datas);
            if (!filter.DatabaseLike)
            {
                var metadata = FillMetaDataForAspForecast(filter, datas);
                sheetDatas.Add(metadata.Name, metadata.Datas);
            }

            var memoryStream = ClosedXmlHelper.AppendXlsxSheetsHavingHeaders(fileXls, sheetDatas);

            return memoryStream;
        }

        private SheetData FillMetaDataForAspForecast(AspForecastSearchViewModel filter, Tuple<List<DataExportViewModel>, List<DataExportViewModel>> datas)
        {
            var allData = new List<List<string>>();
            allData.Add(new List<string> { "SaveId", filter.SaveId.ToString() });
            var startTimePrice = datas.Item1.Min(p => p.DataTime); // price
            var startTimeSale = datas.Item2.Min(p => p.DataTime); // value
            allData.Add(new List<string> { "Start Time", startTimePrice < startTimeSale ? startTimePrice.Date.ToString() : startTimeSale.Date.ToString() });
            var endTimePrice = datas.Item1.Max(p => p.DataTime);
            var endTimeSale = datas.Item2.Max(p => p.DataTime);
            allData.Add(new List<string> { "End Time", endTimePrice > endTimeSale ? endTimePrice.Date.ToString() : endTimeSale.Date.ToString() });
            allData.Add(new List<string> { "SimulationId", startTimePrice < startTimeSale ? startTimePrice.Date.ToString() : startTimeSale.Date.ToString() });
            return new SheetData { Name = "MetaData", Datas = allData };
        }

        private SheetData FillDataForAspForecast(List<DataExportViewModel> datas, string sheetName)
        {
            var allData = new List<List<string>>();
            var headers = new List<string>
            {
                "Perm Id",
                "Geography Id",
                "Name",
                "Product Id",
                "Name",
                "CurrencySpotId",
                "CurrencySpotVersionId",
                "PriceTypeId",
                "PriceTypeName",
                "DataTypeId",
                "EventTypeId",
                "EventName",
                "UnitTypeId",
                "SegmentId",
                "DataTime",
                "Year",
                "Month",
                "Value",
                "Description",
                "UsdSpot",
                "EurSpot",
                "UsdBudget",
                "EurBudget"
            };

            allData.Add(headers);

            allData.AddRange(datas.Select(l => new List<string>
            {
                l.PermId.ToString(),
                l.GeographyId.ToString(),
                l.GeographyName,
                l.ProductId.ToString(),
                l.ProductName,
                l.CurrencySpotId.ToString(),
                l.CurrencySpotVersionId.ToString(),
                l.PriceTypeId.ToString(),
                l.PriceType,
                l.DataTypeId.ToString(),
                l.EventTypeId.ToString(),
                l.EventName,
                l.UnitTypeId.ToString(),
                l.SegmentId.ToString(),
                l.DataTime.Date.ToString(),
                l.Year.ToString(),
                l.Month.ToString(),
                l.Value.ToString(),
                l.Description,
                l.UsdSpot.ToString(),
                l.EurSpot.ToString(),
                l.UsdBudget.ToString(),
                l.EurBudget.ToString()
            }));

            return new SheetData { Name = sheetName, Datas = allData };
        }

        public Tuple<List<DataExportViewModel>, List<DataExportViewModel>> GetNetExport(int simulationId, List<int> geographyId, List<int> productId)
        {
            var allCountries = new CountryRepository().GetCountryExport().ToDictionary(x => x.Id, x => x.Name);
            var allProducts = new ProductRepository().GetProductExport().ToDictionary(x => x.Id, x => x.Name);
            var allEventTypes = new DimensionRepository().GetEventType().ToDictionary(x => x.Id, x => x.Name);
            var allPriceTypes = new PriceTypeRepository().GetAllPriceTypes(0, null).ToDictionary(x => x.Id, x => x.Name);
            var netData = analyserRepository.NetSimulationForecast(simulationId, geographyId, productId);
            var aspData = netData.Price.Select(nD => new DataExportViewModel
            {
                GeographyId = nD.GeographyId,
                GeographyName = allCountries[nD.GeographyId],
                ProductId = nD.ProductId,
                ProductName = allProducts[nD.ProductId],
                CurrencySpotId = nD.CurrencySpotId,
                CurrencySpotVersionId = nD.CurrencySpotVersionId,
                PriceTypeId = nD.PriceTypeId,
                PriceType = allPriceTypes[nD.PriceTypeId],
                DataTypeId = nD.DataTypeId,
                EventTypeId = nD.EventTypeId,
                EventName = allEventTypes[nD.EventTypeId],
                UnitTypeId = nD.UnitTypeId,
                SegmentId = nD.SegmentId,
                DataTime = nD.DataTime,
                Year = nD.DataTime.Year,
                Month = nD.DataTime.Month,
                Value = nD.Value,
                Description = nD.Description,
                UsdSpot = nD.UsdSpot,
                EurSpot = nD.EurSpot,
                UsdBudget = nD.UsdBudget,
                EurBudget = nD.EurBudget
            }).OrderBy(r => r.GeographyName)
                            .ThenBy(r => r.ProductName)
                            .ThenBy(r => r.EventName)
                            .ThenBy(r => r.DataTime).ToList();
            var salesData = netData.Sales.Select(nD => new DataExportViewModel
            {
                GeographyId = nD.GeographyId,
                GeographyName = allCountries[nD.GeographyId],
                ProductId = nD.ProductId,
                ProductName = allProducts[nD.ProductId],
                CurrencySpotId = nD.CurrencySpotId,
                CurrencySpotVersionId = nD.CurrencySpotVersionId,
                PriceTypeId = nD.PriceTypeId,
                PriceType = allPriceTypes[nD.PriceTypeId],
                DataTypeId = nD.DataTypeId,
                EventTypeId = nD.EventTypeId,
                EventName = allEventTypes[nD.EventTypeId],
                UnitTypeId = nD.UnitTypeId,
                SegmentId = nD.SegmentId,
                DataTime = nD.DataTime,
                Year = nD.DataTime.Year,
                Month = nD.DataTime.Month,
                Value = nD.Value,
                Description = nD.Description,
                UsdSpot = nD.UsdSpot,
                EurSpot = nD.EurSpot,
                UsdBudget = nD.UsdBudget,
                EurBudget = nD.EurBudget
            }).OrderBy(r => r.GeographyName)
                            .ThenBy(r => r.ProductName)
                            .ThenBy(r => r.EventName)
                            .ThenBy(r => r.DataTime).ToList(); ;
            return new Tuple<List<DataExportViewModel>, List<DataExportViewModel>>(aspData, salesData);
        }
        public SheetData FillDataForPriceAndVolume(IEnumerable<DataExportViewModel> datas)
        {
            var allData = new List<List<string>>();
            var headers = new List<string>
            {
                "Perm Id",
                "Geography Id",
                "Name",
                "Product Id",
                "Name",
                "CurrencySpotId",
                "CurrencySpotVersionId",
                "PriceTypeId",
                "PriceTypeName",
                "DataTypeId",
                "EventTypeId",
                "EventName",
                "UnitTypeId",
                "SegmentId",
                "DataTime",
                "Year",
                "Month",
                "Value",
                "Description",
                "UsdSpot",
                "EurSpot",
                "UsdBudget",
                "EurBudget"
            };

            allData.Add(headers);

            allData.AddRange(datas.Select(l => new List<string>
            {       
                l.PermId.ToString(),
                l.GeographyId.ToString(),
                l.GeographyName,
                l.ProductId.ToString(),
                l.ProductName,
                l.CurrencySpotId.ToString(),
                l.CurrencySpotVersionId.ToString(),
                l.PriceTypeId.ToString(),
                l.PriceType,
                l.DataTypeId.ToString(),
                l.EventTypeId.ToString(),
                l.EventName,
                l.UnitTypeId.ToString(),
                l.SegmentId.ToString(),
                l.DataTime.Date.ToString(),
                l.Year.ToString(),
                l.Month.ToString(),
                l.Value.ToString(),
                l.Description,
                l.UsdSpot.ToString(),
                l.EurSpot.ToString(),
                l.UsdBudget.ToString(),
                l.EurBudget.ToString()
            }).ToList());

            return new SheetData { Name = "Data", Datas = allData };
        }
    }
}