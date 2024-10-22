using System.Collections.Generic;
using System.Data;
using System.Linq;
using PriceCare.Web.Constants;
using PriceCare.Web.Helpers;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public interface IDimensionDictionaryRepository
    {
        bool Create(DimensionDictionaryModel model);
        DimensionDictionarySearchResponseViewModel GetAllForDimension(DimensionDictionarySearchRequestViewModel request);
        void Update(IEnumerable<DimensionDictionaryModel> model);
        int? GetExistingDimensionId(string [] names, string dimension);

        DimensionDictionarySearchResponseViewModel GetAllGcodsForDimension(
            DimensionDictionarySearchRequestViewModel request);

        void Delete(DimensionDictionaryModel model);
    }
    public class DimensionDictionaryRepository : IDimensionDictionaryRepository
    {
        private const int Systemid = 16;
        public bool Create(DimensionDictionaryModel model)
        {
            model.SystemId = model.SystemId == 0 ? Systemid : model.SystemId;
            if (CanCreateSynonym(model))
            {
                var query = "INSERT INTO DimensionDictionary (Dimension,DimensionId,SystemId,Name) " +
                        "VALUES (@dimension, @dimensionId, @systemId, @name)";

                var dictionnary = new Dictionary<string, object>
                {
                    {"dimension", model.Dimension},
                    {"dimensionId", model.DimensionId},
                    {"systemId", model.SystemId},
                    {"name", model.Name},
                };

                int affectedRows = RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, dictionnary);

                return affectedRows == 1;
            }
            return false;
        }

        public int? GetExistingDimensionId(string[] names, string dimension)
        {
            string query = "SELECT DISTINCT DimensionId FROM DimensionDictionary WHERE Dimension=@dimension AND Name in (";
            foreach (var name in names)
            {
                query += "'"+name + "', ";
            }
            query = query.Substring(0, query.Length - 2);
            query += ")";

            var dictionary = new Dictionary<string, object>
            {
                {"dimension", dimension}
            };

            var id = RequestHelper.ExecuteScalarRequest<int?>(DataBaseConstants.ConnectionString, query, dictionary);
            return id;
        }

        private bool CanCreateSynonym(DimensionDictionaryModel model)
        {            
            const string query = "SELECT COUNT (*) FROM DimensionDictionary WHERE Dimension=@dimension AND DimensionId=@dimensionId AND Name=@name AND SystemId=@systemId";
            var dictionary = new Dictionary<string, object>
            {
                {"dimension", model.Dimension},
                {"dimensionId", model.DimensionId},
                {"name", model.Name},
                {"systemId", model.SystemId}
            };

            var count = RequestHelper.ExecuteScalarRequest<int>(DataBaseConstants.ConnectionString, query, dictionary);
            return count == 0;
        }

        public DimensionDictionarySearchResponseViewModel GetAllForDimension(DimensionDictionarySearchRequestViewModel request)
        {
            var query = "SELECT '" + request.DimensionType +
                    "' AS Dimension, Id AS DimensionId, 0 AS SystemId, Name AS Name FROM " + request.DimensionType +
                    " WHERE Active=1 Order By Name"; ;

            var result = RequestHelper.ExecuteQuery<DimensionDictionaryModel>(
                    DataBaseConstants.ConnectionString,
                    query,
                    MapData).ToList();

            var totalRows = result.Count();

            // pagination
            //result = result
            //    .Skip(request.PageNumber * request.ItemsPerPage)
            //    .Take(request.ItemsPerPage)
            //    .ToList();

            var viewModel = new DimensionDictionarySearchResponseViewModel
            {
                DimensionDictionary = result,
                IsLastPage = (result.Count() + (request.PageNumber * request.ItemsPerPage)) >= totalRows,
                PageNumber = ++request.PageNumber,
                TotalRows = totalRows
            };

            return viewModel;
        }

        public DimensionDictionarySearchResponseViewModel GetAllGcodsForDimension(DimensionDictionarySearchRequestViewModel request)
        {
            var query = "SELECT * FROM DimensionDictionary WHERE Dimension = @dimensionType AND SystemId=16";

            var result = RequestHelper.ExecuteQuery<DimensionDictionaryModel>(
                    DataBaseConstants.ConnectionString,
                    query,
                    MapData,
                    new Dictionary<string, object>
                    {
                       { "dimensionType", request.DimensionType}
                    }
                ).ToList();



            query = "SELECT '" + request.DimensionType +
                    "' AS Dimension, Id AS DimensionId, 0 AS SystemId, Name AS Name FROM " + request.DimensionType +
                    " WHERE Active=1";

            var result2 = RequestHelper.ExecuteQuery<DimensionDictionaryModel>(
                DataBaseConstants.ConnectionString,
                query,
                MapData).ToList();
            
            result.AddRange(result2);

            var totalRows = result.Count();

            // pagination
            //result = result
            //    .Skip(request.PageNumber * request.ItemsPerPage)
            //    .Take(request.ItemsPerPage)
            //    .ToList();

            var viewModel = new DimensionDictionarySearchResponseViewModel
            {
                DimensionDictionary = result,
                IsLastPage = (result.Count() + (request.PageNumber * request.ItemsPerPage)) >= totalRows,
                PageNumber = ++request.PageNumber,
                TotalRows = totalRows
            };

            return viewModel;
        }

        public void Delete(DimensionDictionaryModel model)
        {
            const string query = "DELETE FROM DimensionDictionary WHERE DimensionId=@dimensionId AND SystemId=@systemId AND Name=@name";

            var dictionary = new Dictionary<string, object>
            {                
                {"dimensionId", model.DimensionId},
                {"systemId", 16},
                {"name", model.Name}
            };

            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, dictionary);
        }

        public void Update(IEnumerable<DimensionDictionaryModel> models)
        {
            foreach (var model in models)
            {
                Update(model);
            }
        }

        private void Update(DimensionDictionaryModel model)
        {
            var query = "UPDATE DimensionDictionary SET DimensionId=@dimensionId, Name=@name " +
                        "WHERE Dimension=@dimension AND SystemId=@oldSystemId AND DimensionId=@oldDimensionId AND Name=@oldName";
            var dictionnary = new Dictionary<string, object> { 
                { "dimensionId", model.NewDimensionId },
                { "name", model.NewName },
                { "dimension", model.Dimension },
                { "oldSystemId", model.SystemId },
                { "oldDimensionId", model.DimensionId },
                { "oldName", model.Name },
            };

            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, dictionnary);
        }


        public DimensionDictionaryModel MapData(DataRow row)
        {
            var product = new DimensionDictionaryModel
            {
                Dimension = row["Dimension"].ToString(),
                Name = row["Name"].ToString(),
                DimensionId = (int)row["DimensionId"],
                SystemId = (int)row["SystemId"]
            };
            product.NewName = product.Name;
            product.NewDimensionId = product.DimensionId;
            return product;
        }

    }
}