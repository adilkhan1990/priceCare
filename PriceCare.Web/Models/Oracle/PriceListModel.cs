using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Models.Oracle
{
    public class PriceListModel
    {
       public Int32 ID { get; set; }
      public string REGION { get; set; }
      public string ORGANIZATION_ID { get; set; }
      public string COUNTRY { get; set; }
      public string PRICE_LIST_ID { get; set; }
      public string PRICE_LIST_NAME { get; set; }
      public string TYPE_TO_CHANNEL { get; set; }
      public string STATUS_TO_STATUS { get; set; }
      public string PRODUCT_FAMILY { get; set; }
      public string PRODUCT_NUMBER { get; set; }
      public string PRODUCT_NAME { get; set; }
      public string UOM { get; set; }
      public double? STRENGTH { get; set; }
      public int? PACK { get; set; }
      public DateTime? EFFECTIVE_DATE { get; set;}
      public DateTime? EXPIRATION_DATE { get; set; }
      public string AVAILABLE_ON_CONTRACTS { get; set; }
      public string AVAILABLE_ON_PRICING_DOCS { get; set; }
      public double? PRICELIST_PRICE { get; set; }
      public double? PRICELIST_PRICE_MCG { get; set; }
      public string PRICELIST_CURRENCY_TYPE { get; set; }
      public double? EXCHANGE_RATE { get; set; }
      public double? PRICELIST_PRICE_IN_USD { get; set; }
      public double? PRICELIST_PRICE_IN_USD_MCG { get; set; }
    }
}