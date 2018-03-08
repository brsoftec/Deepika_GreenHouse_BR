using System;
using System.Collections.Generic;
using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.UserCreatedBusiness;
using GH.Core.Extensions;
using GH.Core.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using GH.Core.ViewModels;
using GH.Web.Helpers;
using NLog;

namespace GH.Core.Services
{
    public class UserCreatedBusinessService : IUserCreatedBusinessService
    {
        private const string APPROVED = "approved";
        private const string PENDING = "pending";

        private IMongoCollection<UserCreatedBusiness> UcbCollection;

        private MongoRepository<UserCreatedBusiness> _repository;

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public UserCreatedBusinessService()
        {
            var db = MongoContext.Db;
            UcbCollection = db.UserCreatedBusinesses;
            _repository = new MongoRepository<UserCreatedBusiness>();
        }

        public UserCreatedBusiness CreateUcb(UserCreatedBusiness ucb)
        {
            ucb.Id = ObjectId.GenerateNewId();

            _repository.Add(ucb);

            Log.Debug("Inserted user created business " + ucb.Id.ToString());

            return ucb;
        }

        public UserCreatedBusiness UpdateUcb(UserCreatedBusiness ucb)
        {
            var filter = Builders<UserCreatedBusiness>.Filter.Eq(x => x.Id, ucb.Id);

            UcbCollection.ReplaceOne(filter, ucb);

            return ucb;
        }

        public UserCreatedBusiness GetUcbById(string id)
        {
            try
            {
                var Id = new ObjectId(id);
                var result = _repository.Single(Id);
                return result;
            }
            catch (Exception ex)
            {
                return null;
//                throw new CustomException(ex.Message);
            }
        }

        public List<UserCreatedBusiness> SearchUcb(string keyword="", string status = "", int start = 0, int length = 5)
        {
            var filter = Builders<UserCreatedBusiness>.Filter.Empty;

            if (!string.IsNullOrEmpty(status))
            {
                filter = Builders<UserCreatedBusiness>.Filter.Eq(x => x.Status, status);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                filter = filter & Builders<UserCreatedBusiness>.Filter.Regex(x => x.Name, new BsonRegularExpression(keyword, "i"));
            }

            List<UserCreatedBusiness> results = UcbCollection.Find(filter)
                .SortBy(u => u.Name)
                .Skip(start)
                .Limit(length)
                .ToList();

            return results;
        }
        
        
        public class UcbClaim
        {
            public string id { get; set; }
            public string ucbId { get; set; }
            public string ucbName { get; set; }
            public string name { get; set; }
            public string phone { get; set; }
            public string email { get; set; }
            public string message{ get; set; }
        }


        public FuncResult ClaimUcb(UcbClaim claim)

        {
            var baseUrl = Util.UrlHelper.GetCurrentBaseUrl();
            var subject = $"Claim request for business {claim.ucbName}";
            
            
//            var toEmail = claim.email;
            var toEmail = "support@regit.today";
            if (string.IsNullOrEmpty(toEmail)) return new FuncResult(false, "ucb.claim.notsent");

            var emailModel = new BusinessClaimEmailViewModel
            {
                subject = subject,
                ucbName = claim.ucbName,
                ucbId = claim.ucbId,
                message = claim.message,
                claimerName = claim.name,
                claimerPhone = claim.phone,
                claimerEmail = claim.email
            };


            string emailContent =
                ViewUtils.RenderPartialViewToString("_EmailTemplate_BusinessClaim", emailModel);

            IMailService mailService = new MailService();
            mailService.SendMailAsync(new NotificationContent
            {
                Title = "Regit Admin Notification - " + subject,
                Body = string.Format(emailContent, ""),
                SendTo = new[] {toEmail}
            });
            return new FuncResult(true, "ucb.claim.sent");
        }

        public void ChangeStatus(string id, string status)
        {
            var filter = Builders<UserCreatedBusiness>.Filter.Eq(x=>x.Id,ObjectId.Parse(id));

            var update = Builders<UserCreatedBusiness>.Update.Set(s => s.Status, status);

            UcbCollection.UpdateOne(filter, update);
        }

        public void Delete(string id)
        {
            UcbCollection.DeleteOne(Builders<UserCreatedBusiness>.Filter.Eq(x=>x.Id, ObjectId.Parse(id)));
        }

    }
}