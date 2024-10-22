using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Exceptions
{
    public class NoAccessException : Exception
    {
        public NoAccessException(string message) : base(message) { }
    }
}