using System.Collections.Generic;
using System.Linq;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository.Utils
{
    internal class GprmContainer
    {
        public GprmContainer(List<GprmRuleViewModel> rules, List<GprmReviewedPriceViewModel> reviewedPrices,
            List<GprmSubRuleViewModel> subRules, List<GprmReferencedPriceViewModel> referencedPrices)
        {
            Rules = rules;
            ReviewedPrices = reviewedPrices;
            SubRules = subRules;
            ReferencedPrices = referencedPrices;

            Initialize();
        }

        private void Initialize()
        {
            RulesForGeography = Rules.ToLookup(r => r.GeographyId);
            ReviewedPricesForGeography = ReviewedPrices.ToLookup(r => r.GeographyId);
            SubRulesForGeography = SubRules.ToLookup(r => r.GeographyId);
            ReferencedPricesForGeography = ReferencedPrices.ToLookup(r => r.GeographyId);
        }

        public List<GprmRuleViewModel> Rules { get; set; }
        public List<GprmReviewedPriceViewModel> ReviewedPrices { get; set; }
        public List<GprmSubRuleViewModel> SubRules { get; set; }
        public List<GprmReferencedPriceViewModel> ReferencedPrices { get; set; }

        public ILookup<int, GprmRuleViewModel> RulesForGeography { get; private set; }
        public ILookup<int, GprmReviewedPriceViewModel> ReviewedPricesForGeography { get; private set; }
        public ILookup<int, GprmSubRuleViewModel> SubRulesForGeography { get; private set; }
        public ILookup<int, GprmReferencedPriceViewModel> ReferencedPricesForGeography { get; private set; }

        public List<int> GetDistinctGeographyId()
        {
            return Rules.Select(dRd => dRd.GeographyId)
                .Concat(ReviewedPrices.Select(dRd => dRd.GeographyId))
                .Concat(SubRules.Select(dRd => dRd.GeographyId))
                .Concat(ReferencedPrices.Select(dRd => dRd.GeographyId))
                .Distinct()
                .ToList();
        }
    }
}