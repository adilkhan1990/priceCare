using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace PriceCare.Web.Helpers
{
    public static class FileHelper
    {
        public static Stream GetFileStream(MultipartMemoryStreamProvider provider)
        {
            Stream buffer = null;
            foreach (var file in provider.Contents)
            {
                var readAsByteArrayAsyncTask = file.ReadAsStreamAsync();
                readAsByteArrayAsyncTask.Wait();
                buffer = readAsByteArrayAsyncTask.Result;
            }
            return buffer;
        }        
    }
}