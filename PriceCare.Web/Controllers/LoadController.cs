using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using ClosedXML.Excel;
using PriceCare.Web.Constants;
using PriceCare.Web.Repository;
using PriceCare.Web.Models;

namespace PriceCare.Web.Controllers
{
    [RoutePrefix("api/load")]
    [Authorize]
    public class LoadController : ApiController
    {
        private const string pricesWorksheet = "Prices";
        private readonly ILoadRepository loadRepository;
        private readonly IFileHttpResponseCreator fileHttpResponseCreator;
        private readonly IPriceListToExcelDataTransformer priceListToExcelDataTransformer;
        private readonly IPriceListFromExcelTransformer _priceListFromExcelTransformer;

        public LoadController(ILoadRepository loadRepository, IFileHttpResponseCreator fileHttpResponseCreator, IPriceListToExcelDataTransformer priceListToExcelDataTransformer
            , IPriceListFromExcelTransformer priceListFromExcelTransformer)
        {
            this.loadRepository = loadRepository;
            this.fileHttpResponseCreator = fileHttpResponseCreator;
            this.priceListToExcelDataTransformer = priceListToExcelDataTransformer;
            _priceListFromExcelTransformer = priceListFromExcelTransformer;
        }

        [Route("get")]
        [HttpPost]
        public LoadSearchResponseViewModel GetLoads(LoadSearchRequest request) 
        {
            return loadRepository.GetPagedLoads(request);
        }

        [HttpPost]
        [Route("cancel/{loadId}")]
        public void CancelLoad(int loadId)
        {
            loadRepository.Cancel(loadId);
        }

        [Route("getStatus")]
        public IEnumerable<FilterItemViewModel> GetLoadStatus()
        {
            return loadRepository.GetLoadStatusForFilter();
        }

        [HttpPost]
        [Route("newLoad")]
        public string NewLoad(LoadCreateModel model)
        {
            var id = loadRepository.Create(new LoadViewModel { Name = model.Name, Status = 1, UserName = User.Identity.Name });
            return loadRepository.StartLoadSource(id);
        }

        [Route("detail/{loadId}")]
        public LoadDetailViewModel GetLoad(int loadId)
        {
            return loadRepository.GetLoadDetail(loadId);
        }

        [Route("start/{loadId}")]
        public string StartLoad(int loadId)
        {
            return loadRepository.StartLoadSource(loadId);
        }

        [Route("next/{loadId:int}/{loadItemName}")]
        public object GetNext(int loadId, string loadItemName)
        {
            var itemsToValidate = loadRepository.GetLoadItemDetailToValidate(loadId, loadItemName);

            return itemsToValidate.GroupBy(i => i.ProductId, (key, group) => 
                new { ProductId = key, GeographyIds = group.Select(g => g.GeographyId) }).ToList();
        }

        [HttpPost]
        [Route("validateLoadItemDetail")]
        public object ValidateLoadItemDetail(ValidateLoadItemDetailViewModel model)
        {
            foreach (var geographyId in model.GeographyIds)
            {
                loadRepository.ValidateLoadItemDetail(model.LoadId, model.LoadItemName, model.ProductId, geographyId);
            }
            
            var loadItem = loadRepository.GetLoadItem(model.LoadId, model.LoadItemName);
            if (loadItem.Status == (int)LoadStatus.Validated)
            {
                return new {Url = "/data/load/" + model.LoadId};
            }
            return new {Url = ""};
        }


        [Route("validateLoadItem/{loadId}/{loadItemName}")]
        public void ValidateLoadItem(int loadId, string loadItemName)
        {
            loadRepository.ValidateLoadItem(loadId, loadItemName);
        }

        [HttpPost]
        [Route("validateLoadItemId/{loadItemId}")]
        public void ValidateLoadItem(int loadItemId)
        {
            loadRepository.ValidateLoadItemId(loadItemId);
        }

        [Route("volumeScenario")]
        public object GetVolumeScenario()
        {
            return loadRepository.GetLoadedVolumeScenario();
        }

        [HttpPost]
        [Route("updateVolumeScenario")]
        public void SaveVolumeScenario(List<VolumeScenarioViewModel> model)
        {
            loadRepository.UpdateLoadedVolumeScenario(model);
        }

        [Route("excel/get/sku")]
        public HttpResponseMessage GetSkuExcel()
        {
            var skuPriceList = loadRepository.GetSkuPriceListExcelExport();
            var headers = priceListToExcelDataTransformer.GetHeaders();
            var priceList = priceListToExcelDataTransformer.Transform(skuPriceList);

            var merged = headers.Concat(priceList).ToList();
            var data = new Dictionary<string, List<List<object>>> { { pricesWorksheet, merged } };

            return fileHttpResponseCreator.GenerateExcelFromTemplate("priceList", data);

        }

        [HttpPost, Route("excel/post/sku")]
        public void PostSkuExcel()
        {
            if (!Request.Content.IsMimeMultipartContent())
                throw new ArgumentException("Invalid request");

            var file = HttpContext.Current.Request.Files[0];
                        
            using(var fileStream = file.InputStream)
            {
                var xlWorkbook = new XLWorkbook(fileStream);
                var prices = _priceListFromExcelTransformer.ReadPrices(xlWorkbook.Worksheet(pricesWorksheet)).ToList();
                loadRepository.SavePriceList(prices);
            }
        }       
    }

    public class LoadCreateModel
    {
        public string Name { get; set; }
    }
}