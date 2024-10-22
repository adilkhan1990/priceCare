using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using PriceCare.Web.Helpers;

namespace PriceCare.Web.Controllers
{
    public class FileHttpResponseCreator : IFileHttpResponseCreator
    {
        public HttpResponseMessage GenerateExcelFromTemplate(string name, Dictionary<string, List<List<object>>> dictionary)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);


            string templateFilePath = HttpContext.Current.Server.MapPath(@"~\App_Data\" + name + "_template.xlsx");

            string newFileName = name + DateTime.Now.Ticks + ".xlsx";


            var fileXls = File.ReadAllBytes(templateFilePath);

            var memoryStream = ClosedXmlHelper.AppendXlsxSheetsHavingHeaders(fileXls, dictionary);

            response.Content = new StreamContent(memoryStream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(templateFilePath));
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = newFileName
            };

            return response;
        }

        public HttpResponseMessage DownloadExcelTemplate(string fileName)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            string templateFilePath = HttpContext.Current.Server.MapPath(@"~\App_Data\" + fileName);
                        
            Byte[] content = File.ReadAllBytes(templateFilePath);

            response.Content = new StreamContent(new MemoryStream(content));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(templateFilePath));
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };

            return response;
        }
    }
}