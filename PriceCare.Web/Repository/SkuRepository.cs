using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using ClosedXML.Excel;
using Newtonsoft.Json;
using PriceCare.Central;
using PriceCare.Web.Constants;
using PriceCare.Web.Helpers;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public class SkuRepository: ISkuRepository
    {
        private readonly LoadRepository loadRepository = new LoadRepository();
        public List<SkuViewModel> GetAllSkus(int countryId, int productId, int status, int formulationId)
        {
            string queryString = "SELECT S.Id, S.GeographyId, S.ProductId, S.Name, S.Dosage, S.PackSize, S.ProductNumber, S.FormulationId, DT.Name Dimension, S.Active, S.FactorUnit, U.Name AS Unit " +
                                 "FROM SKU S, DimensionType DT, Unit U " +
                                 "WHERE S.FormulationId = DT.Id AND S.FactorUnit = U.Factor";
            var dictionary = new Dictionary<string, object>();

            //COUNTRIES FILTER
            if (countryId > 0)
            {
                queryString += " AND S.GeographyId = @countryId ";
                dictionary.Add("countryId", countryId);
            }

            //PRODUCT FILTER
            if (productId > 0)
            {
                queryString += " AND S.ProductId = @productId ";
                dictionary.Add("productId", productId);
            }

            //Status FILTER
            if (status > 0)
            {
                queryString += " AND S.Active = @active ";
                dictionary.Add("active", status == 1 ? 1 : 0); // 1 = active, 2 = inactive
            }

            // Device FILTER (formulation)
            if (formulationId > 0)
            {
                queryString += " AND DT.Id = @formulationId ";
                dictionary.Add("formulationId", formulationId);
            }

            queryString += " ORDER BY S.Dosage";

            var result = RequestHelper.ExecuteQuery<SkuViewModel>(DataBaseConstants.ConnectionString, queryString, MapRow, dictionary).ToList();

            return result;
        }

        public SkuResponseViewModel GetSkus(SkuRequestViewModel skuRequest)
        {
            var result = GetAllSkus(skuRequest.CountryId, skuRequest.ProductId, skuRequest.Status, skuRequest.FormulationId);

            var totalSkus = result.Count;

            // PAGINATION
            result = result
                .Skip(skuRequest.PageNumber * skuRequest.ItemsPerPage)
                .Take(skuRequest.ItemsPerPage)
                .ToList();

            var viewModel = new SkuResponseViewModel
            {
                Skus = result,
                IsLastPage = (result.Count + (skuRequest.PageNumber * skuRequest.ItemsPerPage)) >= totalSkus,
                PageNumber = ++skuRequest.PageNumber,
                TotalSkus = totalSkus
            };

            return viewModel;
        }

        public bool AddSku(SkuViewModel sku)
        {
            if (!SkuExists(sku.ProductNumber, sku.GeographyId))
            {
                const string query =
               "INSERT INTO SKU VALUES (@geographyId, @productId, @name, @productNumber, @dosage, @packSize, @formulationId, @factorUnit, @active)";

                var dictionary = new Dictionary<string, object>
                {
                    {"geographyId", sku.GeographyId},
                    {"productId", sku.ProductId},
                    {"name", sku.Name},
                    {"productNumber", sku.ProductNumber},
                    {"dosage", sku.Dosage},
                    {"packSize", sku.PackSize},
                    {"formulationId", sku.FormulationId},
                    {"factorUnit", sku.FactorUnit},
                    {"active", sku.Status}
                };

                var count = RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, dictionary);
                return count == 1;
            }
            return false;
        }

        private bool SkuExists(string productNumber, int geographyId)
        {
            const string query = "SELECT Id FROM SKU WHERE ProductNumber=@productNumber AND GeographyId=@geographyId";

            var dictionary = new Dictionary<string, object>
            {
                {"productNumber", productNumber},
                {"geographyId", geographyId}
            };

            var result = RequestHelper.ExecuteQuery(DataBaseConstants.ConnectionString, query, row => new SkuViewModel
            {
                Id = (int) row["Id"]
            }, dictionary).ToList();

            return result.Count > 0;
        }

        private static SkuViewModel MapRow(DataRow row)
        {
            var sku = new SkuViewModel
            {
                Id = (int)row["Id"],
                Dosage = (double)row["Dosage"],                
                Formulation = (string)row["Dimension"],
                FormulationId = (int)row["FormulationId"],
                Name = (string)row["Name"],
                PackSize = (double)row["PackSize"],
                Status = (bool)row["Active"],
                FactorUnit = (double)row["FactorUnit"],
                GeographyId = (int)row["GeographyId"],
                ProductId = (int)row["ProductId"],
                ProductNumber = (string)row["ProductNumber"],
                Unit = (string)row["Unit"]
            };
            return sku;
        }

        public void Save(IEnumerable<SkuViewModel> skus)
        {
            foreach (var sku in skus)
            {
                Update(sku);
            }
        }

        private void Update(SkuViewModel sku)
        {
            var query = "UPDATE Sku " +
                        "SET Name = @name, Dosage = @dosage, " +
                        "PackSize = @packSize, FormulationId = @formulationId, FactorUnit = @factorUnit, " +
                        "Active = @active " +
                        "WHERE Id = @id";
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, new Dictionary<string, object>
            {
                {"name", sku.Name},
                {"dosage", sku.Dosage},
                {"packSize", sku.PackSize},
                {"formulationId", sku.FormulationId},
                {"factorUnit", sku.FactorUnit},
                {"active", sku.Status},
                {"Id", sku.Id},
            });
        }

        /// <summary>
        /// Returns if load item is validated or not.
        /// </summary>
        /// <param name="skuSaveModel"></param>
        /// <returns></returns>
        public bool SaveLoad(SkuSaveModel skuSaveModel)
        {
            var skuToUpdate = skuSaveModel.Skus.Where(s => s.Id != 0).ToList();
            var skuToInsert = skuSaveModel.Skus.Where(s => s.Id == 0).ToList();

            if (skuToUpdate.Any())
                Save(skuToUpdate);

            if (skuToInsert.Any())
            {
                foreach (var sku in skuToInsert)
                {
                    AddSku(sku);
                }
            }

            loadRepository.ValidateLoadItemDetail(skuSaveModel.LoadId, LoadConstants.Sku, skuSaveModel.ProductId, skuSaveModel.CountryId);

            var loadItem = loadRepository.GetLoadItem(skuSaveModel.LoadId, LoadConstants.Sku);

            return loadItem.Status == (int)LoadStatus.Validated;            
        }

        #region Export Excel

        public IEnumerable<SkuExportViewModel> GetSkuExport(SkuRequestViewModel skuSearch)
        {
            string queryString = "SELECT * FROM ExportSku (@geographyId, @productId)";

            var countries = skuSearch.AllCountries
                ? new CountryRepository().GetAllCountries().Select(c => c.Id).ToList()
                : new List<int> {skuSearch.CountryId};

            var products = skuSearch.AllProducts
                ? new ProductRepository().GetAllProducts().Select(p => p.Id).ToList()
                : new List<int> {skuSearch.ProductId};

            var parametersDictionary = new Dictionary<string, List<int>>
            {
                { "geographyId", countries },
                { "productId", products}
            };
            var result = RequestHelper
                .ExecuteQuery<SkuExportViewModel>(DataBaseConstants.ConnectionString, queryString, MapSkuExport, parametersDictionary)
                .OrderBy(s => s.GeographyName).ThenBy(s => s.Product).ThenBy(s => s.Dosage);

            return result;
        }

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
            var filter = JsonConvert.DeserializeObject<SkuRequestViewModel>(buffer.FilterJson);

            var fileName = filter.DatabaseLike ? ExcelFileNameConstants.Sku : ExcelTemplateNameConstants.Sku;
            var extention = filter.DatabaseLike ? "xlsx" : "xlsm";

            string templateFilePath = HttpContext.Current.Server.MapPath(@"~\App_Data\"+fileName);

            var memoryStream = FillExcelFile(templateFilePath, filter);

            response.Content = new StreamContent(memoryStream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(templateFilePath));
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "sku_" + DateTime.Now.Ticks + "." + extention
            };

            return response;
        }

        private MemoryStream FillExcelFile(string templateFilePath, SkuRequestViewModel filter)
        {
            var fileXls = File.ReadAllBytes(templateFilePath);

            var datas = GetSkuExport(filter).ToList();

            var sheetDatas = new Dictionary<string, List<List<string>>>();
            var data = FillData(datas);
            sheetDatas.Add(data.Name, data.Datas);

            if (!filter.DatabaseLike)
            {
                var metadata = FillMetaData();
                sheetDatas.Add(metadata.Name, metadata.Datas);
                var listData = FillListData();
                sheetDatas.Add(listData.Name, listData.Datas);
                var setup = FillSetup("ExcelUrlSkuExport");
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

            allData.Add(new List<string> { "Data export sheet", "Export", "MetaData" });
            allData.Add(new List<string> { "Header row", "1" });
            allData.Add(new List<string> { "First data column", "1" });
            allData.Add(new List<string> { "Last data column", "13" });
            allData.Add(new List<string> { "First data row", "2" });
            allData.Add(new List<string> { "Last data row", "1" });
            allData.Add(new List<string> { "Model", "ExcelSkuViewModel", "SkuViewModel" });
            allData.Add(new List<string> { "Url", url });
            allData.Add(new List<string> { "First metadata row", "1" });
            allData.Add(new List<string> { "Last metadata row", "2" });
            allData.Add(new List<string> { "Login", login });
            allData.Add(new List<string> { "Was open", "FALSE" });
            allData.Add(new List<string> { "Was pushed", "FALSE" });

            return new SheetData { Name = "Setup", Datas = allData };
        }

        private SheetData FillMetaData()
        {
            var allData = new List<List<string>>();

            allData.Add(new List<string>
            {
                "SaveId",
                "1"
            });

            allData.Add(new List<string>
            {
                "DeactivatePreviousVersion",
                "false"
            });

            return new SheetData { Name = "MetaData", Datas = allData };
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
                "GeographyId",
                "Name",
                "Factor"
            });

            var countries = new CountryRepository().GetCountryExport().ToList();
            var products = new ProductRepository().GetProductExport().ToList();
            var formulations = new DimensionRepository().GetFormulationType().ToList();

            for (var i = 0; i < formulations.Count() || i < countries.Count() || i < products.Count(); i++)
            {
                var list = new List<string>();
                list.Add((i < formulations.Count()) ? formulations[i].Name : "");
                list.Add((i < formulations.Count()) ? formulations[i].Id.ToString() : "");
                list.Add((i < products.Count()) ? products[i].Name : "");
                list.Add((i < products.Count()) ? products[i].ShortName : "");
                list.Add((i < products.Count()) ? products[i].Id.ToString() : "");
                list.Add((i < countries.Count()) ? countries[i].Name : "");
                list.Add((i < countries.Count()) ? countries[i].Id.ToString() : "");
                list.Add("");
                list.Add("");
                allData.Add(list);
            }

            if (allData.Count() > 2)
            {
                allData[0][7] = "Mcg";
                allData[0][8] = "1";
                allData[1][7] = "Mg";
                allData[1][8] = "1000";
            }

            return new SheetData { Name = "Lists", Datas = allData };
        }

        private SheetData FillData(List<SkuExportViewModel> datas)
        {
            var allData = new List<List<string>>();
            var headers = new List<string>
            {
                "GeographyId",
                "Name",
                "ProductId",
                "Name",
                "Name",
                "ProductNumber",
                "Dosage",
                "PackSize",
                "FormulationId",
                "Name",
                "FactorUnit",
                "unit",
                "Active",
                "Id"
            };

            allData.Add(headers);

            allData.AddRange(datas.Select(c => new List<string>
            {
                c.GeographyId.ToString(),
                c.GeographyName,
                c.ProductId.ToString(),
                c.Product,
                c.ProductName,
                c.ProductNumber,
                c.Dosage.ToString(),
                c.PackSize.ToString(),
                c.FormulationId.ToString(),
                c.Formulation,
                c.FactorUnit.ToString(),
                c.Unit.ToString(),
                c.Active.ToString(),
                c.Id.ToString()
            }));

            return new SheetData { Name = "Data", Datas = allData };
        }

        private static SkuExportViewModel MapSkuExport(DataRow row)
        {
            var sku = new SkuExportViewModel
            {
                Id = (int)row["Id"],
                GeographyId = (int) row["GeographyId"],
                GeographyName = row["Geography"].ToString(),
                ProductId = (int) row["ProductId"],
                Product = row["Product"].ToString(),
                ProductNumber = row["ProductNumber"].ToString(),
                ProductName = row["ProductName"].ToString(),
                Dosage = (int)row["Dosage"],
                PackSize = (int)row["PackSize"],
                FormulationId = (int)row["FormulationId"],
                Formulation = row["Formulation"].ToString(),
                FactorUnit = (int) row["FactorUnit"],
                Unit = row["Unit"].ToString(),
                Active = (bool) row["Active"]
            };
            return sku;
        }

        #endregion


    }
}