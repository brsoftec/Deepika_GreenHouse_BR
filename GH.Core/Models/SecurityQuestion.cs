using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GH.Core.Models
{
    public class SecurityQuestion
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Code { get; set; }
        public string Question { get; set; }
    }
}