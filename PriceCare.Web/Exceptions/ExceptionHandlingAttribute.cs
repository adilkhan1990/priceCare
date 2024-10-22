using System;
using System.Data.Entity.Core;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;

namespace PriceCare.Web.Exceptions
{
    public class ExceptionHandlingAttribute : ExceptionFilterAttribute
    {
        /// <summary>
        /// Catches all exception coming from the API Controllers and returns a propper <see cref="HttpResponseException"/>
        /// </summary>
        /// <param name="context">The <see cref="HttpResponseException"/></param>
        /// <remarks>This does not catch the properly formatted <see cref="HttpResponseException"/></remarks>
        public override void OnException(HttpActionExecutedContext context)
        {
            if (context.Exception is ArgumentException)
            {
                throw new HttpResponseException(context.Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    context.Exception.Message));
            }

            if (context.Exception is ItemNotFoundException)
            {
                throw new HttpResponseException(context.Request.CreateErrorResponse(HttpStatusCode.NotFound,
                    context.Exception.Message));
            }

            if (context.Exception is NoAccessException)
            {
                throw new HttpResponseException(context.Request.CreateErrorResponse(HttpStatusCode.Forbidden,
                    context.Exception.Message));
            }

            if (context.Exception is ObjectNotFoundException)
            {
                throw new HttpResponseException(context.Request.CreateErrorResponse(HttpStatusCode.NotFound,
                    context.Exception.Message));
            }

            //Log Critical errors
            Trace.TraceError(context.Exception.Message + Environment.NewLine + context.Exception.StackTrace);
#if DEBUG
            throw new HttpResponseException(context.Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
               context.Exception.Message + Environment.NewLine + context.Exception.StackTrace));
#else
            throw new HttpResponseException(context.Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                "An error occurred, please try again or contact the administrator."));
#endif
        }
    }
}