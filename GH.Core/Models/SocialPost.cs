using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GH.Core.Models
{
    public class SocialPost : BaseObject
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string SocialId { get; set; }
        public ObjectId AccountId { get; set; }
        public string RawData { get; set; }

        [BsonElementAttribute("Privacy")]
        public PostPrivacy Privacy { get; set; }

        [BsonElementAttribute("Type")]
        public SocialType Type { get; set; }

        public string Message { get; set; }

        [BsonElementAttribute("Photo")]
        public IList<Photo> Photos { get; set; }

        [BsonElementAttribute("Like")]
        public Like Like { get; set; }

        public int Shares { get; set; }

        [BsonElementAttribute("Comment")]
        public IList<Comment> Comments { get; set; }
        public string VideoUrl { get; set; }
        public ObjectId RefObjectId { get; set; }


        [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.String)]
        public DateTime? PullAt { get; set; }
        public string SocialPageId { get; set; }
        public string GroupId { get; set; }

        public SocialPost()
        {
            Like = new Like();
            Photos = new List<Photo>();
            Comments = new List<Comment>();
        }
        public SocialPost(string message, 
            ObjectId accountId, 
            PostPrivacy privacy, 
            SocialType type, 
            DateTime createdOn, 
            ObjectId createdBy, 
            DateTime modifiedOn, 
            ObjectId modifiedBy, 
            string socialId = "", 
            string videoUrl = "", 
            string refObjecId = "",
            string groupId = null)
        {
            Message = message;
            AccountId = accountId;
            Privacy = privacy;
            SocialId = socialId;
            Type = type;
            VideoUrl = videoUrl;
            RefObjectId = RefObjectId;
            CreatedOn = createdOn;
            CreatedBy = createdBy;
            ModifiedOn = modifiedOn;
            ModifiedBy = modifiedBy;
            GroupId = groupId;

            Like = new Like();
            Photos = new List<Photo>();
            Comments = new List<Comment>();
        }
    }

    public class Photo
    {
        public string Url { get; set; }
    }

    public class Like
    {
        public Like()
        {
            RegitLikes = new List<ClassObjectId>();
            FaceBookLikes = new List<SocialAccount>();
        }
        public int TwitterLikeCount { get; set; }
        public IList<ClassObjectId> RegitLikes { get; set; }
        //List userId đã like post bao gồm cả user co tài khoản regit nhưng like trên social => tổng số like được tính trong SocialLikeCount nên UserId không có trong RegitLikeIds
        [BsonElementAttribute("SocialLikes")]
        public IList<SocialAccount> FaceBookLikes { get; set; }
        public int TotalLikes
        {
            get
            {
                return (FaceBookLikes.Count() + RegitLikes.Count + TwitterLikeCount);
            }
        }
    }

    public class ClassObjectId
    {
        public ObjectId Id { get; set; }
    }

    public class Comment
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public ObjectId UserId { get; set; }
        public string SocialId { get; set; }
        public string Message { get; set; }
        public DateTime CreatedOn { get; set; }
        [BsonElementAttribute("From")]
        public SocialAccount From { get; set; }

        public Comment()
        {
            From = new SocialAccount();
        }
    }

    public class SocialAccount
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string PhotoUrl { get; set; }
        public string Link { get; set; }
    }

    public class PostPrivacy
    {
        public PostPrivacyType Type { get; set; }
        public ObjectId GroupId { get; set; }
    }

    public enum SocialType
    {
        Facebook, Twitter, GreenHouse
    }

    public enum PostPrivacyType
    {
        Public, Friends, Private
    }
}