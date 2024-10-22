using System.Linq;
using System.Web.UI.WebControls;
using PriceCare.Web.Constants;
using PriceCare.Web.Helpers;
using PriceCare.Web.Models;
using System.Collections.Generic;
using System.Data;
namespace PriceCare.Web.Repository
{
    public class DimensionRepository
    {
        public IEnumerable<DimensionViewModel> GetMathType()
        {
            return GetDimensionType("Math");
        }
        public IEnumerable<DimensionViewModel> GetEventType()
        {
            return GetDimensionType("Event");//.Where(e => e.ShortName != "None" && e.ShortName != "XX");
        }
        public IEnumerable<DimensionViewModel> GetGeographyType()
        {
            return GetDimensionType("Geography");
        }
        public IEnumerable<DimensionViewModel> GetUnitType()
        {
            return GetDimensionType("Unit");
        }
        public IEnumerable<DimensionViewModel> GetFormulationType()
        {
            return GetDimensionType("Formulation");
        }
        public IEnumerable<DimensionViewModel> GetSystemType()
        {
            return GetDimensionType("System");
        }
        public IEnumerable<DimensionViewModel> GetGprmRuleTypeType()
        {
            return GetDimensionType("GprmRule");
        }
        public IEnumerable<DimensionViewModel> GetDataType()
        {
            return GetDimensionType("Data");
        }

        public IEnumerable<DimensionViewModel> GetWeightType()
        {
            return GetDimensionType("Weight");
        }
        private static IEnumerable<DimensionViewModel> GetDimensionType(string dimension)
        {
            var connectionString = DataBaseConstants.ConnectionString;
            const string query = "SELECT Id, Name, ShortName, FillColor FROM GetDimensionType(@dimension) Order By Name";
            var result = RequestHelper.ExecuteQuery<DimensionViewModel>(
                    connectionString,
                    query,
                    MapDimension,
                    new Dictionary<string, object>
                    {
                        {"dimension",dimension}
                    });
            return result;
        }

        private static DimensionViewModel MapDimension(DataRow row)
        {
            var data = new DimensionViewModel
            {
                Id = (int)row["Id"],
                Name = row["Name"].PreventStringNull(),
                ShortName = row["ShortName"].PreventStringNull(),
                ColorCode = row["FillColor"].PreventStringNull()
            };
            return data;
        }


        public IEnumerable<UnitViewModel> GetUnits()
        {
            var connectionString = DataBaseConstants.ConnectionString;
            const string query = "SELECT Id, Name, DimensionTypeId, Factor FROM Unit";
            var result = RequestHelper.ExecuteQuery<UnitViewModel>(
                    connectionString,
                    query,
                    MapUnit);
            return result;
        }

        private static UnitViewModel MapUnit(DataRow row)
        {
            var data = new UnitViewModel
            {
                Id = (int)row["Id"],
                Name = row["Name"].ToString(),
                DimensionTypeId = (int)row["DimensionTypeId"],
                Factor = (double)row["Factor"]
            };
            return data;
        }
    }
}