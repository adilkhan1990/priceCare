using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Models
{
    public class LoadItemDetailViewModel
    {
        public int Id { get; set; }
        public int LoadItemId { get; set; }
        public int GeographyId { get; set; }
        public int ProductId { get; set; }
        public bool Validated { get; set; }
    }
}