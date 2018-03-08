using System;
using MongoDB.Bson;

namespace GH.Core.Interfaces
{
    public interface IMongoEntity
    {
        ObjectId Id { get; set; }
        DateTime CreatedOn { get; set; }
        DateTime ModifiedOn { get; set; }
        ObjectId CreatedBy { get; set; }
        ObjectId ModifiedBy { get; set; }
    }
}
