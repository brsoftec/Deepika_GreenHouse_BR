using GH.Core.Models;
using GH.Core.ViewModels;
using MongoDB.Bson;

namespace GH.Core.Services
{
    public interface IVerifyTokenService
    {
        VerifyToken GetById(ObjectId id);
        VerifyToken GetByRequestId(string requestId);
        VerifyToken Generate(VerifyPurpose purpose, string phoneNumber, string email, int length = 4);
        VerifyToken RefreshExpiredTime(ObjectId id);
        bool IsPending(VerifyToken verification);
        VerifyToken Verify(string requestId, string token);
        VerifyTokenService.TokenCheckResult Check(string requestId, string token);
        void MarkVerificationAsSent(ObjectId id);
        VerifyToken GenerateOTP(VerifyPurpose purpose, string phoneNumber, string email, string requestId = null, int length = 4);
        VerifyOTPViewModel VerifyOTP(string requestId, string token);
    }
}