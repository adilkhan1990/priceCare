using System;
using System.Configuration;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using PriceCare.Web.Models;

namespace PriceCare.Web
{
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {

         public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) 
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };
            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug in here.
            //manager.RegisterTwoFactorProvider("PhoneCode", new PhoneNumberTokenProvider<ApplicationUser>
            //{
            //    MessageFormat = "Your security code is: {0}"
            //});
            //manager.RegisterTwoFactorProvider("EmailCode", new EmailTokenProvider<ApplicationUser>
            //{
            //    Subject = "Security Code",
            //    BodyFormat = "Your security code is: {0}"
            //});
            manager.EmailService = new EmailService();

            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            
            // Activation (un)lock when log in
            manager.UserLockoutEnabledByDefault = true; // Enables ability to lockout for users when created
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(Double.Parse(ConfigurationManager.AppSettings["DefaultAccountLockoutTimeSpan"]));
            manager.MaxFailedAccessAttemptsBeforeLockout = Convert.ToInt32(ConfigurationManager.AppSettings["MaxFailedAccessAttemptsBeforeLockout"]);


            return manager;
        }
    }

    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            var email = new MailMessage
            {
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = true
            };
            email.To.Add(message.Destination);

            // Check if we have a special message
            if (message is PcIdentityMessage)
            {
                // TODO: implement alternate views
                //email.AlternateViews.Add(new AlternateView());

                switch (((PcIdentityMessage)message).EmailType)
                {
                   
                }
            }

            var mailClient = new SmtpClient();
 
            return mailClient.SendMailAsync(email);
        }

        public static bool IsEmailValid(string email)
        {
            string pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
            + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
            + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

            if (!regex.IsMatch(email))
                return false;

            return true;
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your sms service here to send a text message.
            return Task.FromResult(0);
        }
    }

    public class PcIdentityMessage : IdentityMessage
    {
        public EmailType EmailType{ get; set; }
    }

    public class EmailConstants
    {
        public const string ResetPassword = "PC_ResetPasswordEmail";
        public const string Invitation = "PC_InvitationEmail";
        public const string EmailConfirmation = "PC_EmailConfirmationEmail";
        public const string AccountRequest = "PC_AccountRequestEmail";
        public const string ForgotPassword = "PC_ForgotPasswordEmail";
    }

    public enum EmailType
    {
        ResetPassword = 0,
        Invitation = 1,
        EmailConfirmation = 2,
        AccountRequest = 3
    }

    public enum EmailViewType
    {
        Text = 0, Html = 1
    }

}
    
