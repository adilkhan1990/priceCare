using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Models.Oracle
{
    public class ProductModel
    {
        public Int32 ID { get; set; }
        public string SOURCE_SYSTEM_NAME { get; set; }
        public Int32 PRODUCT_ID { get; set; }
        public string PRODUCT_NAME { get; set; }
        public string IS_ACTIVE { get; set; }
        public string IS_DELETE { get; set; }
        public string SOURCE_SYS_CREATED_BY { get; set; }
        public DateTime SOURCE_SYS_CREATED_DT { get; set; }
        public string SOURCE_SYS_LAST_UPD_BY { get; set; }
        public DateTime SOURCE_SYS_LAST_UPD_DT { get; set; }
        public string ODS_CREATE_BY { get; set; }
        public DateTime ODS_CREATE_DT { get; set; }
        public string ODS_LAST_UPD_BY { get; set; }
        public DateTime ODS_LAST_UPD_DT { get; set; }
    }
}