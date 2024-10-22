using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Models
{
    public class RequestAccessInfoViewModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Reason { get; set; }
        public DateTime Date { get; set; }
        public int Status { get; set; }
        public DateTime? DateStatusChanged { get; set; }
        public string UserStatusChanged { get; set; }
        public string UserNameStatusChanged { get; set; }
    }

    public enum RequestAccessStatus
    {
        New = 1, Accepted = 2, Rejected = 3
    }
}