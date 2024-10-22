using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class ListToSalesRepository : IListToSalesRepository
    {
        private readonly string connectionString = DataBaseConstants.ConnectionString;
        private readonly IVersionRepository versionRepository = new VersionRepository();
        private readonly LoadRepository loadRepository = new LoadRepository();

        public ListToSalesSearchResponseViewModel GetPagedListToSales(ListToSalesSeachRequestViewModel listToSalesSeach)
        {
            var result = GetVersionListToSales(listToSalesSeach.VersionId, listToSalesSeach.CountriesId,
                listToSalesSeach.ProductsId);

            var totalListToSales = result.Count();

            result = result.OrderBy(lts => lts.GeographyName);

            // pagination
            result = result
                .Skip(listToSalesSeach.PageNumber*listToSalesSeach.ItemsPerPage)
                .Take(listToSalesSeach.ItemsPerPage);

            var viewModel = new ListToSalesSearchResponseViewModel
            {
                ListToSales = result,
                IsLastPage =
                    (result.Count() + (listToSalesSeach.PageNumber*listToSalesSeach.ItemsPerPage)) >= totalListToSales,
                PageNumber = ++listToSalesSeach.PageNumber,
                TotalListToSales = totalListToSales
            };

            return viewModel;
        }

        public IEnumerable<ListToSalesViewModel> GetVersionListToSales(int versionId, List<int> geographyId,
            List<int> productId)
        {
            return GetListToSales(true, 0, versionId, geographyId, productId);
        }

        public IEnumerable<ListToSalesViewModel> GetForecastListToSales(int saveId, List<int> geographyId,
            List<int> productId)
        {
            return GetListToSales(false, saveId, 0, geographyId, productId);
        }

        public void SaveVersion(List<ListToSalesViewModel> listToSales)
        {
            var versionId = versionRepository.CreateNewVersion("List To Sales");
            foreach (var sale in listToSales)
            {
                sale.VersionId = versionId;
            }
            // todo SaveId + SaveTypeId
            var datatable = listToSales.ToDataTable(new List<string>() { "GeographyName", "SegmentName", "AspDisplay", "AspUsdBudget", "AspEurBudget", "Tag", "OldValue", "PercentageVariationFromX" });

            RequestHelper.BulkInsert(DataBaseConstants.ConnectionString, "ListToSales", datatable); 
        }

        private IEnumerable<ListToSalesViewModel> GetListToSales(bool isData, int saveId, int versionId,
            List<int> geographyId, List<int> productId)
        {
            const string queryL2S =
                "SELECT * FROM GetListToSales ( @isData , @saveId , @versionId , @geographyId , @productId )";
            var listToSales = RequestHelper.ExecuteQuery(
                connectionString,
                queryL2S,
                MapListToSales,
                new Dictionary<string, object>
                {
                    {"isData", isData},
                    {"saveId", saveId},
                    {"versionId", versionId},
                },
                new Dictionary<string, List<int>>
                {
                    {"geographyId", geographyId},
                    {"productId", productId}
                }
                ).ToList();

            return listToSales;
        }

        private static ListToSalesViewModel MapListToSales(DataRow row)
        {
            var segment = new ListToSalesViewModel
            {
                Id = (int) row["ListToSalesId"],
                GeographyId = (int) row["GeographyId"],
                ProductId = (int) row["ProductId"],
                GeographyName = row["GeographyName"].ToString(),
                SegmentId = (int) row["SegmentId"],
                SegmentName = row["Segment"].ToString(),
                MarketPercentage = (double) row["MarketPercentage"],
                Asp = (double) row["Asp"],
                ImpactPercentage = (double) row["ImpactPercentage"],
                //SaveId = (int)row["SaveId"],
                //SaveTypeId = (int)row["SaveTypeId"],
                //CurrencySpotId = (int)row["CurrencySpotId"],
                //CurrencySpotVersionId = (int)row["CurrencySpotVersionId"]
            };

            return segment;
        }

        #region Export Excel

        public IEnumerable<ListToSalesExportViewModel> GetListToSalesExports(ListToSalesSeachRequestViewModel listToSalesSeach)
        {
            var query =
                "SELECT * FROM ExportListToSalesAssumptions (@isData, @saveId, @versionId, @geographyId, @productId)";

            var countries = listToSalesSeach.AllCountries
                ? new CountryRepository().GetAllCountries().Select(c => c.Id).ToList()
                : listToSalesSeach.CountriesId;
            var products = listToSalesSeach.AllProducts
                ? new ProductRepository().GetAllProducts().Select(p => p.Id).ToList()
                : listToSalesSeach.ProductsId;


            var listToSales = RequestHelper.ExecuteQuery(
                connectionString,
                query,
                MapListToSalesExport,
                new Dictionary<string, object>
                            {
                                {"isData", true},
                                {"saveId", 1}, // 1 for data
                                {"versionId", listToSalesSeach.VersionId},
                            },
                new Dictionary<string, List<int>>
                            {
                                {"geographyId", countries},
                                {"productId", products}
                            }
                ).ToList();

            return listToSales.OrderBy(l => l.GeographyName).ThenBy(l => l.ProductName).ThenBy(l => l.Segment);
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
            var filter = JsonConvert.DeserializeObject<ListToSalesSeachRequestViewModel>(buffer.FilterJson);

            var fileName = filter.DatabaseLike ? ExcelFileNameConstants.ListToSales : ExcelTemplateNameConstants.ListToSales;
            var extention = filter.DatabaseLike ? "xlsx" : "xlsm";

            string templateFilePath = HttpContext.Current.Server.MapPath(@"~\App_Data\"+fileName);

            var memoryStream = FillExcelFile(templateFilePath, filter);

            response.Content = new StreamContent(memoryStream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(templateFilePath));
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "netdata_" + DateTime.Now.Ticks + "." + extention
            };

            return response;
        }

        private MemoryStream FillExcelFile(string templateFilePath, ListToSalesSeachRequestViewModel filter)
        {
            var fileXls = File.ReadAllBytes(templateFilePath);
            var sheetDatas = new Dictionary<string, List<List<string>>>();

            var datas = GetListToSalesExports(filter).ToList();
            var data = FillData(datas);
            sheetDatas.Add(data.Name, data.Datas);
            if (!filter.DatabaseLike)
            {
                var metadata = FillMetaData(filter);
                sheetDatas.Add(metadata.Name, metadata.Datas);
                var listData = FillListData();
                sheetDatas.Add(listData.Name, listData.Datas);
                var setup = FillSetup("ExcelUrlListToSalesAssumptions");
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

            allData.Add(new List<string> { "Data export sheet", "MetaData", "ExportL", "ExportI" });
            allData.Add(new List<string> { "Header row", "1", "1" });
            allData.Add(new List<string> { "First data column", "1", "1" });
            allData.Add(new List<string> { "Last data column", "8", "5" });
            allData.Add(new List<string> { "First data row", "2", "2" });
            allData.Add(new List<string> { "Last data row", "1", "2" });
            allData.Add(new List<string> { "Model", "ExcelListToSalesViewModel", "ExcelListToSales", "ExcelListToSalesImpact" });
            allData.Add(new List<string> { "Url", url });
            allData.Add(new List<string> { "First metadata row", "1" });
            allData.Add(new List<string> { "Last metadata row", "1" });
            allData.Add(new List<string> { "Login", login });
            allData.Add(new List<string> { "DataItems", "ListToSales", "ListToSalesImpact" });
            allData.Add(new List<string> { "Was open", "FALSE" });
            allData.Add(new List<string> { "Was pushed", "FALSE" });
           
            return new SheetData { Name = "Setup", Datas = allData };
        }

        private SheetData FillMetaData(ListToSalesSeachRequestViewModel filter)
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

            return new SheetData { Name = "MetaData", Datas = allData };
        }

        public SheetData FillListData()
        {
            var allData = new List<List<string>>();

            allData.Add(new List<string>
            {
                "Segment",
                "SegmentId",
                "Name",
                "ShortName",
                "ProductId",
                "Name",
                "GeographyId"
            });

            var countries = new CountryRepository().GetCountryExport().ToList();
            var products = new ProductRepository().GetProductExport().ToList();

            for (var i = 0; i < countries.Count() || i < products.Count() ; i++)
            {
                var list = new List<string>();
                list.Add("");
                list.Add("");
                list.Add((i < products.Count()) ? products[i].Name : "");
                list.Add((i < products.Count()) ? products[i].ShortName : "");
                list.Add((i < products.Count()) ? products[i].Id.ToString() : "");
                list.Add((i < countries.Count()) ? countries[i].Name : "");
                list.Add((i < countries.Count()) ? countries[i].Id.ToString() : "");

                allData.Add(list);
            }

            return new SheetData {Name = "Lists", Datas = allData};

        }

        private SheetData FillData(List<ListToSalesExportViewModel> datas)
        {
            var allData = new List<List<string>>();
            var headers = new List<string>
            {
                "ListToSalesId",
                "Geography Id",
                "Geography name",
                "Product Id",
                "Product Name",
                "Currency",
                "EurSpot",
                "UsdSpot",
                "Segment Id",
                "Segment",
                "Asp",
                "MarketPercentage",
                "ImpactPercentage",
                "M1",
                "M2",
                "M3",
                "M4",
                "M5",
                "M6",
                "M7",
                "M8",
                "M9",
                "M10",
                "M11",
                "M12",
            };

            allData.Add(headers);

            allData.AddRange(datas.Select(l => new List<string>
            {
                l.Id.ToString(),
                l.GeographyId.ToString(),
                l.GeographyName,
                l.ProductId.ToString(),
                l.ProductName,
                l.Currency,
                l.EurSpot.ToString(),
                l.UsdSpot.ToString(),
                l.SegmentId.ToString(),
                l.SegmentName,
                l.Asp.ToString(),
                l.MarketPercentage.ToString(),
                l.ImpactPercentage.ToString(),
                l.M1.ToString(),
                l.M2.ToString(),
                l.M3.ToString(),
                l.M4.ToString(),
                l.M5.ToString(),
                l.M6.ToString(),
                l.M7.ToString(),
                l.M8.ToString(),
                l.M9.ToString(),
                l.M10.ToString(),
                l.M11.ToString(),
                l.M12.ToString()
            }));

            return new SheetData { Name = "Data", Datas = allData }; 
        }

        private static ListToSalesExportViewModel MapListToSalesExport(DataRow row)
        {
            var segment = new ListToSalesExportViewModel
            {
                Id = (int)row["ListToSalesId"],
                GeographyId = (int)row["GeographyId"],
                GeographyName = row["GeographyName"].ToString(),
                ProductId = (int)row["ProductId"],
                ProductName = row["ProductName"].ToString(),
                Currency = row["Currency"].ToString(),
                EurSpot = (double) row["EurSpot"],
                UsdSpot = (double) row["UsdSpot"],
                SegmentId = (int)row["SegmentId"],
                SegmentName = row["Segment"].ToString(),
                Asp = (double)row["Asp"],
                MarketPercentage = (double)row["MarketPercentage"],
                ImpactPercentage = (double)row["ImpactPercentage"],
                M1 = (double) row["M1"],
                M2 = (double) row["M2"],
                M3 = (double) row["M3"],
                M4 = (double) row["M4"],
                M5 = (double) row["M5"],
                M6 = (double) row["M6"],
                M7 = (double) row["M7"],
                M8 = (double) row["M8"],
                M9 = (double) row["M9"],
                M10 = (double) row["M10"],
                M11 = (double) row["M11"],
                M12 = (double) row["M12"]
            };

            return segment;
        }

        #endregion

    }
}