using System.Collections.Generic;
using System.Net.Http;

namespace PriceCare.Web.Controllers
{
    public interface IFileHttpResponseCreator
    {
        HttpResponseMessage GenerateExcelFromTemplate(string name, Dictionary<string, List<List<object>>> dictionary);
    }
}