using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Ninject.Activation;
using PriceCare.Web.Constants;
using PriceCare.Web.Helpers;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
	public class VersionRepository : IVersionRepository
	{
        private readonly IAccountRepository accountRepository = new AccountRepository();

	    public int GetCurrentVersion()
	    {
            const string query = "SELECT MAX(Id) FROM Version";
	        return RequestHelper.ExecuteScalarRequest<int>(DataBaseConstants.ConnectionString, query);
	    }
        public IEnumerable<VersionViewModel> GetCurrencyVersion(RateType rateType)
        {
            string query = "SELECT DISTINCT ";
            if (rateType == RateType.Budget)
                query += " CB.VersionId VersionId, V.VersionTime, V.UserId, V.Information FROM CurrencyBudget CB, Version V ";
            else
                query += " CS.VersionId VersionId, V.VersionTime, V.UserId, V.Information  FROM CurrencySpot CS, Version V ";

            query += "WHERE VersionId = V.Id ORDER BY VersionId DESC";

            var result = RequestHelper.ExecuteQuery(DataBaseConstants.ConnectionString, query, row => new VersionViewModel
            {
                VersionId = (int)row["VersionId"],
                VersionTime = Convert.ToDateTime(row["VersionTime"]),
                VersionTimeAsString = Convert.ToDateTime(row["VersionTime"]).ToString("g"),
                UserName = row["UserId"].ToString(),
                Information = row["Information"].ToString()
            }).ToList();

            return result;
        }

        public IEnumerable<VersionViewModel> GetGprmRuleVersion(int geographyId, int productId, int gprmRuleTypeId)
        {
            const string query =
                "SELECT VersionId, Information, VersionData, VersionTime, UserName FROM GetGprmRuleVersion(@geographyId, @productId, @gprmRuleTypeId)";
            return GetVersion(query, geographyId, productId, gprmRuleTypeId).OrderByDescending(v => v.VersionId);
        }
        public IEnumerable<VersionViewModel> GetPriceMapVersion(int geographyId, int productId, int gprmRuleTypeId)
        {
            const string query =
                "SELECT VersionId, Information, VersionData, VersionTime, UserName FROM GetPriceMapVersion(@geographyId, @productId, @gprmRuleTypeId)";
            return GetVersion(query, geographyId, productId, gprmRuleTypeId);
        }

        public IEnumerable<VersionViewModel> GetPriceMapVersion(List<int> geographyId, int productId, int gprmRuleTypeId)
        {
            const string query =
                "SELECT VersionId, Information, VersionData, VersionTime, UserName FROM GetPriceMapVersion(@geographyId, @productId, @gprmRuleTypeId)";
            
            return GetVersion(query, 0, productId, gprmRuleTypeId).OrderByDescending(v => v.VersionTime); // TODO: Jonathan, this must be improved ?
        }

        public IEnumerable<VersionViewModel> GetDataVersion(List<int> geographyId, List<int> productId, List<int> dataTypeId)
        {
            return GetVersion(geographyId, productId, dataTypeId);
        }
        public IEnumerable<VersionViewModel> GetDataVersion(List<int> productId, List<int> dataTypeId)
        {
            return GetVersion(new List<int>(), productId, dataTypeId);
        }
        public IEnumerable<VersionViewModel> GetListToSalesVersion(List<int> geographyId, List<int> productId)
        {
            return GetVersion(geographyId, productId);
        }

	    public int CreateNewVersion(string information)
	    {
	        const string queryString = "INSERT INTO dbo.Version " +
	                             "(Information, VersionTime, UserId) " +
	                             "OUTPUT INSERTED.ID " +
	                             "VALUES " +
	                             "(@information, @versionTime, @userId) ";
            var dictionnary = new Dictionary<string, object> { 
                { "information", information },
                { "versionTime", DateTime.Now },
                { "userId", accountRepository.GetUserId() },
            };

	        return RequestHelper.ExecuteScalarRequest<int>(DataBaseConstants.ConnectionString, queryString, dictionnary);
	    }

	    public IEnumerable<VersionViewModel> GetListToSalesVersion(List<int> productId)
        {
            return GetVersion(new List<int>(), productId);
        }
        private static IEnumerable<VersionViewModel> GetVersion(List<int> geographyId, List<int> productId, List<int> dataTypeId)
        {
            var connectionString = DataBaseConstants.ConnectionString;
            const string query =
                "SELECT VersionId, Information, VersionData, VersionTime, UserName FROM GetDataVersion(@geographyId,@productId,@dataTypeId)";
            var result =
                RequestHelper.ExecuteQuery(
                    connectionString,
                    query,
                    MapVersion,
                    new Dictionary<string, List<int>>
                    {
                        {"geographyId",geographyId},
                        {"productId",productId},
                        {"dataTypeId",dataTypeId}
                    });
            return result.OrderByDescending(v => v.VersionId);
        }
        private static IEnumerable<VersionViewModel> GetVersion(List<int> geographyId, List<int> productId)
        {
            var connectionString = DataBaseConstants.ConnectionString;
            const string query =
                "SELECT VersionId, Information, VersionData, VersionTime, UserName FROM GetListToSalesVersion(@geographyId,@productId)";
            var result =
                RequestHelper.ExecuteQuery(
                    connectionString,
                    query,
                    MapVersion,
                    new Dictionary<string, List<int>>
                    {
                        {"geographyId",geographyId},
                        {"productId",productId}
                    });
            return result;
        }
        private static IEnumerable<VersionViewModel> GetVersion(string query, int geographyId, int productId, int gprmRuleTypeId)
        {
            var connectionString = DataBaseConstants.ConnectionString;
            var result =
                RequestHelper.ExecuteQuery<VersionViewModel>(
                    connectionString,
                    query,
                    MapVersion,
                    new Dictionary<string, object>
                    {
                        {"geographyId",geographyId},
                        {"productId",productId},
                        {"gprmRuleTypeId",gprmRuleTypeId}
                    }).ToList();
            return result;
        }

	    public VersionViewModel GetVersionInfos(int versionId)
	    {
	        var query = "SELECT Id , Information , VersionTime , UserId FROM Version WHERE Id = @versionId";
            var result = RequestHelper.ExecuteQuery<VersionViewModel>(
                    DataBaseConstants.ConnectionString,
                    query,
                    MapVersionFromTableVersion,
                    new Dictionary<string, object>
                    {
                        {"versionId",versionId}
                    }).ToList();
	        return result.FirstOrDefault();
	    }

        private static VersionViewModel MapVersionFromTableVersion(DataRow row)
        {
            var data = new VersionViewModel
            {
                VersionId = (int)row["Id"],
                Information = row["Information"].ToString(),
                VersionTime = Convert.ToDateTime(row["VersionTime"]),
                VersionTimeAsString = Convert.ToDateTime(row["VersionTime"]).ToString("g"),
                UserName = row["UserId"].ToString()
            };
            return data;
        }

        private static VersionViewModel MapVersion(DataRow row)
        {
            var data = new VersionViewModel
            {
                VersionId = (int) row["VersionId"],
                Information = row["Information"].ToString(),
                VersionData = row["VersionData"].ToString(),
                VersionTime = Convert.ToDateTime(row["VersionTime"]),
                VersionTimeAsString = Convert.ToDateTime(row["VersionTime"]).ToString("g"),
                UserName = row["UserName"].ToString()
            };
            return data;
        }
	}
}