using System;
using System.Collections.Generic;

namespace PriceCare.Web.Models
{
    public class UserInfoViewModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int AccountId { get; set; }
        public List<string> Roles { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }        
        public bool IsUserLocked { get; set; }                
        public bool IsDeleted { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DateDeleted { get; set; }
        public string Status { get; set; }
        public DateTime? LastConnectionDate { get; set; }
        public int UserStatusId { get; set; }
    }

    public class SmallUserInfoViewModel
    {
        public string Id { get; set; }
        public string Username { get; set; }   
        public string FirstName { get; set; }
        public string LastName { get; set; }        
    }
}