using GH.Core.Exceptions;
using GH.Core.Extensions;
using GH.Core.Models;
using GH.Core.ViewModels;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Web;

namespace GH.Core.Services
{
    public class VerifyTokenService : IVerifyTokenService
    {
        public const int MAX_TRY = 20;
        public const int EXPIRED_MINUTES = 10;

        private IMongoCollection<VerifyToken> _verifyTokenCollection;

        public VerifyTokenService()
        {
            var db = MongoContext.Db;
            _verifyTokenCollection = db.VerifyTokens;
        }

        public VerifyToken GetById(ObjectId id)
        {
            return _verifyTokenCollection.Find(v => v.Id == id).FirstOrDefault();
        }

        public VerifyToken GetByRequestId(string requestId)
        {
            return _verifyTokenCollection.Find(v => v.RequestId == requestId).FirstOrDefault();
        }

        public VerifyToken Generate(VerifyPurpose purpose, string phoneNumber, string email = null, int length = 4)
        {
            VerifyToken verification = new VerifyToken();
            var now = DateTime.Now;
            verification.CreatedAt = now;
            verification.ExpiredTime = now.AddMinutes(VerifyTokenService.EXPIRED_MINUTES);
            verification.Email = email;
            verification.PhoneNumber = phoneNumber;
            verification.Purpose = purpose;
            verification.Status = TokenStatus.CREATED;

            var max = 1;
            for (int i = 0; i < length; i++)
            {
                max = max * 10;
            }

            verification.Token = new Random().Next(0, max).ToString("D4");

            verification.RequestId = Guid.NewGuid().ToString();

            _verifyTokenCollection.InsertOne(verification);

            return verification;
        }

        public VerifyToken RefreshExpiredTime(ObjectId id)
        {
            var verification = _verifyTokenCollection.Find(t => t.Id == id).FirstOrDefault();
            if (verification == null)
            {
                throw new CustomException("Verification does not exist");
            }
            verification.RefreshExpiredTimeNumber++;
            verification.ExpiredTime = DateTime.Now.AddMinutes(VerifyTokenService.EXPIRED_MINUTES);

            _verifyTokenCollection.UpdateOne(t => t.Id == id,
                Builders<VerifyToken>.Update.Set(t => t.ExpiredTime, verification.ExpiredTime)
                    .Set(t => t.RefreshExpiredTimeNumber, verification.RefreshExpiredTimeNumber));

            return verification;
        }

