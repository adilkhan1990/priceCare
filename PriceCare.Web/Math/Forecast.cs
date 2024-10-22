using System;
using System.Linq;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing;
using PriceCare.Web.Constants;
using PriceCare.Web.Helpers;
using PriceCare.Web.Math.Utils;
using PriceCare.Web.Models;
using PriceCare.Web.Repository;
using System.Collections.Generic;

namespace PriceCare.Web.Math
{
    public class Forecast
    {
        private readonly DataRepository dataRepository = new DataRepository();
        private readonly VersionRepository versionRepository = new VersionRepository();
        private readonly GprmRuleRepository ruleRepository = new GprmRuleRepository();
        private readonly CacheRepository cacheRepository = new CacheRepository();
        private readonly DimensionRepository dimensionRepository = new DimensionRepository();
        private readonly CountryRepository countryRepository = new CountryRepository();
        private readonly SaveRepository saveRepository = new SaveRepository();
        #region Public
        public CacheViewModel CreateSimulation(string userId, int assumptionsSaveId, int simulationDuration, int simulationCurrencyId, int productId, bool isLaunch)
        {
            cacheRepository.ClearCache(userId);
            var dataVersionId = versionRepository.GetCurrentVersion();
            var startTime = !isLaunch ? dataRepository.GetVersionStartTime(dataVersionId) : new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);

            var simulationInfo = new CacheViewModel
            {
                AssumptionsSaveId = assumptionsSaveId,
                CurrencyBudgetVersionId = dataVersionId,
                DataVersionId = dataVersionId,
                Duration = simulationDuration,
                SaveId = 0,
                SimulationCurrencyId = simulationCurrencyId,
                StartTime = startTime,
                UserId = userId,
                Edited = false,
                IsCurrentUser = true,
                IsLaunch = isLaunch
            };
            simulationInfo.Id = cacheRepository.SetCache(simulationInfo);

            var rule = ruleRepository.GetVersionRule(0, productId, simulationInfo.StartTime, simulationInfo.Duration);

            cacheRepository.CreateCacheTable(userId);
            cacheRepository.CacheRule(rule, simulationInfo.Id, userId);

            return simulationInfo;
        }
        public CacheViewModel LoadSimulation(string userId, int saveId, int productId)
        {
            cacheRepository.ClearCache(userId);
            var simulationInfo = saveRepository.GetSimulationInfo(saveId);
            var rule = ruleRepository.GetVersionRule(simulationInfo.DataVersionId, productId, simulationInfo.StartTime, simulationInfo.Duration, simulationInfo.SaveId);

            var cacheInfo = new CacheViewModel
            {
                AssumptionsSaveId = simulationInfo.AssumptionsSaveId,
                CurrencyBudgetVersionId = simulationInfo.CurrencyBudgetVersionId,
                DataVersionId = simulationInfo.DataVersionId,
                Duration = simulationInfo.Duration,
                SaveId = simulationInfo.SaveId,
                SimulationCurrencyId = simulationInfo.SimulationCurrencyId,
                StartTime = simulationInfo.StartTime,
                UserId = userId,
                IsCurrentUser = simulationInfo.UserId == userId,
                Edited = false,
                IsLaunch = simulationInfo.IsLaunch
            };

            cacheInfo.Id = cacheRepository.SetCache(
                cacheInfo);
            cacheRepository.CreateCacheTable(userId);
            cacheRepository.CacheRule(rule, cacheInfo.Id, userId);
            return cacheInfo;
        }
        public List<DataViewModel> GetProductForSimulation(int simulationId, List<int> geographyId, List<int> productId, List<int> dataTypeId, bool includeHidden = false)
        {
            var simulationInfo = cacheRepository.GetSimulation(simulationId);
            var cacheData = cacheRepository.GetDataCache(simulationId, new List<int>(), productId, new List<int>());
            var cacheRule = cacheRepository.GetRule(simulationId, productId.First());

            var dataRule = simulationInfo.SaveId == 0 
                ? SetProductCacheForNewSimulation(simulationInfo, productId.First(), cacheData, cacheRule) 
                : SetProductCacheForSavedSimulation(simulationInfo, productId.First(), cacheData, cacheRule);
            return dataRule.Data
                .Where(d => (dataTypeId.Count == 0 || dataTypeId.Contains(d.DataTypeId)) &&
                    (geographyId.Count == 0 || geographyId.Contains(d.GeographyId)) && (includeHidden || d.IsPublic))
                .OrderBy(d => d.DataTime).ToList();
        }

