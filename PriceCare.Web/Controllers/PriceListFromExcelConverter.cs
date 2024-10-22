using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using PriceCare.Web.Models.Oracle;

namespace PriceCare.Web.Controllers
{
    public interface IPriceListFromExcelTransformer
    {
        IEnumerable<PriceListModel> ReadPrices(IXLWorksheet priceSheet);
    }

    public class PriceListFromExcelTransformer : IPriceListFromExcelTransformer
    {
        private readonly IPriceListToExcelDataTransformer priceListToExcelDataTransformer;

        public PriceListFromExcelTransformer(IPriceListToExcelDataTransformer priceListToExcelDataTransformer)
        {
            this.priceListToExcelDataTransformer = priceListToExcelDataTransformer;
        }

        public IEnumerable<PriceListModel> ReadPrices(IXLWorksheet priceSheet)
        {
            var headers = priceListToExcelDataTransformer.GetHeaders().First().Cast<string>().Select((v, i) => new { val = v, index = i + 1 }).ToDictionary(item => item.val, item => item.index);
            var range = priceSheet.RangeUsed();
            var rowCount = range.RowCount();
            for (int i = 1; i < rowCount; i++) // Skip header
            {
                yield return ReadRow(headers, range.Row(i + 1));
            }
        }

        private PriceListModel ReadRow(Dictionary<string, int> headers, IXLRangeRow row)
        {
            var priceList = new PriceListModel();

            priceList.REGION = row.Cell(headers["REGION"]).GetString();
            priceList.ORGANIZATION_ID = row.Cell(headers["ORGANIZATION_ID"]).GetString();
            priceList.COUNTRY = row.Cell(headers["COUNTRY"]).GetString();
            priceList.PRICE_LIST_ID = row.Cell(headers["PRICE_LIST_ID"]).GetString();
            priceList.PRICE_LIST_NAME = row.Cell(headers["PRICE_LIST_NAME"]).GetString();
            priceList.TYPE_TO_CHANNEL = row.Cell(headers["TYPE_TO_CHANNEL"]).GetString();
            priceList.STATUS_TO_STATUS = row.Cell(headers["STATUS_TO_STATUS"]).GetString();
            priceList.PRODUCT_FAMILY = row.Cell(headers["PRODUCT_FAMILY"]).GetString();
            priceList.PRODUCT_NUMBER = row.Cell(headers["PRODUCT_NUMBER"]).GetString();
            priceList.PRODUCT_NAME = row.Cell(headers["PRODUCT_NAME"]).GetString();
            priceList.UOM = row.Cell(headers["UOM"]).GetString();
            priceList.STRENGTH = row.Cell(headers["STRENGTH"]).GetString() != "" ? row.Cell(headers["STRENGTH"]).GetDouble() : 0;
            priceList.PACK = row.Cell(headers["PACK"]).GetString() != "" ? row.Cell(headers["PACK"]).GetValue<int>() : 0;
            priceList.EFFECTIVE_DATE = row.Cell(headers["EFFECTIVE_DATE"]).GetDateTime();
            priceList.EXPIRATION_DATE = row.Cell(headers["EXPIRATION_DATE"]).GetDateTime();
            priceList.AVAILABLE_ON_CONTRACTS = row.Cell(headers["AVAILABLE_ON_CONTRACTS"]).GetString();
            priceList.AVAILABLE_ON_PRICING_DOCS = row.Cell(headers["AVAILABLE_ON_PRICING_DOCS"]).GetString();
            priceList.PRICELIST_PRICE = row.Cell(headers["PRICELIST_PRICE"]).GetString() != "" ? row.Cell(headers["PRICELIST_PRICE"]).GetDouble() : 0;
            priceList.PRICELIST_PRICE_MCG = row.Cell(headers["PRICELIST_PRICE_MCG"]).GetString() != "" ? row.Cell(headers["PRICELIST_PRICE_MCG"]).GetDouble() : 0;
            priceList.PRICELIST_CURRENCY_TYPE = row.Cell(headers["PRICELIST_CURRENCY_TYPE"]).GetString();
            priceList.EXCHANGE_RATE = row.Cell(headers["EXCHANGE_RATE"]).GetString() != "" ? row.Cell(headers["EXCHANGE_RATE"]).GetDouble() : 0;
            priceList.PRICELIST_PRICE_IN_USD = row.Cell(headers["PRICELIST_PRICE_IN_USD"]).GetString() != "" ? row.Cell(headers["PRICELIST_PRICE_IN_USD"]).GetDouble() : 0;
            priceList.PRICELIST_PRICE_IN_USD_MCG = row.Cell(headers["PRICELIST_PRICE_IN_USD_MCG"]).GetString() != "" ? row.Cell(headers["PRICELIST_PRICE_IN_USD_MCG"]).GetDouble() : 0;

            return priceList;
        }
    }
}