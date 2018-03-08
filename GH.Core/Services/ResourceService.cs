using System;
using System.Collections.Generic;
using System.Linq;
using GH.Core.BlueCode.DataAccess;
using MongoDB.Bson;
using MongoDB.Driver;
using GH.Core.Models;
using GH.Core.Exceptions;
using NLog;
using GH.Core.BlueCode.BusinessLogic;

namespace GH.Core.Services
{
    public class ResourceService : IResourceService
    {
        static IMongoCollection<Resource> _resourceCollection =
            MongoDBConnection.Database.GetCollection<Resource>("Resources");

        static readonly List<Resource> _resources = _resourceCollection.Find(_ => true).ToList();
        static Dictionary<string, Resource> _resourcesPerPath;
        private static Dictionary<string,SubscriptionPlan> _subscriptionPlans;

        static Logger _log = LogManager.GetCurrentClassLogger();

        public ResourceService()
        {
//            var db = MongoContext.Db;
//            _resourceCollection = db.Resources;

            if (_resourcesPerPath == null)
            {
                _resourcesPerPath = new Dictionary<string, Resource>();

                foreach (Resource resource in _resources)
                {
                    if (!String.IsNullOrEmpty(resource.Path))
                    {
                        _resourcesPerPath.Add(resource.Path, resource);
                    }
                    if (resource.Paths != null)
                    {
                        foreach (string path in resource.Paths)
                        {
                            _resourcesPerPath.Add(path, resource);
                        }
                    }
                }
            }
            if (_subscriptionPlans == null)
            {
                _subscriptionPlans = GetSubscriptionPlans();
            }
        }

        public List<Resource> GetAllResources()
        {
            return _resources;
        }

        public Dictionary<string, Resource> GetResourcesPerPath()
        {
            return _resourcesPerPath;
        }

        public List<Resource> GetResources(string type = null)
        {
            List<Resource> resources;

            if (type == null)
            {
                resources = _resources;
            }
            else
            {
                type = type.ToLower();
                resources = _resourceCollection.Find(
                        r => r.Type == type)
                    .ToList();
            }

            if (resources == null)
            {
                throw new CustomException("Error loading resources");
            }

            return resources;
        }

        public Resource GetResource(string id = null)
        {
            Resource resource = _resourceCollection
                .Find(r => r.Id == id).FirstOrDefault();

            return resource;
        }

        public class SubscriptionPlan
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int? Interactions { get; set; }
            public int? HandshakeRelationships { get; set; }
            public int? BusinessMembers { get; set; }

        }
        public class Subscription
        {
            public string Id { get; set; }
            public string AccountId { get; set; }
            public string PlanName { get; set; }
            public SubscriptionPlan Plan { get; set; }

        }

        internal Dictionary<string,SubscriptionPlan> GetSubscriptionPlans()

        {
            Dictionary<string,SubscriptionPlan> subscriptionPlans = new Dictionary<string,SubscriptionPlan>();
            
            var settingsCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Settings");
            var filter = Builders<BsonDocument>.Filter.Eq("key", "PaymentPlan");
            var paymentPlanTemplate = settingsCollection.Find(filter).FirstOrDefault();
            var plans = paymentPlanTemplate["value"].AsBsonDocument.ToBsonDocument()["Data"].AsBsonArray;

            foreach (var plan in plans)
            {
                decimal price = 0;
                Decimal.TryParse(plan["price"].AsString, out price);
                string name = plan["name"].AsString;
  
                subscriptionPlans.Add(name, new SubscriptionPlan
                {
                    Name = name,
                    Price = price,
                    Interactions = GetQuotaCount(plan["quota"]["communication"].AsString),
                    HandshakeRelationships = GetQuotaCount(plan["quota"]["syncRelationships"].AsString),
                    BusinessMembers = GetQuotaCount(plan["quota"]["businessUsers"].AsString)
                });

            }

            return subscriptionPlans;

        }

        internal int? GetQuotaCount(string str)
        {
            if (str == "unlimited") return null;
            int count = 0;
            int.TryParse(str, out count);
            return count;
        }
        
        public Subscription GetSubscriptionByAccountId(string accountId)
        {
            var subcriptionCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Subcription");
            var criteria = Builders<BsonDocument>.Filter.Eq("userId", accountId);
            var rs = new Subscription();
            var subscription = subcriptionCollection.Find(criteria).FirstOrDefault();
            if (subscription ==null)
            {
                var _subscription = new SubcriptionLogic();
                var subId =  _subscription.InsertSubcription(accountId);
                subscription = _subscription.GetSubcriptionById(subId);
            }
            try
            {
                var planName = subscription["subcription"]["CurrentPlan"].AsString;
                rs.Id = subscription["_id"].AsString;
                rs.AccountId = subscription["userId"].AsString;
                rs.PlanName = planName;
                rs.Plan = _subscriptionPlans[planName];
            }
            catch { }
            return rs;
        }
    }
}