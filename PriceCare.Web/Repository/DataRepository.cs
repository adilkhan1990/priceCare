using System.Collections.Generic;
using System.Data;
using System.Linq;
using PriceCare.Web.Constants;
using PriceCare.Web.Helpers;
using System;
using PriceCare.Web.Repository.Utils;
using WebGrease.Css.Extensions;
using PriceCare.Web.Models;
using PriceCare.Web.Math.Utils;

namespace PriceCare.Web.Repository
{
    public class DataRepository : IDataRepository
    {
        private readonly IVersionRepository versionRepository = new VersionRepository();
        private readonly ICountryRepository countryRepository = new CountryRepository();

        public IEnumerable<DateTime> CreateDataTimes(int simulationDuration)
        {
            var result = new List<DateTime>();
            var startMonth = new DateTime(DateTime.Now.Year - 1, 1, 1);
            var endMonth = new DateTime(startMonth.Year + simulationDuration + 1, 12, 1);
            for (var newMonth = startMonth; newMonth <= endMonth; newMonth = newMonth.AddMonths(1))
            {
                result.Add(newMonth);
            }
            return result;
        }
        public IEnumerable<DateTime> CreateDataTimes(DateTime startTime, int simulationDuration)
        {
            var result = new List<DateTime>();
            var startMonth = startTime;
            var endMonth = new DateTime(startMonth.Year + simulationDuration + 1, 12, 1);
            for (var newMonth = startMonth; newMonth <= endMonth; newMonth = newMonth.AddMonths(1))
            {
                result.Add(newMonth);
            }
            return result;
        }
        public IEnumerable<DataViewModel> GetLatestData(int saveId, List<int> geographyId, List<int> productId, List<int> dataTypeId, bool fillData = true)
        {
            var startTime = new DateTime(1753, 1, 1);
            var endTime = new DateTime(4000,12,31);
            return GetData(saveId, 0, 0, startTime, endTime, geographyId, productId, dataTypeId, fillData);
        }
        public IEnumerable<DataViewModel> GetVersionData(int saveId, int versionId, int currencyBudgetVersionId, List<int> geographyId, List<int> productId, List<int> dataTypeId, bool fillData = true)
        {
            var startTime = new DateTime(1753, 1, 1);
            var endTime = new DateTime(4000, 12, 31);
            return GetData(saveId, versionId, currencyBudgetVersionId, startTime, endTime, geographyId, productId, dataTypeId, fillData);
        }
        public IEnumerable<DataViewModel> GetVersionData(int dataTypeId, int saveId, int versionId, int currencyBudgetVersionId, int startYear, int endYear, bool fillData = true)
        {
            var startTime = new DateTime(startYear, 1, 1);
            var endTime = new DateTime(endYear, 12, 31);
            return GetData(saveId, versionId, currencyBudgetVersionId, startTime, endTime, new List<int>(), new List<int>(), new List<int> { dataTypeId }, fillData);
        }
        public IEnumerable<DataViewModel> GetVersionData(List<int> geographyId, List<int> productId, int dataTypeId, int saveId, int versionId, int currencyBudgetVersionId, int startYear, int endYear, bool fillData = true, bool takeInactive = false)
        {
            var startTime = new DateTime(startYear, 1, 1);
            var endTime = new DateTime(endYear, 12, 31);
            return GetData(saveId, versionId, currencyBudgetVersionId, startTime, endTime, geographyId, productId, new List<int> { dataTypeId }, fillData, takeInactive);
        }
        public IEnumerable<DataViewModel> GetSaveData(List<int> geographyId, List<int> productId, int dataTypeId, int saveId, int dataVersionId, int currencyBudgetVersionId, int assumptionsSaveId, int startYear, int endYear)
        {
            var dataSaveId = dataTypeId == (int)DataTypes.Event ? assumptionsSaveId : ApplicationConstants.ImportedDataSaveId;
            var data0 = GetVersionData(geographyId, productId, dataTypeId, dataSaveId, dataVersionId, currencyBudgetVersionId,
                startYear, endYear).ToList();
            var data1 = GetVersionData(geographyId, productId, dataTypeId, saveId, 0, currencyBudgetVersionId,
                startYear, endYear, false, true).ToList();
            return SetSaveData(data0, data1);
        }
        public void SaveVersionData(List<DataViewModel> data)
        {
            if (!data.Any())
                return;
            if (data.First().DataTypeId == (int) DataTypes.Price)
            {
                var currencyId =
                    data.Where(d => d.DataTypeId == (int) DataTypes.Price && d.CurrencySpotVersionId == 0)
                        .Select(d => d.CurrencySpotId)
                        .Distinct().ToList();
                var currencyVersion = RequestHelper.ExecuteQuery(DataBaseConstants.ConnectionString,
                "SELECT * FROM GetLatestSpotRatesVersion(@currencyId)",
                MapCurrencyVersion,
                new Dictionary<string, List<int>> { { "currencyId", currencyId } });
                data.Where(d => d.DataTypeId == (int) DataTypes.Price && d.CurrencySpotVersionId == 0).Join(currencyVersion, d => d.CurrencySpotId, c => c.CurrencyId, (d,c)=> new {d,c}).ForEach(e => e.d.CurrencySpotVersionId = e.c.VersionId);
            }
            var versionId = versionRepository.CreateNewVersion("Data");
            SaveData(data, versionId, ApplicationConstants.ImportedDataSaveId);
        }
        public void SaveVersionData(List<DataViewModel> data, int saveId)
        {
            var versionId = versionRepository.CreateNewVersion("Forecast");
            SaveData(data, versionId, saveId);
        }

