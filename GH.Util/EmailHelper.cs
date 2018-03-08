
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace GH.Util
{
    public class EmailHelper
    {
        public static async Task SendEmail(string smtpServer, int smtpPort, string fromEmail, string fromPassword,string fromDisplayName, string toEmail, string subject, string body, bool isBodyHtml,bool enableSsl, bool sendAsynchronous = true)
        {
            SmtpClient client = new SmtpClient();
            client.Host = smtpServer;
            client.Port = smtpPort;
            client.EnableSsl = enableSsl;
            client.DeliveryFormat = SmtpDeliveryFormat.International;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(fromEmail, fromPassword);
            MailMessage email = new MailMessage(new MailAddress(fromEmail, fromDisplayName),new MailAddress(toEmail, toEmail));
            email.Body = body;
            email.Subject = subject;
            email.IsBodyHtml = isBodyHtml;
            email.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
            if (sendAsynchronous)
            {
                await client.SendMailAsync(email);
            }
            else
            {
                client.Send(email);
            }
            email.Dispose();
        }

        public static async Task SendEmail(string fromDisplayName, string toEmail, string subject, string body, bool isBodyHtml, bool enableSsl, bool sendAsynchronous = true)
        {
            SmtpClient client = new SmtpClient();
            var credential = client.Credentials as NetworkCredential;
            //var smtpSection = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
            client.EnableSsl = enableSsl;
            client.DeliveryFormat = SmtpDeliveryFormat.International;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(credential.UserName, credential.Password);
            MailMessage email = new MailMessage(new MailAddress(credential.UserName, fromDisplayName), new MailAddress(toEmail, toEmail));
            email.Body = body;
            email.Subject = subject;
            email.IsBodyHtml = isBodyHtml;
            email.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            if(sendAsynchronous)
            {
                await client.SendMailAsync(email);
            }
            else
            {
                client.Send(email);
            }

            email.Dispose();
        }

        private static void Client_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
        }
    }
}
/* Put this section to web.config / app.config
 
  <system.net>
    <mailSettings>
      <smtp deliveryMethod = "Network" from="tungtrungvn.test@gmail.com">
        <network host = "smtp.gmail.com" port="587" userName="tungtrungvn.test@gmail.com"  password="1234@ABCD" />
      </smtp>
    </mailSettings>
  </system.net>

*/