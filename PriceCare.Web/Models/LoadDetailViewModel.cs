using System;
using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class LoadDetailViewModel
    {
        public string Name { get; set; }
        public int Status { get; set; }
        public List<LoadDetailItemViewModel> Items { get; set; }
    }

    public class LoadDetailItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public int LoadId { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public bool isDimension { get; set; }
        public int Step { get; set; }
        public bool CanUpdateViaExcel { get; set; }
        public int? RowsToValidate { get; set; }
    }

}