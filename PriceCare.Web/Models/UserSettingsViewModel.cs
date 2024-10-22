using System;

namespace PriceCare.Web.Models
{
    public class UserSettingsViewModel
    {
        public Guid UserId { get; set; }
        public int DefaultRegionId { get; set; }
        public int DefaultCountryId { get; set; }
        public int DefaultProductId { get; set; }
    }
}