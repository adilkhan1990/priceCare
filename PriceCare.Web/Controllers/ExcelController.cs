using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Xml;
using System.Xml.Serialization;
using ClosedXML.Excel;
using PriceCare.Web.Helpers;
using PriceCare.Web.Constants;
using PriceCare.Web.Math;
using PriceCare.Web.Models;
using PriceCare.Web.Repository;

namespace PriceCare.Web.Controllers
{
    [System.Web.Http.Authorize]
    [System.Web.Http.RoutePrefix("api/excel")]
    public class ExcelController : ApiController
    {
        private readonly IDataRepository dataRepository = new DataRepository();
        private readonly Forecast forecastRepository = new Forecast();
        private readonly ListToSalesRepository listToSalesRepository = new ListToSalesRepository();
        private readonly ListToSalesImpactRepository listToSalesImpactRepository = new ListToSalesImpactRepository();
        private readonly CurrencyRepository currencyRepository = new CurrencyRepository();
        private readonly GprmRuleRepository gprmRuleRepository = new GprmRuleRepository();
        private readonly SkuRepository skuRepository = new SkuRepository();
        private readonly PriceTypeRepository priceTypeRepository = new PriceTypeRepository();
        private readonly FileRepository fileRepository = new FileRepository();

        [System.Web.Http.Route("data")]
        [System.Web.Http.HttpGet]
        public string GetData(int dataTypeId, int saveId, int versionId, int currencyBudgetVersionId, int startYear, int endYear)
        {
            var data = dataRepository.GetVersionData(dataTypeId, saveId, versionId, currencyBudgetVersionId, startYear, endYear).Where(d => d.Value != null);
            var xsSubmit = new XmlSerializer(typeof(ExcelDataViewModel));
            var sww = new StringWriter();
            var writer = XmlWriter.Create(sww);
            xsSubmit.Serialize(writer, data);
            var xml = sww.ToString();
            return xml;
        }

        [System.Web.Http.Route("data")]
        [System.Web.Http.HttpPost]
        public void SaveData(ExcelDataViewModel data)
        {
            var cData = data.Data.Select(d => new DataViewModel
            {
                GeographyId = d.GeographyId,
                ProductId = d.ProductId,
                DataTypeId = d.DataTypeId,
                PriceTypeId = d.PriceTypeId,
                DataTime = d.DataTime,
                SegmentId = d.SegmentId,
                CurrencySpotId = d.CurrencySpotId,
                CurrencySpotVersionId = d.CurrencySpotVersionId,
                EventTypeId = d.EventTypeId,
                UnitTypeId = d.UnitTypeId,
                Value = d.Value,
                Description = d.Description,
                Active = d.Active
            }).ToList();
            if (data.SimulationId == 0)
                dataRepository.SaveVersionData(cData, data.SaveId, data.DeactivatePreviousVersion);
            else
                forecastRepository.UpdateSimulation(cData, null, data.SimulationId);
        }

        [System.Web.Http.Route("listToSales")]
        [System.Web.Http.HttpPost]
        public void SaveListToSales(ExcelListToSalesViewModel listToSales)
        {
            if (listToSales.ListToSales.Count != 0)
            {
                var cListToSales = listToSales.ListToSales.Select(l2S => new ListToSalesViewModel
                {
                    Id = 1,
                    GeographyId = l2S.GeographyId,
                    ProductId = l2S.ProductId,
                    CurrencySpotId = l2S.CurrencySpotId,
                    CurrencySpotVersionId = l2S.CurrencySpotVersionId,
                    SegmentId = l2S.SegmentId,
                    Asp = l2S.Asp,
                    MarketPercentage = l2S.MarketPercentage,
                    ImpactPercentage = l2S.ImpactPercentage,
                    SaveId = listToSales.SaveId,
                    SaveTypeId = (int)SaveTypes.Data,
                    Active = true
                }).ToList();
                listToSalesRepository.SaveVersion(cListToSales);
            }
            if (listToSales.ListToSalesImpact.Count != 0)
            {
                var cListToSalesImpact = listToSales.ListToSalesImpact.Select(l2S => new ListToSalesImpactViewModel
                {
                    GeographyId = l2S.GeographyId,
                    ProductId = l2S.ProductId,
                    SegmentId = l2S.SegmentId,
                    ImpactDelay = l2S.ImpactDelay,
                    ImpactPercentage = l2S.ImpactPercentage,
                    SaveId = listToSales.SaveId,
                    SaveTypeId = (int)SaveTypes.Data,
                    Active = true
                }).ToList();
                listToSalesImpactRepository.SaveVersion(cListToSalesImpact);
            }
        }

