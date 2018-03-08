using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GH.Core.Helpers;
using GH.Core.IRepositories;
using GH.Core.IServices;
using GH.Core.Models;
using GH.Core.Repositories;
using GH.Core.ViewModels;
using MongoDB.Bson;
using MongoDB.Driver;
using System.IO;
using System.Web;
using System.Reflection;

namespace GH.Core.Services
{
    public class DisabledUserService : IDisabledUserService
    {
        private IDisabledUserRepository DisabledUserRepository { get; set; }

        public DisabledUserService()
        {
            DisabledUserRepository = new DisabledUserRepository();
        }

        public DisabledUser DisabledUser(DisabledUser disabledUser)
        {
            DisabledUserRepository.Create(disabledUser);
            //Send Email
            var emailTemplate = string.Empty;
            if (System.Web.HttpContext.Current != null)
            {
                emailTemplate = HttpContext.Current.Server.MapPath("/Content/EmailTemplates/EmailTemplate_DisabledUser.html");
            }
            else
            {
                var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                emailTemplate = Path.Combine(appDir, @"Content\EmailTemplates\EmailTemplate_DisabledUser.html");
            }
            string emailContent = string.Empty;
            string _email = disabledUser.User.Profile.Email;
            if (File.Exists(emailTemplate) && !string.IsNullOrEmpty(_email))
            {
                emailContent = File.ReadAllText(emailTemplate);
                string effDate = disabledUser.EffectiveDate.ToString("MMMM dd, yyyy");
               
                var fullName = disabledUser.User.Profile.DisplayName;
                emailContent = emailContent.Replace("[username]", fullName);
                emailContent = emailContent.Replace("[EffectiveDate]", effDate);
                var baseUrl = Util.UrlHelper.GetCurrentBaseUrl();
                var callbackLink = String.Format("{0}/User/SignIn", baseUrl);
                emailContent = emailContent.Replace("[callbacklink]", callbackLink);

                var subject = string.Format("You requested close your account in Regit.");
                IMailService mailService = new MailService();

                mailService.SendMailAsync(new NotificationContent
                {
                    Title = "Notification from Regit",
                    Body = string.Format(emailContent, ""),
                    SendTo = new[] { _email }
                });

               
            }

                // End send Email

                return disabledUser;
        }

        public bool IsDisabled(ObjectId userId)
        {
            var disableUser = DisabledUserRepository.GetDisableUserByUserId(userId).FirstOrDefault();
            return disableUser != null && disableUser.EffectiveDate < DateTime.Now &&
                   disableUser.Until > DateTime.Now;
        }

        public bool IsDisabled(string email)
        {
            var disableUser = DisabledUserRepository.GetDisableUserByEmail(email).FirstOrDefault();
            return disableUser != null && disableUser.EffectiveDate < DateTime.Now &&
                   disableUser.Until > DateTime.Now;
        }

        public DisabledUser GetDisabledUserById(string id)
        {
            var disabledUser = DisabledUserRepository.GetById(id);
            return disabledUser;
        }

        public DisabledUser GetDisabledUserByEmail(string email)
        {
            var disabledUser = DisabledUserRepository.GetDisableUserByEmail(email).FirstOrDefault();
            return disabledUser;
        }

        public bool IsExisted(ObjectId userId)
        {
            var disableUser = DisabledUserRepository.GetDisableUserByUserId(userId).FirstOrDefault();
            return disableUser != null;
        }

        public Task<UpdateResult> EnableUsers(EnableParameters parameter)
        {
            var disableUsers = new List<DisabledUser>();
            foreach (DisableUserViewModel user in parameter.DisableUsers)
            {
                disableUsers.Add(new DisabledUser
                {
                    Id = user.Id.ParseToObjectId(),
                    IsEnabled = user.IsEnabled,
                    ModifiedOn = DateTime.Now
                });
            }
            var result = DisabledUserRepository.EnableMany(disableUsers);
            return result;
        }

        public UpdateResult EnableUsers(ObjectId userId)
        {
            var disabledUser = DisabledUserRepository.GetDisableUserByUserId(userId).FirstOrDefault();
            disabledUser.IsEnabled = true;
            disabledUser.ModifiedOn = DateTime.Now;
            var result = DisabledUserRepository.Update(disabledUser);

            //Send Email
            var emailTemplate = string.Empty;
            if (System.Web.HttpContext.Current != null)
            {
                emailTemplate = HttpContext.Current.Server.MapPath("/Content/EmailTemplates/EmailTemplate_EnableUser.html");
            }
            else
            {
                var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                emailTemplate = Path.Combine(appDir, @"Content\EmailTemplates\EmailTemplate_EnableUser.html");
            }
            string emailContent = string.Empty;
            string _email = disabledUser.User.Profile.Email;
            if (File.Exists(emailTemplate) && !string.IsNullOrEmpty(_email))
            {
                emailContent = File.ReadAllText(emailTemplate);
              
                var fullName = disabledUser.User.Profile.DisplayName;
                emailContent = emailContent.Replace("[username]", fullName);
               
                var baseUrl = Util.UrlHelper.GetCurrentBaseUrl();
                var callbackLink = String.Format("{0}/User/SignIn", baseUrl);
                emailContent = emailContent.Replace("[callbacklink]", callbackLink);

                var subject = string.Format("You have restored your closed account.");
                IMailService mailService = new MailService();

                mailService.SendMailAsync(new NotificationContent
                {
                    Title = "Notification from Regit",
                    Body = string.Format(emailContent, ""),
                    SendTo = new[] { _email }
                });


            }

            // End send Email
            return result;
        }

        public DisableUserResult GetAllDisableUser(DisableUserParamter disableUserParamter)
        {
            var builders = Builders<DisabledUser>.Filter;
            //   var filter = builders.Eq("IsEnabled", false) & builders.Gte("Until", DateTime.Now);
            var filter = builders.Eq("IsEnabled", false);
         
            var skip = disableUserParamter.Start * disableUserParamter.Length;
            var disableUsers = DisabledUserRepository.GetAllDisableUser(filter, skip, disableUserParamter.Length).ToList();
            var total = DisabledUserRepository.GetTotal(filter).Count();
            var result = new DisableUserResult
            {
                Total = total,
                DisableUsers = disableUsers.Select(x => new DisableUserViewModel
                {
                    Id = x.Id.ToString(),
                    UserId = x.UserId.ToString(),
                    Name = x.User.AccountType == AccountType.Business
                        ? x.User.CompanyDetails.CompanyName
                        : x.User.Profile.DisplayName,
                    Email = x.User.Profile.Email,
                    EffectiveDate = x.EffectiveDate,
                    Until = x.Until,
                    Reason = x.Reason,
                    IsEnabled = x.IsEnabled
                }).ToList()
            };
            return result;
        }
    }
}