using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.OData.Query.SemanticAst;
using PriceCare.Web.Constants;
using PriceCare.Web.Helpers;
using PriceCare.Web.Math.Utils;
using PriceCare.Web.Models;
using PriceCare.Web.Repository.Utils;

namespace PriceCare.Web.Repository
{
    public class GprmRuleRepository : IGprmRuleRepository
    {
        private readonly IPriceTypeRepository priceTypeRepository;
        private readonly IVersionRepository versionRepository = new VersionRepository();
        private readonly IProductRepository productRepository = new ProductRepository();
        private readonly DimensionRepository dimensionRepository = new DimensionRepository();

        public GprmRuleRepository()
        {
            priceTypeRepository = new PriceTypeRepository();
        }
        public List<RuleTypeViewModel> GetGprmRuleTypes()
        {
            const string query = "SELECT Id, Name FROM RuleType WHERE Id != 3";

            var result = RequestHelper.ExecuteQuery(DataBaseConstants.ConnectionString, query, row => new RuleTypeViewModel
            {
                Id = (int)row["Id"],
                Name = (string)row["Name"]
            }).ToList();

            return result;
        }
        public List<PriceMapViewModel> GetVersionPriceMap(int versionId, int geographyId, int productId, int gprmRuleTypeId, DateTime applicationFrom, int saveId = ApplicationConstants.ImportedDataSaveId)
        {
            return GetPriceMap(saveId, geographyId, productId, gprmRuleTypeId, versionId).Where(gPm => gPm.ApplicableFrom == applicationFrom).ToList();
        }
        public List<RuleViewModel> GetVersionRule(int versionId, int productId, DateTime startTime, int duration, int saveId = ApplicationConstants.ImportedDataSaveId)
        {
            var endTime = new DateTime(startTime.Year + duration, 12, 1);
            startTime = new DateTime(1000,1,1);
            return GetRule(saveId, 0, productId, 0, versionId, endTime).Where(d => d.ApplicableFrom > startTime).ToList();
        } 
        public List<RuleViewModel> GetVersionRule(int versionId, int geographyId, int productId, int gprmRuleTypeId, DateTime applicableFrom)
        {
            var endTime = new DateTime(4000, 12, 31);
            return GetRule(ApplicationConstants.ImportedDataSaveId, geographyId, productId, gprmRuleTypeId, versionId, endTime).Where(gR => gR.ApplicableFrom == applicableFrom).ToList();
        }
        public IEnumerable<DateTime> GetPriceMapApplicableFrom(int versionId, int geographyId, int productId, int gprmRuleTypeId)
        {
            const string query =
                "SELECT ApplicableFrom FROM GetPriceMapApplicableFrom(@geographyId,@productId,@gprmRuleTypeId,@versionId)";
            return GetApplicableFrom(query, geographyId, productId, gprmRuleTypeId, versionId);
        }
        public IEnumerable<DateTime> GetPriceMapApplicableFrom(int versionId, List<int> geographyId, int productId, int gprmRuleTypeId)
        {
            const string query =
                "SELECT ApplicableFrom FROM GetPriceMapApplicableFrom(@geographyId,@productId,@gprmRuleTypeId,@versionId)";
            return GetApplicableFrom(query, 0, productId, gprmRuleTypeId, versionId); // TODO: Jonathan, this must be improved
        }
        public IEnumerable<DateTime> GetRuleApplicableFrom(int versionId, int geographyId, int productId, int gprmRuleTypeId)
        {
            const string query =
                "SELECT ApplicableFrom FROM GetGprmRuleApplicableFrom(@geographyId,@productId,@gprmRuleTypeId,@versionId)";
            var result = GetApplicableFrom(query, geographyId, productId != 0 ? productId : ApplicationConstants.NotSpecifiedProduct, gprmRuleTypeId, versionId).ToList();
            return result.Count() != 0 ? result : new List<DateTime> {new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)};
        }
        public void SaveVersionRule(RuleViewModel ruleCachegptCache)
        {
            var versionId = versionRepository.CreateNewVersion("Data");
            SaveRule(ruleCachegptCache, versionId, ApplicationConstants.ImportedDataSaveId);
        }
        public void SaveVersionRule(RuleViewModel ruleCachegptCache, int saveId)
        {
            var versionId = versionRepository.CreateNewVersion("Data");
            SaveRule(ruleCachegptCache, versionId, saveId);
        }
        public void SavePriceMap(List<PriceMapViewModel> priceMap)
        {
            var versionId = versionRepository.CreateNewVersion("Data");
            foreach (var pM in priceMap)
            {
                SavePriceMap(pM, versionId, ApplicationConstants.ImportedDataSaveId);
            }
        }

        public void SavePriceMap(List<PriceMapViewModel> priceMap, int saveId)
        {
            var versionId = versionRepository.CreateNewVersion("Data");
            foreach (var pM in priceMap)
            {
                SavePriceMap(pM, versionId, saveId);
            }
        }
        public void SaveVersionRule(List<RuleViewModel> rule, int saveId)
        {
            var versionId = versionRepository.CreateNewVersion("Data");
            foreach (var r in rule)
            {
                SaveRule(r, versionId, saveId);
            }
        }

        private static void SavePriceMap(PriceMapViewModel priceMap, int versionId, int saveId)
        {
            var priceMapId = new RulegptIdentifier
            {
                GeographyId = priceMap.GeographyId,
                ProductId = priceMap.ProductId != 0 ? priceMap.ProductId : ApplicationConstants.NotSpecifiedProduct,
                GprmRuleTypeId = priceMap.GprmRuleTypeId
            };
            var applicableFrom = priceMap.ApplicableFrom;
            if (priceMap.Edited)
                InsertSaveReviewed(priceMap, versionId, saveId);

            var basketTable = priceMap.ReferencedData.Where(dB => dB.Edited).Select(dB => AddSaveBasket(priceMapId, applicableFrom, ApplicationConstants.NotSpecifiedSubRuleIndex, dB, versionId, saveId)).ToList();
            if (basketTable.Count != 0)
                InsertSaveBasket(basketTable);
        }