        [System.Web.Http.Route("currency")]
        [System.Web.Http.HttpPost]
        public void SaveCurrency(ExcelCurrencyViewModel currency)
        {
            SaveCurrenciesModel cCurrency;
            if (currency.CurrencyBudget.Count != 0)
            {
                cCurrency = new SaveCurrenciesModel
                {
                    RateType = RateType.Budget,
                    Currencies = ConvertCurrencyObject(currency.CurrencyBudget)
                };
                currencyRepository.SaveCurrencies(cCurrency, false);
            }
            if (currency.CurrencySpot.Count != 0)
            {
                cCurrency = new SaveCurrenciesModel
                {
                    RateType = RateType.Spot,
                    Currencies = ConvertCurrencyObject(currency.CurrencySpot)
                };
                currencyRepository.SaveCurrencies(cCurrency, false);
            }
        }

        [System.Web.Http.Route("priceType")]
        [System.Web.Http.HttpPost]
        public void SavePriceType(ExcelPriceTypeViewModel priceType)
        {
            priceTypeRepository.Save(priceType.Data.Where(pT => pT.Id != 0));
            foreach (var newPriceType in priceType.Data.Where(pT => pT.Id == 0))
            {
                priceTypeRepository.AddPriceType(newPriceType);
            }
        }

        [System.Web.Http.Route("sku")]
        [System.Web.Http.HttpPost]
        public void SaveSku(ExcelSkuViewModel sku)
        {
            skuRepository.Save(sku.Data.Where(s => s.Id != 0));
            foreach (var newSku in sku.Data.Where(s => s.Id == 0))
            {
                skuRepository.AddSku(newSku);
            }
        }
        
        [System.Web.Http.Route("priceMap")]
        [System.Web.Http.HttpPost]
        public void SavePriceMap(ExcelPriceMapViewModel priceMap)
        {
            gprmRuleRepository.SaveExcelMap(priceMap);
        } 
        
        private static List<CurrencyViewModel> ConvertCurrencyObject(IEnumerable<ExcelCurrency> currencies)
        {
            return currencies.Select(c => new CurrencyViewModel
            {
                Id = c.CurrencyId,
                EurRate = c.EUR,
                UsdRate = c.USD
            }).ToList();
        }


        [System.Web.Http.Route("postFilterExcel")]
        [System.Web.Http.HttpPost]
        public object PostFilterExcel(object search)
        {
            return ExcelDownloadBufferHelper.PostFilterExcel(search);
        }

        [System.Web.Http.Route("getXlsTemplates")]
        public List<XlsTemplateViewModel> GetXlsTemplates()
        {
            return fileRepository.GetXlsTemplates(HttpContext.Current.Server.MapPath(@"~\App_Data\"));
        }
        
        [System.Web.Http.Route("postXlsTemplate")]
        [System.Web.Http.HttpPost]
        public void PostXlsTemplate()
        {
            if (!Request.Content.IsMimeMultipartContent())
                throw new ArgumentException("Invalid request");

            var request = HttpContext.Current.Request;
            if (request.Files.Count == 0)
                throw new ArgumentException("Invalid request");

            var file = HttpContext.Current.Request.Files[0];
            var filename = request.Form.Get("filename").RemoveSpecialCharacters();

            using (Stream f = File.Open(HttpContext.Current.Server.MapPath(@"~\App_Data\" + filename), FileMode.Create))
            {
                file.InputStream.Seek(0, SeekOrigin.Begin);
                file.InputStream.CopyTo(f);
            }
        }

        [System.Web.Http.Route("downloadTemplate")]
        [System.Web.Http.HttpGet]
        public HttpResponseMessage DownloadTemplate([FromUri]string fileName)
        {
            var file = HttpContext.Current.Server.MapPath(@"~\App_Data\" + fileName);
            var stream = new FileStream(file, FileMode.Open);
            var content = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(stream)
            };

            content.Content.Headers.ContentType = fileName.EndsWith(".xlsx")
                ? new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                : new MediaTypeHeaderValue("application/vnd.ms-excel.sheet.macroEnabled.12"); //.xlsm
            
            return content;
        }
    }
}
