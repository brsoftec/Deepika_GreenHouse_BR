using System;
using GH.Core.Extensions;
using GH.Core.Models;
using MongoDB.Bson;

namespace GH.Core.ViewModels
{
    public class FollowTransactionParamter
    {
        public ObjectId FromUser { get; set; }
        public ObjectId ToUser { get; set; }
        public FollowType Type { get; set; }
        public DateTime Date { get; set; }
        public Account User { get; set; }
    }
}