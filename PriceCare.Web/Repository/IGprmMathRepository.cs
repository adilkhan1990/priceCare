using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public interface IGprmMathRepository
    {
        List<GprmMathViewModel> GetRuleMath();
    }
}