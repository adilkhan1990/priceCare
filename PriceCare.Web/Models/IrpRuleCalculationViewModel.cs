using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Models
{
    public class IrpRuleCalculationViewModel
    {
        public int Id { get; set; }
        public int IrpRuleListId { get; set; }
        public int IrpMathId { get; set; }
        public int Argument { get; set; }
        public int? UpId { get; set; }
    }
}