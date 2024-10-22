using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PriceCare.Web.Constants;
using PriceCare.Web.Helpers;
using PriceCare.Web.Math.Utils;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public class CacheRepository
    {
        private readonly IAccountRepository accountRepository = new AccountRepository();
        public void ClearCache(string userId)
        {
            const string query = "ClearUserCacheTable @userId";
            RequestHelper.ExecuteNonQuery(
                DataBaseConstants.ConnectionString,
                query,
                new Dictionary<string, object>
                {
                    {"userId", userId}
                });
        }
        public void DeletePermIds(int simulationId, IEnumerable<IDataPermIdentifier> distinctPermIds)
        {
            var userId = GetSimulation(simulationId).UserId;
            var cacheTable = new CacheTables(userId);
            var permIds = distinctPermIds.Select(dP => dP.PermId).ToList();
            var query = "DELETE @cacheData WHERE PermId IN (SELECT PermId FROM @permIds)";
            query = query.Replace("@cacheData", cacheTable.Data);
            RequestHelper.ExecuteNonQuery(
                DataBaseConstants.ConnectionString,
                query,
                new Dictionary<string, List<string>> { { "permIds", permIds } }
                );
        }
        public void DeleteRule(int simulationId, int geographyId, int productId, int gprmRuleTypeId, DateTime applicableFrom)
        {
            var userId = GetSimulation(simulationId).UserId;
            var cacheTable = new CacheTables(userId);
            const string query = "DELETE @cacheTable WHERE SimulationId=@simulationId AND GeographyId=@geographyId AND ProductId=@productId AND GprmRuleTypeId=@gprmRuleTypeId AND ApplicableFrom=@applicableFrom";
            DeleteRuleComponent(query, cacheTable.Rule, simulationId, geographyId, productId, gprmRuleTypeId, applicableFrom);
            DeleteRuleComponent(query, cacheTable.SubRule, simulationId, geographyId, productId, gprmRuleTypeId, applicableFrom);
            DeleteRuleComponent(query, cacheTable.Basket, simulationId, geographyId, productId, gprmRuleTypeId, applicableFrom);
        }

        private static void DeleteRuleComponent(string query, string cacheTable, int simulationId, int geographyId, int productId, int gprmRuleTypeId, DateTime applicableFrom)
        {
            query = query.Replace("@cacheTable", cacheTable);
            var dictionnary = new Dictionary<string, object>
            {
                {"simulationId",simulationId},
                {"geographyId",geographyId},
                {"productId",productId},
                {"gprmRuleTypeId",gprmRuleTypeId},
                {"applicableFrom",applicableFrom}
            };
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, dictionnary);
        }
        public int SetCache(CacheViewModel model)
        {
            const string query =
                @"INSERT INTO Cache (UserId,SaveId,DataVersionId,CurrencyBudgetVersionId,AssumptionsSaveId,StartTime,Duration,SimulationCurrencyId,Edited,IsLaunch) 
                 OUTPUT INSERTED.ID
                 VALUES (@userId,@saveId,@dataVersionId,@currencyBudgetVersionId,@assumptionsSaveId,@startTime,@Duration,@simulationCurrencyId,@edited,@isLaunch)";
            var dictionnary = new Dictionary<string, object> { 
                {"userId", model.UserId},
                {"saveId", model.SaveId},
                {"dataVersionId", model.DataVersionId},
                {"currencyBudgetVersionId", model.CurrencyBudgetVersionId},
                {"assumptionsSaveId", model.AssumptionsSaveId},
                {"startTime", model.StartTime},
                {"duration", model.Duration},
                {"simulationCurrencyId", model.SimulationCurrencyId},
                {"edited", model.Edited},
                {"isLaunch", model.IsLaunch}
            };
            return RequestHelper.ExecuteScalarRequest<int>(DataBaseConstants.ConnectionString, query, dictionnary);
        }

        public void UpdateSimulationInfo(int simulationId, int saveId)
        {
            const string query = "UPDATE Cache SET SaveId=@saveId WHERE Id=@simulationId";
            var dictionnary = new Dictionary<string, object> { {"saveId",@saveId},{ "simulationId", simulationId } };
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, dictionnary);
        }
        public void SetCacheEdited(int simulationId)
        {
            const string query = "UPDATE Cache SET Edited=1 WHERE Id=@simulationId";
            var dictionnary = new Dictionary<string, object> { { "simulationId", simulationId } };
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, dictionnary);
        }
        public void CacheData(List<DataViewModel> data, int simulationId, string userId)
        {
            var cacheTable = new CacheTables(userId);

            var dataBaseData = data.Select(d => new
            {
                SimulationId = simulationId,
                d.PermId,
                d.GeographyId,
                d.ProductId,
                d.CurrencySpotId,
                d.CurrencySpotVersionId,
                d.PriceTypeId,
                d.DataTypeId,
                d.EventTypeId,
                d.UnitTypeId,
                d.SegmentId,
                d.DataTime,
                d.Value,
                d.Description,
                d.UsdSpot,
                d.EurSpot,
                d.UsdBudget,
                d.EurBudget,
                d.PercentageVariation,
                d.Active,
                d.Edited,
                d.Reviewed
            }).ToList();

            var datatable = dataBaseData.ToDataTable();

            RequestHelper.BulkInsert(DataBaseConstants.ConnectionString, cacheTable.Data, datatable);
        }
        public void CreateCacheTable(string userId)
        {
            var cacheTable = new CacheTables(userId);
            var query = "SELECT * INTO " + cacheTable.Data + " FROM CacheDataTemplate";
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query);
            query = "SELECT * INTO " + cacheTable.Rule + " FROM CacheRuleTemplate";
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query);
            query = "SELECT * INTO " + cacheTable.SubRule + " FROM CacheSubRuleTemplate";
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query);
            query = "SELECT * INTO " + cacheTable.Basket + " FROM CacheBasketTemplate";
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query);
        }
        public void CacheRule(List<RuleViewModel> rule, int simulationId, string userId)
        {
            var cacheTable = new CacheTables(userId);

            var ruleCache = rule.Select(r => new RuleCache
            {
                SimulationId = simulationId,
                GeographyId = r.GeographyId,
                ProductId = r.ProductId,
                GprmRuleTypeId = r.GprmRuleTypeId,
                ApplicableFrom = r.ApplicableFrom,
                Regular = r.Regular,
                GprmMathId = r.GprmMathId,
                Argument = r.Argument,
                WeightTypeId = r.WeightTypeId,
                IrpRuleListId = r.IrpRuleListId,
                LookBack = r.LookBack,
                EffectiveLag = r.EffectiveLag,
                AllowIncrease = r.AllowIncrease,
                ReviewedPriceTypeId = r.ReviewedPriceTypeId,
                ReviewedPriceAdjustment = r.ReviewedPriceAdjustment,
                Geography = r.Geography,
                ReviewedPriceType = r.ReviewedPriceType,
                Edited = r.Edited,
                IsDefault = r.Default,
                Active = r.Active
            });
            var subRuleCache = new List<SubRuleCache>();
            var basketCache = new List<BasketCache>();
            foreach (var r in rule)
            {
                if (r.ReferencedData.Count() == 1)
                {
                    var subRule = r.ReferencedData.First();
                    subRule.GprmMathId = r.GprmMathId;
                    subRule.WeightTypeId = r.WeightTypeId;
                    subRule.Edited = r.Edited;
                    subRule.Argument = r.Argument;
                    subRule.Default = r.Default;
                    subRule.Active = r.Active;
                }
                var defaultBasket = r.DefaultBasket.Select(b => new BasketCache
                {
                    SimulationId = simulationId,
                    GeographyId = r.GeographyId,
                    ProductId = r.ProductId,
                    GprmRuleTypeId = r.GprmRuleTypeId,
                    SubRuleIndex = ApplicationConstants.NotSpecifiedSubRuleIndex,
                    ReferencedGeographyId = b.ReferencedGeographyId,
                    ApplicableFrom = r.ApplicableFrom,
                    ReferencedPriceTypeId = b.ReferencedPriceTypeId,
                    ReferencedPriceAdjustment = b.ReferencedPriceAdjustment,
                    IsDefault = b.Default,
                    Edited = b.Edited,
                    ReferencedGeography = b.ReferencedGeography,
                    ReferencedPriceType = b.ReferencedPriceType,
                    Active = b.Active
                }).ToList();
                basketCache.AddRange(defaultBasket);
                subRuleCache.AddRange(r.ReferencedData.Select(rD => new SubRuleCache
                {
                    SimulationId = simulationId,
                    GeographyId = r.GeographyId,
                    ProductId = r.ProductId,
                    GprmRuleTypeId = r.GprmRuleTypeId,
                    SubRuleIndex = rD.SubRuleIndex,
                    ApplicableFrom = r.ApplicableFrom,
                    GprmMathId = rD.GprmMathId,
                    Argument = rD.Argument,
                    WeightTypeId = rD.WeightTypeId,
                    Edited = rD.Edited,
                    IsDefault = rD.Default,
                    Active = rD.Active
                }));
                foreach (var rD in r.ReferencedData.Where(rD => rD.SubRuleIndex != 0))
                {
                    var subRuleBasket = rD.Basket.Select(b => new BasketCache
                    {
                        SimulationId = simulationId,
                        GeographyId = r.GeographyId,
                        ProductId = r.ProductId,
                        GprmRuleTypeId = r.GprmRuleTypeId,
                        SubRuleIndex = rD.SubRuleIndex,
                        ReferencedGeographyId = b.ReferencedGeographyId,
                        ApplicableFrom = r.ApplicableFrom,
                        ReferencedPriceTypeId = b.ReferencedPriceTypeId,
                        ReferencedPriceAdjustment = b.ReferencedPriceAdjustment,
                        IsDefault = b.Default,
                        Edited = b.Edited,
                        ReferencedGeography = b.ReferencedGeography,
                        ReferencedPriceType = b.ReferencedPriceType,
                        Active = b.Active
                    }).ToList();
                    var editedBasket = defaultBasket.Where(dB => dB.Edited).Select(dB => dB.ReferencedGeographyId).ToList();
                    basketCache.AddRange(subRuleBasket.Where(sRb => !editedBasket.Contains(sRb.ReferencedGeographyId)));
                    basketCache.AddRange(defaultBasket.Where(dB => dB.Edited).Select(b => new BasketCache
                    {
                        SimulationId = simulationId,
                        GeographyId = r.GeographyId,
                        ProductId = r.ProductId,
                        GprmRuleTypeId = r.GprmRuleTypeId,
                        SubRuleIndex = rD.SubRuleIndex,
                        ReferencedGeographyId = b.ReferencedGeographyId,
                        ApplicableFrom = r.ApplicableFrom,
                        ReferencedPriceTypeId = b.ReferencedPriceTypeId,
                        ReferencedPriceAdjustment = b.ReferencedPriceAdjustment,
                        IsDefault = true,
                        Edited = b.Edited,
                        ReferencedGeography = b.ReferencedGeography,
                        ReferencedPriceType = b.ReferencedPriceType,
                        Active = b.Active
                    }));
                }
            }
            var ruleDataTable = ruleCache.ToList().ToDataTable();
            var subRuleDataTable = subRuleCache.ToList().ToDataTable();
            var basketDataTable = basketCache.ToList().ToDataTable();

            RequestHelper.BulkInsert(DataBaseConstants.ConnectionString, cacheTable.Rule, ruleDataTable);
            RequestHelper.BulkInsert(DataBaseConstants.ConnectionString, cacheTable.SubRule, subRuleDataTable);
            RequestHelper.BulkInsert(DataBaseConstants.ConnectionString, cacheTable.Basket, basketDataTable);
        }
        public List<DataViewModel> GetDataCache(int simulationId)
        {
            return GetDataCache(simulationId, new List<int>(), new List<int>(), new List<int>());
        }

        public IEnumerable<DateTime> GetRuleApplicableFrom(string userId, int geographyId, int productId,
            int gprmRuleTypeId)
        {
            var cacheTable = new CacheTables(userId);
            var query =
                @"SELECT DISTINCT ApplicableFrom FROM @cacheRule WHERE
                    GeographyId=@geographyId AND
                    ProductId=@productId AND
                    GprmRuleTypeId=@gprmRuleTypeId";
            query = query.Replace("@cacheRule", cacheTable.Rule);
            var result = RequestHelper.ExecuteQuery<DateTime>(
                    DataBaseConstants.ConnectionString,
                    query,
                    MapApplicableFrom,
                    new Dictionary<string, object>
                    {
                        {"geographyId",geographyId},
                        {"productId",productId},
                        {"gprmRuleTypeId",gprmRuleTypeId}
                    }
                ).OrderByDescending(r => r.Date).ToList();
            return result.Count() != 0 ? result : new List<DateTime> { new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1) };
        }
        private static DateTime MapApplicableFrom(DataRow row)
        {
            return (DateTime)row["ApplicableFrom"];
        }
        public RuleDefinitionViewModel GetRule(int simulationId, int geographyId, int productId, int gprmRuleTypeId, DateTime applicableFrom)
        {
            var r = GetRuleCache(simulationId, geographyId, productId, gprmRuleTypeId, applicableFrom).FirstOrDefault();
            var s = GetSubRuleCache(simulationId, geographyId, productId, gprmRuleTypeId, applicableFrom).ToList();
            var b = GetBasketCache(simulationId, geographyId, productId, gprmRuleTypeId, applicableFrom).ToList();

            if (r == null)
                return new RuleDefinitionViewModel();

            var result = new RuleDefinitionViewModel
            {
                Regular = r.Regular,
                SelectedReviewedPriceTypeId = r.ReviewedPriceTypeId,
                Adjustement = r.ReviewedPriceAdjustment,   
                GprmMathId = r.GprmMathId,
                Argument = r.Argument,
                WeightTypeId = r.WeightTypeId,
                IrpRuleListId = r.IrpRuleListId,
                LookBack = r.LookBack,
                EffectiveLag = r.EffectiveLag,
                AllowIncrease = r.AllowIncrease,
                Default = r.IsDefault,
                Active = r.Active,
                Edited = r.Edited,
                DefaultBasket = b.Where(bp => bp.SubRuleIndex == 0 && bp.Active).Select(bp => new Basket
                {
                    ReferencedGeography = bp.ReferencedGeography,
                    ReferencedPriceType = bp.ReferencedPriceType,
                    ReferencedGeographyId = bp.ReferencedGeographyId,
                    ReferencedPriceTypeId = bp.ReferencedPriceTypeId,
                    ReferencedPriceAdjustment = bp.ReferencedPriceAdjustment,
                    Default = bp.IsDefault,
                    Edited = bp.Edited
                }).OrderBy(gBfS => gBfS.ReferencedGeography).ToList(),
                ReferencedData = GetSubRuleForRule(s, b, false)
            };
            result.ReviewedPriceTypeOptions = new PriceTypeRepository().GetPriceTypes(geographyId, productId, applicableFrom).ToList();

            return result;
        }

        private static List<RuleViewModel> BuildRule(List<RuleCache> rule, IEnumerable<SubRuleCache> subRule, IEnumerable<BasketCache> basket, bool forSave)
        {
            var distinctRule = rule.Distinct(new RulegptaEqualityComparer());

            var rT = rule.ToLookup(r => r, new RulegptaEqualityComparer());
            var sT = subRule.ToLookup(r => r, new RulegptaEqualityComparer());
            var bT = basket.ToLookup(r => r, new RulegptaEqualityComparer());

            return distinctRule.Select(dR => new RuleViewModel
            {
                Geography = rT[dR].First().Geography,
                ReviewedPriceType = rT[dR].First().ReviewedPriceType,
                ApplicableFrom = rT[dR].First().ApplicableFrom,
                GeographyId = rT[dR].First().GeographyId,
                ProductId = rT[dR].First().ProductId,
                GprmRuleTypeId = rT[dR].First().GprmRuleTypeId,
                ReviewedPriceTypeId = rT[dR].First().ReviewedPriceTypeId,
                ReviewedPriceAdjustment = rT[dR].First().ReviewedPriceAdjustment,
                Regular = rT[dR].First().Regular,
                GprmMathId = rT[dR].First().GprmMathId,
                Argument = rT[dR].First().Argument,
                WeightTypeId = rT[dR].First().WeightTypeId,
                IrpRuleListId = rT[dR].First().IrpRuleListId,
                LookBack = rT[dR].First().LookBack,
                EffectiveLag = rT[dR].First().EffectiveLag,
                AllowIncrease = rT[dR].First().AllowIncrease,
                Edited = rT[dR].First().Edited,
                Default = rT[dR].First().IsDefault,
                DefaultBasket = bT[dR].Where(b => b.SubRuleIndex == 0 && (forSave || b.Active)).Select(b => new Basket
                {
                    ReferencedGeography = b.ReferencedGeography,
                    ReferencedPriceType = b.ReferencedPriceType,
                    ReferencedGeographyId = b.ReferencedGeographyId,
                    ReferencedPriceTypeId = b.ReferencedPriceTypeId,
                    ReferencedPriceAdjustment = b.ReferencedPriceAdjustment,
                    Edited = b.Edited,
                    Default = b.IsDefault,
                    Active = b.Active
                }).ToList(),
                ReferencedData = GetSubRuleForRule(sT[dR], bT[dR], forSave)
            }).ToList();
        }
        public List<RuleViewModel> GetRule(int simulationId, int productId)
        {
            var rule = GetRuleCache(simulationId, productId).ToList();
            var subRule = GetSubRuleCache(simulationId, productId).ToList();
            var basket = GetBasketCache(simulationId, productId).ToList();
            return BuildRule(rule, subRule, basket, false);
        }

        public List<RuleViewModel> GetRule(int simulationId, bool forSave = false)
        {
            var rule = GetRuleCache(simulationId).ToList();
            var subRule = GetSubRuleCache(simulationId).ToList();
            var basket = GetBasketCache(simulationId).ToList();
            return BuildRule(rule, subRule, basket, forSave);
        }
        private static List<SubRuleViewModel> GetSubRuleForRule(IEnumerable<SubRuleCache> subRule, IEnumerable<BasketCache> basket, bool forSave)
        {
            return (from sR in subRule
                    where (sR.Active || forSave)
                    let i = sR.SubRuleIndex
                    select new SubRuleViewModel
                    {
                        SubRuleIndex = sR.SubRuleIndex,
                        GprmMathId = sR.GprmMathId,
                        Argument = sR.Argument,
                        WeightTypeId = sR.WeightTypeId,
                        Default = sR.IsDefault,
                        Edited = sR.Edited,
                        Active = sR.Active,
                        Basket = basket.Where(b => b.SubRuleIndex == i && (forSave || b.Active)).Select(b => new Basket
                        {
                            ReferencedGeography = b.ReferencedGeography,
                            ReferencedPriceType = b.ReferencedPriceType,
                            ReferencedGeographyId = b.ReferencedGeographyId,
                            ReferencedPriceTypeId = b.ReferencedPriceTypeId,
                            ReferencedPriceAdjustment = b.ReferencedPriceAdjustment,
                            Edited = b.Edited,
                            Default = b.IsDefault,
                            Active = b.Active
                        }).OrderBy(gBfS => gBfS.ReferencedGeography).ToList()
                    }).ToList();
        }

        public bool IsProductInCache(string userId, int productId)
        {
            var cacheTable = new CacheTables(userId);
            var query = "SELECT COUNT(*) FROM @cacheData WHERE ProductId=@productId";
            query = query.Replace("@cacheData", cacheTable.Rule);
            var check = RequestHelper.ExecuteScalarRequest<int>(DataBaseConstants.ConnectionString, query, new Dictionary<string, object> { { "productId", productId } });
            return check != 0;
        }

        public List<DataViewModel> GetDataCache(int simulationId, List<int> geographyId, List<int> productId, List<int> dataTypeId)
        {
            var simulationInfo = GetSimulation(simulationId);
            var cacheTable = new CacheTables(simulationInfo.UserId);
            var connectionString = DataBaseConstants.ConnectionString;
            var query = CacheDataQuery(cacheTable.Data);
            var result = RequestHelper.ExecuteQuery(
                connectionString,
                query,
                MapDataCache,
                new Dictionary<string, List<int>>
                {
                    {"geographyId",geographyId},
                    {"productId",productId},
                    {"dataTypeId",dataTypeId}
                }).ToList();
            return result.OrderBy(fR => fR.DataTime).ToList();
        }
        private IEnumerable<RuleCache> GetRuleCache(int simulationId, int geographyId, int productId, int gprmRuleTypeId, DateTime applicableFrom)
        {
            var simulationInfo = GetSimulation(simulationId);
            var cacheTable = new CacheTables(simulationInfo.UserId);
            var connectionString = DataBaseConstants.ConnectionString;
            var query =
                "SELECT * FROM @cacheRule WHERE GeographyId=@geographyId AND ProductId=@productId AND GprmRuleTypeId=@gprmRuleTypeId AND ApplicableFrom=@applicableFrom";
            query = query.Replace("@cacheRule", cacheTable.Rule);
            var result = RequestHelper.ExecuteQuery<RuleCache>(
                connectionString,
                query,
                MapRuleCache,
                new Dictionary<string, object>
                {
                    {"geographyId",geographyId},
                    {"productId",productId},
                    {"gprmRuleTypeId",gprmRuleTypeId},
                    {"applicableFrom",applicableFrom}
                }).ToList();
            return result;
        }
        private IEnumerable<SubRuleCache> GetSubRuleCache(int simulationId, int geographyId, int productId, int gprmRuleTypeId, DateTime applicableFrom)
        {
            var simulationInfo = GetSimulation(simulationId);
            var cacheTable = new CacheTables(simulationInfo.UserId);
            var connectionString = DataBaseConstants.ConnectionString;
            var query =
                "SELECT * FROM @cacheSubRule WHERE GeographyId=@geographyId AND ProductId=@productId AND GprmRuleTypeId=@gprmRuleTypeId AND ApplicableFrom=@applicableFrom";
            query = query.Replace("@cacheSubRule", cacheTable.SubRule);
            var result = RequestHelper.ExecuteQuery<SubRuleCache>(
                connectionString,
                query,
                MapSubRuleCache,
                new Dictionary<string, object>
                {
                    {"geographyId",geographyId},
                    {"productId",productId},
                    {"gprmRuleTypeId",gprmRuleTypeId},
                    {"applicableFrom",applicableFrom}
                }).ToList();
            return result;
        }
        private IEnumerable<BasketCache> GetBasketCache(int simulationId, int geographyId, int productId, int gprmRuleTypeId, DateTime applicableFrom)
        {
            var simulationInfo = GetSimulation(simulationId);
            var cacheTable = new CacheTables(simulationInfo.UserId);
            var connectionString = DataBaseConstants.ConnectionString;
            var query =
                "SELECT * FROM @cacheBasket WHERE GeographyId=@geographyId AND ProductId=@productId AND GprmRuleTypeId=@gprmRuleTypeId AND ApplicableFrom=@applicableFrom";
            query = query.Replace("@cacheBasket", cacheTable.Basket);
            var result = RequestHelper.ExecuteQuery<BasketCache>(
                connectionString,
                query,
                MapBasketCache,
                new Dictionary<string, object>
                {
                    {"geographyId",geographyId},
                    {"productId",productId},
                    {"gprmRuleTypeId",gprmRuleTypeId},
                    {"applicableFrom",applicableFrom}
                }).ToList();
            return result;
        }
        private IEnumerable<RuleCache> GetRuleCache(int simulationId)
        {
            var simulationInfo = GetSimulation(simulationId);
            var cacheTable = new CacheTables(simulationInfo.UserId);
            var connectionString = DataBaseConstants.ConnectionString;
            var query =
                "SELECT * FROM @cacheRule";
            query = query.Replace("@cacheRule", cacheTable.Rule);
            var result = RequestHelper.ExecuteQuery<RuleCache>(
                connectionString,
                query,
                MapRuleCache).ToList();
            return result;
        }
        private IEnumerable<RuleCache> GetRuleCache(int simulationId, int productId)
        {
            var simulationInfo = GetSimulation(simulationId);
            var cacheTable = new CacheTables(simulationInfo.UserId);
            var connectionString = DataBaseConstants.ConnectionString;
            var query =
                "SELECT * FROM @cacheRule WHERE ProductId=@productId";
            query = query.Replace("@cacheRule", cacheTable.Rule);
            var result = RequestHelper.ExecuteQuery<RuleCache>(
                connectionString,
                query,
                MapRuleCache,
                new Dictionary<string, object>
                {
                    {"productId",productId}
                }).ToList();
            return result;
        }
        private IEnumerable<SubRuleCache> GetSubRuleCache(int simulationId)
        {
            var simulationInfo = GetSimulation(simulationId);
            var cacheTable = new CacheTables(simulationInfo.UserId);
            var connectionString = DataBaseConstants.ConnectionString;
            var query =
                "SELECT * FROM @cacheSubRule";
            query = query.Replace("@cacheSubRule", cacheTable.SubRule);
            var result = RequestHelper.ExecuteQuery<SubRuleCache>(
                connectionString,
                query,
                MapSubRuleCache).ToList();
            return result;
        }
        private IEnumerable<SubRuleCache> GetSubRuleCache(int simulationId, int productId)
        {
            var simulationInfo = GetSimulation(simulationId);
            var cacheTable = new CacheTables(simulationInfo.UserId);
            var connectionString = DataBaseConstants.ConnectionString;
            var query =
                "SELECT * FROM @cacheSubRule WHERE ProductId=@productId";
            query = query.Replace("@cacheSubRule", cacheTable.SubRule);
            var result = RequestHelper.ExecuteQuery<SubRuleCache>(
                connectionString,
                query,
                MapSubRuleCache,
                new Dictionary<string, object>
                {
                    {"productId",productId}
                }).ToList();
            return result;
        }
        private IEnumerable<BasketCache> GetBasketCache(int simulationId)
        {
            var simulationInfo = GetSimulation(simulationId);
            var cacheTable = new CacheTables(simulationInfo.UserId);
            var connectionString = DataBaseConstants.ConnectionString;
            var query =
                "SELECT * FROM @cacheBasket";
            query = query.Replace("@cacheBasket", cacheTable.Basket);
            var result = RequestHelper.ExecuteQuery<BasketCache>(
                connectionString,
                query,
                MapBasketCache).ToList();
            return result;
        }
        public IEnumerable<BasketCache> GetBasketCache(int simulationId, int productId)
        {
            var simulationInfo = GetSimulation(simulationId);
            var cacheTable = new CacheTables(simulationInfo.UserId);
            var connectionString = DataBaseConstants.ConnectionString;
            var query =
                "SELECT * FROM @cacheBasket WHERE ProductId=@productId";
            query = query.Replace("@cacheBasket", cacheTable.Basket);
            var result = RequestHelper.ExecuteQuery<BasketCache>(
                connectionString,
                query,
                MapBasketCache,
                new Dictionary<string, object>
                {
                    {"productId",productId}
                }).ToList();
            return result;
        }
        private static DataViewModel MapDataCache(DataRow row)
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
                PercentageVariation = row["PercentageVariation"].PreventNull(),
                Active = (bool)row["Active"],
                Edited = (bool)row["Edited"],
                Reviewed = (bool)row["Reviewed"]
            };
            return data;
        }
        private static RuleCache MapRuleCache(DataRow row)
        {
            var data = new RuleCache
            {
                GeographyId = (int)row["GeographyId"],
                ProductId = (int)row["ProductId"],
                GprmRuleTypeId = (int)row["GprmRuleTypeId"],
                ApplicableFrom = (DateTime)row["ApplicableFrom"],
                Regular = (bool)row["Regular"],
                GprmMathId = (int)row["GprmMathId"],
                Argument = (int)row["Argument"],
                WeightTypeId = (int)row["WeightTypeId"],
                IrpRuleListId = (int)row["IrpRuleListId"],
                LookBack = (int)row["Lookback"],
                EffectiveLag = (int)row["EffectiveLag"],
                AllowIncrease = (bool)row["AllowIncrease"],
                ReviewedPriceTypeId = (int)row["ReviewedPriceTypeId"],
                ReviewedPriceAdjustment = (double)row["ReviewedPriceAdjustment"],
                Geography = row["Geography"].PreventStringNull(),
                ReviewedPriceType = row["ReviewedPriceType"].PreventStringNull(),
                Edited = (bool)row["Edited"],
                IsDefault = (bool)row["IsDefault"],
                Active = (bool)row["Active"]
            };
            return data;
        }
        private static SubRuleCache MapSubRuleCache(DataRow row)
        {
            var data = new SubRuleCache
            {
                GeographyId = (int)row["GeographyId"],
                ProductId = (int)row["ProductId"],
                GprmRuleTypeId = (int)row["GprmRuleTypeId"],
                SubRuleIndex = (int)row["SubRuleIndex"],
                ApplicableFrom = (DateTime)row["ApplicableFrom"],
                GprmMathId = (int)row["GprmMathId"],
                Argument = (int)row["Argument"],
                WeightTypeId = (int)row["WeightTypeId"],
                Edited = (bool)row["Edited"],
                IsDefault = (bool)row["IsDefault"],
                Active = (bool)row["Active"]
            };
            return data;
        }
        private static BasketCache MapBasketCache(DataRow row)
        {
            var data = new BasketCache
            {
                GeographyId = (int)row["GeographyId"],
                ProductId = (int)row["ProductId"],
                GprmRuleTypeId = (int)row["GprmRuleTypeId"],
                SubRuleIndex = (int)row["SubRuleIndex"],
                ReferencedGeographyId = (int)row["ReferencedGeographyId"],
                ApplicableFrom = (DateTime)row["ApplicableFrom"],
                ReferencedPriceTypeId = (int)row["ReferencedPriceTypeId"],
                ReferencedPriceAdjustment = (double)row["ReferencedPriceAdjustment"],
                Edited = (bool)row["Edited"],
                IsDefault = (bool)row["IsDefault"],
                ReferencedGeography = row["ReferencedGeography"].PreventStringNull(),
                ReferencedPriceType = row["ReferencedPriceType"].PreventStringNull(),
                Active = (bool)row["Active"]
            };
            return data;
        }
        public int GetFirstSimulation()
        {
            var connectionString = DataBaseConstants.ConnectionString;
            var userId = accountRepository.GetUserId();
            const string query =
                "SELECT * FROM Cache WHERE UserId = @userId ";
            var result = RequestHelper.ExecuteQuery<CacheViewModel>(
                connectionString,
                query,
                MapCacheViewModel,
                new Dictionary<string, object>
                {
                    {"userId", userId}
                }).ToList();
            var simulation = result.FirstOrDefault();
            if (simulation != null)
                return simulation.Id;
            return -1;
        }
        public CacheViewModel GetFirstSimulationAndSaveId()
        {
            var connectionString = DataBaseConstants.ConnectionString;
            var userId = accountRepository.GetUserId();
            const string query =
                "SELECT * FROM Cache WHERE UserId = @userId ";
            var result = RequestHelper.ExecuteQuery<CacheViewModel>(
                connectionString,
                query,
                MapCacheViewModel,
                new Dictionary<string, object>
                {
                    {"userId", userId}
                }).ToList();

            var cacheInfo = result.FirstOrDefault();
            if (cacheInfo != null)
                cacheInfo.IsCurrentUser = true;

            return cacheInfo;

        }
        public CacheViewModel GetSimulation(int simulationId)
        {
            var connectionString = DataBaseConstants.ConnectionString;
            const string query =
                "SELECT * FROM Cache WHERE Id = @simulationId ";
            var result = RequestHelper.ExecuteQuery<CacheViewModel>(
                connectionString,
                query,
                MapCacheViewModel,
                new Dictionary<string, object>
                {
                    {"simulationId", simulationId}
                }).ToList();
            var simulation = result.FirstOrDefault();
            return simulation ?? new CacheViewModel();
        }

        public CacheViewModel GetSimulation(int saveId, string userId)
        {
            var connectionString = DataBaseConstants.ConnectionString;
            const string query =
                "SELECT * FROM Cache WHERE UserId = @userId AND SaveId = @saveId ";
            var result = RequestHelper.ExecuteQuery<CacheViewModel>(
                connectionString,
                query,
                MapCacheViewModel,
                new Dictionary<string, object>
                {
                    {"saveId", saveId},
                    {"userId", userId}
                }).ToList();
            var simulation = result.FirstOrDefault();
            return simulation ?? new CacheViewModel();
        }
        private static CacheViewModel MapCacheViewModel(DataRow row)
        {
            var data = new CacheViewModel
            {
                Id = (int)row["Id"],
                UserId = row["UserId"].ToString(),
                SaveId = (int)row["SaveId"],
                DataVersionId = (int)row["DataVersionId"],
                CurrencyBudgetVersionId = (int)row["CurrencyBudgetVersionId"],
                AssumptionsSaveId = (int)row["AssumptionsSaveId"],
                StartTime = (DateTime)row["StartTime"],
                Duration = (int)row["Duration"],
                SimulationCurrencyId = (int)row["SimulationCurrencyId"],
                Edited = (bool)row["Edited"],
                IsLaunch = (bool)row["IsLaunch"]
            };
            return data;
        }
        private static string CacheDataQuery(string cacheData)
        {
            const string query = @"WITH
	            countGeography (CountId)
	            AS
	            (
		            SELECT COUNT(Id)  FROM @geographyId
	            ),
	            geographyIds (Id)
	            AS
	            (
		            SELECT g.Id
		            FROM Geography g
		            LEFT JOIN @geographyId gId
		            ON g.Id = gid.Id
		            LEFT JOIN countGeography cg
		            ON cg.CountId != 0
		            WHERE cg.CountId is null OR gId.Id IS NOT NULL
	            ),
	            countProduct (CountId)
	            AS
	            (
		            SELECT COUNT(Id)  FROM @productId
	            ),
	            productIds (Id)
	            AS
	            (
		            SELECT g.Id
		            FROM Product g
		            LEFT JOIN @productId gId
		            ON g.Id = gid.Id
		            LEFT JOIN countProduct cg
		            ON cg.CountId != 0
		            WHERE cg.CountId is null OR gId.Id IS NOT NULL
	            ),
	            countDataTypeId (CountId)
	            AS
	            (
		            SELECT COUNT(Id)  FROM @dataTypeId
	            ),
	            dataTypeIds (Id)
	            AS
	            (
		            SELECT g.Id
		            FROM dbo.GetDimensionType('Data') g
		            LEFT JOIN @dataTypeId gId
		            ON g.Id = gid.Id
		            LEFT JOIN countDataTypeId cg
		            ON cg.CountId != 0
		            WHERE cg.CountId is null OR gId.Id IS NOT NULL
	            )
	            SELECT
		            D.PermId,
		            D.GeographyId,
		            D.ProductId,
		            D.CurrencySpotId,
		            D.CurrencySpotVersionId,
		            D.PriceTypeId,
		            D.DataTypeId,
		            D.EventTypeId,
		            D.UnitTypeId,
		            D.SegmentId,
		            D.DataTime,
		            D.Value,
		            D.Description,
		            D.UsdSpot,
		            D.EurSpot,
		            D.UsdBudget,
		            D.EurBudget,
		            D.PercentageVariation,
                    D.Active,
		            D.Edited,
		            D.Reviewed
	            FROM @cacheData AS D
		            INNER JOIN geographyIds AS G ON D.GeographyId=G.Id
		            INNER JOIN productIds AS P ON D.ProductId=P.Id
		            INNER JOIN dataTypeIds AS DT ON D.DataTypeId=DT.Id
	            --WHERE
		            --D.Active=1";
            return query.Replace("@cacheData", cacheData);
        }

        public Tuple<bool, bool, bool> CheckLaunchScenario(string userId)
        {
            var cacheTable = new CacheTables(userId);
            var queryLaunch =
                "SELECT COUNT(*) FROM @cacheTable WHERE DataTypeId=@dataTypeId AND (EventTypeId=@launchTarget OR EventTypeId=@launchDefault OR EventTypeId=@launchLaunch)";
            queryLaunch = queryLaunch.Replace("@cacheTable", cacheTable.Data);
            var launchCount = RequestHelper.ExecuteScalarRequest<int>(
                DataBaseConstants.ConnectionString, 
                queryLaunch, 
                new Dictionary<string, object>
                {
                    {"dataTypeId",(int)DataTypes.Event},
                    {"launchTarget",(int)EventTypes.LaunchTarget},
                    {"launchDefault",(int)EventTypes.LaunchDefaultRule},
                    {"launchLaunch",(int)EventTypes.LaunchLaunchRule}
                });
            var queryVolume =
                "SELECT COUNT(*) FROM @cacheTable WHERE DataTypeId=@dataTypeId";
            queryVolume = queryVolume.Replace("@cacheTable", cacheTable.Data);
            var volumeCount = RequestHelper.ExecuteScalarRequest<int>(
                DataBaseConstants.ConnectionString,
                queryVolume,
                new Dictionary<string, object>
                {
                    {"dataTypeId", (int) DataTypes.Volume}
                });
            return new Tuple<bool, bool, bool>(launchCount != 0, volumeCount != 0, true);
        }
    }
}