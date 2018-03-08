using GH.Core.Exceptions;
using GH.Core.Extensions;
using GH.Core.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.Services
{
    public class SecurityQuestionService : ISecurityQuestionService
    {
        IMongoCollection<SecurityQuestion> _securityQuestionCollection;

        public SecurityQuestionService()
        {
            var db = MongoContext.Db;
            _securityQuestionCollection = db.SecurityQuestions;
        }
        
        public SecurityQuestion AddSecurityQuestion(SecurityQuestion question)
        {
            var dupCode = GetByCode(question.Code);
            if (dupCode != null)
            {
                throw new CustomException(new ErrorViewModel { Message = "New question duplicate code with exist questions" });
            }
            _securityQuestionCollection.InsertOne(question);
            return question;
        }

        public SecurityQuestion UpdateSecurityQuestion(SecurityQuestion question)
        {
            var exist = GetByCode(question.Code);
            if (exist == null)
            {
                throw new CustomException(new ErrorViewModel { Message = "Question does not exist" });
            }
            exist.Question = question.Question;
            _securityQuestionCollection.ReplaceOne(Builders<SecurityQuestion>.Filter.Where(q => q.Id == exist.Id), exist);

            return exist;
        }

        public SecurityQuestion GetById(ObjectId id)
        {
            return _securityQuestionCollection.Find(Builders<SecurityQuestion>.Filter.Where(q => q.Id == id)).FirstOrDefault();
        }

        public SecurityQuestion GetByCode(string code)
        {
            return _securityQuestionCollection.Find(Builders<SecurityQuestion>.Filter.Where(q => q.Code == code)).FirstOrDefault();
        }

        public IList<SecurityQuestion> GetAll()
        {
            return _securityQuestionCollection.Find(new BsonDocument()).ToList();
        }

    }
}