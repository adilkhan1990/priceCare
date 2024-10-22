using System;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Security;
using PriceCare.Web.Constants;

namespace PriceCare.Web.Attributes
{
    /// <summary>
    /// This class is an action filter attribute to apply on web API action
    /// It makes sure that the action it is applied on will not extend the forms authentication timeout
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]

    public class SessionExpiryPreventerAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Removes the <see cref="FormsAuthentication.FormsCookieName"/> from the response to prevent extending the lifetime
        /// </summary>
        /// <param name="actionExecutedContext">The action executed context.</param>
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            // Removes the Forms Cookie Name from the response
            HttpContext.Current.Response.Cookies.Remove(CookieConstants.CookieSecurity);
            HttpContext.Current.GetOwinContext().Response.Cookies.Delete(CookieConstants.CookieSecurity);

            base.OnActionExecuted(actionExecutedContext);
        }
    }
}