        public void SaveVersionData(List<DataViewModel> data, int saveId, bool replaceAll)
        {
            int versionId;
            if (replaceAll)
            {
                versionId = versionRepository.CreateNewVersion("Cancel Previous Version");
                var dataTypeId = data.First().DataTypeId;
                var previousData = GetLatestData(saveId, new List<int>(), new List<int>(), new List<int> {dataTypeId}, false).ToList();
                previousData.ForEach(pD => pD.Active = false);
                SaveData(previousData, versionId, saveId);
            }
            versionId = versionRepository.CreateNewVersion("Excel");
            SaveData(data, versionId, saveId);
        }

        public DateTime GetVersionStartTime(int versionId)
        {
            return RequestHelper.ExecuteScalarRequest<DateTime>(DataBaseConstants.ConnectionString,
                "SELECT TOP 1 StartDate FROM GetStartDate(@versionId)",
                new Dictionary<string, object>{{"versionId",versionId}});
        }
        private static void SaveData(IEnumerable<DataViewModel> data, int versionId, int saveId)
        {
            var dataBaseData = data.Select(d => new
            {
                Id = 0,
                d.PermId,
                SaveId = saveId,
                VersionId = versionId,
                d.GeographyId,
                d.ProductId,
                d.DataTypeId,
                d.PriceTypeId,
                d.DataTime,
                d.SegmentId,
                d.CurrencySpotId,
                d.CurrencySpotVersionId,
                d.EventTypeId,
                d.UnitTypeId,
                Value = d.Value ?? 0,
                d.Description,
                d.Active
            }).ToList();

            var datatable = dataBaseData.ToDataTable();

            RequestHelper.BulkInsert(DataBaseConstants.ConnectionString, "Data", datatable);
        }
        public IEnumerable<DataViewModel> SetSaveData(List<DataViewModel> xData, List<DataViewModel> xSave)
        {
            var index = xSave.Distinct(new DataPermIdentityEqualityComparer());
            var xDataByPerm = xData.ToLookup(pd => pd, new DataPermIdentityEqualityComparer());
            var xSaveByPerm = xSave.ToLookup(pd => pd, new DataPermIdentityEqualityComparer());
            foreach (var i in index)
            {
                var p = i;
                var dataValue = xDataByPerm[p].FirstOrDefault();
                var saveValue = xSaveByPerm[p].First();
                if (dataValue != null)
                {
                    if (saveValue.Active == false)
                    {
                        dataValue.Value = null;
                        dataValue.EventTypeId = (int)EventTypes.NotSpecified;

                    }
                    else
                    {
                        dataValue.Value = saveValue.Value;
                        dataValue.EventTypeId = saveValue.EventTypeId;
                    }
                }
                else if (saveValue.Active)
                {
                    
                    saveValue.PercentageVariation = 0;
                    xData.Add(saveValue);
                }
            }
            return xData;
        }
        private IEnumerable<DataViewModel> GetData(int saveId, int versionId, int currencyBudgetVersionId,
            DateTime startTime, DateTime endTime, List<int> geographyId, List<int> productId, List<int> dataTypeId, bool fillData = true, bool takeInactive = false)
        {
            var result = new List<DataViewModel>();
            var noCurrencyDataTypeId = dataTypeId.Where(dT => dT != (int)DataTypes.Price).ToList();
            var currencyDataTypeId = dataTypeId.Where(dT => dT == (int)DataTypes.Price).ToList();

            if (noCurrencyDataTypeId.Count != 0)
                result.AddRange(GetDataNoCurrency(saveId, versionId, startTime, endTime, geographyId, productId, noCurrencyDataTypeId, takeInactive));
            if (currencyDataTypeId.Count != 0)
                result.AddRange(GetDataCurrency(saveId, versionId, currencyBudgetVersionId, startTime, endTime, geographyId, productId, currencyDataTypeId, takeInactive));
            if (fillData)
            {
                result = FillData(result, geographyId, productId, dataTypeId, endTime).ToList();
                result = DataVariation(result).ToList();
            }

            return result.OrderBy(r => r.DataTime);
        }
        private static IEnumerable<DataViewModel> GetDataCurrency(int saveId, int versionId, int currencyBudgetVersionId,
            DateTime startTime, DateTime endTime, List<int> geographyId, List<int> productId, List<int> dataTypeId, bool takeInactive = false)
        {
            var connectionString = DataBaseConstants.ConnectionString;
            const string query =
                "SELECT * FROM GetDataCurrency(@saveId,@versionId,@currencyBudgetVersionId,@startTime,@endTime,@takeInactive,@geographyId,@productId,@dataTypeId)";
            return RequestHelper.ExecuteQuery(
                connectionString,
                query,
                MapData,
                new Dictionary<string, object>
                {
                    {"saveId",saveId},
                    {"versionId",versionId},
                    {"currencyBudgetVersionId",currencyBudgetVersionId},
                    {"startTime",startTime},
                    {"endTime",endTime},
                    {"takeInactive",takeInactive}
                },
                new Dictionary<string, List<int>>
                {
                    {"geographyId",geographyId},
                    {"productId",productId},
                    {"dataTypeId",dataTypeId},
                }
                ).OrderBy(d => d.DataTime);
        }
        private static IEnumerable<DataViewModel> GetDataNoCurrency(int saveId, int versionId,
            DateTime startTime, DateTime endTime, List<int> geographyId, List<int> productId, List<int> dataTypeId, bool takeInactive = false)
        {
            var connectionString = DataBaseConstants.ConnectionString;
            const string query =
                "SELECT * FROM GetDataNoCurrency(@saveId,@versionId,@startTime,@endTime,@takeInactive,@geographyId,@productId,@dataTypeId)";
            return RequestHelper.ExecuteQuery(
                connectionString,
                query,
                MapData,
                new Dictionary<string, object>
                {
                    {"saveId",saveId},
                    {"versionId",versionId},
                    {"startTime",startTime},
                    {"endTime",endTime},
                    {"takeInactive",takeInactive}
                },
                new Dictionary<string, List<int>>
                {
                    {"geographyId",geographyId},
                    {"productId",productId},
                    {"dataTypeId",dataTypeId}
                }
                ).OrderBy(d => d.DataTime);
        }

