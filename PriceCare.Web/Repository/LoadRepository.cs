using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Ajax.Utilities;using PriceCare.Web.Constants;
using PriceCare.Web.Helpers;
using PriceCare.Web.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using PriceCare.Web.Models.Oracle;

namespace PriceCare.Web.Repository
{
    public class LoadRepository : ILoadRepository
    {
        public LoadViewModel GetLoad(int loadId)
        {
            const string query = "SELECT Id, Name, CreationDate, LastUpdateDate, Status, UserId, Comment FROM Load WHERE Id=@id";
            var dictionary = new Dictionary<string, object>{{"id", loadId}};

            var result = RequestHelper.ExecuteQuery(DataBaseConstants.ConnectionString, query, row => new LoadViewModel
            {
                Id = (int)row["Id"],
                Name = (string)row["Name"],
                CreationDate = (DateTime)row["CreationDate"],
                LastUpdateDate = (DateTime)row["LastUpdateDate"],
                Status = (int)row["Status"],
                UserName = (string)row["UserId"],                
            }, dictionary).First();
           
            return result;
        }

        public LoadDetailItemViewModel GetNextLoadItemToValidate(int loadId)
        {
            const string query =
                "SELECT Id, LoadId, Name, Status, LastUpdateDate, IsDimension, Step, CanUpdateViaExcel FROM LoadItem WHERE LoadId=@loadId AND Status=2";

            var dictionary = new Dictionary<string, object> {{"loadId", loadId}};

            var result =
                RequestHelper.ExecuteQuery(DataBaseConstants.ConnectionString, query, row => new LoadDetailItemViewModel
                {
                    Id = (int)row["Id"],
                    LoadId = (int)row["LoadId"],
                    Name = (string)row["Name"],
                    Status = (int)row["Status"],
                    LastUpdateDate = (DateTime)row["LastUpdateDate"],
                    isDimension = (bool)row["IsDimension"],
                    Step = (int)row["Step"],
                    CanUpdateViaExcel = (bool)row["CanUpdateViaExcel"]
                }, dictionary).FirstOrDefault();

            return result;
        }

        public LoadDetailItemViewModel GetLoadItem(int loadId, string loadItemName)
        {
            const string query = "SELECT Id, LoadId, Name, Status, LastUpdateDate, IsDimension, Step, CanUpdateViaExcel FROM LoadItem WHERE LoadId=@loadId AND Name=@name";
            var dictionary = new Dictionary<string, object>
            {
                { "loadId", loadId },
                { "name", loadItemName }
            };

            var loadItem =
                RequestHelper.ExecuteQuery(DataBaseConstants.ConnectionString, query, row => new LoadDetailItemViewModel
                {
                    Id = (int) row["Id"],
                    LoadId = (int) row["LoadId"],
                    Name = (string) row["Name"],
                    Status = (int) row["Status"],
                    LastUpdateDate = (DateTime) row["LastUpdateDate"],
                    isDimension = (bool) row["IsDimension"],
                    Step = (int) row["Step"],
                    CanUpdateViaExcel = (bool) row["CanUpdateViaExcel"]
                }, dictionary).First();

            return loadItem;
        }

        public IEnumerable<LoadViewModel> GetLoads(int status, int? loadId = null)
        {
            var dictionnary = new Dictionary<string, object>();
            IEnumerable<LoadViewModel> loads = new List<LoadViewModel>();
            string queryString = "SELECT l.Id as Id, l.Name as Name, CreationDate, LastUpdateDate, Status, ls.Name as StatusName, UserId FROM Load l, LoadStatus ls WHERE l.Status = ls.Id ";
            if (status != 0)
            {
                queryString += "AND Status = @status ";
                dictionnary.Add("status", status);
            }
            if (loadId != null)
            {
                queryString += "AND l.Id = @loadId ";
                dictionnary.Add("loadId", loadId);
            }

            queryString += "ORDER BY CreationDate DESC";

            loads =
                RequestHelper.ExecuteQuery<LoadViewModel>(DataBaseConstants.ConnectionString, queryString, MapDataToLoad, dictionnary);
            return loads;
        }

        public LoadSearchResponseViewModel GetPagedLoads(LoadSearchRequest request)
        {
            var loads = GetLoads(request.StatusId).ToList();

            var totalLoads = loads.Count();

            loads = loads
                .Skip(request.PageNumber*request.ItemsPerPage)
                .Take(request.ItemsPerPage)
                .ToList();

            return new LoadSearchResponseViewModel
            {
                Loads = loads,
                IsLastPage = (loads.Count() + (request.PageNumber*request.ItemsPerPage)) >= totalLoads,
                PageNumber = ++request.PageNumber,
                TotalLoads =  totalLoads
            };
        }

        public IEnumerable<LoadDetailItemViewModel> GetLoadItems(int loadId)
        {
            var loadItems = new List<LoadDetailItemViewModel>();
            const string queryString = @"SELECT l.Id as Id, l.Name as Name, Status, LastUpdateDate, IsDimension, ls.Name as StatusName, l.Step, l.CanUpdateViaExcel, l.LoadId as LoadId
                                        FROM LoadItem l, LoadStatus ls 
                                        WHERE l.Status = ls.Id AND LoadId = @LoadId ORDER BY l.Step ASC";
            var dictionnary = new Dictionary<string, object> { 
                    { "LoadId", loadId }
                };
            loadItems =
                RequestHelper.ExecuteQuery<LoadDetailItemViewModel>(DataBaseConstants.ConnectionString, queryString, MapDataToLoadItem, dictionnary).ToList();
            return loadItems;
        }

        public IEnumerable<LoadDetailItemViewModel> GetLoadItemById(int loadItemId)
        {
            var loadItems = new List<LoadDetailItemViewModel>();
            const string queryString = @"SELECT l.Id as Id, l.Name as Name, Status, LastUpdateDate, IsDimension, ls.Name as StatusName, l.Step, l.CanUpdateViaExcel, l.LoadId as LoadId
                                        FROM LoadItem l, LoadStatus ls 
                                        WHERE l.Status = ls.Id AND l.Id = @loadItemId ORDER BY l.Step ASC";
            var dictionnary = new Dictionary<string, object> { 
                    { "loadItemId", loadItemId }
                };
            loadItems =
                RequestHelper.ExecuteQuery<LoadDetailItemViewModel>(DataBaseConstants.ConnectionString, queryString, MapDataToLoadItem, dictionnary).ToList();
            return loadItems;
        }

        public IEnumerable<FilterItemViewModel> GetLoadStatusForFilter()
        {
            List<FilterItemViewModel> loadStatus = new List<FilterItemViewModel>() { new FilterItemViewModel
                {
                    Text = "All status",
                    TextShort = "All status",
                    Selected = true
                }};
            string queryString = "SELECT Id, Name FROM LoadStatus";
            loadStatus.AddRange(RequestHelper.ExecuteQuery<FilterItemViewModel>(DataBaseConstants.ConnectionString, queryString, MapDataToLoadStatusFilter));
            return loadStatus;
        }

        public void Cancel(int loadId)
        {
            var dictionnary = new Dictionary<string, object>
            {
                {"id", loadId},
                {"status", 5},
            };
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString,
                "UPDATE [dbo].[Load] SET [Status] = @status WHERE [Id] = @id", dictionnary);
        }

        public int Create(LoadViewModel load)
        {
            const string queryString =
                @"INSERT INTO [dbo].[Load]
                       ([Name]
                       ,[CreationDate]
                       ,[LastUpdateDate]
                       ,[Status]
                       ,[UserId])
                 OUTPUT INSERTED.ID
                 VALUES
                       (@Name
                       ,@CreationDate
                       ,@LastUpdateDate
                       ,@Status
                       ,@UserId)";

            var dictionnary = new Dictionary<string, object> { 
                { "Name", load.Name },
                { "CreationDate", DateTime.Now },
                { "LastUpdateDate", DateTime.Now },
                { "Status", load.Status },
                { "UserId", load.UserName },
            };
            return RequestHelper.ExecuteScalarRequest<int>(DataBaseConstants.ConnectionString, queryString, dictionnary);

        }

