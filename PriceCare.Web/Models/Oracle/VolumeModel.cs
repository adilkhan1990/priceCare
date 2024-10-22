using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Models.Oracle
{
    public class VolumeModel
    {
       public Int32 ID { get; set; }
       public Int32 YEAR { get; set;}
       public Int32 MONTH { get; set; }
       public string SALES_ORGANIZATION { get; set; }
       public string COUNTRY { get; set; }
       public string PRODUCT_FAMILY { get; set; }
       public string PRODUCT_ID { get; set; }
       public string SALE_CURRENCY { get; set; }
       public double EXCHANGE_RATE { get; set; }
       public double TOT_INV_QTY_MCGASFL { get; set;}
       public double TOTAL_INV_AMTASFLOA { get; set; }
    }
}