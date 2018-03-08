using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GH.Core.BlueCode.Entity.Common;

namespace GH.Core.Models
{
    public class BusinessPost : BaseObject, IMongoDBEntity
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string SocialId { get; set; }
        public ObjectId AccountId { get; set; }
        public PostPrivacy Privacy { get; set; }
        public List<SocialType> SocialTypes { get; set; }
        public string Message { get; set; }
        public IList<Photo> Photos { get; set; }
        public string VideoUrl { get; set; }
        public ObjectId RefObjectId { get; set; }

        public PostStatus Status { get; set; }
        public ObjectId BusinessAccountId { get; set; }
        public string SocialPageId { get; set; }

        public List<PostStateFlow> Workflows { get; set; }

        public BusinessPost()
        {
            Photos = new List<Photo>();
            Workflows = new List<PostStateFlow>();
        }

        public BusinessPost(string message,
            ObjectId accountId,
            ObjectId businessAccountId,
            string socialPageId,
            PostPrivacy privacy,
            List<SocialType> types,
            DateTime createdOn,
            ObjectId createdBy,
            DateTime modifiedOn,
            ObjectId modifiedBy,
            List<Photo> photos,
            PostStatus _status = PostStatus.Draft,
            string socialId = "",
            string videoUrl = "",
            string refObjecId = "")
        {

            Message = message;
            AccountId = accountId;
            Privacy = privacy;
            SocialId = socialId;
            SocialTypes = types;
            VideoUrl = videoUrl;
            RefObjectId = RefObjectId;
            CreatedOn = createdOn;
            CreatedBy = createdBy;
            ModifiedOn = modifiedOn;
            ModifiedBy = modifiedBy;
            Photos = photos;
            Status = _status;
            BusinessAccountId = businessAccountId;
            SocialPageId = socialPageId;
            Workflows = new List<PostStateFlow>();
        }

    }

    public class PostStateFlow
    {
        [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.String)]
        public DateTime ExecuteTime { get; set; }
        public PostStateAction Action { get; set; }
        public string Comment { get; set; }
        public ObjectId Executor { get; set; }
    }

    public enum PostStateAction
    {
        Reject,
        Repost,
        Approve
    }

    public enum PostStatus
    {
        Draft,
        Edited,
        Rejected,
        Approved,
        Posted
    }
}