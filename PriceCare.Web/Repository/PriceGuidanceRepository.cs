using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using Newtonsoft.Json;
using PriceCare.Central;
using PriceCare.Web.Constants;
using PriceCare.Web.Helpers;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public class PriceGuidanceRepository : IPriceGuidanceRepository
    {
        private readonly GprmRuleRepository gprmRuleRepository = new GprmRuleRepository();
        public PriceMapSearchResponse GetAllPriceGuidances(PriceGuidanceRequest model)
        {
            return gprmRuleRepository.BuildGprmRuleResponse(model.CountriesId, model.ProductId, model.RuleTypeId, model.VersionId, 
                model.ApplicableFrom, model.PageNumber, model.ItemsPerPage);
        }

        public List<FilterItemViewModel> GetApplicableFromList(GetApplicableFromModel model)
        {
            var applicableFrom = gprmRuleRepository.
                GetPriceMapApplicableFrom(model.VersionId, model.GeographyIds, model.ProductId, model.GprmRuleTypeId).OrderByDescending(d => d).ToList();
            var result = new List<FilterItemViewModel>();
            result.AddRange(applicableFrom.Select(r => new FilterItemViewModel
            {
                Text = r.ToString("u"),

            }).ToList());

            return result;
        }

        #region Export Excel

        public HttpResponseMessage GetExcel(string token)
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
            var filter = JsonConvert.DeserializeObject<PriceGuidanceRequest>(buffer.FilterJson);

            var fileName = filter.DatabaseLike ? ExcelFileNameConstants.PriceMap : ExcelTemplateNameConstants.PriceMap;
            var extention = filter.DatabaseLike ? "xlsx" : "xlsm";

            string templateFilePath = HttpContext.Current.Server.MapPath(@"~\App_Data\"+fileName);

            var memoryStream = FillExcelFile(templateFilePath, filter);

            response.Content = new StreamContent(memoryStream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(templateFilePath));
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "pricemap_" + DateTime.Now.Ticks + "." + extention
            };

            return response;
        }

        private MemoryStream FillExcelFile(string templateFilePath, PriceGuidanceRequest filter)
        {
            var fileXls = File.ReadAllBytes(templateFilePath);
            var sheetDatas = new Dictionary<string, List<List<string>>>();

            var allCountries = new CountryRepository().GetAllCountries().Select(c => c.Id).ToList();

            var countries = filter.AllCountries
                ? allCountries
                : filter.CountriesId;
            var products = filter.AllProducts
                ? new ProductRepository().GetAllProducts().Where(p => p.Id != ApplicationConstants.NotSpecifiedProduct).Select(p => p.Id)
                : new List<int> { filter.ProductId };


            var datas = gprmRuleRepository.GetVersionPriceMap(filter.VersionId, 0, filter.AllProducts ? 0 : filter.ProductId, filter.RuleTypeId, filter.ApplicableFrom)
                .Where(pm => countries.Contains(pm.GeographyId) && products.Contains(pm.ProductId))
                .OrderBy(p => p.Geography)
                .ToList();

            var data = FillData(datas);
            sheetDatas.Add(data.Name, data.Datas);
            if (!filter.DatabaseLike)
            {
                var metadata = FillMetaData(filter);
                sheetDatas.Add(metadata.Name, metadata.Datas);
                var listData = FillListData(filter, allCountries, products.ToList());
                sheetDatas.Add(listData.Name, listData.Datas);
                var setup = FillSetup("ExcelUrlPriceMapExport");
                sheetDatas.Add(setup.Name, setup.Datas); 
            }
            
            var memoryStream = ClosedXmlHelper.AppendXlsxSheetsHavingHeaders(fileXls, sheetDatas);

            return memoryStream;
        }

        public SheetData FillSetup(string keyUrl)
        {
            var allData = new List<List<string>>();
            var accountRepository = new AccountRepository();
            var login = accountRepository.GetUserInfo().Username;
            var url = ConfigurationManager.AppSettings[keyUrl];

            allData.Add(new List<string> { "Data export sheet", "MetaData", "ExportRwd", "ExportRed" });
            allData.Add(new List<string> { "Header row", "1", "1" });
            allData.Add(new List<string> { "First data column", "1", "1" });
            allData.Add(new List<string> { "Last data column", "6", "8" });
            allData.Add(new List<string> { "First data row", "2", "2" });
            allData.Add(new List<string> { "Last data row", "1", "2" });
            allData.Add(new List<string> { "Model", "ExcelPriceMapViewModel", "ExcelReviewed", "ExcelReferenced" });
            allData.Add(new List<string> { "Url", url });
            allData.Add(new List<string> { "First metadata row", "1" });
            allData.Add(new List<string> { "Last metadata row", "2" });
            allData.Add(new List<string> { "Login", login });
            allData.Add(new List<string> { "DataItems", "Reviewed", "Referenced" });
            allData.Add(new List<string> { "Was open", "FALSE" });
            allData.Add(new List<string> { "Was pushed", "FALSE" });

            return new SheetData { Name = "Setup", Datas = allData };
        }

        private SheetData FillMetaData(PriceGuidanceRequest filter)
        {
            var allData = new List<List<string>>();

            allData.Add(new List<string>
            {
                "SaveId",
                "1"
            });

            allData.Add(new List<string>
            {
                "ApplicableFrom",
                filter.ApplicableFrom.Date.ToString()
            });

            allData.Add(new List<string>
            {
                "SimulationId",
                "0"
            });

            return new SheetData { Name = "MetaData", Datas = allData }; 
        }

        public SheetData FillListData(PriceGuidanceRequest filter, List<int> countries, List<int> products)
        {
            var allData = new List<List<string>>();

            allData.Add(new List<string>
            {
                "ProductId",
                "Name",
                "ShortName",
                "ShortName",
                "Id",
                "CountryProduct",
                "ShortName"
            });

            var productsInfo = new ProductRepository().GetProductExport().ToList();
            var priceTypes = new PriceTypeRepository().GetAllPriceTypes(0, null).ToList();
            var existingPriceTypes = new List<PriceTypeExport>();

            foreach (var product in products)
            {
                var productPriceTypes = new PriceTypeRepository().GetPriceTypesProduct(countries, product, filter.ApplicableFrom);
                existingPriceTypes.AddRange(productPriceTypes.Select(pPt => new PriceTypeExport
                {
                    GeographyId = pPt.GeographyId,
                    CountryProduct = pPt.GeographyId + "_" + product,
                    ShortName = pPt.ShortName
                }));
            }

            existingPriceTypes.Sort((x,y) => String.Compare(x.CountryProduct, y.CountryProduct, StringComparison.Ordinal));

            for (var i = 0; i < priceTypes.Count() || i < productsInfo.Count() || i < existingPriceTypes.Count(); i++)
            {
                var list = new List<string>();
                list.Add((i < productsInfo.Count()) ? productsInfo[i].Id.ToString() : "");
                list.Add((i < productsInfo.Count()) ? productsInfo[i].Name : "");
                list.Add((i < productsInfo.Count()) ? productsInfo[i].ShortName : "");
                list.Add((i < priceTypes.Count()) ? priceTypes[i].ShortName : "");
                list.Add((i < priceTypes.Count()) ? priceTypes[i].Id.ToString() : "");
                list.Add((i < existingPriceTypes.Count()) ? existingPriceTypes[i].CountryProduct : "");
                list.Add((i < existingPriceTypes.Count()) ? existingPriceTypes[i].ShortName : "");

                allData.Add(list);
            }

            return new SheetData { Name = "Lists", Datas = allData }; 
        }

        public bool SavePriceMap(List<GprmRuleRowViewModel> rows, DateTime applicableFrom, int ruleTypeId, int productId)
        {
            var priceMaps = new List<PriceMapViewModel>();

            foreach (var row in rows)
            {
                foreach (var cell in row.Cells)
                {
                    if (cell.Edited)
                    {
                        if (cell.ReviewedPrice != null) 
                        {
                            if (priceMaps.All(pm => pm.GeographyId != row.ReviewedPrice.GeographyId))
                            {
                                priceMaps.Add(new PriceMapViewModel
                                {
                                    Active = true,
                                    ApplicableFrom = applicableFrom,
                                    Default = row.Cells[0].ReviewedPrice.Default,
                                    ProductId = productId,
                                    GeographyId = row.Cells[0].ReviewedPrice.GeographyId,
                                    GprmRuleTypeId = ruleTypeId,
                                    ReferencedData = new List<GprmReferencedPriceViewModel>(),
                                    ReviewedPriceAdjustment = row.Cells[0].ReviewedPrice.ReviewedPriceAdjustment,
                                    ReviewedPriceTypeId = row.Cells[0].ReviewedPrice.ReviewedPriceTypeId,           
                                    Edited = true                                    
                                });
                            }                           
                        }                        
                        else // it's a referenced price
                        {
                            var newReferencedData = new GprmReferencedPriceViewModel
                            {
                                Active = cell.ReferencedPrice.Active,
                                ApplicableFrom = applicableFrom,
                                Default = cell.ReferencedPrice.Default,
                                Edited = true,
                                GeographyId = cell.ReferencedPrice.GeographyId,
                                ReferencedGeographyId = cell.ReferencedPrice.ReferencedGeographyId,
                                ProductId = productId,
                                GprmRuleTypeId = ruleTypeId,
                                ReferencedPriceAdjustment = cell.ReferencedPrice.ReferencedPriceAdjustment,
                                ReferencedPriceTypeId = cell.ReferencedPrice.ReferencedPriceTypeId,
                                SubRuleIndex = cell.ReferencedPrice.SubRuleIndex
                            };
                            var priceMap = priceMaps.FirstOrDefault(pm => pm.GeographyId == cell.ReferencedPrice.GeographyId);

                            if (priceMap == null)
                            {
                                var newPriceMap = new PriceMapViewModel
                                {
                                    Active = true,
                                    ApplicableFrom = applicableFrom,
                                    Default = row.ReviewedPrice.Default,
                                    Edited = true,
                                    GeographyId = row.ReviewedPrice.GeographyId,
                                    GprmRuleTypeId = ruleTypeId,
                                    ProductId = productId,
                                    ReviewedPriceAdjustment = row.ReviewedPrice.ReviewedPriceAdjustment,
                                    ReferencedData = new List<GprmReferencedPriceViewModel>(),
                                    ReviewedPriceTypeId = row.ReviewedPrice.ReviewedPriceTypeId
                                };
                                newPriceMap.ReferencedData.Add(newReferencedData);
                                priceMaps.Add(newPriceMap);
                            }
                            else
                            {
                                priceMap.ReferencedData.Add(newReferencedData);
                            }                            
                        }                        
                    }
                }
            }

            gprmRuleRepository.SavePriceMap(priceMaps);
            return true;
        }

        private SheetData FillData(List<PriceMapViewModel> datas)
        {
            var allData = new List<List<string>>();
            var headers = new List<string>
            {
                "ApplicableFrom",
                "Default",
                "Geography",
                "Geography Id",
                "GprmRuleTypeId",
                "Product Id",
                "Referenced Count",
                "Reviewed Price Adjustment",
                "Reviewed Price Type",
                "Reviewed Price Type Id",
                "Referended Geography",
                "Referended Geography Id",
                "GprmRuleTypeId",
                "Product Id",
                "Reviewed Price Adjustment",
                "Reviewed Price Type",
                "Reviewed Price Type Id",
                "Sub Rule Index"
            };

            allData.Add(headers);

            foreach (var referencedCountry in datas)
            {
                foreach (var referencingCountry in referencedCountry.ReferencedData)
                {

                    allData.Add(new List<string>
                    {       
                        referencedCountry.ApplicableFrom.Date.ToString(),
                        referencedCountry.Default.ToString(),
                        referencedCountry.Geography,
                        referencedCountry.GeographyId.ToString(),
                        referencedCountry.GprmRuleTypeId.ToString(),
                        referencedCountry.ProductId.ToString(),
                        referencedCountry.ReferencedData.Count.ToString(),
                        referencedCountry.ReviewedPriceAdjustment.ToString(),
                        referencedCountry.ReviewedPriceType,
                        referencedCountry.ReviewedPriceTypeId.ToString(),

                        referencingCountry.ReferencedGeography,
                        referencingCountry.ReferencedGeographyId.ToString(),
                        referencingCountry.GprmRuleTypeId.ToString(),
                        referencingCountry.ProductId.ToString(),
                        referencingCountry.ReferencedPriceAdjustment.ToString(),
                        referencingCountry.ReferencedPriceType,
                        referencingCountry.ReferencedPriceTypeId.ToString(),
                        referencingCountry.SubRuleIndex.ToString()
                    });

                }
            }

            return new SheetData { Name = "Data", Datas = allData }; 
        }

        #endregion
    }
}