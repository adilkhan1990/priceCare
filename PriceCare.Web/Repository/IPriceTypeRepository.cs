using System;
using System.Collections.Generic;
using System.Net.Http;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public interface IPriceTypeRepository
    {
        List<PriceTypeViewModel> GetPriceTypes(int geographyId, int productId, DateTime applicableFrom);        
        IEnumerable<PriceTypeViewModel> GetAllPriceTypes(int currencyId, string searchText = "");        
        PriceTypeSearchResponseViewModel GetPagedPriceTypes(PriceTypeSearchRequestViewModel priceTypeSearchRequest);
        IEnumerable<PriceTypeViewModel> GetVersionPriceTypes(int versionId, int reviewedGeographyId,int productId, int referencedGeographyId);
        IEnumerable<PriceTypeViewModel> GetVersionPriceTypes(int versionId, int reviewedGeographyId, int productId);
        IEnumerable<PriceTypeViewModel> GetForecastPriceTypes(int saveId, int reviewedGeographyId, int productId, int referencedGeographyId);
        bool AddPriceType(PriceTypeViewModel model);
        void SaveLoad(PriceTypeSaveModel priceTypeSave);
        void Save(IEnumerable<PriceTypeViewModel> priceTypes);

        IEnumerable<PriceTypeExport> GetPriceTypeExports(int versionId);
        bool IsValid(PriceTypeViewModel priceType);
        HttpResponseMessage GetExcel(string token);
        List<PriceTypeViewModel> GetPriceTypesProduct(List<int> geographyId, int productId, DateTime applicableFrom);
    }
}