using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Ninject.Activation;
using PriceCare.Web.Constants;
using PriceCare.Web.Helpers;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly string connectionString = DataBaseConstants.ConnectionString;
        private readonly LoadRepository loadRepository = new LoadRepository();
        private readonly IDimensionDictionaryRepository dimensionDictionaryRepository = new DimensionDictionaryRepository();

        public IEnumerable<ProductViewModel> GetAllProductsForFilter()
        {
            const string query = "SELECT Id, Name, ShortName FROM Product ORDER BY Name";

            var result = RequestHelper.ExecuteQuery(DataBaseConstants.ConnectionString, query, row => new ProductViewModel
            {
                Id = (int) row["Id"],
                Name = (string) row["Name"],
                ShortName = (string) row["ShortName"],
            }).Where(row => row.ShortName != "XX").ToList();
            return result;
        } 

        public IEnumerable<ProductViewModel> GetAllProducts()
        {
            const string query = "Exec GetProducts @OnlyActive = true, @OnlyDefault = true";
            var result = RequestHelper.ExecuteQuery(
                    connectionString,
                    query,
                    MapData
                );

            return result.OrderBy(p => p.Name);
        }

        public ProductSearchResponseViewModel GetPagedProducts(ProductSearchRequestViewModel productSearch)
        {
            const string query = "Exec GetProducts @OnlyActive = false, @OnlyDefault = true";

            var result = RequestHelper.ExecuteQuery(
                    connectionString,
                    query,
                    MapData
                );

            var totalProducts = result.Count();

            // pagination
            result = result
                .Skip(productSearch.PageNumber * productSearch.ItemsPerPage)
                .Take(productSearch.ItemsPerPage)
                .ToList();

            var viewModel = new ProductSearchResponseViewModel
            {
                Products = result,
                IsLastPage = (result.Count() + (productSearch.PageNumber * productSearch.ItemsPerPage)) >= totalProducts,
                PageNumber = ++productSearch.PageNumber,
                TotalProducts = totalProducts
            };

            return viewModel;
        }

        public void SaveLoadProduct(SaveProductModel saveProductModel)
        {
            var productsToUpdate = new List<ProductViewModel>();
            foreach (var productToSave in saveProductModel.Products)
            {
                if (productToSave.Id != 0)
                {
                    productsToUpdate.Add(productToSave);
                }
                else
                {
                    var id = dimensionDictionaryRepository.GetExistingDimensionId(
                        new []
                        {
                            productToSave.Name, 
                            productToSave.ExportName, 
                            productToSave.DisplayName
                        },
                        DimensionConstant.Product);
                    if (id == null)
                    {
                        InsertProduct(productToSave);
                    }
                    else
                    {
                        productToSave.Id = (int) id;
                        productsToUpdate.Add(productToSave);
                    }
                }
            }

            if (productsToUpdate.Any())
                SaveProduct(productsToUpdate);

            loadRepository.ValidateLoadItem(saveProductModel.LoadId, LoadConstants.Product);
            
        }

        private void InsertProduct(ProductViewModel product)
        {
            var queryProduct = "INSERT INTO Product (Name,ShortName,UnitTypeId,Active) " +
                               "OUTPUT INSERTED.ID " +
                               "VALUES (@name, @shortName, @unitTypeId, @active)";
            var productId = RequestHelper.ExecuteScalarRequest<int>(DataBaseConstants.ConnectionString, queryProduct,
                new Dictionary<string, object>
                {
                    {"name", product.Name},
                    {"shortName", product.ShortName},
                    {"unitTypeId", 4},
                    {"active", 1}
                });

            var dimension = new DimensionDictionaryModel
            {
                Dimension = DimensionConstant.Product,
                DimensionId = productId,
                SystemId = (int) DimensionType.Excel,
                Name = product.ExportName
            };

            dimensionDictionaryRepository.Create(dimension);

            dimension.SystemId = (int) DimensionType.Display;
            dimension.Name = product.DisplayName;

            dimensionDictionaryRepository.Create(dimension);

            dimension.SystemId = (int) DimensionType.Gcods;
            dimension.Name = product.Name;

            dimensionDictionaryRepository.Create(dimension);

            var queryProductUnit = "INSERT INTO ProductUnit (ProductId, UnitId, FactorScreen, Active, IsDefault) " +
                                   "VALUES (@productId, @unitId, @factorScreen, @active, @isDefault)";
            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, queryProductUnit,
                new Dictionary<string, object>
                {
                    {"productId", productId},
                    {"unitId", product.BaseConsolidationUnitId},
                    {"factorScreen", product.FactorScreen},
                    {"active", product.Active},
                    {"isDefault", 1}
                });
        }

        public void SaveProduct(IEnumerable<ProductViewModel> products)
        {
            var listIds = products.Select(p => p.Id);
            string query = "SELECT * FROM Product WHERE Id in ( ";
            foreach (var id in listIds)
            {
                query += id +" , ";
            }
            query = query.Remove(query.Length - 3); 
            query += " )";
            var originalProducts = RequestHelper.ExecuteQuery<ProductViewModel>(connectionString, query, MapProduct);
            foreach (var product in products)
            {
                var original = originalProducts.FirstOrDefault(op => op.Id == product.Id);

                if(original == null) 
                    continue;

                const string queryL2S = "Exec UpdateProduct @ProductId = @productId, " +
                                    "@UpdateExportName = @updateExportName, @ExportName = @exportName, " +
                                    "@UpdateDisplayName = @updateDisplayName, @DisplayName = @displayName, " +
                                    "@UpdateShortName = @updateShortName, @ShortName = @shortName, " +
                                    "@UpdateActiveStatus = @updateActiveStatus, @ActiveStatus = @activeStatus, " +
                                    "@UpdateDefaultConsolidationFactor = @updateDefaultConsolidationFactor, " +
                                    "@DefaultUnitId = @defaultUnitId, @DefaultUnitFactor = @defaultUnitFactor ";
                RequestHelper.ExecuteScalarRequest<ProductViewModel>(
                    connectionString,
                    queryL2S,
                    new Dictionary<string, object>
                    {
                        {"productId", product.Id},
                        {"updateExportName", original.ExportName != product.ExportName},
                        {"exportName", product.ExportName},
                        {"updateDisplayName", original.DisplayName != product.DisplayName},
                        {"displayName", product.DisplayName},
                        {"updateShortName", original.ShortName != product.ShortName},
                        {"shortName", product.ShortName},
                        {"updateActiveStatus", original.Active != product.Active},
                        {"activeStatus", product.Active},
                        {
                            "updateDefaultConsolidationFactor", original.BaseConsolidationUnitId != product.BaseConsolidationUnitId
                                    && System.Math.Abs(original.FactorScreen - product.FactorScreen) >= 0.00001
                        },
                        {"defaultUnitId", product.BaseConsolidationUnitId},
                        {"defaultUnitFactor", product.FactorScreen},
                    }
                );
            }
            
        }

        public IEnumerable<ProductUnitViewModel> GetProductUnits(int productId)
        {
            const string query = "SELECT Id, ProductId, UnitId, FactorScreen, Active, IsDefault " +
                                 "FROM ProductUnit " +
                                 "WHERE ProductId=@productId";

            var dictionary = new Dictionary<string, object>
            {
                {"productId", productId}
            };

            var units = RequestHelper.ExecuteQuery(DataBaseConstants.ConnectionString, query,
                row => new ProductUnitViewModel
                {
                    Id = (int)row["Id"],
                    ProductId = (int)row["ProductId"],
                    UnitId = (int)row["UnitId"],
                    FactorScreen = (int)row["FactorScreen"],
                    Active = (bool)row["Active"],
                    IsDefault = (bool)row["IsDefault"]
                }, dictionary);

            return units;
        }

        public bool AddProductUnit(ProductUnitViewModel model)
        {            
            if (model.IsDefault)
            {
                SetProductUnitAsNotDefault(model.ProductId);
            }

            const string query =
                "INSERT INTO ProductUnit VALUES (@productId, @unitId, @factorScreen, @active, @isDefault)";

            var dictionary = new Dictionary<string, object>
            {
                {"productId", model.ProductId},
                {"unitId", model.UnitId},
                {"factorScreen", model.FactorScreen},
                {"active", model.Active},
                {"isDefault", model.IsDefault}
            };

            var count = RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, dictionary);

            return count ==1;
        }

        public bool DeleteProductUnit(ProductUnitViewModel model)
        {
            const string query = "DELETE FROM ProductUnit WHERE id=@id";

            var dictionary = new Dictionary<string, object>
            {
                {"id", model.Id}
            };

            var count = RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, dictionary);

            return count == 1;
        }

        public bool UpdateProductUnit(ProductUnitViewModel model)
        {
            if (model.IsDefault)
            {
                SetProductUnitAsNotDefault(model.ProductId);
            }

            const string query =
                "UPDATE ProductUnit SET ProductId=@productId, UnitId=@unitId, FactorScreen=@factorScreen, Active=@active, IsDefault=@isDefault WHERE Id=@id";

            var dictionary = new Dictionary<string, object>
            {
                {"productId", model.ProductId},
                {"unitId", model.UnitId},
                {"factorScreen", model.FactorScreen},
                {"active", model.Active},
                {"id", model.Id},
                {"isDefault", model.IsDefault}
            };

            var count = RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, dictionary);

            return count == 1;
        }

        private void SetProductUnitAsNotDefault(int productId)
        {
            const string query = "UPDATE ProductUnit Set IsDefault=@isDefault WHERE productId=@productId";

            var dictionary = new Dictionary<string, object>
            {
                {"isDefault", false},
                {"productId", productId}
            };

            RequestHelper.ExecuteNonQuery(DataBaseConstants.ConnectionString, query, dictionary);
        }

        private static ProductViewModel MapData(DataRow row)
        {
            var product = new ProductViewModel
            {
                Id = (int) row["ProductId"],
                Name = row["Name"].ToString(),
                ShortName = row["ShortName"].ToString(),
                BaseConsolidationUnit = row["Unit"].ToString().ToLower(),                             
                BaseConsolidationUnitId = (int)row["UnitId"],                             
                ExportName = row["ExportName"].ToString(),
                Factor = (double) row["Factor"],
                DisplayName = row["DisplayName"].ToString(),
                FactorScreen = (int)row["FactorScreen"]
            };

            product.DisplayConsolidationUnit = product.FactorScreen + product.BaseConsolidationUnit;

            if(row.Table.Columns.Contains("ActiveStatus"))
                product.Active = (bool) row["ActiveStatus"];

            return product;
        }

        public static ProductViewModel MapProduct(DataRow row)
        {
            var product = new ProductViewModel
            {
                Id = (int)row["Id"],
                Name = row["Name"].ToString(),
                ShortName = row["ShortName"].ToString(),
                UnitTypeId = (int)row["UnitTypeId"],
                Active = (bool)row["Active"]
            };
            return product;
        }
        
        #region Export Product


        public IEnumerable<ProductViewModel> GetProductExport()
        {
            var query = "SELECT * FROM GetProductExportName() ORDER BY Name";

            return RequestHelper.ExecuteQuery(DataBaseConstants.ConnectionString, query, MapProductExport);
        }

        private ProductViewModel MapProductExport(DataRow row)
        {
            var product = new ProductViewModel
            {
                Id = (int)row["ProductId"],
                Name = row["Name"].ToString(),
                ShortName = row["ShortName"].ToString()
            };

            return product;
        }

        #endregion
    }
}