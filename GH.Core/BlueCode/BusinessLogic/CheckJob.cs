using GH.Core.Services;
using GH.Core.ViewModels;
using Quartz;
using RegitSocial.Business.Notification;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace GH.Core.BlueCode.BusinessLogic
{
    public class CheckJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            checkExpired();
            CheckVault();
        }
       
        public void checkExpired()
        {
           
            var checkBill = new BillingLogic();
            var _accountService = new AccountService();
            var listBillExpired =  checkBill.GetListBillingByEndDate();
            foreach(var bill in listBillExpired)
            {
                try
                {
                    var user = _accountService.GetByAccountId(bill.UserId);
                    var dateExpired = bill.dateend.ToShortDateString();
                    var displayName = user.Profile.DisplayName;
                    var email = user.Profile.Email;
                    var tier = bill.productname;
                   
                    if(user !=null && !string.IsNullOrEmpty(email))
                    {
                        TaskSendEmailExpired(displayName, email, tier, dateExpired);
                    }

                }
                catch { }
               
            }
        }
        public void TaskSendEmailExpired(string displayName, string email, string tier, string dateExpired)
        {
            Task taskA = new Task(() =>
               SendEmailExpired(displayName, email, tier, dateExpired)
            );
            taskA.Start();
        }
        public void SendEmailExpired(string displayName, string email, string tier, string dateExpired)
        {
            
            var emailTemplate = string.Empty;
            emailTemplate = HostingEnvironment.MapPath("~/Content/EmailTemplates/EmailTemplate_ExpiredPromoCode.html");
           
            string emailContent = string.Empty;
           var baseUrl= ConfigurationManager.AppSettings["baseUrl"];

            if (File.Exists(emailTemplate))
            {
                emailContent = File.ReadAllText(emailTemplate);
                emailContent = emailContent.Replace("[user]", displayName);
                emailContent = emailContent.Replace("[dateExpired]", dateExpired);
                emailContent = emailContent.Replace("[tier]", tier);
                emailContent = emailContent.Replace("[callbacklink]", baseUrl);
             
                IMailService mailService = new MailService();
                mailService.SendMailAsync(new NotificationContent
                {
                    Title = "Notification from Regit",
                    Body = string.Format(emailContent, ""),
                    SendTo = new[] { email }
                });

            }
        }

        // Vault
        //  new NotificationBusinessLogic().SendNotificationMessageGoverment(User.Identity.GetUserId());

        public void CheckVault()
        {
            var acc = new AccountService();
            var listAccount = acc.GetListALlUserEmailVerified();
            var noti = new NotificationBusinessLogic();
            foreach (var item in listAccount)
            {
                try
                {
                    TaskSendNotificationVaultExpired(item.AccountId);
                }
                catch { }
            }
        }
        public void TaskSendNotificationVaultExpired(string userId)
        {
            Task taskB = new Task(() =>
               new NotificationBusinessLogic().SendNotificationMessageGoverment(userId)
            );
            taskB.Start();
        }

        //static async Task RunAsync()
        //{
        //     HttpClient client = new HttpClient();

        //    // New code:
        //    client.BaseAddress = new Uri("http://localhost:55268/");
        //    client.DefaultRequestHeaders.Accept.Clear();
        //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //    Console.ReadLine();
        //}
    }
}