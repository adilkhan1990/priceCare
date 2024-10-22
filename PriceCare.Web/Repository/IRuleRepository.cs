using System.Collections.Generic;
using PriceCare.Web.Models;
namespace PriceCare.Web.Repository
{
    public interface IRuleRepository
    {
        //RuleDefinitionViewModel GetDefinition();
        List<RuleTypeViewModel> GetGprmRuleTypes();
    }
}
