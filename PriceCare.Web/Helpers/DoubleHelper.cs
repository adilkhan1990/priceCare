using PriceCare.Web.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Helpers
{
    public static class DoubleHelper
    {
        public static bool Equal(double? x, double? y)
        {
            if (x == y)
                return true;

            if (x.HasValue && y.HasValue)
            {
                return System.Math.Abs((x.Value + y.Value)) > ApplicationConstants.DoubleComparisonThreshold && System.Math.Abs((x.Value - y.Value) / (x.Value + y.Value) * 2) < ApplicationConstants.DoubleComparisonThreshold;
            }
            
            return false;
        }
    }
}