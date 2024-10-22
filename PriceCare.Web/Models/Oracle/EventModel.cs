using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Models.Oracle
{
    public class EventModel
    {
       public Int32 ID { get; set; }
       public Int32 YEAR { get; set; }
       public Int32 MONTH { get; set; }
       public string TYPE_OF_CUT { get; set; }
       public string CUSTOM_TYPE_OF_CUT { get; set; }
       public double? SIZE_OF_CUT_PCT { get; set; }
       public double? PROBABILITY_OF_CUT_PCT { get; set; }
       public string PRODUCT_FAMILY { get; set; }
       public string COUNTRY { get; set; }
       public string ISO_COUNTRY_2_CHAR { get; set; }
       public string COMMENTS { get; set; }
    }
}