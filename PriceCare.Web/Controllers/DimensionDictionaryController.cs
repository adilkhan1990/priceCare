using System.Collections.Generic;
using System.Web.Http;
using PriceCare.Web.Models;
using PriceCare.Web.Repository;

namespace PriceCare.Web.Controllers
{
    [RoutePrefix("api/dimensionDictionary")]
    [Authorize]
    public class DimensionDictionaryController : ApiController
    {
        private readonly IDimensionDictionaryRepository dimensionDictionaryRepository = new DimensionDictionaryRepository();

        [Route("all")]
        [HttpPost]
        public DimensionDictionarySearchResponseViewModel GetAllForDimension(
            DimensionDictionarySearchRequestViewModel request)
        {
            return dimensionDictionaryRepository.GetAllForDimension(request);
        }

        [Route("allGcods")]
        [HttpPost]
        public DimensionDictionarySearchResponseViewModel GetAllGcodsForDimension(
            DimensionDictionarySearchRequestViewModel request)
        {
            return dimensionDictionaryRepository.GetAllGcodsForDimension(request);
        }

        [Route("create")]
        [HttpPost]
        public bool Create(DimensionDictionaryModel model)
        {
            return dimensionDictionaryRepository.Create(model);
        }

        [Route("update")]
        [HttpPost]
        public void Update(IEnumerable<DimensionDictionaryModel> models)
        {
            dimensionDictionaryRepository.Update(models);
        }

        [Route("synonyms/delete")]
        [HttpPost]
        public void DeleteSynonym(DimensionDictionaryModel model)
        {
            dimensionDictionaryRepository.Delete(model);
        }
    }
}