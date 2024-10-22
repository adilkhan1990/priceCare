using System.Collections.Generic;
using PriceCare.Web.Constants;
using PriceCare.Web.Helpers;
using PriceCare.Web.Models;
﻿using System.Linq;

namespace PriceCare.Web.Repository
{
    public class RuleRepository : IRuleRepository
    { 
        //public RuleDefinitionViewModel GetDefinition()
        //{
        //    var model = new RuleDefinitionViewModel
        //    {
        //        Products = new ProductRepository().GetAllProducts().ToList(),
        //        PriceTypes = new PriceTypeRepository().GetAllPriceTypes(12).ToList(),
        //        MainRules = new List<string> {"Average", "Median"}
        //    };
        //    return model;
        //}

        public List<RuleTypeViewModel> GetGprmRuleTypes()
        {
            const string query = "SELECT Id, Name FROM RuleType WHERE Id != 3";

            var result = RequestHelper.ExecuteQuery(DataBaseConstants.ConnectionString, query, row => new RuleTypeViewModel
            {
                Id = (int)row["Id"],
                Name = (string)row["Name"]
            }).ToList();

            return result;
        }
    }
}