        private int CheckRule(RuleViewModel rule)
        {
            var mathTypes = dimensionRepository.GetMathType().ToList();
            var math = mathTypes.Where(mT => mT.Id == rule.GprmMathId).Select(mT => mT.ShortName).First();
            var allCalculation = new LoadRepository().GetAllRuleCalculation().ToList();
            var ruleCalculation = allCalculation.Where(s => s.IrpMathId == rule.GprmMathId && s.UpId == null).ToList();
            var foundRule = true;
            var irpRuleListId = (int)0;
            if (rule.ReferencedData.Count(r => r.Active) == 1)
            {
                var singleRule = ruleCalculation.Where(r => allCalculation.All(sR => sR.UpId != r.Id)).ToList();
                irpRuleListId = singleRule.Where(s => s.IrpMathId == rule.GprmMathId).Select(s => s.IrpRuleListId).First();
            }
            else
            {
                var subRules = rule.ReferencedData.Where(sR => sR.SubRuleIndex > 0 && sR.Active).ToList();
                var newRuleCalculation = new List<IrpRuleCalculationViewModel> { };
                foreach (var r in ruleCalculation)
                {
                    var subRuleCalculation = allCalculation.Where(s => s.UpId == r.Id).ToList();
                    foundRule = true;
                    if (subRules.Count == subRuleCalculation.Count)
                    {
                        foreach (var sR in subRuleCalculation)
                        {
                            foundRule = foundRule && rule.ReferencedData.Any(rD => rD.GprmMathId == sR.IrpMathId);
                        }
                    }
                    else
                        foundRule = false;
                    if (foundRule)
                    {
                        irpRuleListId = r.IrpRuleListId;
                        break;
                    }
                }
                
                
                if (!foundRule)
                {
                    math = math + "[ ";
                    foreach (var rD in subRules)
                    {
                        math = math + mathTypes.Where(mT => mT.Id == rD.GprmMathId).Select(mT => mT.ShortName).First() + "(" + rD.Argument + ") ";
                    }
                    math = math + "]";

                    var queryString = "INSERT INTO dbo.IrpRule " +
                                 "(Description, Active) " +
                                 "OUTPUT INSERTED.ID " +
                                 "VALUES " +
                                 "(@description, @active) ";
                    var dictionnary = new Dictionary<string, object> { 
                        { "description", math },
                        { "active", true }};

                    irpRuleListId =  RequestHelper.ExecuteScalarRequest<int>(DataBaseConstants.ConnectionString, queryString, dictionnary);
                    queryString = @"INSERT INTO dbo.IrpRuleCalculation (IrpRuleListId, UpId, IrpMathId, Argument) OUTPUT INSERTED.ID VALUES (@irpRuleListId, @upId, @irpMathId, @argument)";
                    dictionnary = new Dictionary<string, object> { 
                        { "irpRuleListId", irpRuleListId },
                        { "upId", DBNull.Value },
                        { "irpMathId", rule.GprmMathId},
                        { "argument", 0}};
                    var upId = RequestHelper.ExecuteScalarRequest<int>(DataBaseConstants.ConnectionString, queryString, dictionnary);
                    foreach (var rD in subRules)
                    {
                        newRuleCalculation.Add(new IrpRuleCalculationViewModel
                        {
                                IrpRuleListId = irpRuleListId,
                                IrpMathId = rD.GprmMathId,
                                Argument = rD.Argument,
                                UpId = upId
                        });
                    }
                    var dataBaseData = newRuleCalculation.Select(d => new
                    {
                        Id = 0,
                        d.IrpRuleListId,
                        d.UpId,
                        d.IrpMathId,
                        d.Argument
                    }).ToList();

                    var datatable = dataBaseData.ToDataTable();

                    RequestHelper.BulkInsert(DataBaseConstants.ConnectionString, "dbo.IrpRuleCalculation", datatable);
                }

            }
            return irpRuleListId;
        }
        private void SaveRule(RuleViewModel rule, int versionId, int saveId)
        {
            var tmpRuleListId = CheckRule(rule);
            if (rule.IrpRuleListId != tmpRuleListId)
            {
                rule.IrpRuleListId = tmpRuleListId;
                rule.Edited = true;
            }
            var products = rule.ProductId != 0 ? new List<int> { rule.ProductId } : new List<int>();
            if (rule.ProductId == 0)
                products.AddRange(productRepository.GetAllProducts().Select(p => p.Id));

            var applicableFrom = rule.ApplicableFrom;

            foreach (var p in products)
            {
                var ruleId = new RulegptIdentifier
                {
                    GeographyId = rule.GeographyId,
                    ProductId = p != 0 ? p : ApplicationConstants.NotSpecifiedProduct,
                    GprmRuleTypeId = rule.GprmRuleTypeId
                };

                if (rule.Edited)
                {
                    if (p != ApplicationConstants.NotSpecifiedProduct && rule.ProductId == 0)
                        InsertSaveRule(rule, versionId, saveId, ruleId, false);
                    else
                        InsertSaveRule(rule, versionId, saveId, ruleId, true);
                }
                    
                var subRuleTable = new List<SubRuleTable>();
                var basketTable = new List<BasketTable>();

                var defaultBasket = rule.DefaultBasket.Where(dB => dB.Edited).ToList();
                basketTable.AddRange(defaultBasket.Select(dB => AddSaveBasket(ruleId, applicableFrom, ApplicationConstants.NotSpecifiedSubRuleIndex, dB, versionId, saveId)));

                var subRules = rule.ReferencedData;
                foreach (var sR in subRules.Where(sR => sR.SubRuleIndex != 0))
                {
                    var editedBasket = sR.Basket.Where(dB => dB.Edited).Select(dB => dB.ReferencedGeographyId).ToList();
                    basketTable.AddRange(defaultBasket.Where(dB => !editedBasket.Contains(dB.ReferencedGeographyId)).Select(dB => AddSaveBasket(ruleId, applicableFrom, sR.SubRuleIndex, dB, versionId, saveId)));
                }
                foreach (var subRuleBasket in subRules.Select(sR => SaveSubRule(ruleId, applicableFrom, sR, versionId, saveId)))
                {
                    if (subRuleBasket.Item1 != null)
                        subRuleTable.Add(subRuleBasket.Item1);
                    basketTable.AddRange(subRuleBasket.Item2);
                }
                if (subRuleTable.Count != 0)
                    InsertSaveSubRule(subRuleTable);
                if (basketTable.Count != 0)
                    InsertSaveBasket(basketTable);
            }
        }

        private static Tuple<SubRuleTable,List<BasketTable>> SaveSubRule(IRulegptIdentifier rulegptId, DateTime applicableFrom, SubRuleViewModel subRule, int versionId, int saveId)
        {
            SubRuleTable subRuleTable;
            var basket = subRule.Basket;

            if (subRule.Edited)
                subRuleTable = AddSaveSubRule(rulegptId, applicableFrom, subRule, versionId, saveId);
            else
                subRuleTable = null;
            if (!subRule.Active)
                basket.ForEach(b =>
                {
                    b.Active = false;
                    b.Edited = true;
                } );
            var basketTable = basket.Where(b => b.Edited).Select(b => AddSaveBasket(rulegptId, applicableFrom, subRule.SubRuleIndex, b, versionId, saveId)).ToList();
            return new Tuple<SubRuleTable, List<BasketTable>>(subRuleTable, basketTable);
        }
        private static SubRuleTable AddSaveSubRule(IRulegptIdentifier rulegptId, DateTime applicableFrom,
            SubRuleViewModel subRule, int versionId, int saveId)
        {
            return new SubRuleTable
            {
                GeographyId = rulegptId.GeographyId,
                ProductId = rulegptId.ProductId,
                SubRuleIndex = subRule.SubRuleIndex,
                VersionId = versionId,
                GprmRuleTypeId = rulegptId.GprmRuleTypeId,
                GprmMathId = subRule.GprmMathId,
                Argument = subRule.Argument,
                WeightTypeId = subRule.WeightTypeId,
                SaveTypeId = (int)SaveTypes.Data,
                SaveId = saveId,
                Active = subRule.Active,
                ApplicableFrom = applicableFrom
            };
        }

