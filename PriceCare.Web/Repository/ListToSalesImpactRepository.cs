using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using PriceCare.Web.Constants;
using PriceCare.Web.Helpers;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public class ListToSalesImpactRepository : IListToSalesImpactRepository
    {
        private readonly string connectionString = DataBaseConstants.ConnectionString;
        private readonly IVersionRepository versionRepository = new VersionRepository();


        private IEnumerable<ListToSalesImpactViewModel> GetListToSalesImpacts(bool isData, int saveId, int versionId,
            List<int> geographyId, List<int> productId)
        {
            const string queryL2St = "SELECT * FROM GetListToSalesImpact ( @isData , @saveId , @versionId , @geographyId , @productId )";
            var listToSalesImpact = RequestHelper.ExecuteQuery(
                connectionString,
                queryL2St,
                MapListToSalesImpact,
                new Dictionary<string, object>
                {
                    {"isData", isData},
                    {"saveId",saveId},
                    {"versionId",versionId},
                },
                new Dictionary<string, List<int>>
                {
                    {"geographyId",geographyId},
                    {"productId",productId}
                }
                ).ToList();

            return listToSalesImpact;
        }

        public IEnumerable<ListToSalesImpactViewModel> GetVersionListToSalesImpact(int versionId, List<int> geographyId, List<int> productId)
        {
            return GetListToSalesImpacts(true, 0, versionId, geographyId, productId);
        }

        public ListToSalesImpactSearchResponseViewModel GetPagedListToSalesImpact(
            ListToSalesSeachRequestViewModel listToSalesSeachRequest)
        {
            var result = GetVersionListToSalesImpact(listToSalesSeachRequest.VersionId, listToSalesSeachRequest.CountriesId, listToSalesSeachRequest.ProductsId);

            result = result.OrderBy(lts => lts.GeographyName);

            var allCountriesFound = result.Select(c => c.GeographyId).Distinct();
            var allCountriesToSend = allCountriesFound.Skip(listToSalesSeachRequest.PageNumber * listToSalesSeachRequest.ItemsPerPage)
                .Take(listToSalesSeachRequest.ItemsPerPage);
            var totalCountries = allCountriesFound.Count();

            result = result.Where(ltsi => allCountriesToSend.Contains(ltsi.GeographyId));

            var viewModel = new ListToSalesImpactSearchResponseViewModel
            {
                ListToSalesImpacts = result,
                IsLastPage =
                    (allCountriesToSend.Count() + (listToSalesSeachRequest.PageNumber * listToSalesSeachRequest.ItemsPerPage)) >= totalCountries,
                PageNumber = ++listToSalesSeachRequest.PageNumber,
                TotalListToSales = totalCountries
            };

            return viewModel;
        }

        private static ListToSalesImpactViewModel MapListToSalesImpact(DataRow row)
        {
            return new ListToSalesImpactViewModel
            {
                Id = (int)row["Id"],
                GeographyId = (int)row["GeographyId"],
                GeographyName = row["GeographyName"].ToString(),
                ProductId = (int)row["ProductId"],
                SegmentId = (int)row["SegmentId"],
                SegmentName = row["SegmentName"].ToString(),
                ImpactDelay = (int)row["ImpactDelay"],
                ImpactPercentage = (double)row["ImpactPercentage"],
                //SaveId = (int)row["SaveId"],
                //SaveTypeId = (int)row["SaveTypeId"],
                //VersionId = (int)row["VersionId"]
            };
        }

        public void SaveVersion(List<ListToSalesImpactViewModel> listToSalesImpact)
        {
            // check integrity ?
            var versionId = versionRepository.CreateNewVersion("List To Sales Impact");
            foreach (var impact in listToSalesImpact)
            {
                impact.VersionId = versionId;
            }
            // todo SaveId + SaveTypeId
            var datatable = listToSalesImpact.ToDataTable(new List<string>() { "GeographyName", "SegmentName"});

            RequestHelper.BulkInsert(DataBaseConstants.ConnectionString, "ListToSalesImpact", datatable); 
        }
    }
}