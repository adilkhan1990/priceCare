using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Models.Oracle
{
    public class ExchangeRateModel
    {
        public Int32 ID { get; set; }
        public string ATE_TYPE { get; set; }
        public string FROM_CURR { get; set; }
        public string TO_CURR { get; set; }
        public DateTime? EFFECTIVE_DATE { get; set; }
        public double? EXCHANGE_RATE { get; set; }
        public DateTime? CREATE_DATE { get; set; }
        public DateTime? UPDATE_DATE { get; set; }
    }
}