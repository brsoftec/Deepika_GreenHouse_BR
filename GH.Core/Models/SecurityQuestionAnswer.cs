using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GH.Core.Models
{
    public class SecurityQuestionAnswer
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public ObjectId QuestionId { get; set; }
        public string Answer { get; set; }
    }
}