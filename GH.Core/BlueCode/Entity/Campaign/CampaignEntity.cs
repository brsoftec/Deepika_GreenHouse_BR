﻿using GH.Core.BlueCode.Entity.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.BlueCode.Entity.Campaign
{

    [BsonDiscriminator("Campaign")]
    public class Campaign : IMongoDBEntity
    {

        public ObjectId Id { get; set; }
        public string userId { get; set; }
        public CampaignContent campaign { get; set; }


        public class Age
        {
            public string type { get; set; }
            public int min { get; set; }
            public int max { get; set; }
        }

        public class Location
        {
            public string type { get; set; }
            public string country { get; set; }
            public string area { get; set; }
        }

        public class Spend
        {
            public string type { get; set; }
            public string effectiveDate { get; set; }
            public string endDate { get; set; }
        }

        public class Criteria
        {
            public string gender { get; set; }
            public Age age { get; set; }
            public Location location { get; set; }
            public Spend spend { get; set; }
        }

        public class Field
        {
            public string id { get; set; }
            public string displayName { get; set; }
            public string jsPath { get; set; }
            public string path { get; set; }
            public string label { get; set; }
            public bool optional { get; set; }
            public string type { get; set; }
            public string options { get; set; }
        }

        public class Group
        {
            public string name { get; set; }
            public string displayName { get; set; }
        }

        public class CampaignContent
        {
            public string type { get; set; }
            public string status { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string termsType { get; set; }
            public string termsUrl { get; set; }
            public string target { get; set; }
            public string distribute { get; set; }
            public string socialShare { get; set; }
            public bool indefinite { get; set; }
            public Criteria criteria { get; set; }
            public bool paid { get; set; }
            public string price { get; set; }
            public string priceCurrency { get; set; }
            public string verb { get; set; }
            public string notes { get; set; }
            public string participants { get; set; }
            public string usercodetype { get; set; }
            public List<Field> fields { get; set; }
            public List<Group> groups { get; set; }
        }


    }
}