        public List<DataViewModel> GetProductsForSimulation(int simulationId, List<int> geographyId, List<int> productId, List<int> dataTypeId)
        {
            var tmpSimulation = new List<DataViewModel>();
            foreach (var product in productId)
            {
                tmpSimulation.AddRange(
                    GetProductForSimulation(simulationId, geographyId, new List<int>{product}, dataTypeId).ToList());
            }
            return tmpSimulation;
        }
        public DataRule SetProductCacheForNewSimulation(CacheViewModel simulationInfo, int productId, List<DataViewModel> cacheData, List<RuleViewModel> cacheRule)
        {
            var userId = simulationInfo.UserId;
            var simulationId = simulationInfo.Id;
            var dataVersionId = simulationInfo.DataVersionId;
            var assumptionsSaveId = simulationInfo.AssumptionsSaveId;
            var simulationDuration = simulationInfo.Duration;
            var startTime = simulationInfo.StartTime;
            var startYear = startTime.Year - 1;
            var endYear = startYear + simulationDuration + 2;
            var simulationCurrencyId = simulationInfo.SimulationCurrencyId;
                
            var priceData = dataRepository.GetVersionData(new List<int>(), new List<int> { productId }, (int)DataTypes.Price, ApplicationConstants.ImportedDataSaveId, dataVersionId, dataVersionId,
                startYear, endYear).ToList();
            var volumeData = dataRepository.GetVersionData(new List<int>(), new List<int> { productId }, (int)DataTypes.Volume, ApplicationConstants.ImportedDataSaveId, dataVersionId, dataVersionId,
                startYear, endYear).ToList();
            var eventData = dataRepository.GetVersionData(new List<int>(), new List<int> { productId }, (int)DataTypes.Event, assumptionsSaveId, dataVersionId, dataVersionId,
                startYear, endYear).ToList();

            priceData = dataRepository.SetSaveData(priceData,
                cacheData.Where(cD => cD.DataTypeId == (int) DataTypes.Price).ToList()).ToList();
            volumeData = dataRepository.SetSaveData(volumeData,
                cacheData.Where(cD => cD.DataTypeId == (int)DataTypes.Volume).ToList()).ToList();
            eventData = dataRepository.SetSaveData(eventData,
                cacheData.Where(cD => cD.DataTypeId == (int)DataTypes.Event).ToList()).ToList();
            
            List<RuleViewModel> rule;
            if (!cacheRepository.IsProductInCache(userId, productId))
            {
                rule = ruleRepository.GetVersionRule(dataVersionId, productId, startTime, simulationDuration);
                cacheRepository.CacheRule(rule, simulationId, userId);
            }
            else
            {
                rule = cacheRule;
            }
            var forecast = RunSimulation(priceData, volumeData, eventData, rule, simulationDuration, simulationCurrencyId, startTime);
            
            return new DataRule
            { 
                Data = forecast,
                Rule = rule
            };
        }
        public DataRule SetProductCacheForSavedSimulation(CacheViewModel cacheInfo, int productId, List<DataViewModel> cacheData, List<RuleViewModel> cacheRule)
        {
            var simulationId = cacheInfo.Id;
            var userId = cacheInfo.UserId;
            var saveId = cacheInfo.SaveId;
            var simulationInfo = saveRepository.GetSimulationInfo(saveId);
            var saveForecast = GetSavedSimulation(simulationInfo.SaveId, simulationInfo, productId, cacheData, cacheRule);
            if (!cacheRepository.IsProductInCache(userId, productId))
                cacheRepository.CacheRule(saveForecast.Rule, simulationId, userId);
            return saveForecast;
        }
        public DataRule GetSavedSimulation(int saveId, SaveViewModel simulationInfo, int productId, List<DataViewModel> cacheData, List<RuleViewModel> cacheRule)
        {
            var result = new DataRule();
            var dataVersionId = simulationInfo.DataVersionId;
            var currencyBudgetVersionId = simulationInfo.CurrencyBudgetVersionId;
            var assumptionsSaveId = simulationInfo.AssumptionsSaveId;
            var simulationDuration = simulationInfo.Duration;
            var startTime = simulationInfo.StartTime;
            var startYear = startTime.Year - 1;
            var endYear = startYear + simulationDuration + 2;
            var simulationCurrencyId = simulationInfo.SimulationCurrencyId;

            var priceData =
                dataRepository.GetSaveData(new List<int>(), new List<int> { productId }, (int)DataTypes.Price, saveId, dataVersionId, currencyBudgetVersionId,
                    assumptionsSaveId, startYear, endYear).ToList();
            var volumeData =
                dataRepository.GetSaveData(new List<int>(), new List<int> { productId }, (int)DataTypes.Volume, saveId, dataVersionId, currencyBudgetVersionId,
                    assumptionsSaveId, startYear, endYear).ToList();
            var eventData =
                dataRepository.GetSaveData(new List<int>(), new List<int> { productId }, (int)DataTypes.Event, saveId, dataVersionId, currencyBudgetVersionId,
                    assumptionsSaveId, startYear, endYear).ToList();

            priceData = dataRepository.SetSaveData(priceData,
                cacheData.Where(cD => cD.DataTypeId == (int)DataTypes.Price).ToList()).ToList();
            volumeData = dataRepository.SetSaveData(volumeData,
                cacheData.Where(cD => cD.DataTypeId == (int)DataTypes.Volume).ToList()).ToList();
            eventData = dataRepository.SetSaveData(eventData,
                cacheData.Where(cD => cD.DataTypeId == (int)DataTypes.Event).ToList()).ToList();

            List<RuleViewModel> rule;
            if (cacheRule == null || cacheRule.Count == 0)
                rule = ruleRepository.GetVersionRule(dataVersionId, productId, startTime, simulationDuration, saveId);
            else
                rule = cacheRule;

            var forecast = RunSimulation(priceData, volumeData, eventData, rule, simulationDuration, simulationCurrencyId, startTime);

            result.Rule = rule;
            result.Data = forecast;

            return result;
        }
        public List<DataViewModel> CompareSimulation(int simulationId, int saveId, int productId, List<int> geographyIds, int dataTypeId)
        {
            var saveInfo = saveRepository.GetSimulationInfo(saveId);
            var saveForecast = GetSavedSimulation(saveId, saveInfo, productId, null, null);
            var saveData = saveForecast.Data.Where(d => d.DataTypeId == dataTypeId).ToList();
            var cacheData = GetProductForSimulation(
                simulationId,
                geographyIds,
                new List<int> {productId},
                new List<int> {dataTypeId});
            return SimulationDifference(cacheData, saveData);
        }
        public List<DataViewModel> SimulationDifference(List<DataViewModel> data0, List<DataViewModel> data1)
        {
            var index = data0.Distinct(new DataPermIdentityEqualityComparer());
            var data0T = data0.ToLookup(d => d, new DataPermIdentityEqualityComparer());
            var data1T = data1.ToLookup(d => d, new DataPermIdentityEqualityComparer());
            foreach (var i in index)
            {
                var d0 = data0T[i].First();
                var d1 = data1T[i].FirstOrDefault();
                if (d1 != null)
                    d0.PercentageVariation = d1.Value != 0 ? (d0.Value - d1.Value) / d1.Value : 1;
                else
                    d0.PercentageVariation = 1;
            }
            return data0;
        } 
        public void UpdateSimulation(List<DataViewModel> updatedData0, RuleViewModel updatedRule, int simulationId)
        {
            var simulationInfo = cacheRepository.GetSimulation(simulationId);
            var updatedData = updatedData0.Where(uD => uD.DataTime <= simulationInfo.StartTime || uD.DataTypeId != (int)DataTypes.Price).ToList();

            if (updatedData.Count == 0 && updatedRule == null)
                return;
            if (updatedRule != null)
            {
                cacheRepository.DeleteRule(simulationId, updatedRule.GeographyId, updatedRule.ProductId, updatedRule.GprmRuleTypeId, updatedRule.ApplicableFrom);
                cacheRepository.CacheRule(new List<RuleViewModel>{updatedRule}, simulationId,simulationInfo.UserId);
            }

            if (updatedData.Count != 0)
            {
                var distinctData = updatedData.Distinct(new DataPermIdentityEqualityComparer()).ToList();
                cacheRepository.DeletePermIds(simulationId, distinctData);
                cacheRepository.SetCacheEdited(simulationId);
                cacheRepository.CacheData(updatedData, simulationId, simulationInfo.UserId);
            }
        }
        public List<DataViewModel> RunSimulation(List<DataViewModel> priceData, IEnumerable<DataViewModel> volumeData, IEnumerable<DataViewModel> eventData, IEnumerable<RuleViewModel> rule, int simulationDuration, int simulationCurrencyId, DateTime startTime)
        {
            var eventTypes = dimensionRepository.GetEventType().ToList();
            var mathTypes = dimensionRepository.GetMathType().ToList();
            var geographyNames = countryRepository.GetAllCountries().ToList();

            priceData.RemoveAll(pD => pD.DataTime > startTime);

            var productPrice = priceData.ToLookup(d => d, new ProductIdentityComparer());
            var productVolume = volumeData.ToLookup(d => d, new ProductIdentityComparer());
            var productEvent = eventData.ToLookup(d => d, new ProductIdentityComparer()); 
            
            var gptrRule = rule.ToLookup(r => r, new RulegptEqualityComparer());
            
            var forecast = new List<DataViewModel>();
            var timeData = dataRepository.CreateDataTimes(simulationDuration);
            
            var timeSimulation = timeData.Where(tD => tD.Date > startTime).OrderBy(tD => tD.Date);
            var endTime = timeSimulation.Max();
            
            var productsIndex = priceData.Distinct(new ProductIdentityComparer());
            foreach (var product in productsIndex)
            {
                var p = product;

                var pPrice = productPrice[p].ToList();
                var pVolume = productVolume[p].ToList();
                var pEvent = productEvent[p].ToList();
                var pEventReal = pEvent.Where(d => d.EventTypeId != (int) EventTypes.NotSpecified && d.EventTypeId != (int) EventTypes.NoEvent).ToList();

                pPrice = InitializeForecast(pPrice, startTime, timeSimulation, gptrRule);

                var pBasePrice = pPrice.ToLookup(pP => pP, new GeographyDataTimeIdentityComparer());

                var gptPrice = pPrice.ToLookup(pP => pP, new GeographyDataTimePriceTypeIdentityComparer());
                var gptVolume = pVolume.ToLookup(pV => pV, new GeographyDataTimePriceTypeIdentityComparer());
                var gpVolume = pVolume.ToLookup(pV => pV, new GeographyProductIdentityComparer());
                var gptEvent = pEventReal.ToLookup(pE => pE, new GeographyDataTimePriceTypeIdentityComparer());

                var gptEventIndex = pEventReal.Distinct(new GeographyDataTimePriceTypeIdentityComparer());

                foreach (var eventTime in gptEventIndex.Where(gE => gE.DataTime > startTime && gE.DataTime <= endTime))
                {
                    var t = eventTime.DataTime;
                    var g = eventTime.GeographyId;
                    var e = gptEvent[eventTime].First();

                    var basePrice = pBasePrice[new GeographyDataTimeIdentifier(g, t.AddMonths(-1))].ToList();
                    
                    if (!basePrice.Any())
                        continue;
                    
                    string description;
                    var eventValue = e.Value ?? 0;

                    if (p.ProductId == ApplicationConstants.NotSpecifiedProduct &&
                        (e.EventTypeId == (int) EventTypes.LaunchTarget ||
                         e.EventTypeId == (int) EventTypes.LaunchDefaultRule ||
                         e.EventTypeId == (int) EventTypes.LaunchLaunchRule))
                    {
                        var volumeToShift = gpVolume[new GeographyProductIdentifier(g, p.ProductId)].ToList();
                        volumeToShift = ShiftVolume(volumeToShift, t).ToList();
                    }

                    switch (e.EventTypeId)
                    {
                        case (int)EventTypes.NoEvent:
                        case (int)EventTypes.NotSpecified:
                            break;
                        case (int)EventTypes.ExoCut:
                        case (int)EventTypes.IncCut:
                        case (int)EventTypes.OthCut:
                        case (int)EventTypes.RvlCut:
                        case (int)EventTypes.TcCut:
                            description = CreateDescription(new DescriptionViewModel
                            {
                                Event = eventTypes.Where(gEt => gEt.Id == e.EventTypeId).Select(gEt => gEt.Name).Single(),
                                PercentageVariation = e.Value
                            });
                            gptPrice = AddDataVariation(gptPrice, basePrice, e.EventTypeId, t, eventValue, description, timeSimulation);
                            break;
                        case (int)EventTypes.LaunchTarget:
                            description = CreateDescription(new DescriptionViewModel
                            {
                                Event = eventTypes.Where(gEt => gEt.Id == e.EventTypeId).Select(gEt => gEt.Name).Single(),
                                LaunchPrice = e.Value
                            });
                            gptPrice = AddDataValue(gptPrice, basePrice, e.EventTypeId, t, eventValue, description, timeSimulation);
                            break;
                        case (int)EventTypes.LaunchLaunchRule:
                            var gptRuleLaunch = GetRule(gptrRule, g, p.ProductId != ApplicationConstants.NotSpecifiedProduct ? p.ProductId : 0, (int)GprmRuleTypes.Launch, t);
                            gptPrice = ApplyRule(gptPrice, basePrice, gptVolume, gptRuleLaunch, e.EventTypeId, t, eventTypes, mathTypes, geographyNames, timeSimulation, simulationCurrencyId);
                            break;
                        case (int)EventTypes.LaunchDefaultRule:
                        case (int)EventTypes.Review:
                            var gptRuleDefault = GetRule(gptrRule, g, p.ProductId != ApplicationConstants.NotSpecifiedProduct ? p.ProductId : 0, (int)GprmRuleTypes.Default, t);
                            gptPrice = ApplyRule(gptPrice, basePrice, gptVolume, gptRuleDefault, e.EventTypeId, t, eventTypes, mathTypes, geographyNames, timeSimulation, simulationCurrencyId);
                            break; 
                    }
                }
                forecast.AddRange(pPrice);
                forecast.AddRange(pVolume);
                forecast.AddRange(pEvent);
            }
            return forecast;
        }

