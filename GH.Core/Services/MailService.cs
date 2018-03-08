using GH.Core.ViewModels;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace GH.Core.Services
{
    public class MailService : IMailService
    {
        public Task SendMailAsync(NotificationContent email)
        {
            return Task.Run(() =>
            {
                SendMail(email);
            });
        }
        public void SendMail(NotificationContent email)
        {
            string smtpFrom = ConfigurationManager.AppSettings["SMTP_FROM"];
            string smtpUser = ConfigurationManager.AppSettings["SMTP_USERNAME"];
            string smtpPassword = ConfigurationManager.AppSettings["SMTP_PASSWORD"];
            string smtpHost = ConfigurationManager.AppSettings["SMTP_HOST"];
            int smtpPort = int.Parse(ConfigurationManager.AppSettings["SMTP_PORT"]);
            bool smtpEnableSsl = bool.Parse(ConfigurationManager.AppSettings["SMTP_ENABLE_SSL"]);

            // Create an SMTP client with the specified host name and port.
            using (SmtpClient client = new SmtpClient(smtpHost, smtpPort))
            {
                // Create a network credential with your SMTP user name and password.
                client.Credentials = new NetworkCredential(smtpUser, smtpPassword);

                // Use SSL when accessing Amazon SES. The SMTP session will begin on an unencrypted connection, and then 
                // the client will issue a STARTTLS command to upgrade to an encrypted connection using SSL.
                client.EnableSsl = true;
                // client.EnableSsl = true;
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(smtpFrom);
                foreach (var item in email.SendTo)
                {
                    var toEmail = item.ToLower();
                    mailMessage.To.Add(toEmail);
                }
                mailMessage.Subject = email.Title;
                mailMessage.Body = email.Body;

                mailMessage.IsBodyHtml = true;

                client.Send(mailMessage);
            }
        }

        public Task SendMailAttachmentAsync(NotificationContent email, string filePath, string contentHtml)
        {
            return Task.Run(() =>
            {
                SendMailByAttachment(email, filePath, contentHtml);

            });
        }

        public void SendMailByAttachment(NotificationContent email, string filePath, string contentHtml)
        {
            string smtpFrom = ConfigurationManager.AppSettings["SMTP_FROM"];
            string smtpUser = ConfigurationManager.AppSettings["SMTP_USERNAME"];
            string smtpPassword = ConfigurationManager.AppSettings["SMTP_PASSWORD"];
            string smtpHost = ConfigurationManager.AppSettings["SMTP_HOST"];
            int smtpPort = int.Parse(ConfigurationManager.AppSettings["SMTP_PORT"]);
            bool smtpEnableSsl = bool.Parse(ConfigurationManager.AppSettings["SMTP_ENABLE_SSL"]);

            // Create an SMTP client with the specified host name and port.
            using (SmtpClient client = new SmtpClient(smtpHost, smtpPort))
            {
                // Create a network credential with your SMTP user name and password.
                client.Credentials = new NetworkCredential(smtpUser, smtpPassword);

                // Use SSL when accessing Amazon SES. The SMTP session will begin on an unencrypted connection, and then 
                // the client will issue a STARTTLS command to upgrade to an encrypted connection using SSL.
                client.EnableSsl = true;

                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(smtpFrom);
                foreach (var item in email.SendTo)
                {
                    var toEmail = item.ToLower();
                    mailMessage.To.Add(toEmail);
                }
                mailMessage.Subject = email.Title;
                mailMessage.Body = email.Body;
                try
                {
                    if (filePath != "")
                    {
                        Attachment attachment;
                        attachment = new Attachment(filePath);
                        mailMessage.Attachments.Add(attachment);
                    }
                    if (contentHtml != "")
                        mailMessage.Body = email.Body + contentHtml;

                }
                catch { }
                // End attachments
                mailMessage.IsBodyHtml = true;
                client.Send(mailMessage);
            }

        }

        public Task SendMailBCCAsync(NotificationContent email, string[] listBCC)
        {
            return Task.Run(() =>
            {
                SendMailBcc(email, listBCC);

            });
        }

        public void SendMailBcc(NotificationContent email, string[] listBCC)
        {
            string smtpFrom = ConfigurationManager.AppSettings["SMTP_FROM"];
            string smtpUser = ConfigurationManager.AppSettings["SMTP_USERNAME"];
            string smtpPassword = ConfigurationManager.AppSettings["SMTP_PASSWORD"];
            string smtpHost = ConfigurationManager.AppSettings["SMTP_HOST"];
            int smtpPort = int.Parse(ConfigurationManager.AppSettings["SMTP_PORT"]);
            bool smtpEnableSsl = bool.Parse(ConfigurationManager.AppSettings["SMTP_ENABLE_SSL"]);

            // Create an SMTP client with the specified host name and port.
            using (SmtpClient client = new SmtpClient(smtpHost, smtpPort))
            {
                // Create a network credential with your SMTP user name and password.
                client.Credentials = new NetworkCredential(smtpUser, smtpPassword);

                // Use SSL when accessing Amazon SES. The SMTP session will begin on an unencrypted connection, and then 
                // the client will issue a STARTTLS command to upgrade to an encrypted connection using SSL.
                client.EnableSsl = true;
                // client.EnableSsl = true;
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(smtpFrom);
                foreach (var item in email.SendTo)
                {
                    var toEmail = item.ToLower();
                    mailMessage.To.Add(toEmail);
                }
                if (listBCC != null)
                {
                    foreach (var itemBcc in listBCC)
                    {
                        var emailBcc = itemBcc.ToLower();
                        mailMessage.Bcc.Add(emailBcc);
                    }
                }
                mailMessage.Subject = email.Title;
                mailMessage.Body = email.Body;

                mailMessage.IsBodyHtml = true;

                client.Send(mailMessage);
            }
        }

    }
}
