using GH.Core.Models;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace GH.Core.ViewModels
{
    public class SocialPostViewModel
    {
        public SocialPostViewModel()
        {
            Photos = new List<Photo>();
            PostShared = new PostShared();
        }
        public string Id { get; set; }
        public string SocialId { get; set; }
        public AccountViewModel Account { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public PostPrivacyType Privacy { get; set; }
        //[JsonConverter(typeof(StringEnumConverter))]
        public List<SocialType> Types { get; set; }
        public string Message { get; set; }
        public List<Photo> Photos { get; set; }
        public bool IsLiked { get; set; }
        public bool IsCanDeleted { get; set; }
        public int TotalLike { get; set; }
        public int TotalComment { get; set; }
        public int TotalShares { get; set; }
        public string VideoUrl { get; set; }
        public string RefObjectId { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsSharePost { get; set; }
        public PostShared PostShared { get; set; }
    }

    public class CommentViewModel
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string SocialId { get; set; }
        public bool IsUserInSystem { get; set; }
        public AccountViewModel Account { get; set; }
        public SocialAccountViewModel SocialAccount { get; set; }
        public string Message { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class SocialAccountViewModel
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string PhotoUrl { get; set; }
        public string Link { get; set; }
    }

    public class PostSocialNetworkViewModel
    {
        public SocialPost Post { get; set; }
        public SocialType Type { get; set; }
        public string PageId { get; set; }
        public bool IsPostToFacebookPage { get; set; }
    }

    public class PostShared
    {
        public PostShared()
        {
            Photos = new List<Photo>();
        }
        public string Id { get; set; }
        public string SocialId { get; set; }
        public AccountViewModel Account { get; set; }
        public string Message { get; set; }
        public string VideoUrl { get; set; }
        public List<Photo> Photos { get; set; }
    }
}