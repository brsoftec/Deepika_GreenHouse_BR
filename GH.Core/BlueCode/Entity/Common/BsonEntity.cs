using MongoDB.Bson;

namespace GH.Core.BlueCode.Entity.Common
{
    public class BsonEntity : BsonDocument, IMongoDBEntity
    {
        public ObjectId Id
        {
            get; set;
        }
    }
}
