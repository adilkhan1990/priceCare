using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PriceCare.Web.Models.Oracle;

namespace PriceCare.Web.Controllers
{
    public interface IPriceListToExcelDataTransformer
    {
        List<List<object>> GetHeaders();
        List<List<object>> Transform(IEnumerable<PriceListModel> skuPriceList);
    }

    public class PriceListToExcelDataTransformer : IPriceListToExcelDataTransformer
    {
        private List<object> headers = new List<object>
            {
                "REGION",
                "ORGANIZATION_ID",
                "COUNTRY",
                "PRICE_LIST_ID",
                "PRICE_LIST_NAME",
                "TYPE_TO_CHANNEL",
                "STATUS_TO_STATUS",
                "PRODUCT_FAMILY",
                "PRODUCT_NUMBER",
                "PRODUCT_NAME",
                "UOM",
                "STRENGTH",
                "PACK",
                "EFFECTIVE_DATE",
                "EXPIRATION_DATE",
                "AVAILABLE_ON_CONTRACTS",
                "AVAILABLE_ON_PRICING_DOCS",
                "PRICELIST_PRICE",
                "PRICELIST_PRICE_MCG",
                "PRICELIST_CURRENCY_TYPE",
                "EXCHANGE_RATE",
                "PRICELIST_PRICE_IN_USD",
                "PRICELIST_PRICE_IN_USD_MCG"
            };

        public List<List<object>> GetHeaders()
        {
            var allData = new List<List<object>>();

            allData.Add(headers);

            return allData;
        }

        public List<List<object>> Transform(IEnumerable<PriceListModel> skuPriceList)
        {
            var allData = skuPriceList.Select(p => new List<object>
            {              
               p.REGION,
               p.ORGANIZATION_ID,
               p.COUNTRY,
               p.PRICE_LIST_ID,
               p.PRICE_LIST_NAME,
               p.TYPE_TO_CHANNEL,
               p.STATUS_TO_STATUS,
               p.PRODUCT_FAMILY,
               p.PRODUCT_NUMBER,
               p.PRODUCT_NAME,
               p.UOM,
               p.STRENGTH,
               p.PACK,
               p.EFFECTIVE_DATE,
               p.EXPIRATION_DATE,
               p.AVAILABLE_ON_CONTRACTS,
               p.AVAILABLE_ON_PRICING_DOCS,
               p.PRICELIST_PRICE,
               p.PRICELIST_PRICE_MCG,
               p.PRICELIST_CURRENCY_TYPE,
               p.EXCHANGE_RATE,
               p.PRICELIST_PRICE_IN_USD,
               p.PRICELIST_PRICE_IN_USD_MCG
            })
                       .ToList();

            return allData;
        }
    }
}