using GH.Core.Services;
using GH.Web.Areas.User.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using GH.Core.Exceptions;
using GH.Core.BlueCode.BusinessLogic;
using GH.Core.BlueCode.Entity.Outsite;
using GH.Core.BlueCode.Entity.Notification;
using System.IO;
using System.Reflection;
using GH.Core.ViewModels;
using GH.Core.Models;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    [RoutePrefix("Api/Networks")]
    public class NetworksApiController : ApiController
    {
        private INetworkService _networkService;
        private IAccountService _accountService;
        private IOutsiteBusinessLogic _outsiteBusinessLogic;

        public NetworksApiController()
        {
            _networkService = new NetworkService();
            _accountService = new AccountService();
            _outsiteBusinessLogic = new OutsiteBusinessLogic();
        }

        [HttpGet, Route("")]
        public async Task<List<NetworkViewModel>> GetNetworksOfCurrentUser()
        {
            var currentUserId = HttpContext.Current.User.Identity.GetUserId();

            var account = _accountService.GetByAccountId(currentUserId);

            var networks = _networkService.GetNetworksOfuser(account.Id);

            return networks.Select(n => new NetworkViewModel {Id = n.Id.ToString(), Name = n.Name})
                .OrderByDescending(n => n.Name).ToList();
        }

        // Hoang Vu
        [HttpGet, Route("Friends")]
        public async Task<FriendsInNetworkFullViewModel> GetUsersInNetwork(string networkId)
        {
            var network = _networkService.GetNetworkById(new MongoDB.Bson.ObjectId(networkId));

            if (network == null)
            {
                throw new CustomException("Network not found");
            }

            var account = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
            if (account.Id != network.NetworkOwner)
            {
                throw new CustomException("Permission denied");
            }
            var result = new FriendsInNetworkFullViewModel
            {
                Total = network.Friends.Count,
                Friends = new List<FriendInNetworkFullInfo>()
            };
            try
            {
                foreach (var fr in network.Friends)
                {
                    var FriendFullInfo = new FriendInNetworkFullInfo();
                    FriendFullInfo.Id = fr.Id.ToString();
                    FriendFullInfo.UserId = fr.UserId;
                    FriendFullInfo.Relationship = fr.Relationship;
                    FriendFullInfo.IsEmergency = fr.IsEmergency;
                    FriendFullInfo.Rate = fr.Rate;
                    FriendFullInfo.NetworkId = networkId;
                    var acc = new Account();
                    try
                    {
                        acc = _accountService.GetByAccountId(fr.UserId);
                        string birthday = "";

                        if (!string.IsNullOrEmpty(acc.Id.ToString()))
                        {
                            if (!string.IsNullOrEmpty(acc.Profile.Birthdate.ToString()))
                            {
                                try
                                {
                                    var tem = (DateTime) acc.Profile.Birthdate;
                                    birthday = tem.ToString("dd-MM-yyyy");
                                }
                                catch
                                {
                                }
                            }
                            FriendFullInfo.Avatar = acc.Profile.PhotoUrl;
                            FriendFullInfo.DisplayName = acc.Profile.DisplayName;
                            FriendFullInfo.FirstName = acc.Profile.FirstName;
                            FriendFullInfo.LastName = acc.Profile.LastName;
                            FriendFullInfo.Phone = acc.Profile.PhoneNumber;
                            FriendFullInfo.Email = acc.Profile.Email;
                            FriendFullInfo.Street = acc.Profile.Street;
                            FriendFullInfo.Country = acc.Profile.Country;
                            FriendFullInfo.City = acc.Profile.City;
                            FriendFullInfo.BirthDay = birthday;
                        }
                    }
                    catch
                    {
                    }
                    if (!string.IsNullOrEmpty(FriendFullInfo.DisplayName))
                        result.Friends.Add(FriendFullInfo);
                }
            }
            catch
            {
            }
            return result;
        }

        [HttpGet, Route("SearchUsersForInvitation")]
        public async Task<List<FriendInNetworkInfo>> SearchUsersForInvitation(string keyword)
        {
            var account = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());

            var users = _networkService.SearchUsersForInvitation(account.Id, keyword);

            return users.Select(u => new FriendInNetworkInfo
            {
                Id = u.Id.ToString(),
                Avatar = u.Profile.PhotoUrl,
                DisplayName = u.Profile.DisplayName
            }).ToList();
        }

        [Authorize, HttpPost, HttpGet, Route("GetFriends")]
        public async Task<List<FriendInNetworkInfo>> GetFriends()
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();
            var users = _networkService.GetFriends(userId);
            return users.Select(u => new FriendInNetworkInfo
            {
                Id = u.AccountId.ToString(),
                Avatar = u.Profile.PhotoUrl,
                DisplayName = u.Profile.DisplayName
            }).ToList();
        }

        [HttpGet, Route("Invitations")]
        public async Task<List<InvitationViewModel>> GetInvitations(string fromId = null)
        {
            var account = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());

            MongoDB.Bson.ObjectId? from = null;
            if (!string.IsNullOrEmpty(fromId))
            {
                from = new MongoDB.Bson.ObjectId(fromId);
            }

            var invitations = _networkService.GetReceivedInvitations(account.Id, from);
            var senderIds = invitations.Select(i => i.From).ToList();

            var senders = _accountService.GetByListId(senderIds);

            var results = new List<InvitationViewModel>();
            foreach (var invitation in invitations)
            {
                var sender = senders.FirstOrDefault(s => s.Id == invitation.From);
                var networkName = "";
                if (!string.IsNullOrEmpty(invitation.NetworkName))
                {
                    networkName = invitation.NetworkName;
                }
                var relationship = "";
                if (!string.IsNullOrEmpty(invitation.Relationship))
                    relationship = invitation.Relationship;

                var isEmergency = false;
                if (!string.IsNullOrEmpty(invitation.IsEmergency.ToString()))
                {
                    isEmergency = invitation.IsEmergency;
                }
                var rate = 0;
                if (!string.IsNullOrEmpty(invitation.Rate.ToString()))
                {
                    rate = invitation.Rate;
                }

                var inv = new InvitationViewModel
                {
                    Id = invitation.Id.ToString(),
                    NetworkName = networkName,
                    Relationship = relationship,
                    IsEmergency = isEmergency,
                    Rate = rate,
                    Sender = new FriendInNetworkInfo
                    {
                        Id = sender.Id.ToString(),
                        Avatar = sender.Profile.PhotoUrl,
                        DisplayName = sender.Profile.DisplayName
                    }
                };
                if (!String.IsNullOrEmpty(invitation.InviteId))
                {
                    inv.InviteId = invitation.InviteId;
                }

                results.Add(inv);
            }

            return results;
        }

        [HttpGet, Route("GetSendInvitations")]
        public async Task<List<SendInvitationViewModel>> GetSendInvitations()
        {
            var account = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
            var invitations = _networkService.GetSendInvitations(account.Id);
            var receiverIds = invitations.Select(i => i.To).ToList();
            var receivers = _accountService.GetByListId(receiverIds);

            var results = new List<SendInvitationViewModel>();
            foreach (var invitation in invitations)
            {
                var receiver = receivers.FirstOrDefault(s => s.Id == invitation.To);
                var networkName = "";
                if (!string.IsNullOrEmpty(invitation.NetworkName))
                {
                    networkName = invitation.NetworkName;
                }
                var relationship = "";
                if (!string.IsNullOrEmpty(invitation.Relationship))
                    relationship = invitation.Relationship;

                var isEmergency = false;
                if (!string.IsNullOrEmpty(invitation.IsEmergency.ToString()))
                {
                    isEmergency = invitation.IsEmergency;
                }
                var rate = 0;
                if (!string.IsNullOrEmpty(invitation.Rate.ToString()))
                {
                    rate = invitation.Rate;
                }

                var inv = new SendInvitationViewModel
                {
                    Id = invitation.Id.ToString(),
                    SentAt = invitation.SentAt,
                    NetworkName = networkName,
                    Relationship = relationship,
                    IsEmergency = isEmergency,
                    Rate = rate,
                    Receiver = new FriendInNetworkInfo
                    {
                        Id = receiver.Id.ToString(),
                        Avatar = receiver.Profile.PhotoUrl,
                        DisplayName = receiver.Profile.DisplayName
                    }
                };

                if (!String.IsNullOrEmpty(invitation.InviteId))
                {
                    inv.InviteId = invitation.InviteId;
                }

                results.Add(inv);
            }

            return results;
        }

        [HttpPost, Route("Invite")]
        public async Task Invite(InviteFriendModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            var sender = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
            var receiver = _accountService.GetById(new MongoDB.Bson.ObjectId(model.ReceiverId));

            if (receiver == null)
            {
                throw new CustomException("Receiver not found");
            }

            // Write activity log
            if (sender.AccountActivityLogSettings.RecordNetwork)
            {
                string title = "You invited " + receiver.Profile.DisplayName + " to your network";
                string type = "network";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(sender.AccountId, title, type);
            }

            _networkService.Invite(sender.Id, receiver.Id);
        }


        [HttpPost, Route("Invite/Cancel")]
        public async Task CancelInvitation(AcceptDenyInvitationModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            var remover = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());

            // Write activity log
            if (remover.AccountActivityLogSettings.RecordNetwork)
            {
                string title = "You removed network invitation to ";
                string type = "network";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(remover.AccountId, title, type);
            }

            _networkService.RemoveInvitation(new MongoDB.Bson.ObjectId(model.InvitationId), remover.Id);
        }


        [HttpPost, Route("Invite/Accept")]
        public async Task AcceptInvitation(AcceptDenyInvitationModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            var accepter = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());

            // Write activity log
            if (accepter.AccountActivityLogSettings.RecordNetwork)
            {
                string title = "You accepted an invitation to join network.";
                string type = "network";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(accepter.AccountId, title, type);
            }
            _networkService.AcceptInvitation(new MongoDB.Bson.ObjectId(model.InvitationId), accepter.Id);
        }


        [HttpPost, Route("Invite/Deny")]
        public async Task DenyInvitation(AcceptDenyInvitationModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            var denier = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());

            // Write activity log
            if (denier.AccountActivityLogSettings.RecordNetwork)
            {
                string title = "You denied an invitation to join network.";
                string type = "network";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(denier.AccountId, title, type);
            }
            _networkService.DenyInvitation(new MongoDB.Bson.ObjectId(model.InvitationId), denier.Id);
        }

        [HttpPost, Route("MoveFriend")]
        public void MoveFriend(MoveFriendModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            var account = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());

            // var fAccount = _accountService.GetByAccountId(model.FriendId);
            if (account.AccountActivityLogSettings.RecordNetwork)
            {
                string title = "You moved a person across your network.";
                string type = "network";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(account.AccountId, title, type);
            }

            _networkService.MoveNetwork(account.Id, new MongoDB.Bson.ObjectId(model.FriendId),
                new MongoDB.Bson.ObjectId(model.FromNetworkId), new MongoDB.Bson.ObjectId(model.ToNetworkId));
        }

        [HttpPost, Route("RemoveFriend")]
        public async Task RemoveFriend(RemoveFriendModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            var account = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
            var accountFriend = _accountService.GetById(new MongoDB.Bson.ObjectId(model.FriendId));
            // Write log


            if (account.AccountActivityLogSettings.RecordNetwork)
            {
                string title = "You removed a person from your network.";
                string type = "network";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(account.AccountId, title, type);
            }
            _networkService.RemoveFromNetwork(account.Id, new MongoDB.Bson.ObjectId(model.FriendId),
                new MongoDB.Bson.ObjectId(model.NetworkId));

            var delegation = new DelegationBusinessLogic();
            try
            {
                delegation.DeleteDelegation(account.AccountId, accountFriend.AccountId);
            }
            catch
            {
            }
        }

        #region TrustNetwork

        [HttpPost, Route("InviteTrustEmergency")]
        public async Task InviteTrustEmergency(TrustEmergencyModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }
            var sender = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
            var receiver = _accountService.GetById(new MongoDB.Bson.ObjectId(model.ReceiverId));

            if (receiver == null)
            {
                throw new CustomException("Receiver not found");
            }
            // Write activity log        
            if (sender.AccountActivityLogSettings.RecordNetwork)
            {
                string title = "You invited " + receiver.Profile.DisplayName + " to your emergency network";
                string type = "network";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(sender.AccountId, title, type);
            }

            _networkService.InviteTrustEmergency(sender.Id, receiver.Id, model.Relationship, model.IsEmergency,
                model.Rate);
        }

        [HttpPost, Route("UpdateTrustEmergency")]
        public async Task UpdateTrustEmergency(FriendInNetworkFullInfo model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            var account = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
            if (!string.IsNullOrEmpty(account.AccountId))
            {
                _networkService.UpdateTrustEmergency(account.Id, account.AccountId, account.Profile.DisplayName,
                    new MongoDB.Bson.ObjectId(model.NetworkId),
                    new MongoDB.Bson.ObjectId(model.Id), model.UserId, model.DisplayName, model.Relationship,
                    model.IsEmergency, model.Rate);
            }
            // Write log
            if (account.AccountActivityLogSettings.RecordNetwork)
            {
                string title = "You removed a person as emergency contact.";
                string type = "network";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(account.AccountId, title, type);
            }
        }

        [HttpPost, Route("UpdateNetworkRelationship")]
        public async Task UpdateNetworkRelationship(FriendInNetworkFullInfo model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            var account = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
            if (!string.IsNullOrEmpty(account.AccountId))
            {
                _networkService.UpdateNetworkRelationship(account.Id, account.AccountId, account.Profile.DisplayName,
                    new MongoDB.Bson.ObjectId(model.NetworkId),
                    new MongoDB.Bson.ObjectId(model.Id), model.UserId, model.Relationship);
            }
            // Write log
            if (account.AccountActivityLogSettings.RecordNetwork)
            {
                string title = "You updated a network relationship.";
                string type = "network";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(account.AccountId, title, type);
            }
        }

        //AcceptTrustEmergency
        [HttpPost, Route("AcceptTrustEmergency")]
        public async Task AcceptTrustEmergency(AcceptDenyInvitationModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }
            var accepter = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());

            // Write log
            if (accepter.AccountActivityLogSettings.RecordNetwork)
            {
                string title = "You accepted an invitation to join network.";
                string type = "network";
                var act = new ActivityLogBusinessLogic();
                act.WriteActivityLogFromAcc(accepter.AccountId, title, type);
            }
            _networkService.AcceptTrustEmergency(new MongoDB.Bson.ObjectId(model.InvitationId), accepter.Id,
                model.Relationship, model.IsEmergency, model.Rate);
        }

        [HttpGet, Route("SearchUsersForTrustEmergency")]
        public async Task<List<FriendInNetworkInfo>> SearchUsersForTrustEmergency(string keyword)
        {
            var account = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
            var users = _networkService.SearchUsersForTrustEmergency(account.Id, keyword);
            return users.Select(u => new FriendInNetworkInfo
            {
                Id = u.Id.ToString(),
                Avatar = u.Profile.PhotoUrl,
                DisplayName = u.Profile.DisplayName
            }).ToList();
        }

        #endregion TrustNetwork

        [HttpGet, Route("GetVaultByUserId")]
        public HttpResponseMessage GetVaultByUserId(string userId)
        {
            string jsonCompanyDetails = "";
            try
            {
                var _infomationVault = new InfomationVaultBusinessLogic();
                jsonCompanyDetails = _infomationVault.GetInformationVaultJsonByUserId(userId);
            }
            catch
            {
            }
            var response = Request.CreateResponse<object>(System.Net.HttpStatusCode.OK,
                MongoDB.Bson.Serialization.BsonSerializer.Deserialize<object>(jsonCompanyDetails));
            return response;
        }

        [HttpPost, Route("InviteEmergencyByEmail")]
        public async Task InviteEmergencyByEmail(EmailInviteEmergency inviteMail)
        {
            var fromAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());

            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            // Out site
            var outsite = new Outsite();
            outsite.DateCreate = DateTime.Now;
            outsite.Email = inviteMail.ToEmail;
            outsite.FromUserId = fromAccount.AccountId;
            outsite.CompnentId = "";
            outsite.Description = "";
            outsite.FromDisplayName = fromAccount.Profile.DisplayName;
            outsite.Type = EnumNotificationType.InviteEmergency;

            string toEmail = inviteMail.ToEmail;
            try
            {
                string OutsiteId = _outsiteBusinessLogic.InsertOutsite(outsite);
                //
                var emailTemplate = string.Empty;
                if (System.Web.HttpContext.Current != null)
                {
                    emailTemplate =
                        HttpContext.Current.Server.MapPath(
                            "/Content/EmailTemplates/EmailTemplate_InviteForEmercy.html");
                }
                else
                {
                    var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    emailTemplate = Path.Combine(appDir, @"Content\EmailTemplates\EmailTemplate_InviteForEmercy.html");
                }
                string emailContent = string.Empty;
                if (File.Exists(emailTemplate))
                {
                    emailContent = File.ReadAllText(emailTemplate);
                    var fullName = fromAccount.Profile.DisplayName;
                    emailContent = emailContent.Replace("[username]", fullName);
                    var baseUrl = Util.UrlHelper.GetCurrentBaseUrl();

                    var callbackLink = String.Format("{0}/User/SignUpEx?id={1}", baseUrl, OutsiteId);
                    emailContent = emailContent.Replace("[callbacklink]", callbackLink);
                    var subject = string.Format("{0} invites you to join Regit Social", fullName);
                    IMailService mailService = new MailService();
                    await mailService.SendMailAsync(new NotificationContent
                    {
                        Title = "Notification from Regit",
                        Body = string.Format(emailContent, ""),
                        SendTo = new[] {toEmail}
                    });
                }
            }
            catch
            {
            }
            // Outsite
        }

        [HttpPost, Route("InviteEmergencyByListEmail")]
        public async Task InviteEmergencyByListEmail(ListEmailInviteEmergency lstMail)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ModelState);
            }

            try
            {
                var fromAccount = _accountService.GetByAccountId(HttpContext.Current.User.Identity.GetUserId());
                _networkService.InviteListEmailOutsideForEmergency(fromAccount.AccountId, lstMail.ToEmail,
                    lstMail.InviteId);
            }
            catch
            {
            }
        }
    }
}