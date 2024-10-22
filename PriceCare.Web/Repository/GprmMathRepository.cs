using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PriceCare.Web.Constants;
using PriceCare.Web.Helpers;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public class GprmMathRepository : IGprmMathRepository
    {
        public List<GprmMathViewModel> GetRuleMath()
        {
            const string query = "SELECT Id, Name, ShortName FROM RuleMath";

            var result =
                RequestHelper.ExecuteQuery(DataBaseConstants.ConnectionString, query, row => new GprmMathViewModel
                {
                    Id = (int) row["Id"],
                    Name = (string) row["Name"],
                    ShortName = (string) row["ShortName"]
                }).ToList();

            return result;
        }
    }
}