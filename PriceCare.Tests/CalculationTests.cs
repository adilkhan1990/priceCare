using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PriceCare.Web.Models;
using PriceCare.Web.Repository;
using PriceCare.Web.Math;
using System.Collections.Generic;

namespace PriceCare.Tests
{
    [TestClass]
    public class ForecastTests
    {
        private readonly Forecast forecastRep = new Forecast();
        private readonly CacheRepository cacheRep = new CacheRepository();
        private readonly SaveRepository saveRep = new SaveRepository();
        private readonly GprmRuleRepository ruleRep = new GprmRuleRepository();
        private readonly Analyzer analyzerRepository = new Analyzer();
        [ClassInitialize]
        public static void SetupForClass(TestContext context)
        {
        }

        [TestInitialize]
        public void SetupForEachTests()
        {
            //var rule = ruleRep.GetVersionRule(0, 14, 1, 30, new DateTime(2014, 10, 1));
            
            //var testDate = new DateTime(635528691902800000);
            //var testTicks = testDate.Ticks;

            const string userId = "dcc8f049-ece3-4901-b7eb-83bdeabe801d";
            const int scenarioSaveId = 1;
            const int simulationDuration = 3;
            const int simulationId = 375;
            const int productId = 3;
            const int geographyId = 13;

            //var timer = new Stopwatch();
            //double t;
            //timer.Start();
            //forecastRep.CreateSimulation(userId, scenarioSaveId, simulationDuration, 1);
            //timer.Stop();
            //t = timer.ElapsedMilliseconds;
            //saveRep.SaveCache(simulationId, "Test JHB2", "first OK", false, false, false, false);
            //var an = analyzerRepository.GetAnalyzerData(simulationId, new List<int> { geographyId }, productId, (int)DataTypes.NetPrice);

            //var eventData =
            //    cacheRep.GetDataCache(simulationId, new List<int>(), new List<int> { productId }, new List<int> { (int)DataTypes.Event }).ToList();
            //var anEvents = eventData.Where(eD => eD.GeographyId == geographyId && eD.Value != null).ToList();

            //var si = analyzerRepository.EvaluateSalesImpact(simulationId, productId, anEvents);
            //    cacheRep.GetDataCache(simulationId, new List<int>(), new List<int> { productId }, new List<int> { (int)DataTypes.Event }).ToList();
            //var anEvents = eventData.Where(eD => eD.GeographyId == geographyId && eD.Value != null).ToList();

            //var si = analyzerRepository.EvaluateSalesImpact(simulationId, productId, anEvents);
            //var b = 1;
        }

        [TestCleanup]
        public void CleanupForEachTests()
        {

        }

        [ClassCleanup]
        public static void CleanupForClass()
        {

        }

        [TestMethod]
        public void GetIntTest()
        {
            Assert.AreEqual(1, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExceptionTest()
        {

        }
    }
}
