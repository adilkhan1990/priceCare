using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using PriceCare.Web.Models;
using PriceCare.Web.Repository;
using Microsoft.AspNet.Identity;
using PriceCare.Web.Math;

namespace PriceCare.Web.Controllers
{
    [RoutePrefix("api/rule")]
    [Authorize]
    public class RuleController : ApiController
    {
        private readonly IRuleRepository ruleRepository;
        private readonly IGprmRuleRepository gprmRuleRepository;
        private readonly CacheRepository cacheRepository;
        private readonly DimensionRepository dimensionRepository;
        private readonly IPriceTypeRepository priceTypeRepository;
        private readonly ILoadRepository loadRepository;

        public RuleController(IRuleRepository ruleRepository, IPriceTypeRepository priceTypeRepository, ILoadRepository loadRepository)
        {
            this.ruleRepository = ruleRepository;
            this.priceTypeRepository = priceTypeRepository;
            this.loadRepository = loadRepository;
            gprmRuleRepository = new GprmRuleRepository();  
            dimensionRepository = new DimensionRepository();
            cacheRepository = new CacheRepository();
        }

        [Route("ruleTypes")]
        public List<RuleTypeViewModel> GetRuleTypes()
        {
            return ruleRepository.GetGprmRuleTypes();
        } 

        [Route("rules")]
        [HttpPost]
        public RuleDefinitionViewModel GetRules(RuleRequest model)
        {
            if (model.Forecast)
            {
                return cacheRepository.GetRule(model.SimulationId, model.GeographyId, model.ProductId, model.GprmRuleTypeId, model.ApplicableFrom);
            }
            if (model.Validate)
            {
                return loadRepository.GetRulesToValidate(model);
            }
            return gprmRuleRepository.GetRules(model.VersionId, model.GeographyId, model.ProductId, model.GprmRuleTypeId, model.ApplicableFrom);
        }

        [Route("priceTypes")]
        [HttpPost]
        public List<PriceTypeViewModel> GetPriceTypes(PriceTypeRequest model)
        {
            var priceTypes = priceTypeRepository.GetPriceTypes(model.CountryId, model.ProductId, model.ApplicableFrom);
            return priceTypes;
            //return priceTypeRepository.GetPriceTypes(model.CountryId, model.ProductId, model.ApplicableFrom);
        }
        
        [Route("applicableFrom")]
        [HttpPost]
        public List<FilterItemViewModel> GetRuleApplicableFrom(RuleRequest model)
        {
            var result = new List<FilterItemViewModel>();
            List<DateTime> applicableFromList;
            if (model.Forecast)
            {
                new Forecast().GetProductForSimulation(model.SimulationId, new List<int> { model.GeographyId }, new List<int> { model.ProductId }, new List<int> { (int)DataTypes.Price });
                applicableFromList = cacheRepository.GetRuleApplicableFrom(User.Identity.GetUserId(), model.GeographyId, model.ProductId, model.GprmRuleTypeId).ToList();
            }   
            else
                applicableFromList = gprmRuleRepository.GetRuleApplicableFrom(model.VersionId, model.GeographyId, model.ProductId, model.GprmRuleTypeId).ToList();

             result.AddRange(applicableFromList.Select(r => new FilterItemViewModel
            {
                Text = r.ToString("u"),
            }).ToList());

            return result;
        }

        [Route("math")]
        public List<DimensionViewModel> GetRuleMath()
        {            
            return dimensionRepository.GetMathType().ToList();
        }

        [Route("save")]
        [HttpPost]
        public object SaveRules(SaveRuleViewModel model)
        {
            RuleViewModel rule = new RuleViewModel
            {
                Active = model.RuleDefinition.Active,
                AllowIncrease = model.RuleDefinition.AllowIncrease,
                ApplicableFrom = model.ApplicableFrom,
                Argument = model.RuleDefinition.Argument,
                Default = model.RuleDefinition.Default,
                DefaultBasket = model.RuleDefinition.DefaultBasket,
                Edited = model.RuleDefinition.Edited,
                EffectiveLag = model.RuleDefinition.EffectiveLag,
                Geography = model.Geography,
                GeographyId = model.GeographyId,
                ReferencedData = model.RuleDefinition.ReferencedData,
                GprmMathId = model.RuleDefinition.GprmMathId,
                GprmRuleTypeId = model.GprmRuleTypeId,
                IrpRuleListId = model.RuleDefinition.IrpRuleListId,
                LookBack = model.RuleDefinition.LookBack,
                ProductId = model.ProductId,
                Regular = model.RuleDefinition.Regular,
                ReviewedPriceAdjustment = model.RuleDefinition.Adjustement,
                ReviewedPriceTypeId = model.RuleDefinition.SelectedReviewedPriceTypeId,
                WeightTypeId = model.RuleDefinition.WeightTypeId
            };

            gprmRuleRepository.SaveVersionRule(rule);
            
            return true;
        }

        [Route("updateCache")]
        [HttpPost]
        public object UpdateCache(CacheRuleViewModel model)
        {
            RuleViewModel rule = new RuleViewModel
            {
                Active = model.RuleDefinition.Active,
                AllowIncrease = model.RuleDefinition.AllowIncrease,
                ApplicableFrom = model.ApplicableFrom,
                Argument = model.RuleDefinition.Argument,
                Default = model.RuleDefinition.Default,
                DefaultBasket = model.RuleDefinition.DefaultBasket,
                Edited = model.RuleDefinition.Edited,
                EffectiveLag = model.RuleDefinition.EffectiveLag,
                Geography = model.Geography,
                GeographyId = model.GeographyId,
                ReferencedData = model.RuleDefinition.ReferencedData,
                GprmMathId = model.RuleDefinition.GprmMathId,
                GprmRuleTypeId = model.GprmRuleTypeId,
                IrpRuleListId = model.RuleDefinition.IrpRuleListId,
                LookBack = model.RuleDefinition.LookBack,
                ProductId = model.ProductId,
                Regular = model.RuleDefinition.Regular,
                ReviewedPriceAdjustment = model.RuleDefinition.Adjustement,
                ReviewedPriceTypeId = model.RuleDefinition.SelectedReviewedPriceTypeId,
                WeightTypeId = model.RuleDefinition.WeightTypeId
            };

            var forecast = new Forecast();
            forecast.UpdateSimulation(new List<DataViewModel>(), rule, model.SimulationId);

            return true;
        }       
    }

}