using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using PriceCare.Web.Models;
using PriceCare.Web.Repository;

namespace PriceCare.Web.Controllers
{
    [RoutePrefix("api/product")]
    [Authorize]
    public class ProductController : ApiController
    {
        private readonly IProductRepository productRepository;
        private readonly ILoadRepository loadRepository;

        public ProductController(IProductRepository productRepository, ILoadRepository loadRepository)
        {
            this.productRepository = productRepository;
            this.loadRepository = loadRepository;
        }

        [Route("all")]
        public IEnumerable<ProductViewModel>  GetAllProducts()
        {
            return productRepository.GetAllProducts();
        }

        [Route("products")]
        public IEnumerable<ProductViewModel> GetAllProductsForFilter()
        {
            return productRepository.GetAllProductsForFilter();
        } 
        
        [Route("paged")]
        [HttpPost]
        public ProductSearchResponseViewModel GetAllProducts(ProductSearchRequestViewModel productSearch)
        {
            if (productSearch.Validate)
                return loadRepository.GetProductToValidate(productSearch);
            return productRepository.GetPagedProducts(productSearch);
        }

        [Route("save")]
        [HttpPost]
        public void SaveProduct(SaveProductModel save)
        {
            if(save.Validate)
                productRepository.SaveLoadProduct(save);
            productRepository.SaveProduct(save.Products);
        }

        [Route("units/{productId}")]
        public List<ProductUnitViewModel> GetProductUnits(int productId)
        {
            return productRepository.GetProductUnits(productId).ToList();
        }

        [Route("addProductUnit")]
        public bool AddProductUnit(ProductUnitViewModel model)
        {
            return productRepository.AddProductUnit(model);
        }

        [HttpPost]
        [Route("deleteProductUnit")]
        public bool DeleteProductUnit(ProductUnitViewModel model)
        {
            return productRepository.DeleteProductUnit(model);
        }

        [Route("updateProductUnit")]
        public bool UpdateProductUnit(ProductUnitViewModel model)
        {
            return productRepository.UpdateProductUnit(model);
        }
    }
}