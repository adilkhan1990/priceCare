namespace PriceCare.Web.Models.Oracle
{
    public class SalesModel
    {
        public int ID { get; set; }
        public string SCENARIO_NAME { get; set; }
        public string VOLUME_TYPE { get; set; }
        public int? MONTH { get; set; }
        public int? YEAR { get; set; }
        public string SALES_ORG { get; set; }
        public string COUNTRY_NAME { get; set; }
        public string INDICATION { get; set; }
        public string PRODUCT_FAMILY { get; set; }
        public double? VOLUME_COM { get; set; }
        public double? VOLUME_FREE { get; set; }
        public double? CY_ACTUAL_RATE_USD_AMOUNT { get; set; }
        public double? FUTURE_ACTUAL_RATE_USD_AMOUNT { get; set; }
        public double? CY_BUDGET_RATE_USD_AMOUNT { get; set; }
        public double? FUTURE_BUDGET_RATE_USD_AMOUNT { get; set; }
    }
}