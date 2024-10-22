using System;
using System.Configuration;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public interface IEmailTemplateRepository
    {
        EmailTemplateViewModel GetDefaultEmail(EmailType emailType, EmailViewType emailViewType);
    }

    public class EmailTemplateRepository : IEmailTemplateRepository
    {
        private static readonly string DefaultResetPasswordEmail =
            ConfigurationManager.AppSettings["DefaultResetPasswordEmail"];

        private static readonly string DefaultInvitationEmail =
            ConfigurationManager.AppSettings["DefaultInvitationEmail"];

        private static readonly string DefaultAccountRequestEmail =
            ConfigurationManager.AppSettings["DefaultAccountRequestEmail"];

        private static readonly string DefaultEmailConfirmationEmail =
            ConfigurationManager.AppSettings["DefaultEmailConfirmationEmail"];

        private FileRepository fileRepository = new FileRepository();

        public EmailTemplateViewModel GetDefaultEmail(EmailType emailType, EmailViewType emailViewType)
        {
            string emailFilename;
            switch (emailType)
            {
                case EmailType.ResetPassword:
                    emailFilename = DefaultResetPasswordEmail;
                    break;
                case EmailType.Invitation:
                    emailFilename = DefaultInvitationEmail;
                    break;
                case EmailType.EmailConfirmation:
                    emailFilename = DefaultEmailConfirmationEmail;
                    break;
                case EmailType.AccountRequest:
                    emailFilename = DefaultAccountRequestEmail;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("emailType");
            }

            switch (emailViewType)
            {
                case EmailViewType.Text:
                    emailFilename += ".txt";
                    break;
                case EmailViewType.Html:
                    emailFilename += ".html";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("emailViewType");
            }

            var file = fileRepository.GetStringContent(emailFilename);

            return new EmailTemplateViewModel
            {
                EmailType = emailType,
                EmailViewType = emailViewType,
                IsDefault = true,
                Content = file
            };
        }
    }
}