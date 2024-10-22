using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PriceCare.Web.Repository;
using PriceCare.Web.Constants;
using System.Linq;

namespace PriceCare.Tests
{
    [TestClass]
    public class LoadTests
    {
        [TestMethod]
        public void Currencies()
        {
            //var loadRespository = new LoadRepository();
            //var items = loadRespository.GetCurrencyToValidate(Web.Models.RateType.Budget);
            //System.Diagnostics.Trace.WriteLine("Loaded : " + items.Where(c => c.Tag == "Loaded").Count());
            //System.Diagnostics.Trace.WriteLine("Deleted : " + items.Where(c => c.Tag == "Deleted").Count());
            //System.Diagnostics.Trace.WriteLine("Edited : " + items.Where(c => c.Tag == "Edited").Count());
            //System.Diagnostics.Trace.WriteLine("Loaded : " + items.Where(c => string.IsNullOrEmpty(c.Tag)).Count());

        }

        [TestMethod]
        public void PriceTypes()
        {
            //var loadRespository = new LoadRepository();
            //var items = loadRespository.GetPriceTypesToValidate();
            //System.Diagnostics.Trace.WriteLine("Loaded : " + items.Where(c => c.Tag == "Loaded").Count());
            //System.Diagnostics.Trace.WriteLine("Deleted : " + items.Where(c => c.Tag == "Deleted").Count());
            //System.Diagnostics.Trace.WriteLine("Edited : " + items.Where(c => c.Tag == "Edited").Count());
            //System.Diagnostics.Trace.WriteLine("Not changed : " + items.Where(c => string.IsNullOrEmpty(c.Tag)).Count());

        }
    }
}
