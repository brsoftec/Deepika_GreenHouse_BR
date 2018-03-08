using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.Invite;
using MongoDB.Bson;
using MongoDB.Driver;
using GH.Core.Models;
using GH.Core.Exceptions;
using GH.Core.Extensions;
using GH.Core.ViewModels;
using GH.Web.Helpers;
using Microsoft.AspNet.Identity;
using NLog;

namespace GH.Core.Services
{
    public class InviteService : IInviteService
    {
        IMongoCollection<Account> _accountCollection;

        private MongoRepository<Invite> _inviteRepository;

        private Logger log = LogManager.GetCurrentClassLogger();

        public InviteService()
        {
            var db = MongoContext.Db;
            _accountCollection = db.Accounts;
            _inviteRepository = new MongoRepository<Invite>();
        }

        public Invite CreateInvite(Invite invite)
        {
            invite.Id = ObjectId.GenerateNewId();

            try
            {
                _inviteRepository.Add(invite);
                log.Debug("Inserted invite " + invite.Id.ToString());
            }
            catch (Exception ex)
            {
                log.Debug("Error inserting invite Id = " + invite.Id.ToString() + " exception " + ex.ToString());
                return null;
            }
            return invite;
        }

        public Invite GetInviteById(string id)
        {
            try
            {
                var Id = new ObjectId(id);
                var result = _inviteRepository.Single(Id);
                return result;
            }
            catch (Exception ex)
            {
                return null;
//                throw new CustomException(ex.Message);
            }
        }

        public void ConvertInviteById(string id)
        {
            try
            {
                var Id = new ObjectId(id);
                Invite invite = _inviteRepository.Single(Id);
                ObjectId fromId = new ObjectId(invite.FromUserId);
//                ObjectId toId = new ObjectId(.User.Identity.GetUserId());

                var accountService = new AccountService();
                
                var account = accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
                ObjectId toId = account.Id;
                
                switch (invite.Category)
                {
                    case "network":
                        var networkService = new NetworkService();
                        networkService.Invite(fromId,toId,id);
                        break;                  
                    case "workflow":
                        var workflowService = new JoiningBusinessInvitationService();
                        string roleNames = invite.Options;
                        List<string> roles = null;
                        if (!String.IsNullOrEmpty(roleNames))
                        {
                            roles = roleNames.Split(',').ToList();
                        }
                        workflowService.Invite(fromId,toId,roles,id);
                        break;
                }
                _inviteRepository.UpdateField(invite,"Status","converted");
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }


        public string UpdateInvite(Invite invite)
        {
            var rs = "";

            var inviteCollection = MongoDBConnection.Database.GetCollection<Invite>(RegitTable.Invite);
            try
            {
                var builder = Builders<Invite>.Filter;
                var filter = builder.Eq(c => c.Id, invite.Id);

                var update = Builders<Invite>.Update
                    .Set(f => f.Email, invite.Email)
                    .Set(f => f.FromUserId, invite.FromUserId)
                    .Set(f => f.FromDisplayName, invite.FromDisplayName)
                    .Set(f => f.Category, invite.Category)
                    .Set(f => f.Message, invite.Message)
                    .Set(f => f.Status, invite.Status);
                inviteCollection.UpdateOne(filter, update);
                rs = invite.Id.ToString();
            }
            catch
            {
            }
            return rs;
        }

        public string SetInviteSent(Invite invite)
        {
            var rs = "";

            var inviteCollection = MongoDBConnection.Database.GetCollection<Invite>(RegitTable.Invite);
            try
            {
                var builder = Builders<Invite>.Filter;
                var filter = builder.Eq(c => c.Id, invite.Id);

                var update = Builders<Invite>.Update
                    .Set(f => f.Status, "sent")
                    .Set(f => f.Sent, DateTime.Now);

                inviteCollection.UpdateOne(filter, update);
                rs = invite.Id.ToString();
            }
            catch
            {
            }
            return rs;
        }

        public void SendInvite(Invite invite)
        {
            var baseUrl = Util.UrlHelper.GetCurrentBaseUrl();
            var callbackLink = $"{baseUrl}/User/SignUp?invite={invite.Id.ToString()}";
            var subject = $"{invite.FromDisplayName} to connect on Regit";

            var emailModel = new InviteEmailViewModel
            {
                category = invite.Category,
                subject = subject,
                message = invite.Message,
                fromName = invite.FromDisplayName,
                toName = invite.ToName ?? "",
                backLink = callbackLink
            };

            string emailContent =
                ViewUtils.RenderPartialViewToString("_EmailTemplate_Invite", emailModel);

            IMailService mailService = new MailService();
            mailService.SendMailAsync(new NotificationContent
            {
                Title = "Invitation from " + subject,
                Body = string.Format(emailContent, ""),
                SendTo = new[] {invite.Email}
            });
            SetInviteSent(invite);
        }

        public async Task<FuncResult> SendInviteAsync(Invite invite)
        {
            var baseUrl = Util.UrlHelper.GetCurrentBaseUrl();
            var callbackLink = $"{baseUrl}/User/SignUp?invite={invite.Id}";
            var subject = $"{invite.FromDisplayName} to connect on Regit";

            var emailModel = new InviteEmailViewModel
            {
                category = invite.Category,
                subject = subject,
                message = invite.Message,
                fromName = invite.FromDisplayName,
                toName = invite.ToName ?? "",
                backLink = callbackLink
            };

            string emailContent =
                ViewUtils.RenderPartialViewToString("_EmailTemplate_Invite", emailModel);

            IMailService mailService = new MailService();
            await mailService.SendMailAsync(new NotificationContent
            {
                Title = "Invitation from " + subject,
                Body = string.Format(emailContent, ""),
                SendTo = new[] {invite.Email}
            });
            SetInviteSent(invite);
            return new OkResult("invite.send.ok");
        }

        public List<Invite> GetInvites(string fromUserId, string cat = null, string status = null)
        {
            var invites = new List<Invite>();
            try
            {
                if (cat != null)
                {
                    if (status == null)
                    {
                        invites = _inviteRepository.Many(l => l.FromUserId.Equals(fromUserId)
                                                              && l.Category.Equals(cat) && l.Status.Equals("sent"))
                            .OrderByDescending(s => s.Created).ToList();
                    }
                    else
                    {
                        invites = _inviteRepository.Many(l => l.FromUserId.Equals(fromUserId)
                                                              && l.Category.Equals(cat) && l.Status.Equals(status))
                            .OrderByDescending(s => s.Created).ToList();
                    }
                }
                else
                {
                    invites = _inviteRepository.Many(l => l.FromUserId.Equals(fromUserId)).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
            return invites;
        }

        public void DeleteInviteById(string id)
        {
            try
            {
                var Id = new ObjectId(id);
                Invite invite = _inviteRepository.Many(l => l.Id.Equals(Id)).FirstOrDefault();
                _inviteRepository.Delete(invite);
            }
            catch
            {
            }
        }
    }
}