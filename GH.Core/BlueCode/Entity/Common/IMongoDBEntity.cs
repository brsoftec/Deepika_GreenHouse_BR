using MongoDB.Bson;

namespace GH.Core.BlueCode.Entity.Common
{
    public interface IMongoDBEntity
    {
        ObjectId Id { get; set; }
    }
}