        public void CreateItem(LoadDetailItemViewModel loadItem)
        {
            const string queryString =
                @"INSERT INTO [dbo].[LoadItem]
                       ([LoadId]
                       ,[Name]
                       ,[Status]
                       ,[LastUpdateDate]
                       ,[IsDimension]
                       ,[Step]
                       ,[CanUpdateViaExcel])
                 VALUES
                       (@LoadId
                       ,@Name
                       ,@Status
                       ,@LastUpdateDate
                       ,@IsDimension
                       ,@Step
                       ,@CanUpdateViaExcel)";

            var dictionnary = new Dictionary<string, object> { 
                { "Name", loadItem.Name },
                { "LastUpdateDate", DateTime.Now },
                { "Status", loadItem.Status },
                { "LoadId", loadItem.LoadId },
                { "IsDimension", loadItem.isDimension },
                { "Step", loadItem.Step },
                { "CanUpdateViaExcel", loadItem.CanUpdateViaExcel },
            };
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, queryString, dictionnary);
        }

        public bool AnyItemForLoad(int loadId)
        {
            string queryString = "SELECT COUNT(*) FROM LoadItem WHERE LoadId = " + loadId;
            //var dictionnary = new Dictionary<string, object> { 
            //    { "LoadId", loadId },
            //};

            return RequestHelper.ExecuteScalarRequest<int>(DataBaseConstants.ConnectionString, queryString) > 0;

        }

        public void CreateDefaultLoadItemSet(int loadId)
        {
            CreateItem(new LoadDetailItemViewModel { LoadId = loadId, Name = LoadConstants.Currency, Status = 2, Step = 1, isDimension = true });
            CreateItem(new LoadDetailItemViewModel { LoadId = loadId, Name = LoadConstants.Product, Status = 2, Step = 1, isDimension = true });
            CreateItem(new LoadDetailItemViewModel { LoadId = loadId, Name = LoadConstants.Country, Status = 2, Step = 2, isDimension = true });
            CreateItem(new LoadDetailItemViewModel { LoadId = loadId, Name = LoadConstants.Sku, Status = 2, Step = 3, isDimension = true, CanUpdateViaExcel = true });
            CreateItem(new LoadDetailItemViewModel { LoadId = loadId, Name = LoadConstants.PriceType, Status = 2, Step = 4, isDimension = true });
            CreateItem(new LoadDetailItemViewModel { LoadId = loadId, Name = LoadConstants.NetData, Status = 2, Step = 5, isDimension = true });
            CreateItem(new LoadDetailItemViewModel { LoadId = loadId, Name = LoadConstants.Price, Status = 2, Step = 5 });
            CreateItem(new LoadDetailItemViewModel { LoadId = loadId, Name = LoadConstants.Volume, Status = 2, Step = 5 });
            CreateItem(new LoadDetailItemViewModel { LoadId = loadId, Name = LoadConstants.Event, Status = 2, Step = 5 });
            CreateItem(new LoadDetailItemViewModel { LoadId = loadId, Name = LoadConstants.Rule, Status = 2, Step = 5 });
            
        }


        public LoadDetailViewModel GetLoadDetail(int loadId)
        {
            var load = GetLoads(0, loadId).First();
            var detail = new LoadDetailViewModel { Name = load.Name, Status = load.Status };
            if (!AnyItemForLoad(loadId))
                CreateDefaultLoadItemSet(loadId);

            detail.Items = GetLoadItems(loadId).ToList();

            var data = GetRemainingRowsToValidateForLoadItems();

            foreach (var item in detail.Items)
            {
                var itm = data.FirstOrDefault(d => d.Id == item.Id);
                if (itm != null)
                    item.RowsToValidate = itm.RowsToValidate;
            }
            return detail;
        }

        private List<LoadDetailItemViewModel> GetRemainingRowsToValidateForLoadItems()
        {            
            const string query =
                "SELECT LoadItemId, Count(*) AS 'RowsToValidate' FROM (SELECT LoadItemId, GeographyId, ProductId FROM LoadItemDetail " +
                "WHERE Validated=0 GROUP BY LoadItemId, GeographyId, ProductId) AS V GROUP BY LoadItemId";

            var dictionary = new Dictionary<string, object>
            {        
                {"validated", false}
            };

            var result = RequestHelper.ExecuteQuery(DataBaseConstants.ConnectionString, query, row => new LoadDetailItemViewModel
            {
                Id = (int)row["LoadItemId"],
                RowsToValidate = (int)row["RowsToValidate"]
            }, dictionary).ToList();
            return result;
        }

        public void ValidateLoadItem(int loadId, string loadItemName)        
        {
            var updateDate = DateTime.Now;
            var dictionary = new Dictionary<string, object>();
            dictionary.Add("loadId", loadId);
            dictionary.Add("loadItemName", loadItemName);
            dictionary.Add("updateDate", updateDate);
            var query = @"UPDATE LoadItem
                            SET [Status]=3, [LastUpdateDate]=@updateDate
                            WHERE LoadId =@loadId and Name =@loadItemName";
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, dictionary);

            var queryLoad = @"Update [Load]
                            Set [Status] = 3, [LastUpdateDate]=@updateDate
                            WHERE Id IN (SELECT LoadId FROM LoadItem
			                             WHERE [Status] = 3
			                             GROUP BY LoadId
			                             HAVING Count(*) = 10)";

            dictionary.Remove("loadId");
            dictionary.Remove("loadItemName");
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, queryLoad, dictionary);

            if(loadItemName == LoadConstants.PriceType)
                CreateLoadItemDetailScenario(loadId);
        }

        public void ValidateLoadItemId(int loadItemId)
        {
            var date = DateTime.Now;
            var dictionary = new Dictionary<string, object>();
            dictionary.Add("loadItemId", loadItemId);
            dictionary.Add("updateDate", date);
            var query = @"UPDATE LoadItem
                            SET [Status]=3, [LastUpdateDate]=@updateDate
                            WHERE Id =@loadItemId";
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, dictionary);

            query = @"Update [Load] Set [LastUpdateDate]=@updateDate WHERE Id IN (SELECT LoadId FROM LoadItem WHERE [Id]=@loadItemId)";

            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, dictionary);

            var queryLoad = @"Update [Load]
                            Set [Status] = 3, [LastUpdateDate]=@updateDate
                            WHERE Id IN (SELECT LoadId FROM LoadItem
			                             WHERE [Status] = 3
			                             GROUP BY LoadId
			                             HAVING Count(*) = 10)";
            var dic = new Dictionary<string, object> {{"updateDate", date}};
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, queryLoad, dic);

            const string queryPush = "EXEC dbo.PushPriceUpdate @loadId,@loadItemId";
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, queryPush, new Dictionary<string, object> { { "loadId", 0 } , {"loadItemId" , loadItemId} });

            var updateTracking = "DELETE [IRP_DB].[dbo].[StatusTracking] WHERE DataTable=@dataTable";
            var parameters = new Dictionary<string, object>
            {
                {"dataTable", "PriceUpdate"},
                {"gprmPushDate", DateTime.Now },
                {"gprmPushBy",  "System"},
                {"pulled", false },

            };
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, updateTracking, parameters);
            updateTracking =
                @"INSERT INTO [IRP_DB].[dbo].[StatusTracking] (DataTable, GprmPushDate, GprmPushBy, Pulled)
                VALUES (@dataTable, @gprmPushDate, @gprmPushBy, @pulled)";
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, updateTracking, parameters);

            var currentLoadItem = GetLoadItemById(loadItemId).FirstOrDefault();
            if(currentLoadItem != null && currentLoadItem.Name == LoadConstants.PriceType)
            {
                CreateLoadItemDetailScenario(currentLoadItem.LoadId);
            }            
            if (currentLoadItem != null && currentLoadItem.Name == LoadConstants.Country)
            {
                CreateLoadItemDetailScenarioForSku(currentLoadItem.LoadId);
            }

            
        }

        public void CreateLoadItemDetailScenarioForSku(int loadId)
        {
            CreateProductGeographyScenario(loadId, LoadConstants.Sku);
            var skuToAUtomaticallyValidate = GetSkuProductGeographiesToNotValidate(loadId);
            var skuLoadItemId = GetLoadItemId(loadId, LoadConstants.Sku);

            var loadUpdates = new List<Tuple<int, int, int>>();

            foreach (var key in skuToAUtomaticallyValidate)
            {

                foreach (var geographyId in key.Value)
                {
                    //ValidateLoadItemDetail(loadId, LoadConstants.Sku, key.Key, geographyId);
                    loadUpdates.Add(new Tuple<int, int, int>(geographyId, key.Key, skuLoadItemId));
                }
            }

            const string queryUpdate = "EXEC ValidateLoadItem @loadItems";
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, queryUpdate,
                new Dictionary<string, List<Tuple<int, int, int>>> { { "loadItems", loadUpdates } });

            const string queryexec = "EXEC UpdateDatabaseForNewCountryProduct";
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, queryexec);

        }

        public void CreateLoadItemDetailScenario(int loadId)
        {//TODO: check to improve speed
            CreateProductGeographyScenario(loadId, LoadConstants.Price);
            CreateProductGeographyScenario(loadId, LoadConstants.Volume);
            CreateProductGeographyScenario(loadId, LoadConstants.Event);
            CreateProductGeographyScenario(loadId, LoadConstants.NetData);
            
            var priceToAutomaticallyValidate = GetDataProductGeographiesToNotValidate(loadId, DataTypes.Price);
            var volumeToAutomaticallyValidate = GetDataProductGeographiesToNotValidate(loadId, DataTypes.Volume);
            var eventToAutomaticallyValidate = GetDataProductGeographiesToNotValidate(loadId, DataTypes.Event);
            var netDataToAutomaticallyValidate = GetNetDataProductGeographiesToNotValidate(loadId);
            var loadUpdates = new List<Tuple<int, int, int>>();
            
            var ruleLoadItemId = GetLoadItemId(loadId, LoadConstants.Rule);
            var priceLoadItemId = GetLoadItemId(loadId, LoadConstants.Price);
            var eventLoadItemId = GetLoadItemId(loadId, LoadConstants.Event);
            var netDataLoadItemId = GetLoadItemId(loadId, LoadConstants.NetData);
            var volumeLoadItemId = GetLoadItemId(loadId, LoadConstants.Volume);


            foreach (var key in priceToAutomaticallyValidate)
            {

                foreach (var geographyId in key.Value)
                {
                    loadUpdates.Add(new Tuple<int, int, int>(geographyId, key.Key, priceLoadItemId));
                }
            }

            foreach (var key in eventToAutomaticallyValidate)
            {

                foreach (var geographyId in key.Value)
                {
                    loadUpdates.Add(new Tuple<int, int, int>(geographyId, key.Key, eventLoadItemId));
                }
            }

            foreach (var key in volumeToAutomaticallyValidate)
            {

                foreach (var geographyId in key.Value)
                {
                    loadUpdates.Add(new Tuple<int, int, int>(geographyId, key.Key, volumeLoadItemId));
                }
            }

            foreach (var key in netDataToAutomaticallyValidate)
            {

                foreach (var geographyId in key.Value)
                {
                    loadUpdates.Add(new Tuple<int, int, int>(geographyId, key.Key, netDataLoadItemId));
                }
            }

            CreateProductGeographyScenarioForRule(loadId, LoadConstants.Rule);
            var ruleToAutomaticallyValidate = GetRulesProductGeographiesToNotValidate(loadId);
            foreach (var key in ruleToAutomaticallyValidate)
            {

                foreach (var geographyId in key.Value)
                {
                    loadUpdates.Add(new Tuple<int, int, int>(geographyId, key.Key, ruleLoadItemId));
                }
            }
            const string queryUpdate = "EXEC ValidateLoadItem @loadItems";
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, queryUpdate,
                new Dictionary<string, List<Tuple<int, int, int>>> {{"loadItems", loadUpdates}});
        }


        #region Check differences
        public CurrencySearchResponseViewModel GetCurrencyToValidate(CurrencySearchRequestViewModel currencySearchRequest)
        {
            var loadedCurrencies = GetLoadedCurrency(currencySearchRequest.RateTypeId);
            var latestCurrencies = new CurrencyRepository().GetAllCurrencies((RateType)currencySearchRequest.RateTypeId, currencySearchRequest.VersionId).ToList();
            //var latestCurrencies = new CurrencyRepository().GetAllCurrencies((RateType)currencySearchRequest.RateTypeId).ToList();

            loadedCurrencies.AddRange(latestCurrencies);
            var groupedCurrencies = loadedCurrencies.GroupBy(c => c.Iso, (key, g) => new { Currency = key, Group = g }).ToList();

            var newCurrencies = new List<CurrencyViewModel>();
            foreach (var item in groupedCurrencies)
            {
                if (item.Group.Count() == 1)
                {
                    var currencyVal = item.Group.First();

                    if (string.IsNullOrEmpty(currencyVal.Tag))
                    {
                        currencyVal.Tag = "Deleted";
                    }
                    newCurrencies.Add(currencyVal);
                }
                else
                {
                    var loadedCurrency = item.Group.First(c => c.Tag == "Loaded");
                    var latestCurrency = item.Group.First(c => string.IsNullOrEmpty(c.Tag));

                    if (!DoubleHelper.Equal(loadedCurrency.UsdRate, latestCurrency.UsdRate) ||
                        !DoubleHelper.Equal(loadedCurrency.EurRate, latestCurrency.EurRate))
                    {
                        latestCurrency.Tag = "Edited";
                        latestCurrency.OldValue = new
                        {
                            UsdRate = latestCurrency.UsdRate,
                            EurRate = latestCurrency.EurRate
                        };
                        latestCurrency.EurRate = loadedCurrency.EurRate;
                        latestCurrency.UsdRate = loadedCurrency.UsdRate;
                        newCurrencies.Add(latestCurrency);
                    }
                    else
                    {
                        newCurrencies.Add(latestCurrency);
                    }
                }
            }

            var currencies = newCurrencies.OrderBy(c => c.Iso).ToList();
            var totalCurrencies = currencies.Count;
            currencies = currencies
                .Skip(currencySearchRequest.PageNumber * currencySearchRequest.ItemsPerPage)
                .Take(currencySearchRequest.ItemsPerPage)
                .ToList();

            var viewModel = new CurrencySearchResponseViewModel
            {
                Currencies = currencies,
                IsLastPage = (currencies.Count() + (currencySearchRequest.PageNumber * currencySearchRequest.ItemsPerPage)) >= totalCurrencies,
                PageNumber = ++currencySearchRequest.PageNumber,
                TotalCurrencies = totalCurrencies
            };

            return viewModel;
        }

        public IEnumerable<PriceListModel> GetSkuPriceListExcelExport()
        {
            string queryString = @"
                WITH Countries 
                AS
                (
	                SELECT  g.Id, g.Name, g.ShortName, g.Iso2, g.GeographyTypeId, g.DisplayCurrencyId, g.Active
	                FROM Geography g
	                UNION
	                SELECT g.Id, dd.Name, g.ShortName, g.Iso2, g.GeographyTypeId, g.DisplayCurrencyId, g.Active
	                FROM Geography g
	                INNER JOIN DimensionDictionary dd
	                ON dd.Dimension = 'Geography'
	                AND dd.DimensionId = g.Id 
                ),
                WithSku
                AS
                (
                SELECT [REGION]
                      ,[ORGANIZATION_ID]
                      ,[COUNTRY]
                      ,[PRICE_LIST_ID]
                      ,[PRICE_LIST_NAME]
                      ,[TYPE_TO_CHANNEL]
                      ,[STATUS_TO_STATUS]
                      ,[PRODUCT_FAMILY]
                      ,[PRODUCT_NUMBER]
                      ,[PRODUCT_NAME]
                      ,ISNULL(s.FactorUnit, CASE [UOM] WHEN 'mcg' THEN 1 ELSE 1000 END) FactorUnit
                      ,ISNULL(s.Dosage,[STRENGTH])[STRENGTH]
                      ,ISNULL(s.PackSize,[PACK]) PACK
                      ,[EFFECTIVE_DATE]
                      ,[EXPIRATION_DATE]
                      ,[AVAILABLE_ON_CONTRACTS]
                      ,[AVAILABLE_ON_PRICING_DOCS]
                      ,[PRICELIST_PRICE]
                      ,[PRICELIST_PRICE_MCG]
                      ,[PRICELIST_CURRENCY_TYPE]
                      ,[EXCHANGE_RATE]
                      ,[PRICELIST_PRICE_IN_USD]
                      ,[PRICELIST_PRICE_IN_USD_MCG]
                  FROM [gco_gprm_price_list_vw] pl
                  INNER JOIN Countries c
                  ON pl.COUNTRY = c.Name
                  LEFT JOIN SKU s
                  on pl.PRODUCT_NUMBER = s.ProductNumber
                  AND c.Id = s.GeographyId
                )
                SELECT [REGION]
                      ,[ORGANIZATION_ID]
                      ,[COUNTRY]
                      ,[PRICE_LIST_ID]
                      ,[PRICE_LIST_NAME]
                      ,[TYPE_TO_CHANNEL]
                      ,[STATUS_TO_STATUS]
                      ,[PRODUCT_FAMILY]
                      ,[PRODUCT_NUMBER]
                      ,[PRODUCT_NAME]
                      ,CASE WHEN FactorUnit = 1 THEN 'mcg' ELSE 'mg' END AS [UOM]
                      ,[STRENGTH]
                      ,[PACK]
                      ,[EFFECTIVE_DATE]
                      ,[EXPIRATION_DATE]
                      ,[AVAILABLE_ON_CONTRACTS]
                      ,[AVAILABLE_ON_PRICING_DOCS]
                      ,[PRICELIST_PRICE]
                      ,[PRICELIST_PRICE]
	                    / FactorUnit
	                    / CASE WHEN STRENGTH = 0 THEN 1 ELSE STRENGTH END 
	                    / CASE WHEN PACK = 0 THEN 1 ELSE PACK END  AS [PRICELIST_PRICE_MCG]      
                      ,[PRICELIST_CURRENCY_TYPE]
                      ,[EXCHANGE_RATE]
                      ,[PRICELIST_PRICE_IN_USD]
                      ,[PRICELIST_PRICE_IN_USD_MCG]
                  FROM WithSku

";
            return RequestHelper.ExecuteQuery(DataBaseConstants.ConnectionString, queryString, PriceListModelConverter);
        }

        public void SavePriceList(List<PriceListModel> prices)
        {
            var priceListDataTable = prices.ToDataTable();
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, "DELETE FROM [gco_gprm_price_list_vw]");
            RequestHelper.BulkInsert(DataBaseConstants.ConnectionString, "gco_gprm_price_list_vw", priceListDataTable);
        }

        private PriceListModel PriceListModelConverter(DataRow row)
        {
            var priceList = new PriceListModel();
            priceList.REGION = row["REGION"].PreventStringNull();
            priceList.ORGANIZATION_ID = row["ORGANIZATION_ID"].PreventStringNull();
            priceList.COUNTRY = row["COUNTRY"].PreventStringNull();
            priceList.PRICE_LIST_ID = row["PRICE_LIST_ID"].PreventStringNull();
            priceList.PRICE_LIST_NAME = row["PRICE_LIST_NAME"].PreventStringNull();
            priceList.TYPE_TO_CHANNEL = row["TYPE_TO_CHANNEL"].PreventStringNull();
            priceList.STATUS_TO_STATUS = row["STATUS_TO_STATUS"].PreventStringNull();
            priceList.PRODUCT_FAMILY = row["PRODUCT_FAMILY"].PreventStringNull();
            priceList.PRODUCT_NUMBER = row["PRODUCT_NUMBER"].PreventStringNull();
            priceList.PRODUCT_NAME = row["PRODUCT_NAME"].PreventStringNull();
            priceList.UOM = row["UOM"].PreventStringNull();
            priceList.STRENGTH = row["STRENGTH"].PreventNull();
            priceList.PACK = row["PACK"] == null ? (int?)null : Convert.ToInt32(row["PACK"]);
            priceList.EFFECTIVE_DATE = row["EFFECTIVE_DATE"] == null ? (DateTime?)null : (DateTime?)row["EFFECTIVE_DATE"];
            priceList.EXPIRATION_DATE = row["EXPIRATION_DATE"] == null ? (DateTime?)null : (DateTime?)row["EXPIRATION_DATE"];
            priceList.AVAILABLE_ON_CONTRACTS = row["AVAILABLE_ON_CONTRACTS"].PreventStringNull();
            priceList.AVAILABLE_ON_PRICING_DOCS = row["AVAILABLE_ON_PRICING_DOCS"].PreventStringNull();
            priceList.PRICELIST_PRICE = row["PRICELIST_PRICE"].PreventNull();
            priceList.PRICELIST_PRICE_MCG = row["PRICELIST_PRICE_MCG"].PreventNull();
            priceList.PRICELIST_CURRENCY_TYPE = row["PRICELIST_CURRENCY_TYPE"].PreventStringNull();
            priceList.EXCHANGE_RATE = row["EXCHANGE_RATE"].PreventNull();
            priceList.PRICELIST_PRICE_IN_USD = row["PRICELIST_PRICE_IN_USD"].PreventNull();
            priceList.PRICELIST_PRICE_IN_USD_MCG = row["PRICELIST_PRICE_IN_USD_MCG"].PreventNull();

            return priceList;
        }

        public PriceTypeSearchResponseViewModel GetPriceTypesToValidate(PriceTypeSearchRequestViewModel priceTypeSearchRequest)
        {
            var loadedPriceTypes = GetLoadedPriceTypes(priceTypeSearchRequest.SearchText);
            var latestPriceTypes = new PriceTypeRepository().GetAllPriceTypes(0, priceTypeSearchRequest.SearchText).Where(pt => pt.Status && pt.Id != ApplicationConstants.NotSpecifiedPrice);

            loadedPriceTypes.AddRange(latestPriceTypes);
            var groupedPriceTypes = loadedPriceTypes.GroupBy(p => p.Name.ToLower(), (key, g) => new { PriceType = key, Group = g }).ToList();

            var newPriceTypes = new List<PriceTypeViewModel>();
            foreach (var item in groupedPriceTypes)
            {
                if (item.Group.Count() == 1)
                {
                    var ptVal = item.Group.First();

                    if (string.IsNullOrEmpty(ptVal.Tag))
                    {
                        ptVal.Tag = "Deleted";
                    }
                    newPriceTypes.Add(ptVal);
                }
                //else
                //{
                //    var latestCurrency = item.Group.First(c => string.IsNullOrEmpty(c.Tag));
                //    newPriceTypes.Add(latestCurrency);
                //}
            }

            var priceTypes = newPriceTypes
                .Skip(priceTypeSearchRequest.PageNumber * priceTypeSearchRequest.ItemsPerPage)
                .Take(priceTypeSearchRequest.ItemsPerPage);


            var viewModel = new PriceTypeSearchResponseViewModel
            {
                PriceTypes = priceTypes,
                IsLastPage = (priceTypes.Count() + (priceTypeSearchRequest.PageNumber * priceTypeSearchRequest.ItemsPerPage)) >= newPriceTypes.Count(),
                PageNumber = ++priceTypeSearchRequest.PageNumber,
                TotalPriceTypes = newPriceTypes.Count()
            };
            return viewModel;
        }

        public Dictionary<int, List<int>> GetDataProductGeographiesToNotValidate(int loadId, DataTypes type, bool includeHistory = true)
        {
            var normalListToValidate = GetLoadItemDetailToValidate(loadId, LoadConstants.Price);
            Dictionary<int, List<int>> productGeographies = new Dictionary<int, List<int>>();
            var model = new List<DataViewModel>();
            if(type == DataTypes.Price)
                model = GetPriceToValidate(new PriceRequestViewModel { GeographyIds = new List<int>(), ProductId = 0 }, false);
            if(type == DataTypes.Event)
                model = GetEventToValidate(new EventSearchRequestViewModel
                {
                    ScenarioTypeId = 1,
                    GeographyId = new List<int> { },
                    ProductId = new List<int> { },
                    EventTypeId = new List<int> { }
                }, false);
            if (type == DataTypes.Volume)
                model = GetVolumeToValidate(new VolumeRequest { GeographyIds = normalListToValidate.Select(n => n.GeographyId).Distinct().ToList(), 
                    ProductId = null }, false);

            if (!includeHistory)
                model =
                    model.Where(m => m.DataTime >= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1)).ToList();
            
            var grouped = model.GroupBy(s => new { s.ProductId, s.GeographyId }, (key, group) => new { Key = key, Values = group }).ToList();

            foreach (var itemToValidate in normalListToValidate)
            {
                var shouldBeRemoved = false;
                if (!grouped.Any(p => p.Key.ProductId == itemToValidate.ProductId && p.Key.GeographyId == itemToValidate.GeographyId))
                {
                    shouldBeRemoved = true;
                }
                else 
                {
                    var group = grouped.First(g => g.Key.GeographyId == itemToValidate.GeographyId && g.Key.ProductId == itemToValidate.ProductId);
                    if (@group.Values.All(g => string.IsNullOrEmpty(g.Tag)))
                    {
                        shouldBeRemoved = true;
                    }
                }


                if (shouldBeRemoved)
                {
                    if (!productGeographies.ContainsKey(itemToValidate.ProductId))
                        productGeographies.Add(itemToValidate.ProductId, new List<int> { itemToValidate.GeographyId });
                    else
                        productGeographies[itemToValidate.ProductId].Add(itemToValidate.GeographyId);
                }
            }

            return productGeographies;
        }

        public Dictionary<int, List<int>> GetNetDataProductGeographiesToNotValidate(int loadId)
        {
            var normalListToValidate = GetLoadItemDetailToValidate(loadId, LoadConstants.NetData);
            Dictionary<int, List<int>> productGeographies = new Dictionary<int, List<int>>();
            var model = GetListToSalesToValidate(new ListToSalesSeachRequestViewModel { 
                ProductsId = normalListToValidate.Select(n => n.ProductId).Distinct().ToList(), 
                CountriesId = normalListToValidate.Select(n => n.GeographyId).Distinct().ToList(), 
                VersionId = new VersionRepository().GetCurrentVersion() });
            var grouped = model.ListToSales.GroupBy(s => new { s.ProductId, s.GeographyId }, (key, group) => new { Key = key, Values = group }).ToList();

            foreach (var itemToValidate in normalListToValidate)
            {
                bool shouldBeRemoved = false;
                if (!grouped.Any(p => p.Key.ProductId == itemToValidate.ProductId && p.Key.GeographyId == itemToValidate.GeographyId))
                {
                    shouldBeRemoved = true;
                }
                else
                {
                    var group = grouped.Where(g => g.Key.GeographyId == itemToValidate.GeographyId && g.Key.ProductId == itemToValidate.ProductId).First();
                    if (@group.Values.All(g => string.IsNullOrEmpty(g.Tag)))
                    {
                        shouldBeRemoved = true;
                    }
                }


                if (shouldBeRemoved)
                {
                    if (!productGeographies.ContainsKey(itemToValidate.ProductId))
                        productGeographies.Add(itemToValidate.ProductId, new List<int> { itemToValidate.GeographyId });
                    else
                        productGeographies[itemToValidate.ProductId].Add(itemToValidate.GeographyId);
                }
            }

            return productGeographies;
        }

       

        public Dictionary<int, List<int>> GetSkuProductGeographiesToNotValidate(int loadId)
        {
            Dictionary<int,List<int>> productGeographies = new Dictionary<int,List<int>>();
            var model = GetSkuToValidate(new SkuRequestViewModel { CountryId = 0, ProductId = 0, Status = 0, FormulationId = 0, ItemsPerPage = 0 });
            var groupedSkus = model.Skus.GroupBy(s => new { s.ProductId, s.GeographyId }, (key, group) => new { Key = key, Skus = group }).ToList();

            Dictionary<int, List<int>> productGeographiesHavingResult = new Dictionary<int, List<int>>();
            var normalListToValidate = GetLoadItemDetailToValidate(loadId, LoadConstants.Sku);


            foreach (var group in groupedSkus)
            {
                if (!productGeographiesHavingResult.ContainsKey(group.Key.ProductId))
                    productGeographiesHavingResult.Add(group.Key.ProductId, new List<int> { group.Key.GeographyId });
                else
                    productGeographiesHavingResult[group.Key.ProductId].Add(group.Key.GeographyId);


                if (group.Skus.All(s => string.IsNullOrEmpty(s.Tag) || s.Tag == "Deleted"))
                {
                    if (!productGeographies.ContainsKey(group.Key.ProductId))
                        productGeographies.Add(group.Key.ProductId, new List<int> { group.Key.GeographyId });
                    else
                        productGeographies[group.Key.ProductId].Add(group.Key.GeographyId);
                }
            }

            foreach (var itemToValidate in normalListToValidate)
            {
                if (productGeographiesHavingResult.ContainsKey(itemToValidate.ProductId) &&
                    !productGeographiesHavingResult[itemToValidate.ProductId].Contains(itemToValidate.GeographyId))
                {
                    if (!productGeographies.ContainsKey(itemToValidate.ProductId))
                        productGeographies.Add(itemToValidate.ProductId, new List<int> { itemToValidate.GeographyId });
                    else
                        productGeographies[itemToValidate.ProductId].Add(itemToValidate.GeographyId);
                }
            }

            return productGeographies;
        }

        public SkuResponseViewModel GetSkuToValidate(SkuRequestViewModel skuRequest)
        {
            var loadedSkus = GetLoadedSku(skuRequest.CountryId, skuRequest.ProductId);
            var latestSkus = new SkuRepository().GetAllSkus(skuRequest.CountryId, skuRequest.ProductId, skuRequest.Status, skuRequest.FormulationId);

            loadedSkus.AddRange(latestSkus);
            var groupedSkus = loadedSkus.GroupBy(p => new { p.ProductNumber, p.GeographyId}, (key, g) => new { Key = key, Group = g }).ToList();

            var newSkus = new List<SkuViewModel>();
            foreach (var item in groupedSkus)
            {
                if (item.Group.Count() == 1 || !item.Group.Any(c => string.IsNullOrEmpty(c.Tag)))
                {
                    var itemVal = item.Group.First();

                    if (string.IsNullOrEmpty(itemVal.Tag))
                    {
                        itemVal.Tag = "Deleted";
                    }
                    newSkus.Add(itemVal);
                }
                else
                {
                    var loadedSku = item.Group.First(c => c.Tag == "Loaded");
                    var latestSku = item.Group.First(c => string.IsNullOrEmpty(c.Tag));

                    //if (loadedSku.PackSize != latestSku.PackSize ||
                    //    loadedSku.FactorUnit != latestSku.FactorUnit || latestSku.Status == false)
                    //{
                    //    latestSku.Tag = "Edited";
                    //    latestSku.OldValue = new
                    //    {
                    //        PackSize = latestSku.PackSize,
                    //        FactorUnit = latestSku.FactorUnit
                    //    };
                    //    latestSku.PackSize = loadedSku.PackSize;
                    //    latestSku.FactorUnit = loadedSku.FactorUnit;
                    //    newSkus.Add(latestSku);
                    //}
                    //else
                    //{
                        newSkus.Add(latestSku);
                    //}
                }
            }
            var totalSkus = newSkus.Count;

            newSkus = newSkus.OrderBy(s => s.Dosage).ToList();

            var result = newSkus
                .Skip(skuRequest.PageNumber * skuRequest.ItemsPerPage)
                .Take(skuRequest.ItemsPerPage)
                .ToList();

            var viewModel = new SkuResponseViewModel
            {
                Skus = skuRequest.ItemsPerPage == 0 ? newSkus : result,
                IsLastPage = (result.Count + (skuRequest.PageNumber * skuRequest.ItemsPerPage)) >= totalSkus,
                PageNumber = ++skuRequest.PageNumber,
                TotalSkus = totalSkus
            };

            return viewModel;
        }

        public CountrySearchResponseViewModel GetCountryToValidate(CountrySearchRequestViewModel countrySearch)
        {
            var loadedCountries = GetLoadedCountry();
            var latestCountries = new CountryRepository().GetAllCountries().Where(c => c.Active);

            loadedCountries.AddRange(latestCountries);
            var groupedCountries = loadedCountries.GroupBy(p => p.Name, (key, g) => new { Country = key, Group = g }).ToList();

            var newCountries = new List<CountryViewModel>();
            foreach (var item in groupedCountries)
            {
                if (item.Group.Count() == 1)
                {
                    var itemVal = item.Group.First();

                    if (string.IsNullOrEmpty(itemVal.Tag))
                    {
                        itemVal.Tag = "Deleted";
                    }
                    newCountries.Add(itemVal);
                }
                //else
                //{
                //    var latestCountry = item.Group.First(c => string.IsNullOrEmpty(c.Tag));
                //    newCountries.Add(latestCountry);
                //}
            }
            var totalCountries = newCountries.Count;
            var countries = newCountries
                .Skip(countrySearch.PageNumber * countrySearch.ItemsPerPage)
                .Take(countrySearch.ItemsPerPage)
                .ToList();

            var viewModel = new CountrySearchResponseViewModel
            {
                Countries = countries,
                IsLastPage = (countries.Count() + (countrySearch.PageNumber * countrySearch.ItemsPerPage)) >= newCountries.Count,
                PageNumber = ++countrySearch.PageNumber,
                TotalCountries = totalCountries
            };

            return viewModel;
        }

        public ProductSearchResponseViewModel GetProductToValidate(ProductSearchRequestViewModel productSearch)
        {
            var loadedProducts = GetLoadedProduct().ToList();
            var latestProducts = new ProductRepository().GetAllProducts().Where(p => p.Active && p.Id != ApplicationConstants.NotSpecifiedProduct).ToList();

            loadedProducts.AddRange(latestProducts);
            var groupedProducts = loadedProducts.GroupBy(p => p.Name, (key, g) => new { Country = key, Group = g }).ToList();

            var newProducts = new List<ProductViewModel>();
            foreach (var item in groupedProducts)
            {
                if (item.Group.Count() == 1)
                {
                    var itemVal = item.Group.First();

                    if (string.IsNullOrEmpty(itemVal.Tag))
                    {
                        itemVal.Tag = "Deleted";
                    }
                    newProducts.Add(itemVal);
                }
                else
                {
                    var latestProduct = item.Group.First(c => string.IsNullOrEmpty(c.Tag));
                    newProducts.Add(latestProduct);
                }
            }
            var result = newProducts
                .Skip(productSearch.PageNumber * productSearch.ItemsPerPage)
                .Take(productSearch.ItemsPerPage)
                .ToList();

            var viewModel = new ProductSearchResponseViewModel
            {
                Products = result,
                IsLastPage = (result.Count() + (productSearch.PageNumber * productSearch.ItemsPerPage)) >= newProducts.Count,
                PageNumber = ++productSearch.PageNumber,
                TotalProducts = newProducts.Count
            };

            return viewModel;
        }

        public ListToSalesSearchResponseViewModel GetListToSalesToValidate(ListToSalesSeachRequestViewModel model)
        {
            var loadeds = GetLoadedListToSales(model.ProductsId, model.CountriesId);
            var latests = new ListToSalesRepository().GetVersionListToSales(model.VersionId, model.CountriesId, model.ProductsId).ToList();

            loadeds.AddRange(latests);
            var grouped = loadeds.GroupBy(p => new { p.GeographyId, p.ProductId }, (key, g) => new { Key = key, Group = g }).ToList();

            var newListToSales = new List<ListToSalesViewModel>();
            foreach (var item in grouped)
            {
                if (item.Group.Count() == 1)
                {
                    var itemVal = item.Group.First();
                    newListToSales.Add(itemVal);
                }
                else
                {
                    var latest = item.Group.First(c => string.IsNullOrEmpty(c.Tag));
                    var loaded = item.Group.First(c => c.Tag == "Loaded");
                    if (!DoubleHelper.Equal(latest.Asp, loaded.Asp))
                    {
                        double percentage = 0;
                        if (latest.Asp != 0 )
                            percentage = (loaded.Asp / latest.Asp) - 1;
                        latest.OldValue = new { Asp = latest.Asp };
                        latest.PercentageVariationFromX = percentage;
                        latest.Tag = "Edited";
                        latest.Asp = loaded.Asp;
                        newListToSales.Add(latest);
                    }
                    else
                    {
                        newListToSales.Add(latest);
                    }
                }
            }
            var result = newListToSales.Where(l => l.Asp != 0 || !string.IsNullOrEmpty(l.Tag)).OrderBy(l => l.GeographyName)
                .ToList();

            var viewModel = new ListToSalesSearchResponseViewModel
            {
                ListToSales = result,
                IsLastPage = (result.Count() + (model.PageNumber * model.ItemsPerPage)) >= newListToSales.Count,
                PageNumber = ++model.PageNumber,
                TotalListToSales = newListToSales.Count
            };

            return viewModel;
        }

        public List<DataViewModel> GetPriceToValidate(PriceRequestViewModel model, bool fillData = true)
        {
            var loadedPrices = GetLoadedPrice(model.GeographyIds, model.ProductId);
            var latestPrices = ((IEnumerable<DataViewModel>)new PriceRepository().GetPrices(model, fillData)).ToList();

            loadedPrices.AddRange(latestPrices);
            var groupedPrices = loadedPrices.GroupBy(p => p.PermId, (key, g) => new { Price = key, Group = g }).ToList();

            var newPrices = new List<DataViewModel>();
            foreach (var item in groupedPrices)
            {
                if (item.Group.Count() == 1)
                {
                    var itemVal = item.Group.First();

                    if (string.IsNullOrEmpty(itemVal.Tag))
                    {
                        itemVal.Tag = "Deleted";
                    }

                    if (itemVal.Value == null)
                        itemVal.Tag = "";
                    newPrices.Add(itemVal);
                }
                else
                {
                    var latestPrice = item.Group.First(c => string.IsNullOrEmpty(c.Tag));
                    var loadedPrice = item.Group.First(c => c.Tag == "Loaded");
                    if (!DoubleHelper.Equal(latestPrice.Value, loadedPrice.Value))
                    {
                        double percentage = 0;
                        if(loadedPrice.Value.HasValue && latestPrice.Value.HasValue)
                            percentage = (loadedPrice.Value.Value / latestPrice.Value.Value) - 1;
                        latestPrice.OldValue = new { Value = latestPrice.Value };
                        latestPrice.Tag = "Edited";
                        latestPrice.Value = loadedPrice.Value;
                        latestPrice.PercentageVariationFromX = System.Math.Abs(percentage);
                        if (!loadedPrice.Value.HasValue || loadedPrice.Value == 0)
                            latestPrice.Tag = "Deleted";
                        newPrices.Add(latestPrice);
                    }
                    else
                    {
                        newPrices.Add(latestPrice);
                    }
                }
            }
            var dataRepository = new DataRepository();
            newPrices = dataRepository.DataVariation(dataRepository.FillData(newPrices, model.GeographyIds, new List<int> { model.ProductId }, new List<int> { (int)DataTypes.Price }).ToList()).ToList();
            foreach (var price in newPrices)
            {
                if (price.Tag == "Loaded")
                {
                    price.PercentageVariationFromX = price.PercentageVariation.HasValue ? System.Math.Abs(price.PercentageVariation.Value) : 0;
                }
            }
            return newPrices.OrderBy(p => p.DataTime).ToList();
        }

        public List<DataViewModel> GetVolumeToValidate(VolumeRequest model, bool fillData = true)
        {
            var loadedVolumes = GetLoadedVolume(model.GeographyIds, model.ProductId);
            var latestVolumes = ((IEnumerable<DataViewModel>)new VolumeRepository().GetVolumes(model, fillData)).ToList();

            loadedVolumes.AddRange(latestVolumes);
            var groupedVolumes = loadedVolumes.GroupBy(p => p.PermId, (key, g) => new { Volume = key, Group = g }).ToList();

            var newVolumes = new List<DataViewModel>();
            foreach (var item in groupedVolumes)
            {
                if (item.Group.Count() == 1)
                {
                    var itemVal = item.Group.First();

                    if (string.IsNullOrEmpty(itemVal.Tag))
                    {
                        itemVal.Tag = "Deleted";
                    }

                    if (!itemVal.Value.HasValue)
                        itemVal.Tag = null;

                    newVolumes.Add(itemVal);
                }
                else
                {
                    var latest = item.Group.First(c => string.IsNullOrEmpty(c.Tag));
                    var loaded = item.Group.First(c => c.Tag == "Loaded");
                    if (!DoubleHelper.Equal(latest.Value, loaded.Value))
                    {
                        double percentage = 0;
                        if (loaded.Value.HasValue && latest.Value.HasValue)
                            percentage = (loaded.Value.Value / latest.Value.Value) - 1;
                        latest.OldValue = new { Value = latest.Value };
                        latest.Tag = "Edited";
                        latest.Value = loaded.Value;
                        latest.PercentageVariationFromX = System.Math.Abs(percentage);
                        if (!loaded.Value.HasValue || loaded.Value == 0)
                            latest.Tag = "Deleted";
                        newVolumes.Add(latest);
                    }
                    else
                    {
                        latest.Tag = null;
                        newVolumes.Add(latest);
                    }
                }
            }
            var dataRepository = new DataRepository();
            newVolumes = dataRepository.DataVariation(dataRepository.FillData(newVolumes, model.GeographyIds, new List<int> { model.ProductId ?? ApplicationConstants.NotSpecifiedProduct }, new List<int> { (int)DataTypes.Volume }).ToList()).ToList();
            foreach (var volume in newVolumes)
            {
                if (volume.Tag == "Loaded")
                {
                    volume.PercentageVariationFromX = volume.PercentageVariation.HasValue ? System.Math.Abs(volume.PercentageVariation.Value) : 0;
                }
            }
            return newVolumes.OrderBy(p => p.DataTime).ToList();
        }

        public List<DataViewModel> GetEventToValidate(EventSearchRequestViewModel model, bool fillData = true)
        {
            DateTime maxDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);//new DateTime(1900,1,1);
            var latestEvents = ((IEnumerable<DataViewModel>)new EventRepository().GetEvents(model, fillData)).ToList();
            if(model.VersionId != null)
                maxDate = new DataRepository().GetVersionStartTime(model.VersionId.Value);
            var loadedEvents = model.SystemId == 16 ? GetLoadedEvent(model.GeographyId, model.ProductId.FirstOrDefault()).Where(d => d.DataTime > maxDate).ToList() : GetIrpEvent(model.GeographyId, model.ProductId.FirstOrDefault()).Where(d => d.DataTime > maxDate).ToList();
            

            loadedEvents.AddRange(latestEvents);
            //var groupedEvents = loadedEvents.GroupBy(p => new { p.PermId, p.EventTypeId }, (key, g) => new { Event = key, Group = g }).ToList();
            var groupedEvents = loadedEvents.GroupBy(p => new { p.PermId }, (key, g) => new { Event = key, Group = g }).ToList();

            var newEvents = new List<DataViewModel>();
            foreach (var item in groupedEvents)
            {
                if (item.Group.Count() == 1 || !item.Group.Any(c => string.IsNullOrEmpty(c.Tag)))
                {
                    var itemVal = item.Group.First();

                    if (string.IsNullOrEmpty(itemVal.Tag) && itemVal.DataTime > maxDate && itemVal.EventTypeId != (int)EventTypes.NoEvent && itemVal.EventTypeId != (int)EventTypes.NotSpecified)
                    {
                        itemVal.Tag = "Deleted";
                    }
                    newEvents.Add(itemVal);
                }
                else
                {
                    var latest = item.Group.First(c => string.IsNullOrEmpty(c.Tag));
                    var loaded = item.Group.First(c => c.Tag == "Loaded");
                    if (!DoubleHelper.Equal(latest.Value, loaded.Value))
                    {
                        double percentage = 0;
                        if (loaded.Value.HasValue && latest.Value.HasValue)
                            percentage = (loaded.Value.Value / latest.Value.Value) - 1;
                        latest.OldValue = new { Value = latest.Value };
                        latest.Tag = "Edited";
                        latest.Value = loaded.Value;
                        latest.EventTypeId = loaded.EventTypeId;
                        latest.PercentageVariationFromX = System.Math.Abs(percentage);
                        newEvents.Add(latest);
                    }
                    else
                    {
                        newEvents.Add(latest);
                    }
                }
            }
            if (fillData)
                newEvents = new DataRepository().FillData(newEvents, model.GeographyId, model.ProductId, new List<int> { (int)DataTypes.Event }).ToList();
            
            return newEvents.OrderBy(p => p.DataTime).ToList();
        }

        public Dictionary<int, List<int>> GetRulesProductGeographiesToNotValidate(int loadId, int systemId = ApplicationConstants.IrpLoadSystem)
        {   
            var gprmRuleRepository = new GprmRuleRepository();
            
            var geographyList = new List<int>();

            var defaultApplicableFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var geographies = new CountryRepository().GetAllCountries().Select(gAc => gAc.Id);
            var gprmRule0 = gprmRuleRepository.GetVersionRule(0, ApplicationConstants.NotSpecifiedProduct, DateTime.Now,0);

            foreach (var geography in geographies)
            {   
                var loadRule = systemId == 16 ? GetLoadedRules(geography) : GetIrpRules(geography);
                if (
                    gprmRule0.Any(aF => aF.GeographyId == geography && aF.ApplicableFrom <= defaultApplicableFrom))
                {
                    var applicableFrom = gprmRule0.Where(aF => aF.GeographyId == geography && aF.ApplicableFrom <= defaultApplicableFrom).Max(aF => aF.ApplicableFrom);
                    var gprmRule = gprmRule0.FirstOrDefault(aF => aF.GeographyId == geography && aF.ApplicableFrom == applicableFrom);
                    if (CompareRulesForLoad(gprmRule, loadRule))
                        geographyList.Add(geography);    
                }
                else if (loadRule.Count == 0)
                    geographyList.Add(geography);
                
            }

            return new Dictionary<int, List<int>>{{ApplicationConstants.NotSpecifiedProduct,geographyList}};

        }

        private static bool CompareRulesForLoad(RuleViewModel gprmRule, List<LoadedRulesViewModel> loadRule)
        {
            if (gprmRule != null && loadRule != null && loadRule.Count != 0)
            {
                var ruleEqual = gprmRule.IrpRuleListId == loadRule.First().IrpRuleId;
                ruleEqual = ruleEqual && (loadRule.First().EffectiveLag == -1 || (gprmRule.EffectiveLag == loadRule.First().EffectiveLag));
                ruleEqual = ruleEqual && (loadRule.First().LookBack == -1 || (gprmRule.LookBack == loadRule.First().LookBack));
                ruleEqual = ruleEqual && (gprmRule.DefaultBasket.Count() == loadRule.Count);
                return ruleEqual 
                    && loadRule.All(l => gprmRule.DefaultBasket.Select(d => d.ReferencedGeographyId).Contains(l.ReferencedCountryId))
                    && gprmRule.DefaultBasket.All(l => loadRule.Select(d => d.ReferencedCountryId).Contains(l.ReferencedGeographyId));
            }
            return ((gprmRule != null && gprmRule.GprmMathId == (int)MathTypes.NotSpecified) || gprmRule == null) && (loadRule == null || loadRule.Count == 0);
        }

        public RuleDefinitionViewModel GetRulesToValidate(RuleRequest model)
        {
            var latestRule = new GprmRuleRepository().GetRules(model.VersionId, model.GeographyId, model.ProductId, model.GprmRuleTypeId, model.ApplicableFrom);

            if (model.ProductId > 0)
                return latestRule;

            var loadedRules = model.SystemId == 16 ? GetLoadedRules(model.GeographyId) : GetIrpRules(model.GeographyId, model.GprmRuleTypeId);
            var defaultBasket = new List<Basket>();
            foreach (var loadedRule in loadedRules)
            {
                defaultBasket.Add(new Basket
                {
                    Default = true,
                    ReferencedGeography = loadedRule.ReferencedGeography,
                    ReferencedGeographyId = loadedRule.ReferencedCountryId,
                    ReferencedPriceTypeOptions = new PriceTypeRepository().GetVersionPriceTypes(1, model.GeographyId, model.ProductId).ToList(),
                    Tag = "Loaded",
                });
            }
            if (latestRule.DefaultBasket != null)
            {
                defaultBasket.AddRange(latestRule.DefaultBasket);
            }
            var groupedBasket = defaultBasket.GroupBy(b => b.ReferencedGeographyId, (key, group) => new { ReferencedGeographyId = key, Group = group }).ToList();
            latestRule.DefaultBasket = new List<Basket>();
            foreach (var basket in groupedBasket)
            {
                if (basket.Group.Count() == 1)
                {
                    var countryBasket = basket.Group.First();
                    countryBasket.Edited = true;
                    if (countryBasket.Tag != "Loaded")
                        countryBasket.Active = false;
                    latestRule.DefaultBasket.Add(countryBasket);
                }
                else
                {
                    if (basket.Group.FirstOrDefault(b => string.IsNullOrEmpty(b.Tag)) != null)
                        latestRule.DefaultBasket.Add(basket.Group.First(b => string.IsNullOrEmpty(b.Tag)));
                }
            }
            var irpRule = loadedRules.FirstOrDefault();
            if (irpRule != null && (latestRule.IrpRuleListId != irpRule.IrpRuleId || (irpRule.LookBack != -1 && latestRule.LookBack != irpRule.LookBack) || (irpRule.EffectiveLag != -1 && latestRule.EffectiveLag != irpRule.EffectiveLag)))
            {
                latestRule.Edited = true;
                var subRuleCalculation = GetSubRuleCalculation(irpRule.IrpRuleId);
                latestRule.IrpRuleListId = irpRule.IrpRuleId;
                latestRule.GprmMathId = subRuleCalculation.First().IrpMathId;
                latestRule.LookBack = irpRule.LookBack != -1 ? irpRule.LookBack : latestRule.LookBack;
                latestRule.EffectiveLag = irpRule.EffectiveLag != -1 ? irpRule.EffectiveLag : latestRule.EffectiveLag;
                latestRule.AllowIncrease = false;
                //HERE
                latestRule.ReferencedData.Where(rD => rD.SubRuleIndex >= subRuleCalculation.Count).ForEach(rD =>
                {
                    rD.Active = false;
                    rD.Edited = true;
                });
                var latestRuleKeep = latestRule.ReferencedData.Where(rD => rD.SubRuleIndex >= subRuleCalculation.Count);
                latestRule.ReferencedData = new List<SubRuleViewModel>();
                latestRule.ReferencedData.AddRange(latestRuleKeep);
                
                if (subRuleCalculation.Count > 1)
                {
                    latestRule.IrpRuleListId = irpRule.IrpRuleId;
                    latestRule.GprmMathId = subRuleCalculation.First().IrpMathId;

                    foreach (var loaded in subRuleCalculation.Where(c => c.UpId != null).ToList())
                    {
                        latestRule.ReferencedData.Add(new SubRuleViewModel
                        {
                            Argument = loaded.Argument,
                            Basket = defaultBasket,
                            Default = true,
                            GprmMathId = loaded.IrpMathId,
                            SubRuleIndex = loaded.Argument,
                            WeightTypeId = ApplicationConstants.DefaultWeightTypeId,
                            Edited = true,
                            Active = true
                        });
                    }
                }
            }
            else if (irpRule == null)
            {
                latestRule.Edited = true;
                latestRule.GprmMathId = (int)MathTypes.NotSpecified;
                foreach (var basket in latestRule.DefaultBasket)
                {
                    basket.Active = false;
                    basket.Edited = true;
                }
                if (latestRule.ReferencedData.Count > 0)
                {
                    foreach (var rD in latestRule.ReferencedData)
                    {
                        rD.Active = false;
                        rD.Edited = true;
                        foreach (var basket in rD.Basket)
                        {
                            basket.Active = false;
                            basket.Edited = true;
                        }
                    }
                }
            }
            latestRule.DefaultBasket = latestRule.DefaultBasket.OrderBy(dB => dB.ReferencedGeography).ToList();
            return latestRule;
        }

        public List<IrpRuleCalculationViewModel> GetSubRuleCalculation(int irpRule)
        {
            var dictionnary = new Dictionary<string, object> { 
                { "irpRule", irpRule }
            };
            var query = @"
                    SELECT [Id] 
                          ,[IrpRuleListId] 
                          ,[IrpMathId]
                          ,[Argument]
                          ,[UpId]
                      FROM [IrpRuleCalculation]
                      WHERE IrpRuleListId = @irpRule";
            var result = RequestHelper.ExecuteQuery<IrpRuleCalculationViewModel>(DataBaseConstants.ConnectionString, query, MapDataToSubRuleCalculation, dictionnary).ToList();
            if (!result.Any())
                result.Add(new IrpRuleCalculationViewModel
                {
                    IrpMathId = (int)MathTypes.NotSpecified,
                    Argument = 0,
                    UpId = null
                });
            if (result.First().IrpMathId <= 0)
            {
                result.First().IrpMathId = (int)MathTypes.NotSpecified;
                result.First().Argument = 0;
                result.First().UpId = null;
            }
                
            return result;
        }

        public List<IrpRuleCalculationViewModel> GetAllRuleCalculation()
        {
            var query = @"
                    SELECT [Id] 
                          ,[IrpRuleListId] 
                          ,[IrpMathId]
                          ,[Argument]
                          ,[UpId]
                      FROM [IrpRuleCalculation]";
            var result = RequestHelper.ExecuteQuery<IrpRuleCalculationViewModel>(DataBaseConstants.ConnectionString, query, MapDataToSubRuleCalculation).ToList();
            return result;
        }

        #endregion

        #region Model function
        public List<CurrencyViewModel> GetLoadedCurrency(int rateTypeId)
        {
            var rateFlag = rateTypeId == (int)RateType.Budget ? "P" : "E";
            var dictionnary = new Dictionary<string, object> { 
                { "rateFlag", rateFlag }
            };
            var query = @"
                    SELECT [FROM_CURR]
                          ,[TO_CURR]
                          ,[EFFECTIVE_DATE]
                          ,[EXCHANGE_RATE]
                      FROM [gco_sap_curr_exchange_rate_vw] s
                      WHERE RATE_TYPE = @rateFlag
                      AND TO_CURR = 'USD'
                      AND LEN(FROM_CURR) = 3
                      AND YEAR(s.EFFECTIVE_DATE) >= (YEAR(GetDate()) - 1)
                      AND s.EFFECTIVE_DATE = (SELECT max(t.EFFECTIVE_DATE)
                                                               FROM [gco_sap_curr_exchange_rate_vw] t
                                                               WHERE t.FROM_CURR = s.FROM_CURR
                                                               AND t.RATE_TYPE = @rateFlag
                                                               AND t.TO_CURR = 'USD'
                                                               AND LEN(FROM_CURR) = 3)
                     
					  union select 'USD','USD',(SELECT max(t.EFFECTIVE_DATE)
                                                               FROM [gco_sap_curr_exchange_rate_vw] t
                                                               WHERE t.FROM_CURR = 'EUR'
                                                               AND t.RATE_TYPE = @rateFlag
                                                               AND t.TO_CURR = 'USD'
                                                               AND LEN(FROM_CURR) = 3),1
 
															    ORDER BY FROM_CURR
";
            var currencies = RequestHelper.ExecuteQuery<CurrencyViewModel>(DataBaseConstants.ConnectionString, query, MapDataToCurrency, dictionnary).ToList();
            var eurVal = currencies.FirstOrDefault(c => c.Iso == "EUR");

            
            foreach (var currency in currencies)
            {
                if (eurVal != null)
                {
                    currency.EurRate = currency.UsdRate / eurVal.UsdRate;
                }
                else
                {
                    currency.EurRate = 0;
                }
            }

            return currencies;
        }

        public List<PriceTypeViewModel> GetLoadedPriceTypes(string searchText)
        {
            var query = @"
                    SELECT distinct
                          [TYPE_TO_CHANNEL]
                          ,[STATUS_TO_STATUS]
                          ,c.Iso as CurrencyName,
	                      c.Id as CurrencyId
                      FROM [gco_gprm_price_list_vw] lpl
                      LEFT OUTER JOIN Currency c ON lpl.PRICELIST_CURRENCY_TYPE = c.Iso
                      WHERE EXPIRATION_DATE > DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) - 1, 0)";
            var dictionary = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(searchText))
            {
                query += "AND ( [TYPE_TO_CHANNEL] LIKE @searchText OR [STATUS_TO_STATUS] LIKE @searchText OR c.Iso LIKE @searchText ) ";
                dictionary.Add("searchText", "%" + searchText + "%");
            }
            //query += "ORDER BY P.Name";
            return RequestHelper.ExecuteQuery<PriceTypeViewModel>(DataBaseConstants.ConnectionString, query, MapDataToPriceTypes, dictionary).OrderBy(p => p.Name).ToList();
        }

        public List<SkuViewModel> GetLoadedSku(int countryId, int productId)
        {
            var dictionary = new Dictionary<string, object>();
            var query = @"SELECT DISTINCT
	                        DC.DimensionId as GeographyId
	                        ,DP.DimensionId as ProductId
                            ,[PRODUCT_NAME]
                            ,FactorUnit = 
	                        CASE UOM
		                        WHEN 'mg' THEN 1000
		                        WHEN 'mcg' THEN 1
		                        ELSE null
	                        END
                            ,[STRENGTH]
                            ,[PACK]
	                        ,PRODUCT_NUMBER
                        FROM (SELECT COUNTRY, PRODUCT_NAME, STRENGTH, PACK, UOM, PRODUCT_NUMBER, ProductFamily =
		                        CASE 
		                            WHEN PRODUCT_FAMILY = 'ARANESP' AND STRENGTH < 150 THEN 'ARANESP - NEPHRO'
		                            WHEN PRODUCT_FAMILY = 'ARANESP' AND STRENGTH = 150 THEN 'ARANESP 150'
		                            WHEN PRODUCT_FAMILY = 'ARANESP' AND STRENGTH > 150 THEN 'ARANESP - ONCO'
		                            ELSE PRODUCT_FAMILY
		                        END,
		                        ROW_NUMBER() OVER(PARTITION BY COUNTRY, PRODUCT_NUMBER ORDER BY PRODUCT_NUMBER) as seqNumber                
	                            FROM [gco_gprm_price_list_vw]) pl
                        INNER JOIN [DimensionDictionary] DC on [COUNTRY]=DC.Name
                        INNER JOIN [DimensionDictionary] DP on ProductFamily=DP.Name
                        WHERE seqNumber = 1 AND DC.SystemId=16 AND DP.SystemId=16";

            if (countryId > 0)
            {
                query += " AND DC.Dimension= 'Geography' AND  DC.DimensionId = @countryId ";
                dictionary.Add("countryId", countryId);
            }

            //PRODUCT FILTER
            if (productId > 0)
            {
                query += " AND DP.Dimension = 'Product' AND DP.DimensionId = @productId ";
                dictionary.Add("productId", productId);
            }

            //query += " ORDER BY S.Dosage";
            return RequestHelper.ExecuteQuery<SkuViewModel>(DataBaseConstants.ConnectionString, query, MapDataToSku, dictionary).ToList();
        }

        public List<ProductViewModel> GetLoadedProduct()
        {
            var query = @"
                    SELECT DISTINCT DP.DimensionId as Id,
                        ISNULL(pr.Name,[PRODUCT_NAME]) as Name
                    FROM [gco_submn_product_vw] p
                    LEFT OUTER JOIN [DimensionDictionary] DP on [PRODUCT_NAME]=DP.Name
                    LEFT OUTER JOIN [Product] pr ON pr.Id = DP.DimensionId";
            return RequestHelper.ExecuteQuery<ProductViewModel>(DataBaseConstants.ConnectionString, query, MapDataToProduct).ToList();
        }

        public List<CountryViewModel> GetLoadedCountry()
        {
            var query = @"
                    SELECT DISTINCT DC.DimensionId as GeographyId
	                    ,ISNULL(g.Name, sc.COUNTRY_NAME) as Name
                        ,[COUNTRY_CODE]
                    FROM [gco_submn_country_vw] sc
                    LEFT OUTER JOIN [DimensionDictionary] DC on COUNTRY_NAME=DC.Name
                    LEFT OUTER JOIN [Geography] g ON g.Id = DC.DimensionId
                    WHERE sc.IS_ACTIVE = 'Y' ";
            return RequestHelper.ExecuteQuery<CountryViewModel>(DataBaseConstants.ConnectionString, query, MapDataToCountry).ToList();
        }

        public List<LoadedRulesViewModel> GetLoadedRules(int referencedCountryId)
        {
            var dictionary = new Dictionary<string, object>();
            var query = @"
                      SELECT 
	                       DC.DimensionId as ReferencingCountryId
                          ,irp.[Description] as IrpRule
	                      ,irp.Id as IrpRuleId
	                      ,DCR.DimensionId as ReferencedCountryId
                          ,DCR.Name as ReferencedGeography
                      FROM [dbo].[gco_gprm_cntry_price_ref_vw]
                      INNER JOIN [DimensionDictionary] DC on [REFING_COUNTRY_NAME]=DC.Name
                      INNER JOIN [DimensionDictionary] DCR on [REFED_CNTRY_COUNTRY_NAME]=DCR.Name
                      INNER JOIN [IrpRule] irp ON irp.[Description] = REFING_CNTRY_FORMULA_USED_GOVT
                      WHERE REFING_CNTRY_FORMULA_USED_GOVT != '' AND REFING_CNTRY_FORMULA_USED_GOVT != 'Negotiation' AND DC.SystemId=16 AND DCR.SystemId=16";

            if(referencedCountryId > 0)
                query += "AND DC.DimensionId = @referencedCountryId";
                                          
            dictionary.Add("referencedCountryId", referencedCountryId);
            return RequestHelper.ExecuteQuery<LoadedRulesViewModel>(DataBaseConstants.ConnectionString, query, MapDataToRule, dictionary).ToList();
        }
        public List<LoadedRulesViewModel> GetIrpRules(int referencingCountryId, int gprmRuleTypeId = (int)GprmRuleTypes.Default)
        {
            var dictionary = new Dictionary<string, object>();
//            var query = @"
//                      SELECT
//	                    grev.Id AS ReferencingCountryId,
//	                    ca.ExternalId AS IrpRuleId,
//	                    CASE WHEN rPr.PriceLag = '' THEN 0 ELSE rPr.PriceLag END AS EffectiveLag,
//	                    CASE WHEN rPr.PriceLookback='' THEN 1 ELSE rPr.PriceLookback END AS LookBack,
//	                    CASE sr.Description
//		                    WHEN 'Periodic Review' THEN 30
//		                    WHEN 'Launch Review' THEN 32
//		                    ELSE 30
//	                    END AS GprmRuleTypeId,
//	                    'Channel: ' + CASE WHEN ch.Description IS NOT NULL THEN ch.Description ELSE 'Not Specified' END + ' | ' +
//	                    'Comparison: ' + CASE WHEN co.Description IS NOT NULL THEN co.Description ELSE 'Not Specified' END + ' | ' +
//	                    'Formulation: ' + CASE WHEN fo.Description IS NOT NULL THEN fo.Description ELSE 'Not Specified' END + ' | ' AS Information,
//	                    STUFF(
//	                    (SELECT
//		                    '\\p' + c.ModifiedBy + ' on '
//		                    + CAST(c.ModifiedDate AS nvarchar(MAX)) + ' : '
//		                    + c.Comment
//	                    FROM [IRP_DB].[dbo].Comment AS c WHERE rPr.Id=c.ReferencePriceRuleId FOR XML PATH('')),1,1,'') AS Comment,
//	                    gref.Id AS ReferencedCountryId,
//                        gref.Name AS ReferencedGeography
//                      FROM
//	                    [IRP_DB].[dbo].ReferencePriceRule AS rPr
//	                    LEFT OUTER JOIN [IRP_DB].[dbo].Channel AS ch ON rPr.RpChannelId=ch.Id
//	                    LEFT OUTER JOIN [IRP_DB].[dbo].Comparison AS co ON rPr.RpComparisonId=co.Id
//	                    LEFT OUTER JOIN [IRP_DB].[dbo].Formulation AS fo ON rPr.RpFormulationId=fo.Id
//	                    LEFT OUTER JOIN [IRP_DB].[dbo].SettingRule AS sr ON rPr.RpSettingRuleId=sr.Id
//	                    LEFT OUTER JOIN [IRP_DB].[dbo].ReferencedCountry AS rc ON rPr.Id=rc.ReferencePriceRuleId
//                        LEFT OUTER JOIN [IRP_DB].[dbo].Calculation AS ca ON rPr.RpCalculationId=ca.Id
//                        LEFT OUTER JOIN Geography AS gref ON gref.Iso2=rc.ReferencedCountryISO
//                        LEFT OUTER JOIN Geography AS grev ON grev.Iso2=rPr.ReferencingCountryISO
//                      WHERE
//	                    rPr.RpCalculationId IS NOT NULL AND
//	                    rc.ReferencedCountryId IS NOT NULL";

//            if (referencingCountryId > 0)
//                query += " AND grev.Id = @referencingCountryId";

            var query = @"
                      DECLARE @ruleId AS TABLE (Id int)
   
                       INSERT INTO @ruleId (Id)
                       SELECT TOP 1 rPr.Id
                       FROM
		                    [IRP_DB].[dbo].ReferencePriceRule AS rPr INNER JOIN
		                    Geography AS gref ON gref.Iso2=rPr.ReferencingCountryISO INNER JOIN
		                    [IRP_DB].[dbo].SettingRule AS sr ON rPr.RpSettingRuleId=sr.Id
                       WHERE gref.Id=@referencingCountryId AND sr.Description='Periodic Review' AND rPr.RpCalculationId IS NOT NULL
                       ORDER BY rPr.Id DESC

                       INSERT INTO @ruleId (Id)
                       SELECT TOP 1 rPr.Id
                       FROM
		                    [IRP_DB].[dbo].ReferencePriceRule AS rPr INNER JOIN
		                    Geography AS gref ON gref.Iso2=rPr.ReferencingCountryISO INNER JOIN
		                    [IRP_DB].[dbo].SettingRule AS sr ON rPr.RpSettingRuleId=sr.Id
                       WHERE gref.Id=@referencingCountryId AND sr.Description='Launch Review' AND rPr.RpCalculationId IS NOT NULL
                       ORDER BY rPr.Id DESC
   
                       SELECT
	                                        grev.Id AS ReferencingCountryId,
	                                        ca.ExternalId AS IrpRuleId,
	                                        CASE WHEN rPr.PriceLag = '' THEN 0 ELSE rPr.PriceLag END AS EffectiveLag,
	                                        CASE WHEN rPr.PriceLookback='' THEN 1 ELSE rPr.PriceLookback END AS LookBack,
	                                        CASE sr.Description
		                                        WHEN 'Periodic Review' THEN 30
		                                        WHEN 'Launch Review' THEN 32
		                                        ELSE 30
	                                        END AS GprmRuleTypeId,
	                                        'Channel: ' + CASE WHEN ch.Description IS NOT NULL THEN ch.Description ELSE 'Not Specified' END + ' | ' +
	                                        'Comparison: ' + CASE WHEN co.Description IS NOT NULL THEN co.Description ELSE 'Not Specified' END + ' | ' +
	                                        'Formulation: ' + CASE WHEN fo.Description IS NOT NULL THEN fo.Description ELSE 'Not Specified' END + ' | ' AS Information,
	                                        STUFF(
	                                        (SELECT
		                                        '\\p' + c.ModifiedBy + ' on '
		                                        + CAST(c.ModifiedDate AS nvarchar(MAX)) + ' : '
		                                        + c.Comment
	                                        FROM [IRP_DB].[dbo].Comment AS c WHERE rPr.Id=c.ReferencePriceRuleId FOR XML PATH('')),1,1,'') AS Comment,
	                                        gref.Id AS ReferencedCountryId,
                                            gref.Name AS ReferencedGeography
                                          FROM
	                                        [IRP_DB].[dbo].ReferencePriceRule AS rPr
	                                        LEFT OUTER JOIN [IRP_DB].[dbo].Channel AS ch ON rPr.RpChannelId=ch.Id
	                                        LEFT OUTER JOIN [IRP_DB].[dbo].Comparison AS co ON rPr.RpComparisonId=co.Id
	                                        LEFT OUTER JOIN [IRP_DB].[dbo].Formulation AS fo ON rPr.RpFormulationId=fo.Id
	                                        LEFT OUTER JOIN [IRP_DB].[dbo].SettingRule AS sr ON rPr.RpSettingRuleId=sr.Id
	                                        LEFT OUTER JOIN [IRP_DB].[dbo].ReferencedCountry AS rc ON rPr.Id=rc.ReferencePriceRuleId
                                            LEFT OUTER JOIN [IRP_DB].[dbo].Calculation AS ca ON rPr.RpCalculationId=ca.Id
                                            LEFT OUTER JOIN Geography AS gref ON gref.Iso2=rc.ReferencedCountryISO
                                            LEFT OUTER JOIN Geography AS grev ON grev.Iso2=rPr.ReferencingCountryISO
                                          WHERE
						                    rPr.Id IN (SELECT Id FROM @ruleId) AND
	                                        rc.ReferencedCountryId IS NOT NULL";

            dictionary.Add("referencingCountryId", referencingCountryId);
            return RequestHelper.ExecuteQuery<LoadedRulesViewModel>(DataBaseConstants.ConnectionString, query, MapIrpDataToRule, dictionary).Where(r => r != null && r.GprmRuleTypeId == gprmRuleTypeId).ToList();
        }

        public List<DataViewModel> GetLoadedPrice(List<int> geographies, int? product)
        {
            var dictionary = new Dictionary<string, object>();
            var query = @"
                    WITH Dates AS (
                        SELECT CAST('2013-01-01' AS DATETIME) AS dt
                        UNION ALL
                        SELECT DATEADD(MONTH, 1, dt)
                        FROM Dates s
                        WHERE DATEADD(MONTH, 1, dt) <= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) - 1, 0))
                    SELECT 
                            DC.DimensionId as GeographyId
                            ,DP.DimensionId as ProductId,
		                    DPC.Id as PriceTypeId
                            ,AVG([PRICELIST_PRICE]/SKU.Dosage/SKU.PackSize/SKU.FactorUnit) as Value
		                    ,DPC.CurrencyId as CurrencySpotId
	                        ,d.dt as DataTime
		                    ,41 as DataTypeId
		                    ,15 as SegmentId
		                    ,23 as EventTypeId
		                    ,5 as UnitTypeId
                    FROM (
                    SELECT COUNTRY, PRICE_LIST_NAME, ProductFamily =
	                    CASE 
	                    WHEN PRODUCT_FAMILY = 'ARANESP' AND STRENGTH < 150 THEN 'ARANESP - NEPHRO'
                        WHEN PRODUCT_FAMILY = 'ARANESP' AND STRENGTH = 150 THEN 'ARANESP 150'
	                    WHEN PRODUCT_FAMILY = 'ARANESP' AND STRENGTH > 150 THEN 'ARANESP - ONCO'
	                    ELSE PRODUCT_FAMILY
	                    END
	                    , PRICELIST_PRICE, EFFECTIVE_DATE, EXPIRATION_DATE, [TYPE_TO_CHANNEL], [STATUS_TO_STATUS], [PRICELIST_CURRENCY_TYPE], [PRODUCT_NUMBER]
                    FROM [gco_gprm_price_list_vw]
                    WHERE [STRENGTH] != 0
                    ) p
                    INNER JOIN Dates d ON DATEADD(MONTH,1,d.dt) >= p.EFFECTIVE_DATE AND  d.dt <= p.EXPIRATION_DATE
                    INNER JOIN [DimensionDictionary] DC on [COUNTRY]=DC.Name
                    INNER JOIN [DimensionDictionary] DP on ProductFamily=DP.Name
                    INNER JOIN [PriceType] DPC on [TYPE_TO_CHANNEL]+' '+ISNULL([STATUS_TO_STATUS],'')+' '+[PRICELIST_CURRENCY_TYPE]=DPC.Name
                    INNER JOIN [SKU] SKU on [PRODUCT_NUMBER]=SKU.[ProductNumber] AND DC.[DimensionId]=SKU.[GeographyId]";

            if(geographies.Count > 0)
            {
                query += @"WHERE DC.DimensionId IN (" + string.Join(",", geographies) + @") ";
            }
            if(product != 0)
            {
                query += @"AND DP.DimensionId = @productId ";
                dictionary.Add("productId", product);
            }
             query += @"GROUP BY DC.DimensionId, DPC.Id, DP.DimensionId, DPC.CurrencyId, d.dt
                    OPTION (maxrecursion 0) ";       
                    
            return RequestHelper.ExecuteQuery<DataViewModel>(DataBaseConstants.ConnectionString, query, MapDataToData, dictionary).ToList();
        }

        public List<DataViewModel> GetLoadedEvent(List<int> geographies, int? product)
        {
            var dictionary = new Dictionary<string, object>();
            var query = @"
                    SELECT DISTINCT DC.DimensionId as GeographyId
                          ,DP.DimensionId as ProductId
                          ,118 as CurrencySpotId
	                      ,1 as PriceTypeId
	                      ,43 as DataTypeId
	                      ,DT.Id as EventTypeId
	                      ,18 as UnitTypeId
	                      ,15 as SegmentId
	                      ,DATEADD(year, [year]-1900, DATEADD(month, [month]-1, DATEADD(day, 1-1, 0))) as DataTime
                          ,MAX([SIZE_OF_CUT_PCT])/100  as Value
                      FROM [gco_gprm_cntry_price_cal_vw] cal inner join
                           [DimensionDictionary] DC on cal.[COUNTRY]=DC.Name  inner join
	                       [DimensionDictionary] DP on cal.[PRODUCT_FAMILY]=DP.Name inner join
	                       [DimensionType] as DT on DT.[ShortName]=cal.[TYPE_OF_CUT]";

            if(geographies.Count > 0)
            {
                query += @"WHERE DC.DimensionId IN (" + string.Join(",", geographies) + @") ";
				
            }

            if(product != 0)
            {
                query += "AND DP.DimensionId = @productId ";
                dictionary.Add("productId", product);
            }
                      
            query +=  @"AND [PROBABILITY_OF_CUT_PCT]>=50
                      AND DC.SystemId = 16 AND DP.SystemId = 16";

            query +=
                " GROUP BY DC.DimensionId, DP.DimensionId, DT.Id, DATEADD(year, [year]-1900, DATEADD(month, [month]-1, DATEADD(day, 1-1, 0)))";

            return RequestHelper.ExecuteQuery<DataViewModel>(DataBaseConstants.ConnectionString, query, MapDataToData, dictionary).ToList();
        }

        public List<DataViewModel> GetIrpEvent(List<int> geographies, int? product)
        {
            var startMonth = new DateTime(DateTime.Now.Year - 1, 1, 1);
            var endMonth = new DateTime(startMonth.Year + ApplicationConstants.MaximumSimulationDuration + 1, 12, 1);
            var dictionary = new Dictionary<string, object>
            {
                {"endMonth", endMonth}
            };
            var query = @"
                    SELECT DISTINCT
	                    g.Id as GeographyId
                        ,DP.DimensionId as ProductId
                        ,118 as CurrencySpotId
	                    ,1 as PriceTypeId
	                    ,43 as DataTypeId
	                    ,at.ExternalId as EventTypeId
	                    ,18 as UnitTypeId
	                    ,15 as SegmentId
	                    ,CAST(CAST(DATEPART(year,cal.ReviewDate) AS NVARCHAR(MAX)) + '-' + CASE WHEN DATEPART(month,cal.ReviewDate) >=10 THEN '' ELSE '0' END + CAST(DATEPART(month,cal.ReviewDate) AS NVARCHAR(MAX)) + '-01' AS DATETIME) as DataTime
                        ,-MAX(cal.PercentChange)/100  as Value
                    FROM
	                    [IRP_DB].[dbo].[PriceAction] cal inner join
	                    [IRP_DB].[dbo].[ProductXML] p on cal.ProductId=p.Id inner join
	                    [IRP_DB].[dbo].[PriceActionType] at on cal.PriceActionTypeId=at.Id inner join
                        [IRP_DB].[dbo].[Probability] pr on cal.ProbabilityId=pr.Id inner join
                        [DimensionDictionary] DP on p.Name=DP.Name inner join
                        [Geography] g on cal.CountryISO=g.Iso2";

            if (geographies.Count > 0)
            {
                query += @" WHERE g.Id IN (" + string.Join(",", geographies) + @") ";

            }

            if (product != 0)
            {
                query += " AND DP.DimensionId = @productId ";
                dictionary.Add("productId", product);
            }

            query += @" AND pr.ExternalId<=2 AND DP.SystemId=16 AND cal.Active=1 AND
                    CAST(CAST(DATEPART(year,cal.ReviewDate) AS NVARCHAR(MAX)) + '-' + CASE WHEN DATEPART(month,cal.ReviewDate) >=10 THEN '' ELSE '0' END + CAST(DATEPART(month,cal.ReviewDate) AS NVARCHAR(MAX)) + '-01' AS DATETIME) <= @endMonth";

            query +=
                " GROUP BY g.Id, DP.DimensionId, at.ExternalId, cal.ReviewDate";

            var result = RequestHelper.ExecuteQuery<DataViewModel>(DataBaseConstants.ConnectionString, query, MapDataToData, dictionary).ToList();

            dictionary = new Dictionary<string, object>
            {
                {"endMonth", endMonth}
            }; 
            query = @"SELECT DISTINCT
	                    g.Id as GeographyId
                        ,DP.DimensionId as ProductId
                        ,118 as CurrencySpotId
	                    ,1 as PriceTypeId
	                    ,43 as DataTypeId
	                    ,28 as EventTypeId
	                    ,18 as UnitTypeId
	                    ,15 as SegmentId
	                    ,CAST(CAST(DATEPART(year,cal.ReviewDate) AS NVARCHAR(MAX)) + '-' + CASE WHEN DATEPART(month,cal.ReviewDate) >=10 THEN '' ELSE '0' END + CAST(DATEPART(month,cal.ReviewDate) AS NVARCHAR(MAX)) + '-01' AS DATETIME) as DataTime
                        ,CAST(0 AS FLOAT) as Value
                    FROM
	                    [IRP_DB].[dbo].[CalendarDate] cal inner join
	                    [IRP_DB].[dbo].[PriceCalendar] pc on cal.PriceCalendarId=pc.id inner join
	                    [IRP_DB].[dbo].[ProductXML] p on pc.ProductId=p.Id inner join
                        [DimensionDictionary] DP on p.Name=DP.Name inner join
                        [Geography] g on pc.CountryISO=g.Iso2";

            if (geographies.Count > 0)
            {
                query += @" WHERE g.Id IN (" + string.Join(",", geographies) + @") ";

            }

            if (product != 0)
            {
                query += " AND DP.DimensionId = @productId ";
                dictionary.Add("productId", product);
            }

            query += @" AND DP.SystemId=16 AND cal.Active=1 AND
                    CAST(CAST(DATEPART(year,cal.ReviewDate) AS NVARCHAR(MAX)) + '-' + CASE WHEN DATEPART(month,cal.ReviewDate) >=10 THEN '' ELSE '0' END + CAST(DATEPART(month,cal.ReviewDate) AS NVARCHAR(MAX)) + '-01' AS DATETIME) <= @endMonth 
                    GROUP BY g.Id, DP.DimensionId, cal.ReviewDate";

            result.AddRange(RequestHelper.ExecuteQuery<DataViewModel>(DataBaseConstants.ConnectionString, query, MapDataToData, dictionary).ToList());

            return result;
        }

        public List<DataViewModel> GetLoadedVolume(List<int> geographies, int? product)
        {
            var dictionary = new Dictionary<string, object>();
            var query = @"
                        WITH Volumes AS (
                        SELECT
                                [MONTH]
                                ,[YEAR]
                                ,[COUNTRY_NAME]
                                ,ProductFamily = 
		                        CASE 
			                        WHEN PRODUCT_FAMILY = 'ARANESP' AND INDICATION = 'Oncology' THEN 'ARANESP - ONCO'
			                        WHEN PRODUCT_FAMILY = 'ARANESP' AND INDICATION = 'Nephrology' THEN 'ARANESP - NEPHRO'
			                        ELSE PRODUCT_FAMILY
		                        END
	                            ,SCENARIO_NAME
	                            ,[VOLUME_COM]
	                            ,[VOLUME_FREE]
                            FROM [gco_gprm_agg_prod_sales_vol_vw]
                            WHERE PRODUCT_FAMILY != ''
                        ), MaxPriority AS (
                            SELECT
                                [MONTH]
                                ,[YEAR]
                                ,[COUNTRY_NAME]
                                ,ProductFamily
	                            ,MAX(lv.[Weight]) as [Weight]
                            FROM [Volumes] v
                            INNER JOIN LoadVolumeScenario lv ON v.SCENARIO_NAME = lv.Name
                            INNER JOIN [DimensionDictionary] DC on v.[COUNTRY_NAME]=DC.Name
							WHERE DC.DimensionId IN (" + string.Join(",", geographies) + @") AND DC.SystemId = 16 AND lv.[Weight]<>0
                            GROUP BY  [MONTH], [YEAR], [COUNTRY_NAME], ProductFamily
                        )
                        SELECT
                                v.[MONTH]
                                ,v.[YEAR]
                                ,DC.DimensionId as GeographyId
                                ,DP.DimensionId as ProductId
                                ,SUM((ISNULL([VOLUME_COM], 0) + ISNULL([VOLUME_FREE],0))*U.Factor) as Value
	                            ,42 as DataTypeId
	                            ,CAST(
                                CAST(v.[YEAR] AS VARCHAR(4)) +
		                            RIGHT('0' + CAST(v.[MONTH] AS VARCHAR(2)), 2) +
		                            RIGHT('0' + CAST(1 AS VARCHAR(2)), 2) 
	                            AS DATETIME) as DataTime
	                            ,118 as CurrencySpotId
	                            ,1 as PriceTypeId
	                            ,20 as EventTypeId
	                            ,18 as UnitTypeId
	                            ,15 as SegmentId
                        FROM Volumes v
                        INNER JOIN LoadVolumeScenario lv ON v.SCENARIO_NAME = lv.Name
                        INNER JOIN MaxPriority mp ON v.[MONTH] = mp.[MONTH] AND v.[YEAR] = mp.[YEAR] AND v.COUNTRY_NAME = mp.COUNTRY_NAME AND mp.ProductFamily = v.ProductFamily AND mp.[Weight] = lv.[Weight]
                        INNER JOIN [DimensionDictionary] DC on v.[COUNTRY_NAME]=DC.Name
                        INNER JOIN [DimensionDictionary] DP on v.ProductFamily=DP.Name
						INNER JOIN [ProductUnit] PU on PU.ProductId = DP.DimensionId
						INNER JOIN [Unit] U on PU.UnitId = u.Id
                        WHERE DC.DimensionId IN (" + string.Join(",", geographies) + @") ";
            if(product != null)
            {
                query += "AND DP.DimensionId = @productId ";
                dictionary.Add("productId", product);
            }

			query += @"AND DC.SystemId = 16 AND DP.SystemId = 16 AND PU.IsDefault = 1
                       GROUP BY  v.[MONTH], v.[YEAR], DC.DimensionId, DP.DimensionId";
            
            return RequestHelper.ExecuteQuery<DataViewModel>(DataBaseConstants.ConnectionString, query, MapDataToData, dictionary).ToList();
        }

        public List<ListToSalesViewModel> GetLoadedListToSales(List<int> productIds, List<int> geographyIds)
        {
            var query = @"
                WITH Volumes AS (
                SELECT
                        [MONTH]
                        ,[YEAR]
                        ,[COUNTRY_NAME]
                        ,ProductFamily = 
		                CASE 
			                WHEN PRODUCT_FAMILY = 'ARANESP' AND INDICATION = 'Oncology' THEN 'ARANESP - ONCO'
			                WHEN PRODUCT_FAMILY = 'ARANESP' AND INDICATION = 'Nephrology' THEN 'ARANESP - NEPHRO'
			                ELSE PRODUCT_FAMILY
		                END
	                    ,SCENARIO_NAME
	                    ,[VOLUME_COM]
	                    ,[VOLUME_FREE]
	                    ,[CY_ACTUAL_RATE_USD_AMOUNT]
                    FROM [gco_gprm_agg_prod_sales_vol_vw]
                    WHERE PRODUCT_FAMILY != ''
                ), MaxPriority AS (
                    SELECT
                        [MONTH]
                        ,[YEAR]
                        ,[COUNTRY_NAME]
                        ,ProductFamily
	                    ,MAX(lv.[Weight]) as [Weight]
                    FROM [Volumes] v
                    INNER JOIN LoadVolumeScenario lv ON v.SCENARIO_NAME = lv.Name
                    INNER JOIN [DimensionDictionary] DC on v.[COUNTRY_NAME]=DC.Name
					WHERE DC.DimensionId IN (" + string.Join(",", geographyIds) + @") AND DC.SystemId = 16 AND lv.[Weight]<>0
                    GROUP BY  [MONTH], [YEAR], [COUNTRY_NAME], ProductFamily
                )
                SELECT
                        DC.DimensionId as GeographyId
                        ,DP.DimensionId as ProductId
                        ,CASE WHEN SUM(ISNULL([VOLUME_COM], 0) + ISNULL([VOLUME_FREE],0))<>0 THEN SUM([CY_ACTUAL_RATE_USD_AMOUNT]) / SUM((ISNULL([VOLUME_COM], 0) + ISNULL([VOLUME_FREE],0))*U.Factor) ELSE 0 END as Asp
	                    ,15 as SegmentId
                FROM Volumes v
                INNER JOIN LoadVolumeScenario lv ON v.SCENARIO_NAME = lv.Name
                INNER JOIN MaxPriority mp ON v.[MONTH] = mp.[MONTH] AND v.[YEAR] = mp.[YEAR] AND v.COUNTRY_NAME = mp.COUNTRY_NAME AND mp.ProductFamily = v.ProductFamily AND mp.[Weight] = lv.[Weight]
                INNER JOIN [DimensionDictionary] DC on v.[COUNTRY_NAME]=DC.Name
                INNER JOIN [DimensionDictionary] DP on v.ProductFamily=DP.Name
                INNER JOIN [ProductUnit] PU on PU.ProductId = DP.DimensionId
                INNER JOIN [Unit] U on PU.UnitId = u.Id
                WHERE v.[MONTH] = DATEPART(MONTH, DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE())-1, 0)) AND v.[YEAR] = DATEPART(YEAR, DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE())-1, 0))
                AND DC.DimensionId IN (" + string.Join(",", geographyIds) + @") 
                AND DP.DimensionId IN (" + string.Join(",", productIds) + @") 
                AND DC.SystemId = 16 AND DP.SystemId = 16
                GROUP BY  v.[MONTH], v.[YEAR], DC.DimensionId, DP.DimensionId";

            return RequestHelper.ExecuteQuery<ListToSalesViewModel>(DataBaseConstants.ConnectionString, query, MapDataToListToSale).ToList();
        }

        public List<VolumeScenarioViewModel> GetLoadedVolumeScenario()
        {
            var query = @"
                    SELECT [Name], [Weight]
                    FROM  LoadVolumeScenario";
            return RequestHelper.ExecuteQuery<VolumeScenarioViewModel>(DataBaseConstants.ConnectionString, query, MapDataToVolumeScenario).ToList();
        }

        public void UpdateLoadedVolumeScenario(List<VolumeScenarioViewModel> model)
        {
            foreach (var item in model)
            {
                var dictionary = new Dictionary<string, object>();
                dictionary.Add("name", item.Name);
                dictionary.Add("weight", item.Weight);
                var query = @"
                    UPDATE LoadVolumeScenario
                    SET [Weight] = @weight
                    WHERE Name = @name";
                RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, dictionary);
            }
           
        }

        public void CreateNewVolumeScenario()
        {
            var query = @"
                        INSERT LoadVolumeScenario (Name, [Weight])
                            SELECT DISTINCT v.SCENARIO_NAME, (SELECT MAX([Weight]) + 1 FROM LoadVolumeScenario)
                            FROM [gco_gprm_agg_prod_sales_vol_vw] v
                            WHERE
                               NOT EXISTS (SELECT * FROM LoadVolumeScenario lv
                                          WHERE lv.Name = v.SCENARIO_NAME)";

            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query);
        }

        #endregion

        #region Map function
        private static LoadItemDetailViewModel MapDataToLoadItemDetail(DataRow row)
        {
            var item = new LoadItemDetailViewModel
            {
                Id = (int)row["Id"],
                GeographyId = (int)row["GeographyId"],
                LoadItemId = (int)row["LoadItemId"],
                Validated = (bool)row["Validated"],
                ProductId = (int)row["ProductId"]
            };

            return item;
        }

        private static LoadedRulesViewModel MapDataToRule(DataRow row)
        {
            var item = new LoadedRulesViewModel
            {
                IrpRule = (string)row["IrpRule"],
                IrpRuleId = (int)row["IrpRuleId"],
                ReferencedCountryId = (int)row["ReferencedCountryId"],
                ReferencingCountryId = (int)row["ReferencingCountryId"],
                ReferencedGeography = (string)row["ReferencedGeography"]
            };

            return item;
        }

        private static LoadedRulesViewModel MapIrpDataToRule(DataRow row)
        {
            if (row["ReferencedCountryId"].PreventIntNull() == null || row["ReferencingCountryId"].PreventIntNull() == null || row["IrpRuleId"].PreventIntNull() == null)
                return null;
            var item = new LoadedRulesViewModel
            {
                ReferencingCountryId = (int)row["ReferencingCountryId"],
                IrpRuleId = (int)row["IrpRuleId"],
                EffectiveLag = row["EffectiveLag"].PreventIntNull() ?? 0,
                LookBack = row["LookBack"].PreventIntNull() ?? 1,
                GprmRuleTypeId = row["GprmRuleTypeId"].PreventIntNull() ?? (int)GprmRuleTypes.Default,
                ReferencedCountryId = (int)row["ReferencedCountryId"],
                ReferencedGeography = row["ReferencedGeography"].PreventStringNull(),
                Information = row["Information"].PreventStringNull(),
                Comment = row["Comment"].PreventStringNull()
            };

            return item;
        }

        private static IrpRuleCalculationViewModel MapDataToSubRuleCalculation(DataRow row)
        {
            int? upid = null;
            if (!(row["UpId"] is System.DBNull))
            {
                upid = (int?) row["UpId"];

            }
            var item = new IrpRuleCalculationViewModel
            {
                Id = (int)row["Id"],
                IrpRuleListId = (int)row["IrpRuleListId"],
                Argument = (int)row["Argument"],
                IrpMathId = (int)row["IrpMathId"],
                UpId = upid,
            };

            return item;
        }

        private static CurrencyViewModel MapDataToCurrency(DataRow row)
        {
            var currency = new CurrencyViewModel
            {
                UsdRate = (double)row["EXCHANGE_RATE"],
                Iso = (string)row["FROM_CURR"],
                Tag = "Loaded"
            };
            currency.Name = "";

            return currency;
        }

        private static PriceTypeViewModel MapDataToPriceTypes(DataRow row)
        {
            var item = new PriceTypeViewModel
            {
                Name = row["TYPE_TO_CHANNEL"] + " " + row["STATUS_TO_STATUS"] + " " + row["CurrencyName"],
                CurrencyName = (string)row["CurrencyName"],
                CurrencyId = (int)row["CurrencyId"],
                Tag = "Loaded"
            };
            item.ShortName = "Not Specified";

            return item;
        }

        private static SkuViewModel MapDataToSku(DataRow row)
        {
            var item = new SkuViewModel
            {
                Name = (string)row["PRODUCT_NAME"],
                Dosage = (double)row["STRENGTH"],
                PackSize = (int)row["PACK"],
                ProductId = (int)row["ProductId"],
                GeographyId = (int)row["GeographyId"],
                Tag = "Loaded",
                ProductNumber = (string)row["PRODUCT_NUMBER"]
            };

            item.FormulationId = 17; // not specified
            item.FactorUnit = 1;

            return item;
        }

        private static ProductViewModel MapDataToProduct(DataRow row)
        {
            var item = new ProductViewModel
            {
                Name = (string)row["Name"],
                Tag = "Loaded"
            };
            item.ShortName = "Not Specified";
            item.ExportName = "Not Specified";
            item.DisplayName = "Not Specified";
            item.BaseConsolidationUnitId = 2;
            item.BaseConsolidationUnit = "mcg";
            item.FactorScreen = 1;
            item.Factor = 1;
            item.DisplayConsolidationUnit = "1mcg";

            return item;
        }

        private static CountryViewModel MapDataToCountry(DataRow row)
        {
            var item = new CountryViewModel
            {
                Name = (string)row["Name"],
                Iso3 = (string)row["COUNTRY_CODE"],
                Tag = "Loaded"
            };
            item.DisplayCurrency = "Not Specified"; // todo change that ?
            item.CurrencyId = 118;
            item.Iso2 = item.Iso3.Substring(0, 2);
            item.ExportName = "Not Specified";
            return item;
        }

        private static VolumeScenarioViewModel MapDataToVolumeScenario(DataRow row)
        {
            var item = new VolumeScenarioViewModel
            {
                Name = (string)row["NAME"],
                Weight = (int)row["Weight"]
            };

            return item;
        }

        private static ListToSalesViewModel MapDataToListToSale(DataRow row)
        {
            var item = new ListToSalesViewModel
            {
                GeographyId = (int)row["GeographyId"],
                Asp = row["Asp"].PreventNull() ?? 0,
                SegmentId = (int)row["SegmentId"],
                ProductId = (int)row["ProductId"],
                Tag = "Loaded"
            };

            return item;
        }

        private static DataViewModel MapDataToData(DataRow row)
        {
            var item = new DataViewModel
            {
                CurrencySpotId = (int)row["CurrencySpotId"],
                DataTime = (DateTime)row["DataTime"],
                DataTypeId = (int)row["DataTypeId"],
                EventTypeId = (int)row["EventTypeId"],
                GeographyId = (int)row["GeographyId"],
                PriceTypeId = (int)row["PriceTypeId"],
                ProductId = (int)row["ProductId"],
                SegmentId = (int)row["SegmentId"],
                UnitTypeId = (int)row["UnitTypeId"],
                Value = row["Value"].PreventNull(),
                Tag = "Loaded"
            };

            return item;
        }

        private static LoadViewModel MapDataToLoad(DataRow row)
        {
            var load = new LoadViewModel
            {
                Id = (int)row["Id"],
                Name = (string)row["Name"],
                CreationDate = (DateTime)row["CreationDate"],
                LastUpdateDate = (DateTime)row["LastUpdateDate"],
                Status = (int)row["Status"],
                UserName = (string)row["UserId"],
                StatusName = (string)row["StatusName"],                
            };

            return load;
        }

        private static LoadDetailItemViewModel MapDataToLoadItem(DataRow row)
        {
            var loadItem = new LoadDetailItemViewModel
            {
                Id = (int)row["Id"],
                Name = (string)row["Name"],
                LastUpdateDate = (DateTime)row["LastUpdateDate"],
                Status = (int)row["Status"],
                StatusName = (string)row["StatusName"],
                Step = (int)row["Step"],
                CanUpdateViaExcel = (bool)row["CanUpdateViaExcel"],
                LoadId = (int)row["LoadId"]
            };

            return loadItem;
        }

        private static FilterItemViewModel MapDataToLoadStatusFilter(DataRow row)
        {
            var loadStatus = new FilterItemViewModel
            {
                Id = (int)row["Id"],
                TextShort = (string)row["Name"],
                Text = (string)row["Name"],

            };

            return loadStatus;
        }
        #endregion

        #region OracleLoad
        public string StartLoadSource(int loadId)
        {
            //1. Loading all table from Source and save it to SQL Server
            //gco_gprm_agg_prod_sales_vol_vw
            try
            {
                const string salesQuery =
                @"SELECT * FROM GCOODS.gco_gprm_agg_prod_sales_vol_vw";
                var sales = OracleRequestHelper.ExecuteQuery(DataBaseConstants.OracleConnectionString, salesQuery, MapDataToOracleSales).ToList();
                var salesDataTable = sales.ToDataTable();
                RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, "DELETE FROM gco_gprm_agg_prod_sales_vol_vw");
                RequestHelper.BulkInsert(DataBaseConstants.ConnectionString, "gco_gprm_agg_prod_sales_vol_vw", salesDataTable);
                CreateNewVolumeScenario();
            }
            catch (Exception e)
            {
                SetLoadOnError(loadId, "Error on loading sales");
                return "Error on loading sales - " + e.Message;
            }

            try
            {
                const string eventQuery =
                @"SELECT * FROM GCOODS.gco_gprm_cntry_price_cal_vw";
                var events = OracleRequestHelper.ExecuteQuery(DataBaseConstants.OracleConnectionString, eventQuery, MapDataToOracleEvent).ToList();
                var eventsDataTable = events.ToDataTable();
                RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, "DELETE FROM [gco_gprm_cntry_price_cal_vw]");
                RequestHelper.BulkInsert(DataBaseConstants.ConnectionString, "gco_gprm_cntry_price_cal_vw", eventsDataTable);
            }
            catch (Exception e)
            {
                SetLoadOnError(loadId, "Error on loading event");
                return "Error on loading event - " + e.Message;
            }

            try
            {
                const string ruleQuery =
               @"SELECT  * FROM GCOODS.gco_gprm_cntry_price_ref_vw";
                var rules = OracleRequestHelper.ExecuteQuery(DataBaseConstants.OracleConnectionString, ruleQuery, MapDataToOracleRule).ToList();
                var rulesDataTable = rules.ToDataTable();
                RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, "DELETE FROM [gco_gprm_cntry_price_ref_vw]");
                RequestHelper.BulkInsert(DataBaseConstants.ConnectionString, "gco_gprm_cntry_price_ref_vw", rulesDataTable);
            }
            catch (Exception e)
            {
                SetLoadOnError(loadId, "Error on loading rules");
                return "Error on loading rules - " + e.Message;
            }

            try
            {
                const string priceListQuery =
                @"SELECT * FROM GCOODS.gco_gprm_price_list_vw";
                var priceList = OracleRequestHelper.ExecuteQuery(DataBaseConstants.OracleConnectionString, priceListQuery, MapDataToOraclePriceList).ToList();
                var priceListDataTable = priceList.ToDataTable();
                RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, "DELETE FROM [gco_gprm_price_list_vw]");
                RequestHelper.BulkInsert(DataBaseConstants.ConnectionString, "gco_gprm_price_list_vw", priceListDataTable);
            }
            catch (Exception e)
            {
                SetLoadOnError(loadId, "Error on loading price lists");
                return "Error on loading price lists - " + e.Message;
            }

            try
            {
                const string exchangeRateQuery =
                @"SELECT * FROM GCOODS.gco_sap_curr_exchange_rate_vw";
                var exchangeRate = OracleRequestHelper.ExecuteQuery(DataBaseConstants.OracleConnectionString, exchangeRateQuery, MapDataToOracleExchangeRate).ToList();
                var pexchangeRateDataTable = exchangeRate.ToDataTable();
                RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, "DELETE FROM [gco_sap_curr_exchange_rate_vw]");
                RequestHelper.BulkInsert(DataBaseConstants.ConnectionString, "gco_sap_curr_exchange_rate_vw", pexchangeRateDataTable);
            }
            catch (Exception e)
            {
                SetLoadOnError(loadId, "Error on loading exchange rates");
                return "Error on loading price exchange rates - " + e.Message; 
            }

            try
            {
                const string countryQuery =
                @"SELECT * FROM GCOODS.gco_submn_country_vw";
                var country = OracleRequestHelper.ExecuteQuery(DataBaseConstants.OracleConnectionString, countryQuery, MapDataToOracleCountry).ToList();
                var countryDataTable = country.ToDataTable();
                RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, "DELETE FROM [gco_submn_country_vw]");
                RequestHelper.BulkInsert(DataBaseConstants.ConnectionString, "gco_submn_country_vw", countryDataTable);
            }
            catch (Exception e)
            {
                SetLoadOnError(loadId, "Error on loading country");
                return "Error on loading country - " + e.Message; 
            }

            try
            {
                const string productQuery =
                @" SELECT * FROM GCOODS.gco_submn_product_vw";
                var product = OracleRequestHelper.ExecuteQuery(DataBaseConstants.OracleConnectionString, productQuery, MapDataToOracleProduct).ToList();
                var productDataTable = product.ToDataTable();
                RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, "DELETE FROM [gco_submn_product_vw]");
                RequestHelper.BulkInsert(DataBaseConstants.ConnectionString, "gco_submn_product_vw", productDataTable);
            }
            catch (Exception e)
            {
                SetLoadOnError(loadId, "Error on loading product");
                return "Error on loading product - " + e.Message; 
            }

            SetLoadCompleted(loadId, "Completed");
            return "Completed";
        }

        private static SalesModel MapDataToOracleSales(IDataReader reader)
        {
            var item = new SalesModel();
            item.COUNTRY_NAME = reader.Get<string>("COUNTRY_NAME");
            item.CY_ACTUAL_RATE_USD_AMOUNT = reader.GetNullable("CY_ACTUAL_RATE_USD_AMOUNT", Convert.ToDouble);
            item.CY_BUDGET_RATE_USD_AMOUNT = reader.GetNullable("CY_BUDGET_RATE_USD_AMOUNT", Convert.ToDouble);
            item.FUTURE_ACTUAL_RATE_USD_AMOUNT = reader.GetNullable("FUTURE_ACTUAL_RATE_USD_AMOUNT", Convert.ToDouble);
            item.FUTURE_BUDGET_RATE_USD_AMOUNT = reader.GetNullable("FUTURE_BUDGET_RATE_USD_AMOUNT", Convert.ToDouble);
            item.INDICATION = reader.Get<string>("INDICATION");
            item.MONTH = reader.GetNullable("MONTH", Convert.ToInt32);
            item.PRODUCT_FAMILY = reader.Get<string>("PRODUCT_FAMILY");
            item.SALES_ORG = reader.Get<string>("SALES_ORG");
            item.SCENARIO_NAME = reader.Get<string>("SCENARIO_NAME");
            item.VOLUME_COM = reader.GetNullable("VOLUME_COM", Convert.ToDouble);
            item.VOLUME_FREE = reader.GetNullable("VOLUME_FREE", Convert.ToDouble);
            item.VOLUME_TYPE = reader.Get<string>("VOLUME_TYPE");
            item.YEAR = reader.GetNullable("YEAR", Convert.ToInt32);

            return item;
        }

        private static EventModel MapDataToOracleEvent(IDataReader reader)
        {
            var item = new EventModel();
            item.COUNTRY = reader.Get<string>("COUNTRY");
            item.COMMENTS = reader.Get<string>("COMMENTS");
            item.ISO_COUNTRY_2_CHAR = reader.Get<string>("ISO_COUNTRY_2_CHAR");
            item.PROBABILITY_OF_CUT_PCT = reader.GetNullable("PROBABILITY_OF_CUT_PCT", Convert.ToDouble);
            item.SIZE_OF_CUT_PCT = reader.GetNullable("SIZE_OF_CUT_PCT", Convert.ToDouble);
            item.TYPE_OF_CUT = reader.Get<string>("TYPE_OF_CUT");
            item.CUSTOM_TYPE_OF_CUT = reader.Get<string>("CUSTOM_TYPE_OF_CUT");
            item.MONTH = reader.GetNullable("MONTH", Convert.ToInt32).Value;
            item.PRODUCT_FAMILY = reader.Get<string>("PRODUCT_FAMILY");
            item.YEAR = reader.GetNullable("YEAR", Convert.ToInt32).Value;

            return item;
        }

        private static RuleModel MapDataToOracleRule(IDataReader reader)
        {
            var item = new RuleModel();
            item.REFED_CNTRY_COUNTRY_NAME = reader.Get<string>("REFED_CNTRY_COUNTRY_NAME");
            item.REFED_CNTRY_DOCUMENT_REQUIRED = reader.Get<string>("REFED_CNTRY_DOCUMENT_REQUIRED");
            item.REFED_CNTRY_MISSING_PCK_PRICE = reader.Get<string>("REFED_CNTRY_MISSING_PCK_PRICE");
            item.REFED_CNTRY_PRICELIST_CHANNEL = reader.Get<string>("REFED_CNTRY_PRICELIST_CHANNEL");
            item.REFED_CNTRY_PRICELIST_OFFCL = reader.Get<string>("REFED_CNTRY_PRICELIST_OFFCL");
            item.REFED_CNTRY_PRICELIST_PUBLIC = reader.Get<string>("REFED_CNTRY_PRICELIST_PUBLIC");
            item.REFED_CNTRY_PRICELIST_TYPE = reader.Get<string>("REFED_CNTRY_PRICELIST_TYPE");
            item.REFING_CNTRY_BASIS_OF_CMPRSN = reader.Get<string>("REFING_CNTRY_BASIS_OF_CMPRSN");
            item.REFING_CNTRY_FOCUS = reader.Get<string>("REFING_CNTRY_FOCUS");
            item.REFING_CNTRY_FORMULA_USED_GOVT = reader.Get<string>("REFING_CNTRY_FORMULA_USED_GOVT");
            item.REFING_CNTRY_FREQUENCY = reader.Get<string>("REFING_CNTRY_FREQUENCY");
            item.REFING_CNTRY_IS_FORMAL = reader.Get<string>("REFING_CNTRY_IS_FORMAL");
            item.REFING_CNTRY_MONTHS_BTW_REF = reader.GetNullable("REFING_CNTRY_MONTHS_BTW_REF", Convert.ToInt32);
            item.REFING_CNTRY_REF_COUNTRY_CNT = reader.GetNullable("REFING_CNTRY_REF_COUNTRY_CNT", Convert.ToInt32).Value;
            item.REFING_CNTRY_REF_MONTH = reader.Get<string>("REFING_CNTRY_REF_MONTH");
            item.REFING_CNTRY_UNIT_OF_CMPRSN = reader.Get<string>("REFING_CNTRY_UNIT_OF_CMPRSN");
            item.REFING_COUNTRY_NAME = reader.Get<string>("REFING_COUNTRY_NAME");

            return item;
        }

        private static PriceListModel MapDataToOraclePriceList(IDataReader reader)
        {
            var item = new PriceListModel();
            item.AVAILABLE_ON_CONTRACTS = reader.Get<string>("AVAILABLE_ON_CONTRACTS");
            item.AVAILABLE_ON_PRICING_DOCS = reader.Get<string>("AVAILABLE_ON_PRICING_DOCS");
            item.COUNTRY = reader.Get<string>("COUNTRY");
            item.EFFECTIVE_DATE = reader.GetNullable<DateTime>("EFFECTIVE_DATE");
            item.EXCHANGE_RATE = reader.GetNullable("EXCHANGE_RATE", Convert.ToDouble);
            item.EXPIRATION_DATE = reader.GetNullable<DateTime>("EXPIRATION_DATE");
            item.ORGANIZATION_ID = reader.Get<string>("ORGANIZATION_ID");
            item.PACK = reader.GetNullable("PACK", Convert.ToInt32);
            item.PRICE_LIST_ID = reader.Get<string>("PRICE_LIST_ID");
            item.PRICE_LIST_NAME = reader.Get<string>("PRICE_LIST_NAME");
            item.PRICELIST_CURRENCY_TYPE = reader.Get<string>("PRICELIST_CURRENCY_TYPE");
            item.PRICELIST_PRICE = reader.GetNullable("PRICELIST_PRICE", Convert.ToDouble);
            item.PRICELIST_PRICE_IN_USD = reader.GetNullable("PRICELIST_PRICE_IN_USD", Convert.ToDouble);
            item.PRICELIST_PRICE_IN_USD_MCG = reader.GetNullable("PRICELIST_PRICE_IN_USD_MCG", Convert.ToDouble);
            item.PRICELIST_PRICE_MCG = reader.GetNullable("PRICELIST_PRICE_MCG", Convert.ToDouble);
            item.PRODUCT_FAMILY = reader.Get<string>("PRODUCT_FAMILY");
            item.PRODUCT_NAME = reader.Get<string>("PRODUCT_NAME");
            item.PRODUCT_NUMBER = reader.Get<string>("PRODUCT_NUMBER");
            item.REGION = reader.Get<string>("REGION");
            item.STATUS_TO_STATUS = reader.Get<string>("STATUS_TO_STATUS");
            item.STRENGTH = reader.GetNullable("STRENGTH", Convert.ToInt32);
            item.TYPE_TO_CHANNEL = reader.Get<string>("TYPE_TO_CHANNEL");
            item.UOM = reader.Get<string>("UOM");

            return item;
        }

        private static ExchangeRateModel MapDataToOracleExchangeRate(IDataReader reader)
        {
            var item = new ExchangeRateModel();
            item.EFFECTIVE_DATE = reader.GetNullable<DateTime>("EFFECTIVE_DATE");
            item.EXCHANGE_RATE = reader.GetNullable("EXCHANGE_RATE", Convert.ToDouble);
            item.ATE_TYPE = reader.Get<string>("RATE_TYPE");
            item.CREATE_DATE = reader.GetNullable<DateTime>("CREATE_DATE");
            item.FROM_CURR = reader.Get<string>("FROM_CURR");
            item.TO_CURR = reader.Get<string>("TO_CURR");
            item.UPDATE_DATE = reader.GetNullable<DateTime>("UPDATE_DATE");

            return item;
        }

        private static CountryModel MapDataToOracleCountry(IDataReader reader)
        {
            var item = new CountryModel();
            item.COUNTRY_CODE = reader.Get<string>("COUNTRY_CODE");
            item.COUNTRY_ID = reader.GetNullable("COUNTRY_ID", Convert.ToInt32).Value;
            item.COUNTRY_NAME = reader.Get<string>("COUNTRY_NAME");
            item.IS_ACTIVE = reader.Get<string>("IS_ACTIVE");
            item.IS_DELETE = reader.Get<string>("IS_DELETE");
            item.ODS_CREATE_BY = reader.Get<string>("ODS_CREATE_BY");
            item.ODS_CREATE_DT = reader.GetNullable<DateTime>("ODS_CREATE_DT").Value;
            item.ODS_LAST_UPD_BY = reader.Get<string>("ODS_LAST_UPD_BY");
            item.ODS_LAST_UPD_DT = reader.GetNullable<DateTime>("ODS_LAST_UPD_DT").Value;
            item.SOURCE_SYS_CREATED_BY = reader.Get<string>("SOURCE_SYS_CREATED_BY");
            item.SOURCE_SYS_CREATED_DT = reader.GetNullable<DateTime>("SOURCE_SYS_CREATED_DT").Value;
            item.SOURCE_SYS_LAST_UPD_BY = reader.Get<string>("SOURCE_SYS_LAST_UPD_BY");
            item.SOURCE_SYS_LAST_UPD_DT = reader.GetNullable<DateTime>("SOURCE_SYS_LAST_UPD_DT").Value;
            item.SOURCE_SYSTEM_NAME = reader.Get<string>("SOURCE_SYSTEM_NAME");

            return item;
        }

        private static ProductModel MapDataToOracleProduct(IDataReader reader)
        {
            var item = new ProductModel();
            item.PRODUCT_ID = reader.GetNullable("PRODUCT_ID", Convert.ToInt32).Value;
            item.PRODUCT_NAME = reader.Get<string>("PRODUCT_NAME");
            item.IS_ACTIVE = reader.Get<string>("IS_ACTIVE");
            item.IS_DELETE = reader.Get<string>("IS_DELETE");
            item.ODS_CREATE_BY = reader.Get<string>("ODS_CREATE_BY");
            item.ODS_CREATE_DT = reader.GetNullable<DateTime>("ODS_CREATE_DT").Value;
            item.ODS_LAST_UPD_BY = reader.Get<string>("ODS_LAST_UPD_BY");
            item.ODS_LAST_UPD_DT = reader.GetNullable<DateTime>("ODS_LAST_UPD_DT").Value;
            item.SOURCE_SYS_CREATED_BY = reader.Get<string>("SOURCE_SYS_CREATED_BY");
            item.SOURCE_SYS_CREATED_DT = reader.GetNullable<DateTime>("SOURCE_SYS_CREATED_DT").Value;
            item.SOURCE_SYS_LAST_UPD_BY = reader.Get<string>("SOURCE_SYS_LAST_UPD_BY");
            item.SOURCE_SYS_LAST_UPD_DT = reader.GetNullable<DateTime>("SOURCE_SYS_LAST_UPD_DT").Value;
            item.SOURCE_SYSTEM_NAME = reader.Get<string>("SOURCE_SYSTEM_NAME");

            return item;
        }

        private void SetLoadOnError(int loadId, string comment)
        {
            UpdateTaskStatus(loadId, comment, 4);
        }

        private void SetLoadCompleted(int loadId, string comment)
        {
            UpdateTaskStatus(loadId, comment, 2);
        }

        private static void UpdateTaskStatus(int loadId, string comment, int status)
        {
            var dictionnary = new Dictionary<string, object>
            {
                {"id", loadId},
                {"comment", comment},
                {"status", status},
            };
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString,
                "UPDATE [dbo].[Load] SET [Status] = @status, [Comment] = @comment WHERE [Id] = @id", dictionnary);
        }

        #endregion

        #region ItemDetail
        public int GetLoadItemId(int loadId, string loadItemName)
        {
            var dictionary = new Dictionary<string, object>();
            dictionary.Add("loadId", loadId);
            dictionary.Add("loadItemName", loadItemName);
            var query = @"SELECT top 1 Id
                            FROM LoadItem
                            WHERE LoadId=@loadId AND Name=@loadItemName";
            return RequestHelper.ExecuteScalarRequest<int>(DataBaseConstants.ConnectionString,query, dictionary);
        }

        public void ValidateLoadItemDetail(int loadId, string loadItemName, int productId, int geographyId)
        {
            var updateDate = DateTime.Now;
            productId = productId == 0 ? ApplicationConstants.NotSpecifiedProduct : productId;
            var loadItemId = GetLoadItemId(loadId, loadItemName);
            var dictionary = new Dictionary<string, object>();
            dictionary.Add("productId", productId);
            dictionary.Add("loadItemId", loadItemId);
            dictionary.Add("geographyId", geographyId);
            var query = @"UPDATE [dbo].[LoadItemDetail]
                        SET [Validated] = 1
                        WHERE GeographyId = @geographyId AND ProductId = @productId AND LoadItemId = @loadItemId";

            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, dictionary);
            

            var validateLoadItemQuery = @"UPDATE LoadItem 
                                            SET [LastUpdateDate] = @updateDate, [Status] = CASE 
                                                WHEN (SELECT Count(*) 
                                                      FROM LoadItemDetail 
                                                      WHERE Validated = 0 AND LoadItemId =@loadItemId) = 0 
                                                    THEN 3 
                                                    ELSE 2 
                                                END
                                            WHERE Id = @loadItemId";
            var dItem = new Dictionary<string, object>();
            dItem.Add("loadItemId", loadItemId);
            dItem.Add("updateDate", updateDate);

            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, validateLoadItemQuery, dItem);

            query = @"Update [Load] Set [LastUpdateDate] = @updateDate WHERE Id IN (SELECT LoadId FROM LoadItem WHERE [Id]=@loadItemId)";
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, dItem);
         
            var queryLoad = @"Update [Load]
                            Set [Status] = 3, [LastUpdateDate] = @updateDate
                            WHERE Id IN (SELECT LoadId FROM LoadItem
			                             WHERE [Status] = 3
			                             GROUP BY LoadId
			                             HAVING Count(*) = 10)";

            const string queryPush = "EXEC dbo.PushPriceUpdate @loadId,@loadItemId";
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, queryPush, new Dictionary<string, object>{{"loadId",loadId},{"loadItemId",0}});

            var dic = new Dictionary<string, object> {{"updateDate", updateDate}};
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, queryLoad, dic);

            var updateTracking = "DELETE [IRP_DB].[dbo].[StatusTracking] WHERE DataTable=@dataTable";
            var parameters = new Dictionary<string, object>
            {
                {"dataTable", "PriceUpdate"},
                {"gprmPushDate", DateTime.Now },
                {"gprmPushBy",  "System"},
                {"pulled", false },

            };
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, updateTracking, parameters);
            updateTracking =
                @"INSERT INTO [IRP_DB].[dbo].[StatusTracking] (DataTable, GprmPushDate, GprmPushBy, Pulled)
                VALUES (@dataTable, @gprmPushDate, @gprmPushBy, @pulled)";
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, updateTracking, parameters);
        }

        public List<LoadItemDetailViewModel> GetLoadItemDetailToValidate(int loadId, string loadItemName)
        {
            var loadItemId = GetLoadItemId(loadId, loadItemName);
            var dictionary = new Dictionary<string, object>();
            dictionary.Add("loadItemId", loadItemId);
            var query = @"SELECT *
                          FROM [dbo].[LoadItemDetail]
                          WHERE LoadItemId = @loadItemId
                          AND Validated = 0";
            return RequestHelper.ExecuteQuery<LoadItemDetailViewModel>(DataBaseConstants.ConnectionString, query, MapDataToLoadItemDetail, dictionary).ToList();

        }

        public void CreateProductGeographyScenario(int loadId, string loadItemName)
        {
            var loadItemId = GetLoadItemId(loadId, loadItemName);
            var dictionary = new Dictionary<string, object>();
            dictionary.Add("loadItemId", loadItemId);
            var query = @"INSERT INTO [dbo].[LoadItemDetail]
                                       ([LoadItemId]
                                       ,[Validated]
                                       ,[GeographyId]
                                       ,[ProductId])
                            SELECT @loadItemId, 0, g.Id, p.Id
                            FROM [Geography] g
                            INNER JOIN Product p ON 1=1
                            WHERE g.GeographyTypeId = 3 AND g.Id !=1 AND p.Id != 15 AND g.Active = 1 AND p.Active = 1";
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, dictionary);
        }

        public void CreateProductGeographyScenarioForRule(int loadId, string loadItemName)
        {
            var loadItemId = GetLoadItemId(loadId, loadItemName);
            var dictionary = new Dictionary<string, object>();
            dictionary.Add("loadItemId", loadItemId);
            var query = @"INSERT INTO [dbo].[LoadItemDetail]
                                       ([LoadItemId]
                                       ,[Validated]
                                       ,[GeographyId]
                                       ,[ProductId])
                            SELECT @loadItemId, 0, g.Id, 15
                            FROM [Geography] g
                            WHERE g.GeographyTypeId = 3 AND g.Id !=1 AND g.Active = 1";
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, dictionary);
        }

        public void CreateLoadItemDetailGeography(int loadId, string loadItemName)
        {
            var loadItemId = GetLoadItemId(loadId, loadItemName);
            var dictionary = new Dictionary<string, object>();
            dictionary.Add("loadItemId", loadItemId);

            var queryGeography = @"INSERT INTO [dbo].[LoadItemDetail]
                                       ([LoadItemId]
                                       ,[Validated]
                                       ,[GeographyId]
                                       ,[ProductId])
                            SELECT @loadItemId, 0, g.Id, 0
                            FROM [Geography] g
                            WHERE g.GeographyTypeId = 3 AND g.Id != 1 AND g.Active = 1";

            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, queryGeography, dictionary);
        }
        #endregion 
    

        
    }

    public static class TableRowExtensions
    {
        public static T? GetNullable<T>(this IDataReader reader, string columnName) where T : struct
        {
            var columnOrdinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(columnOrdinal))
            {
                return null;
            }
            var value = reader.GetValue(columnOrdinal);
            return (T)value;
        }

        public static T Get<T>(this IDataReader reader, string columnName) where T : class
        {
            var columnOrdinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(columnOrdinal))
            {
                return default(T);
            }
            var value = reader.GetValue(columnOrdinal);
            if (value is T)
            {
                return (T)value;
            }
            return null;
        }

        public static T? GetNullable<T>(this IDataReader reader, string columnName, Func<object, T> convertFunc) where T : struct
        {
            var columnOrdinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(columnOrdinal))
            {
                return null;
            }
            var value = reader.GetValue(columnOrdinal);
            if (value is T)
            {
                return (T)value;
            }
            return convertFunc(value);
        }
    }
}