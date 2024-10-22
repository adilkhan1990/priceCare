using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Models.Oracle
{
    public class RuleModel
    {
        public Int32 ID { get; set; }
        public string REFING_COUNTRY_NAME { get; set; }
        public Int32 REFING_CNTRY_REF_COUNTRY_CNT { get; set; }
        public string REFING_CNTRY_FOCUS { get; set; }
        public string REFING_CNTRY_FORMULA_USED_GOVT { get; set; }
        public string REFING_CNTRY_IS_FORMAL { get; set; }
        public string REFING_CNTRY_FREQUENCY { get; set; }
        public int? REFING_CNTRY_MONTHS_BTW_REF { get; set; }
        public string REFING_CNTRY_REF_MONTH { get; set; }
        public string REFING_CNTRY_UNIT_OF_CMPRSN { get; set; }
        public string REFING_CNTRY_BASIS_OF_CMPRSN { get; set; }
        public string REFED_CNTRY_COUNTRY_NAME { get; set; }
        public string REFED_CNTRY_PRICELIST_TYPE { get; set; }
        public string REFED_CNTRY_PRICELIST_CHANNEL { get; set; }
        public string REFED_CNTRY_PRICELIST_PUBLIC { get; set; }
        public string REFED_CNTRY_PRICELIST_OFFCL { get; set; }
        public string REFED_CNTRY_MISSING_PCK_PRICE { get; set; }
        public string REFED_CNTRY_DOCUMENT_REQUIRED { get; set; }
    }
}