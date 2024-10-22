using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PriceCare.Web.Exceptions;
using PriceCare.Web.Helpers.Compression;

namespace PriceCare.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
            jsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            config.MapHttpAttributeRoutes();
            config.Filters.Add(new ExceptionHandlingAttribute());
            config.Formatters.XmlFormatter.UseXmlSerializer = true;

            config.MessageHandlers.Insert(0, new CompressionHandler()); // first runs last

        }
    }
}
