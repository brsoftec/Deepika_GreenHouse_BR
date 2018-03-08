using GH.Core.BlueCode.DataAccess;
using GH.Util;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using GH.Core.Models;
using GH.Core.BlueCode.Entity.Event;

namespace GH.Core.BlueCode.BusinessLogic
{
    public class EventBusinessLogic 
    {
        public List<EventListItem> GetEventsByUser(Account user, string eventType = "All")
        {
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Event");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("userId", user.AccountId);

            if (!string.IsNullOrEmpty(eventType) && !eventType.ToLower().Equals("all"))
            {
                criteria = criteria & filter.Eq("Event.type", eventType);
            }
            
            var totalList = campaignCollection.Find(criteria).ToEnumerable();
            var enumerable = totalList as BsonDocument[] ?? totalList.ToArray();
            var totalItems = enumerable.Count();
            string username = user.Profile.FirstName + " " + user.Profile.LastName;
            var listOfCurrentPage = enumerable.Select(c =>
            {              
                string type = c["Event"]["type"].AsString;
                string starttime = "";
                string startdate = "";
                string endtime = "";
                string enddate = "";
                string location = "";
                string theme = "";

                starttime = c["Event"]["detail"]["starttime"].AsString;
                startdate = c["Event"]["detail"]["startdate"].AsString;
                endtime = c["Event"]["detail"]["endtime"].AsString;
                enddate = c["Event"]["detail"]["enddate"].AsString;
                location = c["Event"]["detail"]["location"].AsString;
                theme = c["Event"]["detail"]["theme"].AsString;
                string timetype = c["Event"]["detail"]["timetype"].AsString;

                return new EventListItem
                {       
                    id=c["_id"].AsString,           
                    username=username,
                    startdate = startdate,
                    starttime = starttime,
                    endtime = endtime,
                    enddate = enddate,
                    location = location,
                    theme = theme,
                    timetype= timetype,
                    name = c["Event"]["name"].AsString,
                    description= c["Event"]["description"].AsString,
                    type = type,
                    note= c["Event"]["UserSettings"]["note"].AsString,
                    syncgoogle= c["Event"]["UserSettings"]["syncgoogle"].AsBoolean
                };
            });

            return listOfCurrentPage.ToList();
        }

        public BsonDocument GetEventById(string eventId)
        {
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Event");
            var criteria = Builders<BsonDocument>.Filter.Eq("_id", eventId);
            var eventobject = campaignCollection.Find(criteria).FirstOrDefault();
            return eventobject;
        }
        public BsonDocument GetEventTemplate()
        {
            var settingCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Settings");
            var filter = Builders<BsonDocument>.Filter.Eq("key", "eventTemplate");
            var campaignTemplate = settingCollection.Find(filter).FirstOrDefault();
            var advertising = campaignTemplate["value"].AsBsonDocument;
            return advertising.ToBsonDocument();
        }
      
        public void InsertEvent(string userId, BsonDocument eventContent)
        {
            try
            {
                var eventCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Event");

                var eventobject = new BsonDocument
            {
                {"_id", BsonHelper.GenerateObjectIdString() },
                {"userId", userId},
                {"Event", eventContent }
            };
                eventCollection.InsertOne(eventobject);
            }
            catch (Exception e) { }
            // Notify to Supervisor
            // TODO
        }
        public void InsertEventwithcampaign(string userId, BsonDocument eventContent,string campaignid)
        {
            try
            {
                var eventCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Event");

                var eventobject = new BsonDocument
            {
                {"_id", BsonHelper.GenerateObjectIdString() },
                 {"campaignid", campaignid },
                {"userId", userId},
                {"Event", eventContent }
            };
                eventCollection.InsertOne(eventobject);
            }
            catch (Exception e) { }
            // Notify to Supervisor
            // TODO
        }
        public void DeleteEvent(string eventId)
        {
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Event");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", eventId);
            campaignCollection.FindOneAndDelete(filter);
        }

        public void DeleteEventfromcampaign(string campaignId)
        {
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Event");
            var filter = Builders<BsonDocument>.Filter.Eq("campaignid", campaignId);
            campaignCollection.FindOneAndDelete(filter);
        }
        public void SaveEvent(string eventId, string eventJson)
        {
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Event");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", eventId);
            //campaignJson = campaignJson.Replace("\"" + campaignId + "\"", "ObjectId(\"" + campaignId + "\")");
            BsonDocument campaign = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(eventJson);
            campaignCollection.ReplaceOne(filter, campaign);
        }
       
    }

}
