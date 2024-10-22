using System.Collections.Generic;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public interface IProductRepository
    {
        IEnumerable<ProductViewModel> GetAllProducts();
        IEnumerable<ProductViewModel> GetAllProductsForFilter();
        ProductSearchResponseViewModel GetPagedProducts(ProductSearchRequestViewModel productSearch);
        void SaveProduct(IEnumerable<ProductViewModel> products);
        void SaveLoadProduct(SaveProductModel saveProductModel);
        IEnumerable<ProductUnitViewModel> GetProductUnits(int productId);
        bool AddProductUnit(ProductUnitViewModel model);
        bool DeleteProductUnit(ProductUnitViewModel model);
        bool UpdateProductUnit(ProductUnitViewModel model);
    }
}