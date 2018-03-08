using GH.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using GH.Web.Areas.User.ViewModels;
using GH.Core.Models;
using MongoDB.Bson;
using GH.Core.BlueCode.Entity.Notification;
using RegitSocial.Business.Notification;
using System.Threading.Tasks;
using Elmah;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    [BaseApi]
    [RoutePrefix("Api/Notifications")]
    public class NotificationApiController : BaseApiController
    {
        INotificationService _notificationService;
        IAccountService _accountService;
        INotificationBusinessLogic notificationBusinessLogic;

        public NotificationApiController()
        {
            _notificationService = new NotificationService();
            _accountService = new AccountService();
            notificationBusinessLogic = new NotificationBusinessLogic();
        }

        [HttpGet, Route("CountUnread")]
        public long CountUnreadNotifications()
        {
            try
            {
                var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());
                return _notificationService.CountUnreadNotifications(currentUser.Id);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        [HttpGet, Route("CountUnreadByAccountId/{accountId}")]
        public long CountUnreadByUserId(string accountId)
        {
            try
            {
                var user = _accountService.GetByAccountId(accountId);
                return _notificationService.CountUnreadNotifications(user.Id);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        [HttpGet, Route("")]
        public NotificationsResult GetNotifications(int? start = 0, int? length = 10,
            DateTime? createdBefore = null)
        {
            var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());
            long total = 0;
            DateTime queryTime = DateTime.Now;
            var notifications =
                _notificationService.GetNotifications(currentUser.Id, start, length, createdBefore, out total,
                    out queryTime);
            var senderIds = notifications.Where(n => n.Creator.HasValue).Select(n => n.Creator.Value).Distinct()
                .ToList();

            var senders = _accountService.GetByListId(senderIds);

            var result = new NotificationsResult();
            result.Total = total;
            result.QueryTime = queryTime;

            foreach (var notification in notifications)
            {
                var viewModel = new NotificationInfo();
                viewModel.CreatedAt = notification.CreatedAt;
                viewModel.Read = notification.Read;
                viewModel.Id = notification.Id.ToString();
                viewModel.TargetType = notification.TargetType.ToString();
                if (notification.Creator.HasValue)
                {
                    var creator = senders.First(s => s.Id == notification.Creator);
                    viewModel.Creator = creator.Profile.DisplayName;
                    viewModel.CreatorAvatar = creator.Profile.PhotoUrl;
                }
                else
                {
                    viewModel.Creator = "REGIT";
                }

                switch (notification.TargetType)
                {
                    case NotificationTargetType.ReviewPost:
                        viewModel.Message = "has writen a post in business. The post need an approval.";
                        viewModel.Url = "/BusinessAccount/Posts/" + notification.TargetId.Value;
                        break;
                    case NotificationTargetType.ApprovePost:
                        viewModel.Message = "has approved a post that you posted";
                        viewModel.Url = "/BusinessAccount/Posts/" + notification.TargetId.Value;
                        break;
                    case NotificationTargetType.RejectPost:
                        viewModel.Message = "has rejected a post that you posted";
                        viewModel.Url = "/BusinessAccount/Posts/" + notification.TargetId.Value;
                        break;
                    default:
                        break;
                }

                result.Notifications.Add(viewModel);
            }

            return result;
        }


        [HttpPost, Route("Read")]
        public void Read(MarkNotificationAsReadModel model)
        {
            var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());
            _notificationService.Read(new ObjectId(model.Id), currentUser.Id);
        }

        [HttpPost, Route("ReadAll")]
        public void ReadAll()
        {
            var currentUser = _accountService.GetByAccountId(User.Identity.GetUserId());
            _notificationService.MarkAllAsRead(currentUser.Id);
        }

        [HttpPost, Route("GetLatestNotificationWithActiveAccount")]
        public LatestNotification GetLatestNotificationWithActiveAccount()
        {
            var currentAccountId = User.Identity.GetUserId();
            var currentUser = _accountService.GetByAccountId(currentAccountId);

            if (currentUser.AccountType != AccountType.Business)
            {
                var businessAccount = _accountService.GetBusinessAccountsLinkWithPersonalAccount(currentUser.Id)
                    .FirstOrDefault();
                if (businessAccount != null)
                {
                    currentAccountId = businessAccount.AccountId;
                }
            }

            var latestNotification = _notificationService.GetLatestNotification(currentAccountId);
            return latestNotification;
        }

        [HttpGet, Route("Latest")]
        public LatestNotification GetLatestNotifications()
        {
            var currentAccountId = User.Identity.GetUserId();
            var latestNotification = _notificationService.GetLatestNotification(currentAccountId);
            return latestNotification;
        }

        [HttpPost, Route("GetLatestNotification")]
        public LatestNotification GetLatestNotification()
        {
            var currentAccountId = User.Identity.GetUserId();
            var latestNotification = _notificationService.GetLatestNotification(currentAccountId);
            return latestNotification;
        }

        [HttpPost, Route("GetLatestNotificationByAccountId/{id}")]
        public LatestNotification GetLatestNotificationByAccountId(string id)
        {
            try
            {
                var latestNotification = _notificationService.GetLatestNotification(id);
                return latestNotification;
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                return null;
            }
        }

        [HttpPost, Route("GetLisNotificationNewView")]
        public HttpResponseMessage GetLisNotificationNewView(HttpRequestMessage request,
            NotifycationModelView notifycationModelView)
        {
            if (notifycationModelView == null)
                notifycationModelView = new NotifycationModelView();
            try
            {
                var accountId = User.Identity.GetUserId();
                IList<NotificationMessage> result = notificationBusinessLogic.ViewNotification(accountId).ToList();
                notifycationModelView.Listitems = result;
            }
            catch
            {
                notifycationModelView.ReturnStatus = false;
                notifycationModelView.ReturnMessage = new string[] {"fail"}.ToList();
            }

            var response = Request.CreateResponse<NotifycationModelView>(HttpStatusCode.OK, notifycationModelView);
            return response;
        }

        [HttpGet, Route("NotifyMissingInformationOfVault")]
        public void NotifyMissingInformationOfVault(string campaignId, string delegatorId)
        {
            var delegatorAccount = _accountService.GetByAccountId(delegatorId);
            if (delegatorAccount != null)
            {
                var delegateeAccount = _accountService.GetByAccountId(User.Identity.GetUserId());

                //Send notification
                var notificationMessage = new NotificationMessage();
                notificationMessage.Id = ObjectId.GenerateNewId();
                notificationMessage.Type = EnumNotificationType.NotifyMissingInformationVaultForRegistration;
                notificationMessage.FromAccountId = delegateeAccount.AccountId;
                notificationMessage.FromUserDisplayName = delegateeAccount.Profile.DisplayName;
                notificationMessage.ToAccountId = delegatorAccount.AccountId;
                notificationMessage.ToUserDisplayName = delegatorAccount.Profile.DisplayName;
                //notificationMessage.Content = delegationMessage.Message;
                notificationMessage.PreserveBag = campaignId;
                notificationBusinessLogic.SendNotification(notificationMessage);
            }
        }


        [HttpGet, Route("GetNotificationByAccountId")]
        public async Task<List<NotificationMessage>> GetNotificationByAccountId(string type = null, int start = 0,
            int take = 10)
        {
            var rs = new List<NotificationMessage>();
            try
            {
                var accountId = User.Identity.GetUserId();
                if (!string.IsNullOrEmpty(accountId))
                    rs = notificationBusinessLogic.GetNotificationByAccountId(accountId, type, start, take);
            }
            catch
            {
            }

            return rs;
        }

        [BaseApi]
        [HttpGet, Route("List")]
        public async Task<HttpResponseMessage> GetNotificationsOfActiveUser([FromUri] int start = 0,
            [FromUri] int take = 10)
        {
            var rs = new List<UserNotification>();
//            try
            {
                rs = notificationBusinessLogic.GetNotifications(AccountId, null, start, take);
            }
/*            catch (Exception e)
            {
                return Request.CreateApiErrorResponse("Notification listing error");
            }*/

            return Request.CreateSuccessResponse(rs, $"List {rs.Count} items");
        }

        [HttpGet, Route("GetNotifications/{accountId}")]
        public async Task<List<UserNotification>> GetNotifications(string accountId, [FromUri] string type = null,
            [FromUri] int start = 0, [FromUri] int take = 10)
        {
            var rs = new List<UserNotification>();
            try
            {
                if (!string.IsNullOrEmpty(accountId))
                    rs = notificationBusinessLogic.GetNotifications(accountId, type, start, take);
            }
            catch
            {
                return null;
            }

            return rs;
        }

        [HttpPost, Route("MarkRead/{notifId}")]
        public HttpResponseMessage MarkRead(string notifId)
        {
            if (string.IsNullOrEmpty(notifId))
                return Request.CreateApiErrorResponse("Missing notification ID", HttpStatusCode.BadRequest);
            try
            {
                notificationBusinessLogic.MarkRead(notifId);
            }
            catch
            {
                return Request.CreateApiErrorResponse("Invalid notification", HttpStatusCode.BadRequest);
            }

            return Request.CreateSuccessResponse(new
            {
                notificationId = notifId,
                isRead = true
            }, "Notification marked read");
        }
    }
}