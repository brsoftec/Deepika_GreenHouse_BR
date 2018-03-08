using GH.Core.Models;
using MongoDB.Bson;
using System.Collections.Generic;


namespace GH.Core.Services
{
    public interface ISecurityQuestionService
    {
        SecurityQuestion AddSecurityQuestion(SecurityQuestion question);
        SecurityQuestion UpdateSecurityQuestion(SecurityQuestion question);
        SecurityQuestion GetById(ObjectId id);
        SecurityQuestion GetByCode(string code);
        IList<SecurityQuestion> GetAll();
    }
}