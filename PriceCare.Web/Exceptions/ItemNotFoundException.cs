using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Exceptions
{
    public class ItemNotFoundException : Exception
    {
        public ItemNotFoundException(string message) : base(message) { }
    }
}