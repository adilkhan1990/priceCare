using System.Collections.Generic;
using PriceCare.Web.Math.Utils;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository.Utils
{
    internal class GprmRuleDataEqualityComparer : IEqualityComparer<IGprmRuleDataViewModel>
    {
        public bool Equals(IGprmRuleDataViewModel x, IGprmRuleDataViewModel y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            return x.GeographyId == y.GeographyId && x.ProductId == y.ProductId && x.GprmRuleTypeId == y.GprmRuleTypeId;
        }

        public int GetHashCode(IGprmRuleDataViewModel obj)
        {
            unchecked
            {
                var hashCode = obj.GeographyId;
                hashCode = (hashCode * 397) ^ obj.ProductId;
                hashCode = (hashCode * 397) ^ obj.GprmRuleTypeId;
                return hashCode;
            }
        }
    }
    
}