        private static BasketTable AddSaveBasket(IRulegptIdentifier rulegptId, DateTime applicableFrom, int subRuleIndex, Basket basket, int versionId, int saveId)
        {
            return new BasketTable
            {
                GeographyId = rulegptId.GeographyId,
                ProductId = rulegptId.ProductId,
                SubRuleIndex = subRuleIndex,
                ReferencedGeographyId = basket.ReferencedGeographyId,
                VersionId = versionId,
                GprmRuleTypeId = rulegptId.GprmRuleTypeId,
                ReferencedPriceTypeId = basket.ReferencedPriceTypeId != 0 ? basket.ReferencedPriceTypeId : ApplicationConstants.NotSpecifiedPrice,
                ReferencedPriceAdjustment = basket.ReferencedPriceAdjustment,
                SaveId = saveId,
                SaveTypeId = (int)SaveTypes.Data,
                Active = basket.Active,
                ApplicableFrom = applicableFrom
            };
        }
        private static BasketTable AddSaveBasket(IRulegptIdentifier rulegptId, DateTime applicableFrom, int subRuleIndex, GprmReferencedPriceViewModel basket, int versionId, int saveId)
        {
            return new BasketTable
            {
                GeographyId = rulegptId.GeographyId,
                ProductId = rulegptId.ProductId,
                SubRuleIndex = subRuleIndex,
                ReferencedGeographyId = basket.ReferencedGeographyId,
                VersionId = versionId,
                GprmRuleTypeId = rulegptId.GprmRuleTypeId,
                ReferencedPriceTypeId = basket.ReferencedPriceTypeId,
                ReferencedPriceAdjustment = basket.ReferencedPriceAdjustment,
                SaveId = saveId,
                SaveTypeId = (int)SaveTypes.Data,
                Active = basket.Active,
                ApplicableFrom = applicableFrom
            };
        }
        private static void InsertSaveRule(RuleViewModel rule, int versionId, int saveId, RulegptIdentifier ruleId, bool insertReviewed)
        {
            var gprmRule = new
                {
                    rule.GeographyId,
                    ruleId.ProductId,
                    VersionId = versionId,
                    rule.Regular,
                    rule.GprmMathId,
                    rule.Argument,
                    rule.WeightTypeId,
                    rule.IrpRuleListId,
                    rule.GprmRuleTypeId,
                    rule.LookBack,
                    rule.EffectiveLag,
                    rule.AllowIncrease,
                    SaveTypeId = (int)SaveTypes.Data,
                    SaveId = saveId,
                    rule.Active,
                    rule.ApplicableFrom
                };
            var gprmRuleTable = gprmRule.ToDataTableSingle();
            RequestHelper.BulkInsert(DataBaseConstants.ConnectionString, "GprmRule", gprmRuleTable);
            if (!insertReviewed) return;
            var gprmReviewedPrice = new
            {
                rule.GeographyId,
                ruleId.ProductId,
                VersionId = versionId,
                rule.GprmRuleTypeId,
                ReviewedPriceTypeId =
                    rule.ReviewedPriceTypeId != 0
                        ? rule.ReviewedPriceTypeId
                        : ApplicationConstants.NotSpecifiedPrice,
                rule.ReviewedPriceAdjustment,
                SaveId = saveId,
                SaveTypeId = (int) SaveTypes.Data,
                rule.Active,
                rule.ApplicableFrom
            };
            var gprmReviewedPriceTable = gprmReviewedPrice.ToDataTableSingle();
            RequestHelper.BulkInsert(DataBaseConstants.ConnectionString, "GprmReviewedPrice", gprmReviewedPriceTable);
        }

        public void SaveExcelMap(ExcelPriceMapViewModel priceMap)
        {
            var reviewed = priceMap.Reviewed;
            var referenced = priceMap.Referenced;
            var applicableFrom = priceMap.ApplicableFrom;
            var saveId = priceMap.SaveId;
            if (reviewed.Count == 0 && referenced.Count == 0)
                return;
            
            var versionId = versionRepository.CreateNewVersion("Data");

            if (reviewed.Count != 0)
            {
                var gprmReviewedPrice = reviewed.Select(r => new
                {
                    r.GeographyId,
                    r.ProductId,
                    VersionId = versionId,
                    r.GprmRuleTypeId,
                    r.ReviewedPriceTypeId,
                    r.ReviewedPriceAdjustment,
                    SaveId = saveId,
                    SaveTypeId = (int)SaveTypes.Data,
                    Active = true,
                    ApplicableFrom = applicableFrom
                }).ToList();
                var gprmReviewedPriceTable = gprmReviewedPrice.ToDataTable();
                RequestHelper.BulkInsert(DataBaseConstants.ConnectionString, "GprmReviewedPrice", gprmReviewedPriceTable);   
            }

            if (referenced.Count == 0) return;
            var referencedT = referenced.Select(b => new BasketTable
            {
                GeographyId = b.GeographyId,
                ProductId = b.ProductId,
                SubRuleIndex = b.SubRuleIndex,
                ReferencedGeographyId = b.ReferencedGeographyId,
                VersionId = versionId,
                GprmRuleTypeId = b.GprmRuleTypeId,
                ReferencedPriceTypeId = b.ReferencedPriceTypeId,
                ReferencedPriceAdjustment = b.ReferencedPriceAdjustment,
                SaveId = saveId,
                SaveTypeId = (int)SaveTypes.Data,
                Active = true,
                ApplicableFrom = applicableFrom
            }).ToList();
            InsertSaveBasket(referencedT);
        }

