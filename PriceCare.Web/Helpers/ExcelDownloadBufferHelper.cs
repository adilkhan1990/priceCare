using System;
using Newtonsoft.Json;
using PriceCare.Central;

namespace PriceCare.Web.Helpers
{
    public static class ExcelDownloadBufferHelper
    {
        public static object PostFilterExcel(object request)
        {
            using (var context = new PriceCareCentral())
            {
                string token = Guid.NewGuid().ToString();

                var excelDownloadBuffer = new ExcelDownloadBuffer()
                {
                    Token = token,
                    FilterJson = JsonConvert.SerializeObject(request)
                };

                context.ExcelDownloadBuffers.Add(excelDownloadBuffer);
                context.SaveChanges();
                return new { Token = token };
            }
        }
    }
}