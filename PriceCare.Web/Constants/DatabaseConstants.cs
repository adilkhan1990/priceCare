using System.Configuration;

namespace PriceCare.Web.Constants
{
    public class DataBaseConstants
    {
        public static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["PriceCare"].ConnectionString;
        public static readonly string OracleConnectionString = ConfigurationManager.ConnectionStrings["OracleSource"].ConnectionString;        
    }

    public class ApplicationConstants
    {
        public const int DefaultCurrencySpotVersionId = 2;
        public const int MaximumSimulationDuration = 10;
        public const double DoubleComparisonThreshold = 0.0001;
        public const int NotSpecifiedPrice = 1;
        public const int ImportedDataSaveId = 1;
        public const int DefaultWeightTypeId = 48;
        public const int NotSpecifiedProduct = 15;
        public const int NotSpecifiedSubRuleIndex = 0;
        public const int CurrencyUnitType = 5;
        public const int ManualPriceType = 140;
        public const int DefaultSegment = 15;
        public const int LaunchOptionScenario = 2;
        public const int LaunchOptionAssumptions = 3;
        public const int UndefinedRegionId = 1005;
        public const int DefaultCurrencyId = 30;
        public const int IrpLoadSystem = 166;

        public const string Product = "Product";
        public const string Currencies = "Currencies";
        public const string Country = "Country";
        public const string PriceTypes = "Price Types";
        public const string NetData = "Net Data";
        public const string Sku = "SKU";
        public const string Price = "Price";
        public const string Volume = "Volume";
        public const string Event = "Event";
        public const string Rule = "Rule";
    }

    public enum GeographyType
    {
        Region = 1,
        Country = 3
    }

    public enum LoadStatus
    {
        Loading = 1,
        NotValidated = 2,
        Validated = 3,
        Error = 4,
        Cancelled = 5
    }

    public static class DimensionConstant
    {
        public const string Geography = "Geography";
        public const string Product = "Product";
    }

    public enum DimensionType
    {
        Excel = 14,
        Gcods = 16,
        Display = 49
    }
}