        private static void InsertSaveReviewed(PriceMapViewModel priceMap, int versionId, int saveId)
        {
            var gprmReviewedPrice = new
            {
                priceMap.GeographyId,
                priceMap.ProductId,
                VersionId = versionId,
                priceMap.GprmRuleTypeId,
                priceMap.ReviewedPriceTypeId,
                priceMap.ReviewedPriceAdjustment,
                SaveId = saveId,
                SaveTypeId = (int)SaveTypes.Data,
                priceMap.Active,
                priceMap.ApplicableFrom
            };
            var gprmReviewedPriceTable = gprmReviewedPrice.ToDataTableSingle();
            RequestHelper.BulkInsert(DataBaseConstants.ConnectionString, "GprmReviewedPrice", gprmReviewedPriceTable);
        }
        private static void InsertSaveSubRule(IList<SubRuleTable> subRule)
        {
            var gprmSubRuleTable = subRule.ToDataTable();
            RequestHelper.BulkInsert(DataBaseConstants.ConnectionString, "GprmSubRule", gprmSubRuleTable);
        }
        private static void InsertSaveBasket(IList<BasketTable> basket)
        {
            var gprmReferencedPriceTable = basket.ToDataTable();
            RequestHelper.BulkInsert(DataBaseConstants.ConnectionString, "GprmReferencedPrice", gprmReferencedPriceTable);
        }
        private static List<PriceMapViewModel> GetPriceMap(int saveId, int geographyId, int productId, int gprmRuleTypeId, int versionId)
        {
            var result = new List<PriceMapViewModel>();
            var gprmReviewedPrice = GetGprmReviewedPrice(saveId, geographyId, productId, gprmRuleTypeId, versionId).OrderBy(gBfS => gBfS.Geography);
            var gprmReferencedPrice = GetGprmReferencedPrice(saveId, geographyId, productId, gprmRuleTypeId, versionId);

            var applicableFrom = gprmReviewedPrice.Select(dRd => dRd.ApplicableFrom).Concat(gprmReferencedPrice.Select(dRd => dRd.ApplicableFrom)).Distinct().ToList();

            var geographies = new CountryRepository().GetAllCountries().Select(g => g.Id);
            var reviewedPricesForGeography = gprmReviewedPrice.ToLookup(r => r.GeographyId);
            var referencedPricesForGeography = gprmReferencedPrice.ToLookup(r => r.GeographyId);

            foreach (var g in geographies)
            {
                var gprmReviewed = reviewedPricesForGeography[g].ToList();
                var gprmReferenced = referencedPricesForGeography[g].ToList();
                foreach (var aF in applicableFrom)
                {
                    var reviewedAf = gprmReviewed.Count == 0 ? new DateTime() :
                        gprmReviewed.Where(gRp => gRp.ApplicableFrom <= aF).Select(gRp => gRp.ApplicableFrom).DefaultIfEmpty(new DateTime(4000, 12, 31)).Max();
                    var referencedAf = gprmReferenced.Count == 0 ? new DateTime() :
                        gprmReferenced.Where(gRp => gRp.ApplicableFrom <= aF).Select(gRp => gRp.ApplicableFrom).DefaultIfEmpty(new DateTime(4000, 12, 31)).Max();
                    var reviewed = gprmReviewed.Where(gRp => gRp.ApplicableFrom == reviewedAf);
                    var referenced = gprmReferenced.Where(gRp => gRp.ApplicableFrom == referencedAf && gRp.Active);
                    result.AddRange(reviewed.Select(rwd => new PriceMapViewModel
                    {
                        Geography = rwd.Geography,
                        ReviewedPriceType = rwd.ReviewedPriceType,
                        ApplicableFrom = aF,
                        GeographyId = rwd.GeographyId,
                        ProductId = rwd.ProductId,
                        GprmRuleTypeId = rwd.GprmRuleTypeId,
                        ReviewedPriceTypeId = rwd.ReviewedPriceTypeId,
                        ReviewedPriceAdjustment = rwd.ReviewedPriceAdjustment,
                        Default = rwd.Default,
                        ReferencedData = GetReferencedForReviewed(rwd, referenced, aF)
                    }));
                }
            }
            return result;
        }
        private static List<RuleViewModel> GetRule(int saveId, int geographyId, int productId, int gprmRuleTypeId, int versionId, DateTime endTime)
        {
            var result = new List<RuleViewModel>();
            var gprm = new GprmContainer(
                GetGprmRule(saveId, geographyId, productId, gprmRuleTypeId, versionId).Where(d => d.ApplicableFrom < endTime).ToList(),
                GetGprmReviewedPrice(saveId, geographyId, productId, gprmRuleTypeId, versionId).Where(d => d.ApplicableFrom < endTime).ToList(),
                GetGprmSubRule(saveId, geographyId, productId, gprmRuleTypeId, versionId).ToList().Where(d => d.ApplicableFrom < endTime).ToList(),
                GetGprmReferencedPrice(saveId, geographyId, productId, gprmRuleTypeId, versionId).Where(d => d.ApplicableFrom < endTime).ToList());

            var geographies = gprm.GetDistinctGeographyId();
            foreach (var g in geographies)
            {
                var gRule = gprm.ReviewedPricesForGeography[g].ToLookup(rP => rP.ApplicableFrom);
                var gRulesLookup = gprm.RulesForGeography[g].ToLookup(rP => rP.ApplicableFrom);
                var gSubRulesLookup = gprm.SubRulesForGeography[g].ToLookup(rP => rP.ApplicableFrom);
                var gReferencedPricesLookup = gprm.ReferencedPricesForGeography[g].ToLookup(rP => rP.ApplicableFrom);
                var gApplicationFrom =
                    gRule.Select(d => d.Key)
                        .Concat(gRulesLookup.Select(d => d.Key))
                        .Concat(gSubRulesLookup.Select(d => d.Key))
                        .Concat(gReferencedPricesLookup.Select(d => d.Key)).Distinct().ToList();

                foreach (var aF in gApplicationFrom)
                {
                    var ruleAf = gRulesLookup.Count == 0 ? new DateTime() : gprm.RulesForGeography[g].Where(gRp => gRp.ApplicableFrom <= aF).Select(gRp => gRp.ApplicableFrom).DefaultIfEmpty(new DateTime(4000, 12, 31)).Max();
                    var reviewedAf = gRule.Count == 0 ? new DateTime() : gprm.ReviewedPricesForGeography[g].Where(gRp => gRp.ApplicableFrom <= aF).Max(gRp => gRp.ApplicableFrom);
                    var subRuleAf = gSubRulesLookup.Count == 0 ? new DateTime() : gprm.SubRulesForGeography[g].Where(gRp => gRp.ApplicableFrom <= aF).Select(gRp => gRp.ApplicableFrom).DefaultIfEmpty(new DateTime(4000, 12, 31)).Max();
                    var referencedAf = gReferencedPricesLookup.Count == 0 ? new DateTime() : gprm.ReferencedPricesForGeography[g].Where(gRp => gRp.ApplicableFrom <= aF).Select(gRp => gRp.ApplicableFrom).DefaultIfEmpty(new DateTime(4000, 12, 31)).Max();

                    var gprmRuleDataEqualityComparer = new GprmRuleDataEqualityComparer();
                    var rule = gRule[reviewedAf];
                    var rulesLookup = gRulesLookup[ruleAf].ToLookup(rP => rP, gprmRuleDataEqualityComparer);
                    var subRulesLookup = gSubRulesLookup[subRuleAf].ToLookup(rP => rP, gprmRuleDataEqualityComparer);
                    var referencedPricesLookup = gReferencedPricesLookup[referencedAf].ToLookup(rP => rP, gprmRuleDataEqualityComparer);
                    result.AddRange(from rwd in rule
                                    let r = rulesLookup[rwd].FirstOrDefault()
                                    let sR = subRulesLookup[rwd]
                                    let red = referencedPricesLookup[rwd]
                                    select new RuleViewModel
                                    {
                                        Geography = rwd != null ? rwd.Geography : "",
                                        ReviewedPriceType = rwd != null ? rwd.ReviewedPriceType : "",
                                        ApplicableFrom = aF,
                                        GeographyId = rwd.GeographyId,
                                        ProductId = rwd.ProductId,
                                        GprmRuleTypeId = rwd.GprmRuleTypeId,
                                        ReviewedPriceTypeId = rwd != null ? rwd.ReviewedPriceTypeId : ApplicationConstants.NotSpecifiedPrice,
                                        ReviewedPriceAdjustment = rwd != null ? rwd.ReviewedPriceAdjustment : 0,
                                        Regular = r == null || r.Regular,
                                        GprmMathId = r != null ? r.GprmMathId : (int)MathTypes.NotSpecified,
                                        Argument = r != null ? r.Argument : 0,
                                        WeightTypeId = r != null ? r.WeightTypeId : ApplicationConstants.DefaultWeightTypeId,
                                        IrpRuleListId = r != null ? r.IrpRuleListId : 0,
                                        LookBack = r != null ? r.LookBack : 0,
                                        EffectiveLag = r != null ? r.EffectiveLag : 0,
                                        AllowIncrease = r == null || r.AllowIncrease,
                                        Default = rwd.Default && (r == null || r.Default),
                                        DefaultBasket = GetBasketForSubRule(0, red.ToList()).OrderBy(gBfS => gBfS.ReferencedGeography).ToList(),
                                        ReferencedData = GetSubRuleForRule(sR.ToList(), red.ToList(), r)
                                    });
                }
            }
            return result;
        }
        private static List<GprmReferencedPriceViewModel> GetReferencedForReviewed(IGprmRuleDataViewModel reviewed, IEnumerable<GprmReferencedPriceViewModel> referenced, DateTime aF)
        {
            var red = referenced.Where(r =>
                r.GeographyId == reviewed.GeographyId &&
                r.ProductId == reviewed.ProductId &&
                r.GprmRuleTypeId == reviewed.GprmRuleTypeId &&
                r.SubRuleIndex == 0);
            var result = red.Select(r => new GprmReferencedPriceViewModel
            {
                Geography = r.Geography,
                ReferencedGeography = r.ReferencedGeography,
                ReferencedPriceType = r.ReferencedPriceType,
                ApplicableFrom = aF,
                GeographyId = r.GeographyId,
                ProductId = r.ProductId,
                SubRuleIndex = r.SubRuleIndex,
                GprmRuleTypeId = r.GprmRuleTypeId,
                ReferencedGeographyId = r.ReferencedGeographyId,
                ReferencedPriceTypeId = r.ReferencedPriceTypeId,
                ReferencedPriceAdjustment = r.ReferencedPriceAdjustment,
                Default = r.Default
            }).OrderBy(gBfS => gBfS.ReferencedGeography).ToList();
            return result;
        }
        private static List<SubRuleViewModel> GetSubRuleForRule(List<GprmSubRuleViewModel> subRule, List<GprmReferencedPriceViewModel> referenced, GprmRuleViewModel rule)
        {
            var result = new List<SubRuleViewModel>();
            if (subRule != null && subRule.Any())
                result.AddRange(subRule.Select(sR => new SubRuleViewModel
                {
                    SubRuleIndex = sR.SubRuleIndex,
                    GprmMathId = sR.GprmMathId,
                    Argument = sR.Argument,
                    WeightTypeId = sR.WeightTypeId,
                    Default = sR.Default,
                    Basket = GetBasketForSubRule(sR.SubRuleIndex, referenced).OrderBy(gBfS => gBfS.ReferencedGeography).ToList(),
                    Active = sR.Active
                }));
            else
            {
                result.Add(new SubRuleViewModel
                {
                    SubRuleIndex = 0,

                    GprmMathId = rule != null ? rule.GprmMathId : (int)MathTypes.NotSpecified,
                    Argument = rule != null ? rule.Argument : 0,
                    WeightTypeId = rule != null ? rule.WeightTypeId : ApplicationConstants.DefaultWeightTypeId,
                    Default = rule == null || rule.Default,
                    Basket = GetBasketForSubRule(0, referenced).OrderBy(gBfS => gBfS.ReferencedGeography).ToList()
                });
            }
            return result.Where(r => r.Active).ToList();
        }
        private static List<Basket> GetBasketForSubRule(int subRuleIndex, List<GprmReferencedPriceViewModel> referenced)
        {
            var result = new List<Basket>();

            var rIndex = referenced.Where(rA => rA.SubRuleIndex == subRuleIndex).Select(rA => new Basket
            {
                ReferencedGeography = rA.ReferencedGeography,
                ReferencedPriceType = rA.ReferencedPriceType,
                ReferencedGeographyId = rA.ReferencedGeographyId,
                ReferencedPriceTypeId = rA.ReferencedPriceTypeId,
                ReferencedPriceAdjustment = rA.ReferencedPriceAdjustment,
                Default = false,
                Active = rA.Active
            }).ToList();
            var rDefaultAll = referenced.Where(rA => rA.SubRuleIndex == 0).Select(rA => new Basket
            {
                ReferencedGeography = rA.ReferencedGeography,
                ReferencedPriceType = rA.ReferencedPriceType,
                ReferencedGeographyId = rA.ReferencedGeographyId,
                ReferencedPriceTypeId = rA.ReferencedPriceTypeId,
                ReferencedPriceAdjustment = rA.ReferencedPriceAdjustment,
                Default = true,
                Active = rA.Active
            }).ToList();
            var rDefault = rDefaultAll.Where(rD => rIndex.All(rI => rI.ReferencedGeographyId != rD.ReferencedGeographyId)).ToList();
            result.AddRange(rIndex);
            result.AddRange(rDefault);

            return result.Where(r => r.Active).ToList();
        }
        private static List<GprmRuleViewModel> GetGprmRule(int saveId, int geographyId, int productId, int gprmRuleTypeId, int versionId)
        {
            const string query =
                "SELECT * FROM " +
                "GetGprmRule" +
                "(@saveId,@geographyId,@productId,@gprmRuleTypeId,@versionId)";
            return GetRuleData(query, MapRule, AddRule, saveId, geographyId, productId, gprmRuleTypeId,
                versionId);
        }
        private static List<GprmReviewedPriceViewModel> GetGprmReviewedPrice(int saveId, int geographyId,
            int productId, int gprmRuleTypeId, int versionId)
        {
            const string query =
                "SELECT * FROM " +
                "GetGprmReviewedPrice" +
                "(@saveId,@geographyId,@productId,@gprmRuleTypeId,@versionId)";
            return GetRuleData(query, MapReviewedPrice, AddReviewedPrice, saveId, geographyId, productId, gprmRuleTypeId, versionId);
        }
        private static IEnumerable<GprmSubRuleViewModel> GetGprmSubRule(int saveId, int geographyId, int productId, int gprmRuleTypeId, int versionId)
        {
            const string query =
                "SELECT * FROM " +
                "GetGprmSubRule" +
                "(@saveId,@geographyId,@productId,@gprmRuleTypeId,@versionId)";
            return GetRuleData(query, MapSubRule, AddSubRule, saveId, geographyId, productId, gprmRuleTypeId, versionId).OrderBy(rD => rD.SubRuleIndex);
        }
        private static List<GprmReferencedPriceViewModel> GetGprmReferencedPrice(int saveId, int geographyId, int productId, int gprmRuleTypeId, int versionId)
        {
            const string query =
                "SELECT * FROM " +
                "GetGprmReferencedPrice" +
                "(@saveId,@geographyId,@productId,@gprmRuleTypeId,@versionId)";
            return GetRuleData(query, MapReferencedPrice, AddReferencedPrice, saveId, geographyId, productId, gprmRuleTypeId, versionId);
        }
        private static List<T> GetRuleData<T>(string query, Func<DataRow, T> mapRuleData, Func<List<T>, List<T>, int, int, DateTime, List<T>> addRuleData, int saveId, int geographyId, int productId, int gprmRuleTypeId, int versionId) where T : IGprmRuleDataViewModel
        {
            var connectionString = DataBaseConstants.ConnectionString;
            var result = RequestHelper.ExecuteQuery(
                    connectionString,
                    query,
                    mapRuleData,
                    new Dictionary<string, object>
                    {
                        {"saveId",saveId},
                        {"geographyId",geographyId},
                        {"productId",productId},
                        {"gprmRuleTypeId",gprmRuleTypeId},
                        {"versionId",versionId}
                    }
                ).ToList();
            var filledResult = FillRuleData(result, addRuleData, new List<int> { productId }, gprmRuleTypeId);
            return filledResult;
        }
        public List<RuleTypeViewModel> GetRuleTypes()
        {
            const string query = "SELECT Id, Name FROM RuleType";

            var result =
                RequestHelper.ExecuteQuery(DataBaseConstants.ConnectionString, query, row => new RuleTypeViewModel
                {
                    Id = (int)row["Id"],
                    Name = (string)row["Name"]
                }).ToList();

            return result;
        }
        private static IEnumerable<DateTime> GetApplicableFrom(string query, int geographyId, int productId, int gprmRuleTypeId, int versionId)
        {
            var connectionString = DataBaseConstants.ConnectionString;
            var result = RequestHelper.ExecuteQuery<DateTime>(
                    connectionString,
                    query,
                    MapApplicableFrom,
                    new Dictionary<string, object>
                    {
                        {"geographyId",geographyId},
                        {"productId",productId},
                        {"gprmRuleTypeId",gprmRuleTypeId},
                        {"versionId",versionId}
                    }
                );
            return result.OrderByDescending(r => r.Date);
        }
        private static List<T> FillRuleData<T>(List<T> dbRuleData, Func<List<T>, List<T>, int, int, DateTime, List<T>> addRuleData, List<int> productId, int gprmRuleTypeId) where T : IGprmRuleDataViewModel
        {
            var ruleData = new List<T>();

            if (dbRuleData.Count == 0)
                return ruleData;

            var defaultRuleData =
                dbRuleData.Where(dRd => dRd.GprmRuleTypeId == (int)GprmRuleTypes.Default && dRd.ProductId == 0).ToList();

            var launchRuleData =
                dbRuleData.Where(dRd => dRd.GprmRuleTypeId == (int)GprmRuleTypes.Launch && dRd.ProductId == 0).ToList();

            var geography = dbRuleData.Select(dRd => dRd.GeographyId).Distinct().ToList();

            var minApplicableFrom = dbRuleData.Min(dRd => dRd.ApplicableFrom);

            if (!productId.Any() || (productId.Count == 1 && productId.First() == 0))
            {
                var products = new ProductRepository();
                productId = products.GetAllProducts().Select(gAp => gAp.Id).Distinct().ToList();
            }

            if (productId.Count == 1 && productId.First() == ApplicationConstants.NotSpecifiedProduct)
                productId = new List<int> {0};

            foreach (var gg in geography)
            {
                var g = gg;
                var defaultRule00 = defaultRuleData.Where(dRd => dRd.GeographyId == g && dRd.ApplicableFrom == minApplicableFrom && dRd.Active).ToList();
                foreach (var pp in productId)
                {
                    var p = pp;
                    var applicableFrom = dbRuleData
                        .Where(rD => (rD.GeographyId == g && rD.ProductId == 0) || (rD.GeographyId == g && rD.ProductId == p))
                        .Select(dRd => dRd.ApplicableFrom).Distinct();
                    var defaultRule0 = defaultRule00;

                    var productDefaultRule0 = dbRuleData.Where(rD => rD.GeographyId == g && rD.ProductId == p && rD.GprmRuleTypeId == (int)GprmRuleTypes.Default && rD.ApplicableFrom == minApplicableFrom && rD.Active).ToList();
                    foreach (var ap in applicableFrom.OrderBy(aF => aF.Date))
                    {
                        var defaultRule = defaultRuleData.Where(dRd => dRd.GeographyId == g && dRd.ApplicableFrom == ap).ToList();
                        var launchRule = launchRuleData.Where(dRd => dRd.GeographyId == g && dRd.ApplicableFrom == ap).ToList();
                        defaultRule.AddRange(addRuleData(defaultRule0, defaultRule, (int)GprmRuleTypes.Default, ApplicationConstants.NotSpecifiedProduct, ap));

                        var productDefaultRule = dbRuleData.Where(rD => rD.GeographyId == g && rD.ProductId == p && rD.GprmRuleTypeId == (int)GprmRuleTypes.Default && rD.ApplicableFrom == ap).ToList();
                        productDefaultRule.AddRange(addRuleData(productDefaultRule0, productDefaultRule, (int)GprmRuleTypes.Default, p, ap));
                        productDefaultRule.AddRange(addRuleData(defaultRule, productDefaultRule, (int)GprmRuleTypes.Default, p, ap));
                        
                        productDefaultRule0 = productDefaultRule.Where(pDr => pDr.Active).ToList();

                        if (gprmRuleTypeId == (int)GprmRuleTypes.Default || gprmRuleTypeId == 0)
                            ruleData.AddRange(productDefaultRule);

                        if (gprmRuleTypeId == (int)GprmRuleTypes.Launch || gprmRuleTypeId == 0)
                        {
                            var productLaunchRule = dbRuleData.Where(rD => rD.GeographyId == g && rD.ProductId == p && rD.GprmRuleTypeId == (int)GprmRuleTypes.Launch && rD.ApplicableFrom == ap).ToList();
                            productLaunchRule.AddRange(addRuleData(launchRule, productLaunchRule, (int)GprmRuleTypes.Launch, p, ap));
                            productLaunchRule.AddRange(addRuleData(productDefaultRule, productLaunchRule, (int)GprmRuleTypes.Launch, p, ap));
                            ruleData.AddRange(productLaunchRule);
                        }

                        defaultRule0 = defaultRule.Where(dR => dR.Active).ToList();
                    }
                }
            }
            return ruleData;
        }
        private static GprmRuleViewModel MapRule(DataRow row)
        {
            var data = new GprmRuleViewModel
            {
                ApplicableFrom = (DateTime)row["ApplicableFrom"],
                GprmRuleTypeId = (int)row["GprmRuleTypeId"],
                GeographyId = (int)row["GeographyId"],
                ProductId = (int)row["ProductId"],
                Regular = (bool)row["Regular"],
                GprmMathId = (int)row["GprmMathId"],
                Argument = (int)row["Argument"],
                WeightTypeId = (int)row["WeightTypeId"],
                IrpRuleListId = (int)row["IrpRuleListId"],
                LookBack = (int)row["LookBack"],
                EffectiveLag = (int)row["EffectiveLag"],
                AllowIncrease = (bool)row["AllowIncrease"],
                Default = false,
                Active = (bool)row["Active"]
            };
            return data;
        }
        public PriceMapSearchResponse BuildGprmRuleResponse(List<int> countriesId, int productId, int gprmRuleTypeId, int versionId, DateTime applicableFrom, int pageNumber, int itemsPerPage)
        {
            var table = new GprmRuleDataTableViewModel
            {
                Rows = new List<GprmRuleRowViewModel>()
            };

            var priceMap = GetVersionPriceMap(versionId, 0, productId, gprmRuleTypeId, applicableFrom)
                .Where(pm => countriesId.Contains(pm.GeographyId))
                .OrderBy(p => p.Geography)
                .ToList();
           
            var totalReviewedPrices = priceMap.Count();

            priceMap = priceMap.Skip(pageNumber * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            var allCountries = countriesId;
            foreach (var pM in priceMap)
            {
                allCountries.AddRange(pM.ReferencedData.Where(rD => !allCountries.Contains(rD.ReferencedGeographyId)).Select(rD => rD.ReferencedGeographyId));
            }

            var listOfPriceTypes = priceTypeRepository.GetPriceTypesProduct(countriesId, productId, applicableFrom);
            
            foreach (var reviewedPrice in priceMap)
            {   
                var reviewedPriceTypeOptions = listOfPriceTypes.Where(pT => pT.GeographyId == reviewedPrice.GeographyId).ToList();
                var reviewedPriceText = reviewedPrice.Geography;

                if (reviewedPrice.ReviewedPriceType != "XX")
                    reviewedPriceText += " - " + reviewedPrice.ReviewedPriceType;

                if (reviewedPrice.ReviewedPriceAdjustment > 0)
                    reviewedPriceText += " (" + reviewedPrice.ReviewedPriceAdjustment * 100+ "%)";

                table.Rows.Add(new GprmRuleRowViewModel
                {                    
                    Cells = new List<GprmRuleCellViewModel>
                    {
                        new GprmRuleCellViewModel
                        {                 
                            PriceTypeOptions = reviewedPriceTypeOptions,
                            IsEditable = true,
                            Text = reviewedPriceText,                          
                            ReviewedPrice = new GprmReviewedPriceViewModel
                            {
                                ApplicableFrom = reviewedPrice.ApplicableFrom,
                                Default = reviewedPrice.Default,
                                Geography = reviewedPrice.Geography,
                                GeographyId = reviewedPrice.GeographyId,
                                GprmRuleTypeId = reviewedPrice.GprmRuleTypeId,
                                ProductId = reviewedPrice.ProductId,
                                ReviewedPriceAdjustment = reviewedPrice.ReviewedPriceAdjustment,
                                ReviewedPriceType = reviewedPrice.ReviewedPriceType,
                                ReviewedPriceTypeId = reviewedPrice.ReviewedPriceTypeId
                            }
                        },
                        new GprmRuleCellViewModel
                        {
                            IsEditable = false,
                            Text = "",                            
                        }
                    }
                });

                foreach (var referencedPrice in reviewedPrice.ReferencedData.OrderBy(p => p.ReferencedGeography))
                {
                    var referencedPriceTypeOptions =
                        listOfPriceTypes.Where(pT => pT.GeographyId == referencedPrice.ReferencedGeographyId).ToList();
                    var text = referencedPrice.ReferencedGeography;

                    if (referencedPrice.ReferencedPriceType != "XX")
                        text += " - " + referencedPrice.ReferencedPriceType;

                    if (referencedPrice.ReferencedPriceAdjustment > 0)
                        text += " (" + referencedPrice.ReferencedPriceAdjustment * 100+ "%)";

                    table.Rows.Add(new GprmRuleRowViewModel
                    {                        
                        ReviewedPrice = new GprmReviewedPriceViewModel
                        {
                            ApplicableFrom = reviewedPrice.ApplicableFrom,
                            Default = reviewedPrice.Default,
                            Geography = reviewedPrice.Geography,
                            GeographyId = reviewedPrice.GeographyId,
                            GprmRuleTypeId = reviewedPrice.GprmRuleTypeId,
                            ProductId = reviewedPrice.ProductId,
                            ReviewedPriceAdjustment = reviewedPrice.ReviewedPriceAdjustment,
                            ReviewedPriceType = reviewedPrice.ReviewedPriceType,
                            ReviewedPriceTypeId = reviewedPrice.ReviewedPriceTypeId
                        },
                        Cells = new List<GprmRuleCellViewModel>
                        {                            
                            new GprmRuleCellViewModel
                            {
                                IsEditable = false,
                                Text = "",   
                            },
                            new GprmRuleCellViewModel
                            {
                                PriceTypeOptions = referencedPriceTypeOptions,
                                IsEditable = true,
                                Text = text,
                                ReferencedPrice = new GprmReferencedPriceViewModel
                                {
                                    ApplicableFrom = referencedPrice.ApplicableFrom,
                                    Active = referencedPrice.Active,
                                    Default = referencedPrice.Default,
                                    Edited = referencedPrice.Edited,
                                    Geography = referencedPrice.Geography,
                                    GeographyId = referencedPrice.GeographyId,
                                    GprmRuleTypeId = referencedPrice.GprmRuleTypeId,
                                    ProductId = referencedPrice.ProductId,
                                    ReferencedGeographyId = referencedPrice.ReferencedGeographyId,
                                    ReferencedPriceAdjustment = referencedPrice.ReferencedPriceAdjustment,
                                    ReferencedPriceTypeId = referencedPrice.ReferencedPriceTypeId,
                                    SubRuleIndex = referencedPrice.SubRuleIndex,
                                    ReferencedGeography = referencedPrice.ReferencedGeography,
                                    ReferencedPriceType = referencedPrice.ReferencedPriceType
                                }
                            }
                        }
                    });
                }
            }

            return new PriceMapSearchResponse
            {
                DataTable = table,
                IsLastPage = priceMap.Count() + (pageNumber * itemsPerPage) >= totalReviewedPrices,
                PageNumber = ++pageNumber,
                TotalPriceMap = totalReviewedPrices
            };
        }
        public RuleDefinitionViewModel GetRules(int versionId, int geographyId, int productId, int gprmRuleTypeId, DateTime applicableFrom)
        {
            var ruleDefinition = new RuleDefinitionViewModel();
            var rules = GetVersionRule(versionId, geographyId, productId != 0 ? productId : ApplicationConstants.NotSpecifiedProduct, gprmRuleTypeId, applicableFrom).ToList();
            
            ruleDefinition.ReviewedPriceTypeOptions = priceTypeRepository.GetPriceTypes(geographyId, productId, applicableFrom).ToList();
            ruleDefinition.ReferencedData = new List<SubRuleViewModel>();

            var rule = rules.FirstOrDefault();
            
            if (rule != null)
            {
                ruleDefinition.IrpRuleListId = rule.IrpRuleListId;
                ruleDefinition.SelectedReviewedPriceTypeId = rule.ReviewedPriceTypeId;
                ruleDefinition.GprmMathId = rule.GprmMathId;
                ruleDefinition.Active = rule.Active;
                ruleDefinition.AllowIncrease = rule.AllowIncrease;
                ruleDefinition.Argument = rule.Argument;
                ruleDefinition.Default = rule.Default;
                ruleDefinition.EffectiveLag = rule.EffectiveLag;
                ruleDefinition.IrpRuleListId = rule.IrpRuleListId;
                ruleDefinition.WeightTypeId = rule.WeightTypeId;
                ruleDefinition.DefaultBasket = rule.DefaultBasket;
                ruleDefinition.LookBack = rule.LookBack;
                ruleDefinition.Adjustement = rule.ReviewedPriceAdjustment;

                if (rule.ReferencedData != null)
                {
                    ruleDefinition.ReferencedData.AddRange(rule.ReferencedData.Select(item => new SubRuleViewModel
                    {
                        Argument = item.Argument,
                        Basket = item.Basket,
                        Default = item.Default,
                        GprmMathId = item.GprmMathId,
                        SubRuleIndex = item.SubRuleIndex,
                        WeightTypeId = item.WeightTypeId,
                        Active = item.Active,                        
                    }));
                }
            }
            return ruleDefinition;
        }
        
        private static GprmReviewedPriceViewModel MapReviewedPrice(DataRow row)
        {
            var data = new GprmReviewedPriceViewModel
            {
                Geography = row["Geography"].PreventStringNull(),
                ReviewedPriceType = row["ReviewedPriceType"].PreventStringNull(),
                ApplicableFrom = (DateTime)row["ApplicableFrom"],
                GprmRuleTypeId = (int)row["GprmRuleTypeId"],
                GeographyId = (int)row["GeographyId"],
                ProductId = (int)row["ProductId"],
                ReviewedPriceTypeId = (int)row["ReviewedPriceTypeId"],
                ReviewedPriceAdjustment = (double)row["ReviewedPriceAdjustment"],
                Default = false,
                Active = (bool)row["Active"]
            };
            return data;
        }
        private static GprmSubRuleViewModel MapSubRule(DataRow row)
        {
            var data = new GprmSubRuleViewModel
            {
                ApplicableFrom = (DateTime)row["ApplicableFrom"],
                GprmRuleTypeId = (int)row["GprmRuleTypeId"],
                GeographyId = (int)row["GeographyId"],
                ProductId = (int)row["ProductId"],
                SubRuleIndex = (int)row["SubRuleIndex"],
                GprmMathId = (int)row["GprmMathId"],
                Argument = (int)row["Argument"],
                WeightTypeId = (int)row["WeightTypeId"],
                Default = false,
                Active = (bool)row["Active"]
            };
            return data;
        }
        private static GprmReferencedPriceViewModel MapReferencedPrice(DataRow row)
        {
            var data = new GprmReferencedPriceViewModel
            {
                Geography = row["Geography"].PreventStringNull(),
                ReferencedGeography = row["ReferencedGeography"].PreventStringNull(),
                ReferencedPriceType = row["ReferencedPriceType"].PreventStringNull(),
                ApplicableFrom = (DateTime)row["ApplicableFrom"],
                GprmRuleTypeId = (int)row["GprmRuleTypeId"],
                GeographyId = (int)row["GeographyId"],
                ProductId = (int)row["ProductId"],
                SubRuleIndex = (int)row["SubRuleIndex"],
                ReferencedGeographyId = (int)row["ReferencedGeographyId"],
                ReferencedPriceTypeId = (int)row["ReferencedPriceTypeId"],
                ReferencedPriceAdjustment = (double)row["ReferencedPriceAdjustment"],
                Default = false,
                Active = (bool)row["Active"]
            };
            return data;
        }
        private static DateTime MapApplicableFrom(DataRow row)
        {
            return (DateTime)row["ApplicableFrom"];
        }
        private static List<GprmRuleViewModel> AddRule(IEnumerable<GprmRuleViewModel> defaultData, IEnumerable<GprmRuleViewModel> productData, int gprmRuleTypeId, int productId, DateTime applicableFrom)
        {
            if (productData.Count() != 0)
                return new List<GprmRuleViewModel>();
            return defaultData.Select(dD => new GprmRuleViewModel
            {
                ApplicableFrom = applicableFrom,
                GeographyId = dD.GeographyId,
                ProductId = productId,
                GprmRuleTypeId = gprmRuleTypeId,
                Regular = dD.Regular,
                GprmMathId = dD.GprmMathId,
                Argument = dD.Argument,
                WeightTypeId = dD.WeightTypeId,
                IrpRuleListId = dD.IrpRuleListId,
                LookBack = dD.LookBack,
                EffectiveLag = dD.EffectiveLag,
                AllowIncrease = dD.AllowIncrease,
                Default = true,
                Active = dD.Active
            }).ToList();
        }
        private static List<GprmReviewedPriceViewModel> AddReviewedPrice(IEnumerable<GprmReviewedPriceViewModel> defaultData, IEnumerable<GprmReviewedPriceViewModel> productData, int gprmRuleTypeId, int productId, DateTime applicableFrom)
        {
            if (productData.Count() != 0)
                return new List<GprmReviewedPriceViewModel>();
            return defaultData.Select(dD => new GprmReviewedPriceViewModel
            {
                Geography = dD.Geography,
                ReviewedPriceType = dD.ReviewedPriceType,
                ApplicableFrom = applicableFrom,
                GeographyId = dD.GeographyId,
                ProductId = productId,
                GprmRuleTypeId = gprmRuleTypeId,
                ReviewedPriceTypeId = dD.ReviewedPriceTypeId,
                ReviewedPriceAdjustment = dD.ReviewedPriceAdjustment,
                Default = true,
                Active = dD.Active
            }).ToList();

        }
        private static List<GprmSubRuleViewModel> AddSubRule(IEnumerable<GprmSubRuleViewModel> defaultData, IEnumerable<GprmSubRuleViewModel> productData, int gprmRuleTypeId, int productId, DateTime applicableFrom)
        {
            var newData = defaultData
                    .Where(r0 => !productData
                        .Any(r =>
                            r.GeographyId == r0.GeographyId &&
                            r.SubRuleIndex == r0.SubRuleIndex));
            
            return newData.Select(dD => new GprmSubRuleViewModel
            {
                ApplicableFrom = applicableFrom,
                GeographyId = dD.GeographyId,
                ProductId = productId,
                SubRuleIndex = dD.SubRuleIndex,
                GprmRuleTypeId = gprmRuleTypeId,
                GprmMathId = dD.GprmMathId,
                Argument = dD.Argument,
                WeightTypeId = dD.WeightTypeId,
                Default = true,
                Active = dD.Active
            }).ToList();

        }
        private static List<GprmReferencedPriceViewModel> AddReferencedPrice(IEnumerable<GprmReferencedPriceViewModel> defaultData, IEnumerable<GprmReferencedPriceViewModel> productData, int gprmRuleTypeId, int productId, DateTime applicableFrom)
        {
            var newData = defaultData
                    .Where(r0 => !productData
                        .Any(r =>
                            r.GeographyId == r0.GeographyId &&
                            r.SubRuleIndex == r0.SubRuleIndex &&
                            r.ReferencedGeographyId == r0.ReferencedGeographyId));
            return newData
                    .Select(dD => new GprmReferencedPriceViewModel
                    {
                        Geography = dD.Geography,
                        ReferencedGeography = dD.ReferencedGeography,
                        ReferencedPriceType = dD.ReferencedPriceType,
                        ApplicableFrom = applicableFrom,
                        GeographyId = dD.GeographyId,
                        ProductId = productId,
                        SubRuleIndex = dD.SubRuleIndex,
                        GprmRuleTypeId = gprmRuleTypeId,
                        ReferencedGeographyId = dD.ReferencedGeographyId,
                        ReferencedPriceTypeId = dD.ReferencedPriceTypeId,
                        ReferencedPriceAdjustment = dD.ReferencedPriceAdjustment,
                        Default = true,
                        Active = dD.Active
                    }).ToList();
        }
    }
}