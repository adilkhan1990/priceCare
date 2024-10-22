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
using Newtonsoft.Json;
using PriceCare.Central;
using PriceCare.Web.Constants;
using PriceCare.Web.Helpers;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public class PriceTypeRepository : IPriceTypeRepository
    {
        private readonly string connectionString = DataBaseConstants.ConnectionString;
        private readonly LoadRepository loadRepository = new LoadRepository();

        public List<PriceTypeViewModel> GetPriceTypes(int geographyId, int productId, DateTime applicableFrom)
        {
            const string query = "SELECT * FROM GetPriceTypes(@geographyId,@productId,@applicableFrom)";
            return RequestHelper.ExecuteQuery<PriceTypeViewModel>(
                connectionString, 
                query, 
                row => new PriceTypeViewModel
                {
                    Id = Convert.ToInt32(row["Id"].ToString()),
                    Name = row["Name"].ToString(),
                    ShortName = row["ShortName"].ToString(),
                    CurrencyId = Convert.ToInt32(row["CurrencyId"].ToString()),
                    CurrencyName = row["CurrencyName"].ToString(),
                    Status = (bool)row["Status"],
                }, 
                new Dictionary<string, object>
                {
                    {"geographyId",geographyId},
                    {"productId",productId},
                    {"applicableFrom",applicableFrom.AddMonths(-1)}
                }).ToList();
        }
        public List<PriceTypeViewModel> GetPriceTypesProduct(List<int> geographyId, int productId, DateTime applicableFrom)
        {
            const string query = "SELECT * FROM GetPriceTypesForProduct(@productId,@applicableFrom,@geographyId)";
            return RequestHelper.ExecuteQuery<PriceTypeViewModel>(
                connectionString,
                query,
                row => new PriceTypeViewModel
                {
                    Id = Convert.ToInt32(row["Id"].ToString()),
                    GeographyId = (int)row["GeographyId"],
                    Name = row["Name"].ToString(),
                    ShortName = row["ShortName"].ToString(),
                    CurrencyId = Convert.ToInt32(row["CurrencyId"].ToString()),
                    CurrencyName = row["CurrencyName"].ToString(),
                    Status = (bool)row["Status"],
                },
                new Dictionary<string, object>
                {
                    {"productId",productId},
                    {"applicableFrom",applicableFrom.AddMonths(-1)}
                },
                new Dictionary<string, List<int>>
                {
                    {"geographyId",geographyId}
                }).ToList();
        }

        public IEnumerable<PriceTypeViewModel> GetVersionPriceTypes(int versionId, int reviewedGeographyId,
            int productId, int referencedGeographyId)
        {
            return GetAllPriceTypes(0);
        }

        public IEnumerable<PriceTypeViewModel> GetVersionPriceTypes(int versionId, int reviewedGeographyId,
            int productId)
        {
            return GetAllPriceTypes(0);
        }

        public IEnumerable<PriceTypeViewModel> GetForecastPriceTypes(int saveId, int reviewedGeographyId,
            int productId, int referencedGeographyId)
        {
            return null;
        }

        public IEnumerable<PriceTypeViewModel> GetAllPriceTypes(int currencyId, string searchText = "")
        {
            string query = "SELECT P.Id, P.Name, P.ShortName, P.CurrencyId, C.Iso CurrencyName, P.Active " +
                           "FROM pricetype P, currency C " +
                           "WHERE P.CurrencyId = C.Id ";

            var dictionary = new Dictionary<string, object>();

            if (currencyId > 0)
            {
                query += "AND P.CurrencyId = @currencyId ";                                 
                dictionary.Add("currencyId", currencyId);
            }
            if (!string.IsNullOrEmpty(searchText))
            {
                query += "AND ( P.Name LIKE @searchText OR P.ShortName LIKE @searchText ) ";
                dictionary.Add("searchText", "%" + searchText + "%");
            }
            query += "ORDER BY P.ShortName";
            var result = RequestHelper.ExecuteQuery<PriceTypeViewModel>(connectionString, query, MapData, dictionary);

            return result;            
        }

        public IEnumerable<PriceTypeViewModel> GetPriceTypes(PriceTypeSearchRequestViewModel priceTypeSearchRequest)
        {
            string query = "SELECT P.Id, P.Name, P.ShortName, P.CurrencyId, C.Iso CurrencyName, P.Active " +
                          "FROM pricetype P, currency C " +
                          "WHERE P.CurrencyId = C.Id ";
            var dictionary = new Dictionary<string, object>();

            if (priceTypeSearchRequest.Status > 0)
            {
                query += "AND P.Active = @active ";
                dictionary.Add("active", (priceTypeSearchRequest.Status == 1) ? 1 : 0);
            }

            if (!string.IsNullOrEmpty(priceTypeSearchRequest.SearchText))
            {
                query += "AND ( P.Name LIKE @searchText OR P.ShortName LIKE @searchText ) ";
                dictionary.Add("searchText", "%" + priceTypeSearchRequest.SearchText + "%");
            }
            query += " ORDER BY P.Name ";

            return RequestHelper.ExecuteQuery<PriceTypeViewModel>(connectionString, query, MapData, dictionary);
        }

        public PriceTypeSearchResponseViewModel GetPagedPriceTypes(PriceTypeSearchRequestViewModel priceTypeSearchRequest)
        {
            var priceTypes = GetPriceTypes(priceTypeSearchRequest);

            var totalPriceTypes = priceTypes.Count();

            // Pagination
            priceTypes = priceTypes
                .Skip(priceTypeSearchRequest.PageNumber * priceTypeSearchRequest.ItemsPerPage)
                .Take(priceTypeSearchRequest.ItemsPerPage);


            var viewModel = new PriceTypeSearchResponseViewModel
            {
                PriceTypes = priceTypes,
                IsLastPage = (priceTypes.Count() + (priceTypeSearchRequest.PageNumber * priceTypeSearchRequest.ItemsPerPage)) >= totalPriceTypes,
                PageNumber = ++priceTypeSearchRequest.PageNumber,
                TotalPriceTypes = totalPriceTypes
            };

            return viewModel;
        }

        public bool AddPriceType(PriceTypeViewModel model)
        {
            const string query = "INSERT INTO PriceType VALUES (@name, @shortName, @currencyId, @active)";

            var dictionary = new Dictionary<string, object>
            {
                {"name", model.Name},
                {"shortName", model.ShortName},
                {"currencyId", model.CurrencyId},
                {"active", model.Status}
            };

            var rowsAdded = RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, dictionary);           
            return rowsAdded == 1;
        }

        public void SaveLoad(PriceTypeSaveModel priceTypeSaveModel)
        {
            var priceTypeToUpdate = priceTypeSaveModel.PriceTypes.Where(p => p.Id != 0);
            var priceTypeToInsert = priceTypeSaveModel.PriceTypes.Where(p => p.Id == 0);

            if(priceTypeToUpdate.Any())
                Save(priceTypeToUpdate);

            if(priceTypeToInsert.Any())
                foreach (var priceType in priceTypeToInsert)
                {
                    AddPriceType(priceType);
                }
            loadRepository.ValidateLoadItem(priceTypeSaveModel.LoadId, LoadConstants.PriceType);
        }

        public void Save(IEnumerable<PriceTypeViewModel> priceTypes)
        {
            foreach (var priceType in priceTypes)
            {
                Update(priceType);
            }
        }

        private void Update(PriceTypeViewModel priceType)
        {
            var query = "UPDATE PriceType " +
                        "SET Name = @name, ShortName = @shortName, CurrencyId = @currencyId, Active = @active " +
                        "Where Id = @id";
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, new Dictionary<string, object>
            {
                {"name", priceType.Name},
                {"shortName", priceType.ShortName},
                {"currencyId", priceType.CurrencyId},
                {"active", priceType.Status},
                {"id", priceType.Id}
            });
        }

        public bool IsValid(PriceTypeViewModel priceType)
        {
            // Check if short name is already taken
            var query = "SELECT COUNT(*) FROM PriceType WHERE ShortName = @shortName ";
            var dictionary = new Dictionary<string, object>
            {
                {"shortName", priceType.ShortName}
            };
            if (priceType.Id != 0)
            {
                query += " AND Id != @id";
                dictionary.Add("id", priceType.Id);
            }
                
            var count = RequestHelper.ExecuteScalarRequest<int>(DataBaseConstants.ConnectionString, query, dictionary);
            return count == 0;
        }

        private static PriceTypeViewModel MapData(DataRow row)
        {
            var product = new PriceTypeViewModel
            {
                Id = Convert.ToInt32(row["Id"].ToString()),
                Name = row["Name"].ToString(),
                ShortName = row["ShortName"].ToString(),
                CurrencyId = Convert.ToInt32(row["CurrencyId"].ToString()),
                CurrencyName = row["CurrencyName"].ToString(),
                Status = (bool)row["Active"],
            };

            return product;
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
            var filter = JsonConvert.DeserializeObject<PriceTypeSearchRequestViewModel>(buffer.FilterJson);

            var fileTemplate = filter.DatabaseLike ? ExcelFileNameConstants.PriceType: ExcelTemplateNameConstants.PriceType;
            var extention = filter.DatabaseLike ? "xlsx" : "xlsm";

            string templateFilePath = HttpContext.Current.Server.MapPath(@"~\App_Data\"+fileTemplate);

            var memoryStream = FillExcelFile(templateFilePath, filter);

            response.Content = new StreamContent(memoryStream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(templateFilePath));
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "priceType_" + DateTime.Now.Ticks + "." + extention
            };

            return response;
        }

        private MemoryStream FillExcelFile(string templateFilePath, PriceTypeSearchRequestViewModel filter)
        {
            var fileXls = File.ReadAllBytes(templateFilePath);

            var datas = GetPriceTypes(filter).ToList();

            var sheetDatas = new Dictionary<string, List<List<string>>>();
            var data = FillData(datas);
            sheetDatas.Add(data.Name, data.Datas);
            if (!filter.DatabaseLike)
            {
                var metadata = FillMetaData();
                sheetDatas.Add(metadata.Name, metadata.Datas);
                var listData = FillListData();
                sheetDatas.Add(listData.Name, listData.Datas);
                var setup = FillSetup("ExcelUrlPriceTypeExport");
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
            allData.Add(new List<string> { "Header row", "1"});
            allData.Add(new List<string> { "First data column", "1" });
            allData.Add(new List<string> { "Last data column", "5"});
            allData.Add(new List<string> { "First data row", "2"});
            allData.Add(new List<string> { "Last data row", "4"});
            allData.Add(new List<string> { "Model", "ExcelPriceTypeViewModel", "PriceTypeViewModel" });
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

        private SheetData FillListData()
        {
            var currencies = new CurrencyRepository().GetAllCurrencies();
            var allData = new List<List<string>>();

            allData.Add(new List<string>
            {
                "Iso",
                "Id"
            });

            allData.AddRange(currencies.Select(c => new List<string>
            {
                c.Iso,
                c.Id.ToString()
            }));

            return new SheetData { Name = "Lists", Datas = allData };
        }

        private SheetData FillData(List<PriceTypeViewModel> datas)
        {
            var allData = new List<List<string>>();
            var headers = new List<string>
            {
                "Id",
                "Name",
                "ShortName",
                "CurrencyId",
                "Iso",
                "Active"
            };

            allData.Add(headers);

            allData.AddRange(datas.Select(c => new List<string>
            {
                c.Id.ToString(),
                c.Name,
                c.ShortName,
                c.CurrencyId.ToString(),
                c.CurrencyName,
                c.Status.ToString()
            }));

            return new SheetData { Name = "Data", Datas = allData };
        }

        public IEnumerable<PriceTypeExport> GetPriceTypeExports(int versionId)
        {
            var query = "EXEC GetExistingPriceType @SaveId = @saveId";

            return RequestHelper.ExecuteQuery<PriceTypeExport>(DataBaseConstants.ConnectionString, query, MapForExport,
                new Dictionary<string, object>
                {
                    {"SaveId", 1}
                }).OrderBy(p => p.CountryProduct);
        }

        private static PriceTypeExport MapForExport(DataRow row)
        {
            var product = new PriceTypeExport
            {
                CountryProduct = row["CountryProduct"].ToString(),
                ShortName = row["ShortName"].ToString(),
            };

            return product;
        }

        #endregion
    }
}