        public void PublishForecast(SaveSimulationViewModel model)
        {
            var products = new ProductRepository().GetAllProducts().Where(p => p.Active && p.Id != ApplicationConstants.NotSpecifiedProduct).Select(p => new { Id = p.Id, Name = p.Name}).ToList();
            var countries = new CountryRepository().GetAllCountries().Select(c => new {Id = c.Id, Name = c.Name, Iso = c.Iso2}).ToList();
            var currencies = new CurrencyRepository().GetAllCurrencies().Select(c => new { Id = c.Id, Name = c.Iso }).ToList();
            var simulation = cacheRepository.GetSimulation(model.SimulationId);
            var version = versionRepository.GetVersionInfos(simulation.DataVersionId);
            var rows = new List<DataViewModel> {};
            foreach (var p in products)
            {
                rows.AddRange(GetProductForSimulation(model.SimulationId, new List<int> {}, new List<int> { p.Id }, new List<int>{(int)DataTypes.Price}, true).Where(d => d.Reviewed && d.Value != null).ToList());
            }
            var dataBaseData = rows.Select(d => new
            {
                Id = 0,
                d.GeographyId,
                d.ProductId,
                GeographyName = countries.Where(c => c.Id == d.GeographyId).Select(c => c.Name).FirstOrDefault() ?? "",
                GeographyIso2 = countries.Where(c => c.Id == d.GeographyId).Select(c => c.Iso).FirstOrDefault() ?? "",
                ProductName = products.Where(p => p.Id == d.ProductId).Select(p => p.Name).FirstOrDefault() ?? "",
                Date = d.DataTime,
                Price = d.Value ?? 0,
                Currency = currencies.Where(c => c.Id == d.CurrencySpotId).Select(c => c.Name).FirstOrDefault() ?? "",
                UoM = "mcg",
                UnitFactor = 1,
                IsReferenced = d.IsPublic,
                GprmDataLoadDate = version.VersionTime,
                UpdatePushDate = DateTime.Now,
                PushedBy = model.Save.UserName
            }).ToList();

            var datatable = dataBaseData.ToDataTable();

            const string query = "DELETE [IRP_DB].[dbo].[PriceEvolution]";
            RequestHelper.ExecuteNonQuery(
                DataBaseConstants.ConnectionString,
                query);

            RequestHelper.BulkInsert(DataBaseConstants.ConnectionString, "[IRP_DB].[dbo].[PriceEvolution]", datatable);

            var updateTracking = "DELETE [IRP_DB].[dbo].[StatusTracking] WHERE DataTable=@dataTable";
            var parameters = new Dictionary<string, object>
            {
                {"dataTable", "PriceEvolution"},
                {"gprmPushDate", DateTime.Now },
                {"gprmPushBy",  model.Save.UserName},
                {"pulled", false },
                
            };
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, updateTracking, parameters);
            updateTracking =
                @"INSERT INTO [IRP_DB].[dbo].[StatusTracking] (DataTable, GprmPushDate, GprmPushBy, Pulled)
                VALUES (@dataTable, @gprmPushDate, @gprmPushBy, @pulled)";
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, updateTracking, parameters);
        }
        #endregion
        #region Private

        private static IEnumerable<DataViewModel> ShiftVolume(List<DataViewModel> data, DateTime launchDate)
        {
            if (data.All(d => d.Value == null))
                return data;
            var startTime = data.Where(d => d.Value != null && d.Value != 0).Min(d => d.DataTime);
            var deltaTime = (launchDate.Year * 12 + launchDate.Month) - (startTime.Year * 12 + startTime.Month);
            data.Join(data, d1 => d1.DataTime, d2 => d2.DataTime.AddMonths(-deltaTime), (d1, d2) => new { d1, d2 })
                    .ForEach(d => d.d1.Value = (d.d1.DataTime < launchDate) ? 0 : d.d2.Value);
            return data;

        } 
        private static double GetSimulationRate(DataViewModel d, int simulationCurrencyId)
        {
            switch (simulationCurrencyId)
            {
                case (int)SimulationRate.EurBudget:
                    return d.EurBudget ?? 1;
                case (int)SimulationRate.EurSpot:
                    return d.EurSpot ?? 1;
                case (int)SimulationRate.UsdBudget:
                    return d.UsdBudget ?? 1;
                case (int)SimulationRate.UsdSpot:
                    return d.UsdSpot ?? 1;
                default:
                    return 1;
            }
        }
        private static ILookup<IGeographyDataTimePriceTypeIdentifier, DataViewModel> ApplyRule(ILookup<IGeographyDataTimePriceTypeIdentifier, DataViewModel> data, List<DataViewModel> reviewedPrice, ILookup<IGeographyDataTimePriceTypeIdentifier, DataViewModel> referencedVolume, RuleViewModel rule, int eventType, DateTime t,
            IEnumerable<DimensionViewModel> eventTypes, List<DimensionViewModel> mathTypes, IEnumerable<CountryViewModel> geographyNames, IEnumerable<DateTime> timeSimulation, int simulationCurrencyId)
        {
            var description = new DescriptionViewModel
            {
                Event = eventTypes.Where(eT => eT.Id == eventType).Select(eT => eT.Name).First(),
                PercentageVariation = 0
            };

            if (rule == null)
            {
                description.Comment = "No rule specified";
                data = AddDataVariation(data, reviewedPrice, eventType, t, 0, CreateDescription(description), timeSimulation);
                return data;
            }

            var basket = rule.DefaultBasket.Select(dB => dB.ReferencedGeographyId).Join(geographyNames, dB => dB, gN => gN.Id, (dB, gN) => new { dB, gN }).Select(bg => bg.gN.Iso2).ToList();
            description.ReviewedPrice = rule.ReviewedPriceType;
            description.ReviewedPriceAdjustment = rule.ReviewedPriceAdjustment;
            description.LookBack = rule.LookBack;
            description.EfffectiveLag = rule.EffectiveLag;
            description.Math = mathTypes.Where(mT => mT.Id == rule.GprmMathId).Select(mT => mT.ShortName).First();
            if (rule.ReferencedData.Count > 1)
            {
                description.Math = description.Math + "[ ";
                foreach (var rD in rule.ReferencedData)
                {
                    description.Math = description.Math + mathTypes.Where(mT => mT.Id == rD.GprmMathId).Select(mT => mT.ShortName).First() + "(" + rD.Argument + ") ";
                }
                description.Math = description.Math + "]";
            }
            description.Basket = basket;
            description.AllowIncrease = rule.AllowIncrease;

            var revPrice0Test = reviewedPrice.Where(r => r.PriceTypeId == rule.ReviewedPriceTypeId).Select(r => r.Value * GetSimulationRate(r, simulationCurrencyId)).FirstOrDefault();

            if (revPrice0Test == null && reviewedPrice.Count(rP => rP.PriceTypeId == ApplicationConstants.NotSpecifiedPrice) != 0 && rule.ReviewedPriceTypeId == ApplicationConstants.NotSpecifiedPrice)
                revPrice0Test = 0;
            
            if (revPrice0Test == null)
            {
                description.Comment = "Specified reviewed price does not exist";
                data = AddDataVariation(data, reviewedPrice, eventType, t, 0, CreateDescription(description), timeSimulation);
                return data;
            }
            
            var revPrice0 = revPrice0Test.Value;
            var redTime = t.AddMonths(-rule.LookBack);
            var subRuleOutput = (from subRule in rule.ReferencedData
                let subRulePv = (from red in subRule.Basket
                    let redPrice = data[new GeographyDataTimePriceTypeIdentifier(red.ReferencedGeographyId, redTime, red.ReferencedPriceTypeId)].FirstOrDefault()
                    let redVolume = referencedVolume[new GeographyDataTimePriceTypeIdentifier(red.ReferencedGeographyId, redTime, ApplicationConstants.NotSpecifiedPrice)].FirstOrDefault()
                                 where redPrice != null && redPrice.Value != null && System.Math.Abs((double)(redPrice.Value * GetSimulationRate(redPrice, simulationCurrencyId) * (1 - red.ReferencedPriceAdjustment))) > ApplicationConstants.DoubleComparisonThreshold
                    select new PriceVolume
                    {
                        Price = (double)(redPrice.Value * GetSimulationRate(redPrice, simulationCurrencyId) * (1 - red.ReferencedPriceAdjustment)),
                        Volume = redVolume != null ? redVolume.Value ?? 0 : 0
                    }).ToList()
                where subRulePv.Count != 0
                select ApplyMath(subRulePv, subRule.GprmMathId, subRule.Argument)
                into newPrice
                where System.Math.Abs(newPrice) > ApplicationConstants.DoubleComparisonThreshold
                select new PriceVolume
                {
                    Price = newPrice, Volume = 1
                }).ToList();

            if (subRuleOutput.Count == 0)
            {
                description.Comment = "Referenced prices could not be found";
                data = AddDataVariation(data, reviewedPrice, eventType, t, 0, CreateDescription(description), timeSimulation);
                return data;
            }
            var revPrice = ApplyMath(subRuleOutput, rule.GprmMathId, rule.Argument) * (1 - rule.ReviewedPriceAdjustment);
            if (eventType == (int) EventTypes.LaunchDefaultRule || eventType == (int) EventTypes.LaunchLaunchRule)
            {
                var usdRate = reviewedPrice.Where(r => r.PriceTypeId == rule.ReviewedPriceTypeId).Select(r => GetSimulationRate(r, simulationCurrencyId)).FirstOrDefault();
                data = AddDataValue(data, reviewedPrice, eventType, t, revPrice / usdRate, CreateDescription(description), timeSimulation);
                return data;
            }
            
            var percentageVariation = System.Math.Abs(revPrice0) > ApplicationConstants.DoubleComparisonThreshold ? (revPrice - revPrice0) / revPrice0 : 0;
            if (percentageVariation > 0 && rule.AllowIncrease == false)
            {
                description.Comment = "Reviewed price candidate is " + percentageVariation.ToString("0.##%") + " higher than current price and increases are not allowed";
                data = AddDataVariation(data, reviewedPrice, eventType, t, 0, CreateDescription(description), timeSimulation);
                return data;
            }

            description.PercentageVariation = percentageVariation;
            data = AddDataVariation(data, reviewedPrice, eventType, t.AddMonths(rule.EffectiveLag), -percentageVariation,CreateDescription(description), timeSimulation, t);
            return data;
        }
        private static double ApplyMath(List<PriceVolume> subRuleData, int mathId, int argument)
        {
            double result = 0;
            argument = argument == 0 ? 1 : argument;
            var nData = subRuleData.Count();
            var x = subRuleData.Select(sRd => sRd.Price).ToList();
            int nX;
            switch (mathId)
            {
                case (int)MathTypes.Average:
                    result = x.Average();
                    break;
                case (int)MathTypes.Lowest:
                    x.Sort();
                    result = argument > nData ? 0 : x[argument - 1];
                    break;
                case (int)MathTypes.Maximum:
                    x.Sort();
                    x.Reverse();
                    result = argument > nData ? 0 : x[argument - 1];
                    break;
                case (int)MathTypes.Median:
                    x.Sort();
                    nX = x.Count();
                    var mX = nX/2;
                    result = (nX % 2 != 0) ? x[mX] : (x[mX] + x[mX - 1]) / 2;
                    break;
                case (int)MathTypes.WeightedAverage:
                    var xV = subRuleData.Sum(sRd => sRd.Price*sRd.Volume);
                    var vS = subRuleData.Sum(sRd => sRd.Volume);
                    result = System.Math.Abs(vS) > ApplicationConstants.DoubleComparisonThreshold ? xV/vS : 0;
                    break;
                case (int)MathTypes.Quartile:
                    x.Sort();
                    nX = x.Count();
                    var qX = nX/4*argument;
                    result = ((nX*argument) % 4 != 0) ? x[qX] : (x[qX] + x[qX - 1]) / 2;
                    break;
                case (int)MathTypes.NotSpecified:
                    result = 0;
                    break;
            }
            return result;
        }
        private static RuleViewModel GetRule(ILookup<IRulegptIdentifier, RuleViewModel> rule, int geographyId, int productId, int ruleTypeId, DateTime timeData)
        {
            var gpRule = rule[new RulegptIdentifier(geographyId, productId, ruleTypeId)].ToList();
            if (!gpRule.Any() || !gpRule.Any(gR => gR.ApplicableFrom <= timeData))
                return null;
            var applicableFrom = gpRule.Where(gR => gR.ApplicableFrom <= timeData).Max(gR => gR.ApplicableFrom);
            return gpRule.SingleOrDefault(gR => gR.ApplicableFrom == applicableFrom);
        }
        private static string CreateDescription(DescriptionViewModel descriptionItems)
        {
            if (descriptionItems == null)
                return null;
            var header = "<header><h1 class=\"icon-tooltip icon-cog\">@Event</h1></header>";
            var variation = "<h2>Percentage Variation: @PercentageVariation</h2>";
            var launchPrice = "<h2>Launch Price: @LaunchPrice</h2>";
            var comment = "<h2>Comment:</h2><p>@Comment</p>";
            var rule = "<h2>Rule:</h2><p>Reviewed Price: @ReviewedPrice<br/>Look Back: @LookBack<br/>Effective Lag: @EffectiveLag<br/>Adjustment: @Adjustment<br/>Allow Increase: @AllowIncrease<br/>Math: @Math<br/>";
            var basket = "Basket: @Basket</p>";


            header = descriptionItems.Event != null ? header.Replace("@Event", descriptionItems.Event) : null;
            variation = descriptionItems.PercentageVariation != null
                ? variation.Replace("@PercentageVariation", descriptionItems.PercentageVariation.Value.ToString("0.##%")) : null;
            launchPrice = descriptionItems.LaunchPrice != null ? launchPrice.Replace("@LaunchPrice", descriptionItems.LaunchPrice.ToString()) : null;
            comment = descriptionItems.Comment != null ? comment.Replace("@Comment", descriptionItems.Comment) : null;
            rule = descriptionItems.Math != null ? rule
                .Replace("@ReviewedPrice", descriptionItems.ReviewedPrice)
                .Replace("@LookBack", descriptionItems.LookBack.ToString())
                .Replace("@EffectiveLag", descriptionItems.EfffectiveLag.ToString())
                .Replace("@Adjustment", descriptionItems.ReviewedPriceAdjustment != null ? descriptionItems.ReviewedPriceAdjustment.Value.ToString("0.##%") : null)
                .Replace("@Math", descriptionItems.Math)
                .Replace("@AllowIncrease", (descriptionItems.AllowIncrease ?? false) ? "Yes" : "No"): null;
            if (descriptionItems.Basket != null && descriptionItems.Basket.Any())
            {
                var listGeography = string.Join(", ", descriptionItems.Basket.OrderBy(b => b));
                basket = basket.Replace("@Basket", listGeography);
            }
            else
                basket = null;

            var description = header + variation + launchPrice + comment + rule + basket;

            return description;
        }
        private static ILookup<IGeographyDataTimePriceTypeIdentifier, DataViewModel> AddDataVariation(ILookup<IGeographyDataTimePriceTypeIdentifier, DataViewModel> data, List<DataViewModel> baseData, int eventTypeId, DateTime timeData, double percentageVariation, string description, IEnumerable<DateTime> timeSimulation, DateTime? timeEvent = null)
        {
            var distinctPrices = baseData.ToLookup(dP => dP, new GeographyDataTimePriceTypeIdentityComparer());
            var distinctPricesIndex = baseData.Distinct(new GeographyDataTimePriceTypeIdentityComparer()).ToList();

            if (timeEvent == null)
                timeEvent = timeData;
            foreach (var dP in distinctPricesIndex)
            {
                var g = dP.GeographyId;
                var pT = dP.PriceTypeId;
                data[new GeographyDataTimePriceTypeIdentifier(g, timeEvent.Value, pT)].First().EventTypeId = eventTypeId;
                data[new GeographyDataTimePriceTypeIdentifier(g, timeEvent.Value, pT)].First().Description = description;
            }

            foreach (var t in timeSimulation.Where(tS => tS.Date >= timeData))
            {
                foreach (var dP in distinctPricesIndex)
                {
                    var g = dP.GeographyId;
                    var pT = dP.PriceTypeId;
                    var price = distinctPrices[dP].First().Value;
                    data[new GeographyDataTimePriceTypeIdentifier(g, t, pT)].First().Value = price * (1 - percentageVariation);
                    if (t == timeData)
                        data[new GeographyDataTimePriceTypeIdentifier(g, t, pT)].First().PercentageVariation = price == null ? 0 : (System.Math.Abs(price.Value) < ApplicationConstants.DoubleComparisonThreshold ? 0 : -percentageVariation);
                    
                    if (t == timeEvent) continue;
                    data[new GeographyDataTimePriceTypeIdentifier(g, t, pT)].First().EventTypeId = (int)EventTypes.NoEvent;
                    data[new GeographyDataTimePriceTypeIdentifier(g, t, pT)].First().Description = null;
                    if (t == timeData)
                        data[new GeographyDataTimePriceTypeIdentifier(g, t, pT)].First().Description = CreateDescription(new DescriptionViewModel{Comment = "Reviewed price effective"});
                }
            }
            return data;
        }
        private static ILookup<IGeographyDataTimePriceTypeIdentifier, DataViewModel> AddDataValue(ILookup<IGeographyDataTimePriceTypeIdentifier, DataViewModel> data, IEnumerable<DataViewModel> baseData, int eventTypeId, DateTime timeData, double value, string description, IEnumerable<DateTime> timeSimulation)
        {
            var distinctPricesIndex = baseData.Distinct(new GeographyDataTimePriceTypeIdentityComparer()).ToList();
            foreach (var t in timeSimulation.Where(tS => tS.Date >= timeData))
            {
                foreach (var dP in distinctPricesIndex)
                {
                    var g = dP.GeographyId;
                    var pT = dP.PriceTypeId;
                    data[new GeographyDataTimePriceTypeIdentifier(g, t, pT)].First().Value = value;
                    if (t != timeData)
                    {
                        data[new GeographyDataTimePriceTypeIdentifier(g, t, pT)].First().EventTypeId = (int)EventTypes.NoEvent;
                        data[new GeographyDataTimePriceTypeIdentifier(g, t, pT)].First().Description = null;
                    }
                    else
                    {
                        data[new GeographyDataTimePriceTypeIdentifier(g, t, pT)].First().EventTypeId = eventTypeId;
                        data[new GeographyDataTimePriceTypeIdentifier(g, t, pT)].First().Description = description;
                    }
                }
            }
            return data;
        }
        private static List<DataViewModel> InitializeForecast(List<DataViewModel> data, DateTime startTime, IEnumerable<DateTime> timeSimulation, ILookup<IRulegptIdentifier, RuleViewModel> rule)
        {
            var indexData = data.Distinct(new GeographyProductSegmentIdentityComparer());
            var gptData = data.ToLookup(d => d, new GeographyProductSegmentIdentityComparer());
            foreach (var i in indexData)
            {
                var g = i.GeographyId;
                var p = i.ProductId;

                var priceTypeIdDefault = gptData[i].First().PriceTypeId;
                var isPublic = false;

                var priceTypeId = -1;
                var gpRule = GetRule(rule, g, p != ApplicationConstants.NotSpecifiedProduct ? p : 0, (int)GprmRuleTypes.Default, startTime.AddMonths(1));
                
                if (gpRule != null)
                {
                    priceTypeId = gpRule.ReviewedPriceTypeId;
                    isPublic = true;
                    if (gptData[i].All(d => d.PriceTypeId != priceTypeId))
                    {
                        priceTypeId = -1;
                        isPublic = false;
                    }
                }
                    
                gptData[i].Where(d => d.PriceTypeId == (priceTypeId !=-1 ? priceTypeId : priceTypeIdDefault)).ForEach(d =>
                {
                    d.Reviewed = true;
                    d.IsPublic = isPublic;
                });
            }

            var forecast = timeSimulation
                .Join(data.Where(d => d.DataTime == startTime), tS => 1, d => 1, (tS, d) => new { tS, d })
                .Select(f => new DataViewModel
                {
                    GeographyId = f.d.GeographyId,
                    ProductId = f.d.ProductId,
                    CurrencySpotId = f.d.CurrencySpotId,
                    CurrencySpotVersionId = f.d.CurrencySpotVersionId,
                    PriceTypeId = f.d.PriceTypeId,
                    DataTypeId = f.d.DataTypeId,
                    EventTypeId = (int)EventTypes.NoEvent,
                    UnitTypeId = f.d.UnitTypeId,
                    SegmentId = f.d.SegmentId,
                    DataTime = f.tS.Date,
                    Value = f.d.Value,
                    Description = null,
                    UsdSpot = f.d.UsdSpot,
                    EurSpot = f.d.EurSpot,
                    UsdBudget = f.d.UsdBudget,
                    EurBudget = f.d.EurBudget,
                    PercentageVariation = 0,
                    Reviewed = f.d.Reviewed,
                    IsPublic = f.d.IsPublic
                });
            data.AddRange(forecast);
            return data;
        }
        #endregion
    }
}
