using System;
using System.Collections.Generic;
using System.Data;
using PriceCare.Web.Constants;
using PriceCare.Web.Helpers;

namespace PriceCare.Web.Repository
{
    public interface IConsolidationRepository
    {
        object GetConsolidationForProduct(int productId);
    }

    public class ConsolidationRepository : IConsolidationRepository
    {
        public object GetConsolidationForProduct(int productId)
        {
            const string command = @"SELECT pu.[Id] ProductUnitId
      ,u.[id] UnitId
      ,[FactorScreen]
	  ,[IsDefault]
	  ,[Factor]
	  ,[Name]
  FROM [ProductUnit] pu 
  INNER JOIN [Unit] u
  ON pu.UnitId = u.Id
  WHERE [ProductId] = @productId";


            var parameters = new Dictionary<string, object> { { "productId", productId } };

            return RequestHelper.ExecuteQuery<ConsolidationViewModel>(DataBaseConstants.ConnectionString, command, MapRow, parameters);

        }

        private ConsolidationViewModel MapRow(DataRow dataRow)
        {
            return new ConsolidationViewModel
            {
                ProductUnitId = (int)dataRow[0],
                FactorScreen = (int) dataRow[2],
                Factor = (double)dataRow[4],
                Name = (string) dataRow[5],
                IsDefault = (bool) dataRow[3]
            };
        }
    }

    public class ConsolidationViewModel
    {
        public int ProductUnitId { get; set; }
        public int FactorScreen { get; set; }
        public double Factor { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
    }
}