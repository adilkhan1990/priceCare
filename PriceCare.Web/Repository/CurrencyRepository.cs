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
    public class CurrencyRepository : ICurrencyRepository
    {
        private readonly IVersionRepository versionRepository = new VersionRepository();
        private readonly LoadRepository loadRepository = new LoadRepository();
        public IEnumerable<FilterItemViewModel> GetActiveCurrenciesForFilter()
        {
            var currencies = new List<FilterItemViewModel>
            {
                new FilterItemViewModel
                {
                    Text = "All currencies",
                    TextShort = "All currencies",
                    Selected = true
                }
            };
            const string queryString = "SELECT Id, Name, Iso FROM Currency WHERE Active = 1";
            
            currencies.AddRange(RequestHelper.ExecuteQuery(DataBaseConstants.ConnectionString, queryString,
                row => new FilterItemViewModel
                {
                    Id = (int) row["Id"],
                    Text = (string) row["Iso"],
                    TextShort = (string) row["Iso"]
                }).OrderBy(c => c.Text).ToList());
           
            return currencies;
        }

        public CurrencySearchResponseViewModel GetPagedCurrencies(CurrencySearchRequestViewModel currencySearchRequest)
        {
            var currencies = GetAllCurrencies((RateType)currencySearchRequest.RateTypeId, currencySearchRequest.VersionId);

            var totalCurrencies = currencies.Count();

            currencies = currencies
                .Skip(currencySearchRequest.PageNumber*currencySearchRequest.ItemsPerPage)
                .Take(currencySearchRequest.ItemsPerPage);

            var viewModel = new CurrencySearchResponseViewModel
            {
                Currencies = currencies,
                IsLastPage = (currencies.Count() + (currencySearchRequest.PageNumber * currencySearchRequest.ItemsPerPage)) >= totalCurrencies,
                PageNumber = ++currencySearchRequest.PageNumber,
                TotalCurrencies = totalCurrencies
            };

            return viewModel;
        }

        public void SaveCurrencies(SaveCurrenciesModel saveCurrencies, bool updateCurrency = true)
        {            
            string information = saveCurrencies.RateType == RateType.Budget ? "Budget Rate Load" : "Spot Rate Load";

            var versionId = versionRepository.CreateNewVersion(information);

            foreach (var currency in saveCurrencies.Currencies)
            {
                if (updateCurrency)
                    UpdateCurrency(currency);
            
               InsertValues(currency.RateType,currency,versionId);
            }                      
        }

        private int InsertCurrency(CurrencyViewModel currency)
        {
            var query = "INSERT INTO Currency (Name, Iso, Active) OUTPUT INSERTED.ID  VALUES (@name, @iso, @active)";
            var dictionary = new Dictionary<string, object>
                {
                    {"name", currency.Name ?? ""},
                    {"iso", currency.Iso.Length > 3? currency.Iso.Substring(0,3): currency.Iso},
                    {"active", currency.Active}
                };
            return RequestHelper.ExecuteScalarRequest<int>(DataBaseConstants.ConnectionString, query, dictionary);
        }

        private void UpdateCurrency(CurrencyViewModel currency)
        {
            var query = "UPDATE Currency SET Name = @currencyName, Active = @active WHERE Id = @currencyId";
            var dictionary = new Dictionary<string, object>
                {
                    {"currencyName", currency.Name ?? ""},
                    {"currencyId", currency.Id},
                    {"active", currency.Active}
                };
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, dictionary);
        }

        private void InsertValues(RateType rateType, CurrencyViewModel currency, int versionId)
        {
            string tableName = rateType == RateType.Budget ? "CurrencyBudget" : "CurrencySpot";

            var query = "INSERT INTO " + tableName + " (CurrencyId, VersionId, USD, EUR) VALUES (@currencyId, @versionId, @usd, @eur)";
            var dictionary = new Dictionary<string, object>
                {
                    {"usd", currency.UsdRate},
                    {"eur", currency.EurRate},
                    {"currencyId", currency.Id},
                    {"versionId", versionId}
                };
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, dictionary);
        }

        public void SaveLoadCurrencies(SaveCurrenciesModel saveCurrencies)
        {
            string information = "Budget/Spot Rate Load";

            var versionId = versionRepository.CreateNewVersion(information);

            foreach (var currency in saveCurrencies.Currencies)
            {
                if (currency.Tag == LoadHelper.Loaded)
                {
                    currency.Id = InsertCurrency(currency);
                }
                else
                {
                    UpdateCurrency(currency);
                }

                if(currency.Active)
                    InsertValues(currency.RateType, currency, versionId);
            }



            loadRepository.ValidateLoadItem(saveCurrencies.LoadId, LoadConstants.Currency);
        }

        /// <summary>
        /// Use versionId 0 to get latest currencies
        /// </summary>
        /// <param name="rateType"></param>
        /// <returns></returns>
        public IEnumerable<CurrencyViewModel> GetAllCurrencies(RateType rateType)
        {
            return GetAllCurrencies(rateType, 0);
        } 

        public IEnumerable<CurrencyViewModel> GetAllCurrencies(RateType rateType, int versionId)
        {
            var dictionary = new Dictionary<string, object>();
            dictionary.Add("rateType", (int)rateType);
            dictionary.Add("versionId", versionId);
            var queryString = "SELECT * FROM GetCurrency(@rateType, @versionId,1)";

            var result = RequestHelper.ExecuteQuery<CurrencyViewModel>(DataBaseConstants.ConnectionString, queryString, MapData, dictionary).OrderBy(c => c.Name).ToList();

            for (var i = 0; i < result.Count; i++)
            {
                result[i].RateType = rateType;
            }

            return result;
        }

        private static CurrencyViewModel MapData(DataRow row)
        {
            var currency = new CurrencyViewModel
            {
                Id = Convert.ToInt32(row["Id"].ToString()),
                Name = row["Name"].ToString(),
                Iso = row["Iso"].ToString(),                
                UsdRate = (double)row[3],
                EurRate = (double)row[4],
            };
            return currency;
        }

        public IEnumerable<CurrencyViewModel> GetAllCurrencies()
        {
            string queryString = "SELECT * FROM Currency";

            var result = RequestHelper
                .ExecuteQuery<CurrencyViewModel>(DataBaseConstants.ConnectionString, queryString, MapDataCurrencyForList)
                .OrderBy(c => c.Iso);

            return result;
        }

        private static CurrencyViewModel MapDataCurrencyForList(DataRow row)
        {
            var currency = new CurrencyViewModel
            {
                Id = (int)row["Id"],
                Name = row["Name"].ToString(),
                Iso = row["Iso"].ToString(),
                Active = (bool)row["Active"]
            };
            return currency;
        }

        #region Export Excel

        public IEnumerable<CurrencyExportViewModel> GetCurrenciesExport(CurrencySearchRequestViewModel currencySearch)
        {
            string queryString = "SELECT * FROM ExportExchangeRates (@versionId)";

            var parametersDictionary = new Dictionary<string, object> { { "versionId", currencySearch.VersionId } };
            var result = RequestHelper
                .ExecuteQuery<CurrencyExportViewModel>(DataBaseConstants.ConnectionString, queryString, MapCurrencyExport, parametersDictionary)
                .OrderBy(c => c.Iso);

            return result;
        }

        public HttpResponseMessage GetExcel(string token, bool isBudget)
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
            var filter = JsonConvert.DeserializeObject<CurrencySearchRequestViewModel>(buffer.FilterJson);
            
            var fileName = isBudget
                ? ExcelTemplateNameConstants.CurrencyBudget
                : ExcelTemplateNameConstants.CurrencySpot;
            var extention = filter.DatabaseLike ? "xlsx" : "xlsm";

            string templateFilePath = HttpContext.Current.Server.MapPath(@"~\App_Data\"+fileName);

            var memoryStream = FillExcelFile(templateFilePath, filter, isBudget);

            response.Content = new StreamContent(memoryStream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(templateFilePath));
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "currency_" + DateTime.Now.Ticks + "." + extention
            };

            return response;
        }

        private MemoryStream FillExcelFile(string templateFilePath, CurrencySearchRequestViewModel filter, bool isBudget)
        {
            var fileXls = File.ReadAllBytes(templateFilePath);

            var datas = GetCurrenciesExport(filter).ToList();

            var sheetDatas = new Dictionary<string, List<List<string>>>();
            var data = FillData(datas, isBudget);
            sheetDatas.Add(data.Name,data.Datas);
            if (!filter.DatabaseLike)
            {
                var metadata = FillMetaData(filter);
                sheetDatas.Add(metadata.Name, metadata.Datas);
                var setup = FillSetup("ExcelUrlCurrenciesExport");
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

            allData.Add(new List<string> {"Data export sheet", "MetaData", "ExportS", "ExportB"});
            allData.Add(new List<string> {"Header row", "1", "1"});
            allData.Add(new List<string> {"First data column", "1", "1"});
            allData.Add(new List<string> {"Last data column", "3", "3"});
            allData.Add(new List<string> {"First data row", "2", "2"});
            allData.Add(new List<string> {"Last data row", "1", "2"});
            allData.Add(new List<string> { "Model", "ExcelCurrencyViewModel", "ExcelCurrency", "ExcelCurrency" });
            allData.Add(new List<string> {"Url", url});
            allData.Add(new List<string> {"First metadata row", "1"});
            allData.Add(new List<string> {"Last metadata row", "1"});
            allData.Add(new List<string> {"Login", login});
            allData.Add(new List<string> {"DataItems", "CurrencySpot", "CurrencyBudget"});
            allData.Add(new List<string> {"Was open", "FALSE"});
            allData.Add(new List<string> {"Was pushed", "FALSE"});

            return new SheetData { Name = "Setup", Datas = allData }; 
        }

        private SheetData FillMetaData(CurrencySearchRequestViewModel filter)
        {
            var versionInfos = versionRepository.GetVersionInfos(filter.VersionId);
            var allData = new List<List<string>>();

            allData.Add(new List<string>
            {
                "SaveId",
                "1"
            });

            allData.Add(new List<string>
            {
                "VersionDate",
                versionInfos.VersionTime.Date.ToString()
            });

            allData.Add(new List<string>
            {
                "SimulationId",
                "0"
            });

            return new SheetData {Name = "MetaData", Datas = allData};
        }

        private SheetData FillData(List<CurrencyExportViewModel> datas, bool isBudget)
        {
            var allData = new List<List<string>>();
            var headers = new List<string>
            {
                "Id",
                "Name",
                "ISO",
            };

            headers.AddRange(isBudget
                ? new List<string> {"USD Budget", "EUR Budget"}
                : new List<string> {"USD Spot", "EUR Budget"});

            allData.Add(headers);
           
            allData.AddRange(datas.Select(c => new List<string>
            {
                c.Id.ToString(),
                c.Name,
                c.Iso,
                c.UsdBudget.ToString(),
                c.EurBudget.ToString(),
                c.UsdSpot.ToString(),
                c.EurSpot.ToString()
            }));
           
            return new SheetData { Name = "Data", Datas = allData };
        }

        private static CurrencyExportViewModel MapCurrencyExport(DataRow row)
        {
            var currency = new CurrencyExportViewModel
            {
                Id = (int) row["Id"],
                Name = row["Name"].ToString(),
                Iso = row["Iso"].ToString(),
                EurSpot = row["EurSpot"].PreventNull(),
                UsdSpot = row["UsdSpot"].PreventNull(),
                EurBudget = row["EurBudget"].PreventNull(),
                UsdBudget = row["UsdBudget"].PreventNull()
            };
            return currency;
        }

        #endregion

    }
}