        private static IEnumerable<DataViewModel> FillMissingGeography(List<DataViewModel> data, List<CountryViewModel> geographies, IReadOnlyCollection<int> geographyId, List<DateTime> time, int dataTypeId)
        {
            var result = new List<DataViewModel>();

            var dataGeography = data.Select(fD => fD.GeographyId).Distinct();
            var dataProduct = data.Select(fD => fD.ProductId).Distinct();
            var fillGeography = geographyId.Count == 0 ? geographies.Select(g => g.Id).Where(g => dataGeography.All(dG => dG != g)).ToList() : geographyId.Where(g => dataGeography.All(dG => dG != g)).ToList();
            foreach (var dP in dataProduct)
            {
                foreach (var fG in fillGeography)
                {
                    result.AddRange(
                    time.Join(geographies.Where(g => g.Id == fG), t => 1, g => 1, (t, g) => new { t, g })
                    .Select(tg => new DataViewModel
                    {
                        GeographyId = fG,
                        ProductId = dP,
                        CurrencySpotId = tg.g.DisplayCurrencyId,
                        CurrencySpotVersionId = ApplicationConstants.DefaultCurrencySpotVersionId,
                        PriceTypeId = ApplicationConstants.NotSpecifiedPrice,
                        DataTypeId = dataTypeId,
                        EventTypeId = (int)EventTypes.NoEvent,
                        UnitTypeId = ApplicationConstants.CurrencyUnitType,
                        SegmentId = ApplicationConstants.DefaultSegment,
                        DataTime = tg.t.Date
                    })
                    );
                }
            }
            return result;
        }
        public IEnumerable<DataViewModel> FillEmptyData(List<int> existingGeographyId, IReadOnlyCollection<int> geographyId0, IEnumerable<int> productId, int dataTypeId)
        {
            var geographies = countryRepository.GetAllCountries().Where(aG => aG.Active).ToList();
            var result = new List<DataViewModel>();
            var time = new List<DateTime> {new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1)};
            if (geographyId0.Count == 0)
                geographyId0 = geographies.Select(g => g.Id).ToList();
            var geographyId = geographyId0.Where(g => !existingGeographyId.Contains(g)).ToList();
            var usdRates = GetLatestUsdRates();
            foreach (var p in productId)
            {
                foreach (var fG in geographyId)
                {
                    result.AddRange(
                    time.Join(geographies.Where(g => g.Id == fG), t => 1, g => 1, (t, g) => new { t, g })
                    .Select(tg => new DataViewModel
                    {
                        GeographyId = fG,
                        ProductId = p,
                        CurrencySpotId = usdRates.CurrencyId,
                        CurrencySpotVersionId = usdRates.VersionId,
                        PriceTypeId = ApplicationConstants.NotSpecifiedPrice,
                        DataTypeId = dataTypeId,
                        EventTypeId = (int)EventTypes.NoEvent,
                        UnitTypeId = ApplicationConstants.CurrencyUnitType,
                        SegmentId = ApplicationConstants.DefaultSegment,
                        DataTime = tg.t.Date,
                        UsdSpot = usdRates.UsdSpot,
                        UsdBudget = usdRates.UsdBudget,
                        EurSpot = usdRates.EurSpot,
                        EurBudget = usdRates.EurBudget
                    })
                    );
                }
            }
            return result;
        }
        private IEnumerable<DataViewModel> FillEmptyData(List<CountryViewModel> geographies, IReadOnlyCollection<int> geographyId, IEnumerable<int> productId, int dataTypeId, DateTime endTime)
        {
            var result = new List<DataViewModel>();
            var time = CreateDataTimes(ApplicationConstants.MaximumSimulationDuration, new DateTime(DateTime.Now.Year - 1, 1, 1), endTime).ToList();
            if (geographyId.Count == 0)
                geographyId = geographies.Select(g => g.Id).ToList();
            var usdRates = GetLatestUsdRates();
            foreach (var p in productId)
            {
                foreach (var fG in geographyId)
                {
                    result.AddRange(
                    time.Join(geographies.Where(g => g.Id == fG), t => 1, g => 1, (t, g) => new { t, g })
                    .Select(tg => new DataViewModel
                    {
                        GeographyId = fG,
                        ProductId = p,
                        CurrencySpotId = usdRates.CurrencyId,
                        CurrencySpotVersionId = usdRates.VersionId,
                        PriceTypeId = ApplicationConstants.NotSpecifiedPrice,
                        DataTypeId = dataTypeId,
                        EventTypeId = (int)EventTypes.NoEvent,
                        UnitTypeId = ApplicationConstants.CurrencyUnitType,
                        SegmentId = ApplicationConstants.DefaultSegment,
                        DataTime = tg.t.Date,
                        UsdSpot = usdRates.UsdSpot,
                        UsdBudget = usdRates.UsdBudget,
                        EurSpot = usdRates.EurSpot,
                        EurBudget = usdRates.EurBudget
                    })
                    );
                }
            }
            return result;
        }
        private IEnumerable<DataViewModel> FillPriceData(List<DataViewModel> priceData, List<CountryViewModel> geographies, IReadOnlyCollection<int> geographyId, List<int> productId, DateTime endTime)
        {
            var fillData = new List<DataViewModel>();
            if (!priceData.Any())
                return FillEmptyData(geographies, geographyId, productId, (int)DataTypes.Price, endTime);

            var timePrice = priceData.GroupBy(tD => tD.DataTime).Select(gTd => new
            {
                DataTime = gTd.Key,
                CurrencySpotVersionId = gTd.Max(fD => fD.CurrencySpotVersionId)
            }).ToList();

            var distinctPriceData = priceData.Distinct(new PriceDataEqualityComparer());
            var priceDataByPerm = priceData.ToLookup(pd => pd, new DataIdentityEqualityComparer());

            foreach (var distinctPrice in distinctPriceData)
            {
                var p = distinctPrice;
                var hD = priceDataByPerm[p];
                var timeFill = timePrice.Where(tR => hD.All(h => h.DataTime != tR.DataTime)).ToList();
                fillData.AddRange(
                    timeFill.Select(tF => new DataViewModel
                    {
                        GeographyId = p.GeographyId,
                        ProductId = p.ProductId,
                        CurrencySpotId = p.CurrencySpotId,
                        CurrencySpotVersionId = 0,
                        PriceTypeId = p.PriceTypeId,
                        DataTypeId = p.DataTypeId,
                        EventTypeId = (int)EventTypes.NoEvent,
                        UnitTypeId = p.UnitTypeId,
                        SegmentId = p.SegmentId,
                        DataTime = tF.DataTime,
                        UsdBudget = p.UsdBudget,
                        EurBudget = p.EurBudget
                    }));
            }

            var dataGeography = priceData.Select(fD => fD.GeographyId).Distinct();
            var dataProduct = priceData.Select(fD => fD.ProductId).Distinct();
            var fillGeography = geographyId.Count == 0 ? geographies.Select(g => g.Id).Where(g => dataGeography.All(dG => dG != g)).ToList() : geographyId.Where(g => dataGeography.All(dG => dG != g)).ToList();
            var filledGeography = new List<DataViewModel>();
            var usdRates = GetLatestUsdRates();
            foreach (var dP in dataProduct)
            {
                foreach (var fG in fillGeography)
                {
                    filledGeography.AddRange(
                    timePrice.Join(geographies.Where(g => g.Id == fG), t => 1, g => 1, (t, g) => new { t, g })
                    .Select(tg => new DataViewModel
                    {
                        GeographyId = fG,
                        ProductId = dP,
                        CurrencySpotId = usdRates.CurrencyId,
                        CurrencySpotVersionId = usdRates.VersionId,
                        PriceTypeId = ApplicationConstants.NotSpecifiedPrice,
                        DataTypeId = (int)DataTypes.Price,
                        EventTypeId = (int)EventTypes.NoEvent,
                        UnitTypeId = ApplicationConstants.CurrencyUnitType,
                        SegmentId = ApplicationConstants.DefaultSegment,
                        DataTime = tg.t.DataTime,
                        UsdSpot = usdRates.UsdSpot,
                        UsdBudget = usdRates.UsdBudget,
                        EurSpot = usdRates.EurSpot,
                        EurBudget = usdRates.EurBudget
                    })
                    );
                }
            }
            fillData.AddRange(filledGeography);
            return fillData;
        }
        private IEnumerable<DataViewModel> FillVolumeData(List<DataViewModel> volumeData, List<CountryViewModel> geographies, IReadOnlyCollection<int> geographyId, List<int> productId, DateTime endTime)
        {
            var fillData = new List<DataViewModel>();
            if (!volumeData.Any())
                return FillEmptyData(geographies, geographyId, productId, (int)DataTypes.Volume, endTime);
            var timeVolume = CreateDataTimes(ApplicationConstants.MaximumSimulationDuration, volumeData.Min(eD => eD.DataTime), endTime).ToList();

            var distinctVolumeData = volumeData.Distinct(new VolumeDataEqualityComparer());
            var volumeDataByPerm = volumeData.ToLookup(pd => pd, new DataIdentityEqualityComparer());

            foreach (var dD in distinctVolumeData)
            {
                var p = dD;
                var hD = volumeDataByPerm[dD];

                var timeFill = timeVolume.Where(tR => hD.All(h => h.DataTime != tR.Date)).ToList();
                fillData.AddRange(
                    timeFill.Select(tF => new DataViewModel
                    {
                        GeographyId = p.GeographyId,
                        ProductId = p.ProductId,
                        CurrencySpotId = p.CurrencySpotId,
                        CurrencySpotVersionId = ApplicationConstants.DefaultCurrencySpotVersionId,
                        PriceTypeId = p.PriceTypeId,
                        DataTypeId = p.DataTypeId,
                        EventTypeId = (int)EventTypes.NoEvent,
                        UnitTypeId = p.UnitTypeId,
                        SegmentId = p.SegmentId,
                        DataTime = tF.Date
                    }));
            }
            fillData.AddRange(FillMissingGeography(volumeData, geographies, geographyId, timeVolume, (int)DataTypes.Volume));
            return fillData;
        }
        private IEnumerable<DataViewModel> FillEventData(List<DataViewModel> eventData, List<CountryViewModel> geographies, IReadOnlyCollection<int> geographyId, IEnumerable<int> productId, DateTime endTime)
        {
            var fillData = new List<DataViewModel>();
            if (!eventData.Any())
                return FillEmptyData(geographies, geographyId, productId, (int)DataTypes.Event, endTime);
            var timeEvent = CreateDataTimes(ApplicationConstants.MaximumSimulationDuration, eventData.Min(eD => eD.DataTime), endTime).ToList();

            var distinctEventData = eventData.Distinct(new EventDataEqualityComparer());
            var eventDataByPerm = eventData.ToLookup(pd => pd, new DataIdentityEqualityComparer());

            foreach (var dD in distinctEventData)
            {
                var p = dD;
                var hD = eventDataByPerm[dD];

                var timeFill = timeEvent.Where(tR => hD.All(h => h.DataTime != tR.Date)).ToList();
                fillData.AddRange(
                    timeFill.Select(tF => new DataViewModel
                    {
                        GeographyId = p.GeographyId,
                        ProductId = p.ProductId,
                        CurrencySpotId = p.CurrencySpotId,
                        CurrencySpotVersionId = ApplicationConstants.DefaultCurrencySpotVersionId,
                        PriceTypeId = p.PriceTypeId,
                        DataTypeId = p.DataTypeId,
                        EventTypeId = (int)EventTypes.NoEvent,
                        UnitTypeId = p.UnitTypeId,
                        SegmentId = p.SegmentId,
                        DataTime = tF.Date
                    }));
            }
            fillData.AddRange(FillMissingGeography(eventData, geographies, geographyId, timeEvent, (int)DataTypes.Event));
            return fillData;
        }
        public IEnumerable<DataViewModel> FillData(List<DataViewModel> hollowData, List<int> geographyId, List<int> productId, List<int> dataTypeId, DateTime? endTime = null)
        {
            var fillData = hollowData.ToList();
            var geographies = countryRepository.GetAllCountries().Where(aG => aG.Active).ToList();
            if (!dataTypeId.Any())
                dataTypeId = new List<int> { (int)DataTypes.Price, (int)DataTypes.Volume, (int)DataTypes.Event };
            foreach (var dT in dataTypeId)
            {
                var data = hollowData.Where(hD => hD.DataTypeId == dT).ToList();
                switch (dT)
                {
                    case (int)DataTypes.Price:
                        fillData.AddRange(AddCurrencyData(FillPriceData(data, geographies, geographyId, productId, endTime ?? new DateTime(4000, 12, 31))));
                        break;
                    case (int)DataTypes.Volume:
                        fillData.AddRange(FillVolumeData(data, geographies, geographyId, productId, endTime ?? new DateTime(4000, 12, 31)));
                        break;
                    case (int)DataTypes.Event:
                        fillData.AddRange(FillEventData(data, geographies, geographyId, productId, endTime ?? new DateTime(4000,12,31)));
                        break;
                }
            }
            return fillData;
        }
        private static IEnumerable<DateTime> CreateDataTimes(int simulationDuration, DateTime startTime, DateTime endTime)
        {
            var result = new List<DateTime>();
            var startMonth = new DateTime(DateTime.Now.Year - 1, 1, 1);
            var endMonth = new DateTime(System.Math.Min(new DateTime(startMonth.Year + simulationDuration + 1, 12, 1).Ticks, endTime.Ticks));
            for (var newMonth = startTime; newMonth <= endMonth; newMonth = newMonth.AddMonths(1))
            {
                result.Add(newMonth);
            }
            return result;
        }
        public LatestUsdViewModel GetLatestUsdRates()
        {
            var data = RequestHelper.ExecuteQuery(
                DataBaseConstants.ConnectionString,
                "SELECT * FROM GetLatestUsdRates()",
                MapUsdRates);
            return data.First();
        }

