using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.Provider;
using Owin;
using System;
using PriceCare.Web.Constants;
using PriceCare.Web.Models;

namespace PriceCare.Web
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context and user manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie

            app.UseBasicAuthentication(new BasicAuthenticationOptions
            {
                ValidateCredentials = BasicAuthenticationValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                    validateInterval: TimeSpan.FromMinutes(30),
                    regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager)),
            });
           
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager)),
                    OnApplyRedirect = ctx =>
                    {
                        if (!IsAjaxRequest(ctx.Request))
                        {
                            ctx.Response.Redirect(ctx.RedirectUri);
                        }
                    }
                },
                CookieName = CookieConstants.CookieSecurity,
                SlidingExpiration = true,
            });
        }


        private static bool IsAjaxRequest(IOwinRequest request)
        {
            IReadableStringCollection query = request.Query;
            if ((query != null) && (query["X-Requested-With"] == "XMLHttpRequest"))
            {
                return true;
            }
            IHeaderDictionary headers = request.Headers;
            return ((headers != null) && (headers["X-Requested-With"] == "XMLHttpRequest"));
        }
    }


    public static class BasicAuthenticationValidator
    {
        public static Func<BasicValidateIdentityContext, Task> OnValidateIdentity<TManager, TUser>(
            TimeSpan validateInterval, Func<TManager, TUser, Task<ClaimsIdentity>> regenerateIdentity)
            where TManager : UserManager<TUser, string>
            where TUser : class, IUser<string>
        {
            return OnValidateIdentity(validateInterval, regenerateIdentity, id => id.GetUserId());
        }

        public static Func<BasicValidateIdentityContext, Task> OnValidateIdentity<TManager, TUser, TKey>(
            TimeSpan validateInterval, Func<TManager, TUser, Task<ClaimsIdentity>> regenerateIdentityCallback,
            Func<ClaimsIdentity, TKey> getUserIdCallback)
            where TManager : UserManager<TUser, TKey>
            where TUser : class, IUser<TKey>
            where TKey : IEquatable<TKey>
        {
            return async context =>
            {
                var manager = context.OwinContext.GetUserManager<TManager>();
                try
                {
                    var userTask = manager.FindAsync(context.Username, context.Password);
                    userTask.Wait();
                    var user = userTask.Result;
                    if (user != null)
                    {
                        var identityTask = regenerateIdentityCallback(manager, user);
                        identityTask.Wait();
                        var identity = identityTask.Result;

                        context.OwinContext.Authentication.SignIn(new ClaimsIdentity[1]
                        {
                            identity
                        });
                        context.Identity = identity;
                    }
                    else
                    {
                        context.OwinContext.Authentication.SignOut(new string[1]
                        {
                            context.Options.AuthenticationType
                        });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            };
        }
    }

    class BasicAuthnMiddleware : AuthenticationMiddleware<BasicAuthenticationOptions>
    {
        public BasicAuthnMiddleware(OwinMiddleware next, BasicAuthenticationOptions options)
            : base(next, options)
        {
        }

        protected override AuthenticationHandler<BasicAuthenticationOptions> CreateHandler()
        {
            return new BasicAuthenticationHandler();
        }
    }

    public class BasicAuthenticationOptions : AuthenticationOptions
    {
        public Func<BasicValidateIdentityContext, Task> ValidateCredentials { get; set; }

        public string Realm { get; set; }

        public BasicAuthenticationOptions()
            : base("Basic") { }
    }

    public static class BasicAuthnMiddlewareExtensions
    {
        public static IAppBuilder UseBasicAuthentication(this IAppBuilder app, BasicAuthenticationOptions options)
        {
            return app.Use(typeof(BasicAuthnMiddleware), options);
        }
    }

    public class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
    {
        protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            var authzValue = Request.Headers.Get("Authorization");
            if (string.IsNullOrEmpty(authzValue) || !authzValue.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return await TryGetPrincipalFromBasicCredentialsUsing(Request, Options.ValidateCredentials);

        }

        protected override Task ApplyResponseChallengeAsync()
        {
            if (Response.StatusCode == 401)
            {
                Response.Headers.Append("WWW-Authenticate", "Basic realm=" + Options.Realm);
            }
            return Task.FromResult<object>(null);
        }

        public async Task<AuthenticationTicket> TryGetPrincipalFromBasicCredentialsUsing(
             IOwinRequest req,
             Func<BasicValidateIdentityContext, Task> validate)
        {
            string pair;
            try
            {
                var autorizationHeader = req.Headers.Get("Authorization");
                var autorizationValue = autorizationHeader.Substring("Basic ".Length);
                pair = Encoding.UTF8.GetString(Convert.FromBase64String(autorizationValue));
            }
            catch (FormatException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }
            var ix = pair.IndexOf(':');
            if (ix == -1) return null;
            var username = pair.Substring(0, ix);
            var pw = pair.Substring(ix + 1);
            var context = new BasicValidateIdentityContext(this.Context, username, pw, this.Options);
            validate(context);
            var authenticationTicket = new AuthenticationTicket(context.Identity, context.Properties);
            return authenticationTicket;
        }
    }

    public class BasicValidateIdentityContext : BaseContext<BasicAuthenticationOptions>
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public ClaimsIdentity Identity { get; set; }
        public AuthenticationProperties Properties { get; set; }


        /// <summary>
        /// Creates a new instance of the context object.
        /// 
        /// </summary>
        /// <param name="context"/>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="options"/>
        public BasicValidateIdentityContext(IOwinContext context, string username, string password, BasicAuthenticationOptions options)
            : base(context, options)
        {
            Username = username;
            Password = password;
        }
    }
}