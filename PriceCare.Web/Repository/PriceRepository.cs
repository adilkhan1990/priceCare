using System.Collections.Generic;
using PriceCare.Web.Constants;
using PriceCare.Web.Math;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public class PriceRepository : IPriceRepository
    {
        private readonly Forecast forecastRepository = new Forecast();
        
        public object GetPrices(PriceRequestViewModel model, bool fillData = true)
        {
        	var dr = new DataRepository();

            var products = new List<int> {  };
            if (model.ProductId != 0)
                products.Add(model.ProductId);

            if (model.VersionId.HasValue)
            {
                return dr.GetVersionData(
                        ApplicationConstants.ImportedDataSaveId,
                        model.VersionId.Value,
                        0,
                        model.GeographyIds,
                        products,
                        new List<int> { (int)DataTypes.Price },
                        fillData);
            }
            return dr.GetLatestData(
                ApplicationConstants.ImportedDataSaveId,
                model.GeographyIds,
                products,
                new List<int> { (int)DataTypes.Price }, fillData);
        }

        public object GetVersions(PriceRequestViewModel model)
        {
            var dr = new VersionRepository();
            var data = dr.GetDataVersion(model.GeographyIds ?? new List<int>(),
                 new List<int> { model.ProductId }, new List<int> { (int)DataTypes.Price });
            return data;
        }
        public List<DataViewModel> GetDataCache(PriceRequestViewModel dataSearch)
        {
                
            return forecastRepository.GetProductsForSimulation(dataSearch.SimulationId, dataSearch.GeographyIds,
                new List<int> { dataSearch.ProductId }, new List<int> { (int)DataTypes.Price });
        }

    }
}