        private static LatestUsdViewModel MapUsdRates(DataRow row)
        {
            return new LatestUsdViewModel
            {
                CurrencyId = (int)row["Id"],
                VersionId = (int)row["VersionId"],
                UsdSpot = (double)row["UsdSpot"],
                UsdBudget = (double)row["UsdBudget"],
                EurSpot = (double)row["EurSpot"],
                EurBudget = (double)row["EurBudget"]
            };
        }
        private static IEnumerable<DataCurrencyViewModel> GetDataCurrency(IEnumerable<Tuple<int, int>> currencyVersion)
        {
            var connectionString = DataBaseConstants.ConnectionString;
            const string query =
                "SELECT * FROM GetSpotCurrency(@currencyVersion)";
            return RequestHelper.ExecuteQuery(
                connectionString,
                query,
                MapDataCurrency,
                new Dictionary<string, IEnumerable<Tuple<int, int>>>
                {
                    {"currencyVersion", currencyVersion}
                });
        }
        private static IEnumerable<DataViewModel> AddCurrencyData(IEnumerable<DataViewModel> fillData)
        {
            var data = fillData.ToList();
            var currencyVersion = data.Select(d => new Tuple<int, int>(d.CurrencySpotId, d.CurrencySpotVersionId)).Distinct();
            var currencyData = GetDataCurrency(currencyVersion).ToList();
            data.Join(currencyData,
                a => new Tuple<int, int>(a.CurrencySpotId, a.CurrencySpotVersionId),
                b => new Tuple<int, int>(b.CurrencyId, b.VersionId),
                (a, b) => new { a, b }).ForEach(ab =>
                {
                    ab.a.UsdSpot = ab.b.Eur;
                    ab.a.EurSpot = ab.b.Eur;
                });
            return data;
        }
        public IEnumerable<DataViewModel> DataVariation(IEnumerable<DataViewModel> x)
        {
            var y = x.ToList();
            y.Join(y,
                a => new { a.GeographyId, a.ProductId, a.PriceTypeId, a.DataTypeId, a.DataTime.AddMonths(-1).Date },
                b => new { b.GeographyId, b.ProductId, b.PriceTypeId, b.DataTypeId, b.DataTime.Date },
                (a, b) => new { a, b })
                .ForEach(ab => ab.a.PercentageVariation = ab.b.Value != 0 ? (ab.a.Value - ab.b.Value) / ab.b.Value : 0);
            return y;
        }
        private static DataViewModel MapData(DataRow row)
        {
            var data = new DataViewModel
            {
                GeographyId = (int)row["GeographyId"],
                ProductId = (int)row["ProductId"],
                CurrencySpotId = (int)row["CurrencySpotId"],
                CurrencySpotVersionId = (int)row["CurrencySpotVersionId"],
                PriceTypeId = (int)row["PriceTypeId"],
                DataTypeId = (int)row["DataTypeId"],
                EventTypeId = (int)row["EventTypeId"],
                UnitTypeId = (int)row["UnitTypeId"],
                SegmentId = (int)row["SegmentId"],
                DataTime = (DateTime)row["DataTime"],
                Value = row["Value"].PreventNull(),
                Description = row["Description"].PreventStringNull(),
                UsdSpot = row["UsdSpot"].PreventNull(),
                EurSpot = row["EurSpot"].PreventNull(),
                UsdBudget = row["UsdBudget"].PreventNull(),
                EurBudget = row["EurBudget"].PreventNull(),
                Active = (bool)row["Active"]
            };
            return data;
        }
        private static DataCurrencyViewModel MapDataCurrency(DataRow row)
        {
            var data = new DataCurrencyViewModel
            {
                CurrencyId = (int)row["CurrencyId"],
                VersionId = (int)row["VersionId"],
                Usd = (double)row["USD"],
                Eur = (double)row["EUR"]
            };
            return data;
        }

        private static CurrencyVersionViewModel MapCurrencyVersion(DataRow row)
        {
            return new CurrencyVersionViewModel
            {
                CurrencyId = (int)row["Id"],
                VersionId = (int)row["VersionId"]
            };
        }
    }
}