        public bool IsPending(VerifyToken verification)
        {
            if ((verification.Status == TokenStatus.CREATED || verification.Status == TokenStatus.SENT) &&
                verification.ExpiredTime > DateTime.Now)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public VerifyToken Verify(string requestId, string token)
        {
            var verification = _verifyTokenCollection.Find(t => t.RequestId == requestId).FirstOrDefault();
            if (verification == null)
            {
                throw new CustomException("RequestId does not match with any verification");
            }

            switch (verification.Status)
            {
                case TokenStatus.VERIFIED:
                    throw new CustomException("The verification was checked once time");
                case TokenStatus.CANCELED:
                    throw new CustomException("The verification has been canceled");
                case TokenStatus.EXPIRED:
                    throw new CustomException("The verification has been expired");
                default:
                    break;
            }

            bool valid = false;
            if (verification.Token != token)
            {
                verification.TryNumber++;
                if (verification.TryNumber == VerifyTokenService.MAX_TRY)
                {
                    verification.Status = TokenStatus.CANCELED;
                }
            }
            else if (verification.ExpiredTime < DateTime.Now)
            {
                verification.Status = TokenStatus.EXPIRED;
            }
            else
            {
                valid = true;
                verification.Status = TokenStatus.VERIFIED;
                verification.VerifiedAt = DateTime.Now;
            }

            _verifyTokenCollection.UpdateOne(f => f.Id == verification.Id, Builders<VerifyToken>.Update
                .Set(v => v.Status, verification.Status)
                .Set(v => v.TryNumber, verification.TryNumber)
                .Set(v => v.VerifiedAt, verification.VerifiedAt));

            if (!valid)
            {
                if (verification.Status == TokenStatus.EXPIRED)
                {
                    throw new CustomException("The PIN has expired");
                }
                else if (verification.Status == TokenStatus.CANCELED)
                {
                    throw new CustomException(
                        "You have tried verifying unsuccessful for 3 times. The verification has been canceled");
                }
                else
                {
                    throw new CustomException("Your verification token is incorrect");
                }
            }

            return verification;
        }

        public class TokenCheckResult
        {
            public TokenStatus Status { get; set; }
            public VerifyToken Token { get; set; }
        }

        public TokenCheckResult Check(string requestId, string token)
        {
            var verification = _verifyTokenCollection.Find(t => t.RequestId == requestId).FirstOrDefault();
            if (verification == null)
            {
                return new TokenCheckResult {Status = TokenStatus.INVALID};
            }

            if (verification.Status != TokenStatus.SENT)
            {
                if (verification.Status == TokenStatus.VERIFIED)
                    verification.Status = TokenStatus.FINISHED;
                return new TokenCheckResult {Status = verification.Status};
            }

            bool valid = false;
            if (verification.Token != token)
            {
                verification.TryNumber++;
                if (verification.TryNumber == VerifyTokenService.MAX_TRY)
                {
                    verification.Status = TokenStatus.CANCELED;
                }
            }
            else if (verification.ExpiredTime < DateTime.Now)
            {
                verification.Status = TokenStatus.EXPIRED;
            }
            else
            {
                valid = true;
                verification.Status = TokenStatus.VERIFIED;
                verification.VerifiedAt = DateTime.Now;
            }

            _verifyTokenCollection.UpdateOne(f => f.Id == verification.Id, Builders<VerifyToken>.Update
                .Set(v => v.Status, verification.Status)
                .Set(v => v.TryNumber, verification.TryNumber)
                .Set(v => v.VerifiedAt, verification.VerifiedAt));

            if (!valid)
            {
                if (verification.Status == TokenStatus.EXPIRED)
                {
                    return new TokenCheckResult {Status = verification.Status};
                } else if (verification.Status == TokenStatus.CANCELED)
                {
                    return new TokenCheckResult {Status = verification.Status};
                } else
                {
                    return new TokenCheckResult {Status = TokenStatus.FAILED};
                }
            }

            return new TokenCheckResult
            {
                Status = verification.Status,
                Token = verification
            };
        }

        public void MarkVerificationAsSent(ObjectId id)
        {
            _verifyTokenCollection.UpdateOne(t => t.Id == id,
                Builders<VerifyToken>.Update.Set(t => t.Status, TokenStatus.SENT));
        }

        #region otp

        public VerifyToken GenerateOTP(VerifyPurpose purpose, string phoneNumber, string email, string requestId = null,
            int length = 4)
        {
            VerifyToken verification = new VerifyToken();
            verification.RequestId = Guid.NewGuid().ToString();
            if (!string.IsNullOrEmpty(requestId))
            {
                var existOTP = _verifyTokenCollection.Find(t => t.RequestId == requestId).FirstOrDefault();
                if (existOTP != null)
                {
                    verification.RequestId = requestId;
                    _verifyTokenCollection.DeleteOne(t => t.RequestId == requestId);
                }
            }
            var now = DateTime.Now;
            verification.CreatedAt = now;
            verification.ExpiredTime = now.AddMinutes(VerifyTokenService.EXPIRED_MINUTES);
            verification.Email = email;
            verification.PhoneNumber = phoneNumber;
            verification.Purpose = purpose;
            verification.Status = TokenStatus.CREATED;
            var max = 1;
            for (int i = 0; i < length; i++)
            {
                max = max * 10;
            }

            verification.Token = new Random().Next(0, max).ToString("D4");
            _verifyTokenCollection.InsertOne(verification);

            return verification;
        }

        public VerifyOTPViewModel VerifyOTP(string requestId, string token)
        {
            var verifyOTPViewModel = new VerifyOTPViewModel();
            verifyOTPViewModel.RequestId = requestId;
            verifyOTPViewModel.Code = token;

            var verification = _verifyTokenCollection.Find(t => t.RequestId == requestId).FirstOrDefault();
            if (verification == null)
            {
                verifyOTPViewModel.Status = "RequestId does not match with any verification";
            }

            bool valid = false;
            if (verification.Token != token)
            {
                verifyOTPViewModel.Status = "Incorrect PIN. Please re-enter.";
                verification.TryNumber++;
                if (verification.TryNumber > 5)
                {
                    verifyOTPViewModel.Status = "The PIN has expired";
                }
                _verifyTokenCollection.UpdateOne(f => f.Id == verification.Id,
                    Builders<VerifyToken>.Update.Set(v => v.TryNumber, verification.TryNumber));
            }
            else if (verification.ExpiredTime < DateTime.Now)
            {
                verifyOTPViewModel.Status = "The PIN has expired";
            }
            else
            {
                valid = true;
                _verifyTokenCollection.DeleteOne(t => t.RequestId == requestId);
            }
            verifyOTPViewModel.Verify = valid;

            return verifyOTPViewModel;
        }

        #endregion otp
    }
}