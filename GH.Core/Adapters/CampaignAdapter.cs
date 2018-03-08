using GH.Core.Extensions;
using GH.Core.ViewModels;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static GH.Core.BlueCode.Entity.Campaign.Campaign;

namespace GH.Core.Adapters
{
    public class CampaignAdapter
    {
        public static CampaignDto BsonToCampaignDto(BsonDocument intdoc, string name=null)
        {
            var checkValid = true;
                var camp = new CampaignDto();
                BsonDocument cp = intdoc["campaign"].AsBsonDocument;
                camp.Id = intdoc.GHValue<string>("_id");
                camp.userId = intdoc.GHValue<string>("userId");

                camp.campaign.name = cp.GHValue<string>("name");
                camp.campaign.type = cp.GHValue<string>("type");
                camp.campaign.description = cp.GHValue<string>("description");
      
                BsonDocument cr = cp["criteria"].AsBsonDocument;
            var criteria = new Criteria();
            // spend
            BsonDocument sp = cr["spend"].AsBsonDocument;
            if(sp !=null)
            {
                var spend = new Spend();
                spend.type = sp.GetValue("type", BsonString.Empty).AsString;
                string efd = sp.GetValue("effectiveDate", BsonString.Empty).AsString;
                string edd = sp.GetValue("endDate", BsonString.Empty).AsString;
                DateTime starDay = Convert.ToDateTime(efd);
                DateTime endDay = Convert.ToDateTime(edd);
                if (starDay > DateTime.Now)
                    checkValid = false;
                if(endDay < DateTime.Now && spend.type != "Daily")
                    checkValid = false;

                spend.effectiveDate = Convert.ToDateTime(efd).ToShortDateString();
               
                spend.endDate = Convert.ToDateTime(edd).ToShortDateString();
                criteria.spend = spend;
            }

            camp.campaign.criteria = criteria;
            camp.campaign.verb = cp.GHValue<string>("verb");
            var fields = new List<Field>();
                 var lstField = cp["fields"].AsBsonArray;
                foreach(var item in lstField)
                {
                    var field = new Field();
                    var bs = item.ToBsonDocument();
                    field.id = bs.GHValue<string>("id");
                    field.label = bs.GHValue<string>("label");
                    field.displayName = bs.GHValue<string>("displayName");
                    field.jsPath = bs.GHValue<string>("jsPath");
                    field.type = bs.GHValue<string>("type");
                field.options = bs.GHValue<string>("options");
                field.optional = bs.GHValue<bool>("optional");
                if (field != null)
                    fields.Add(field);
            }
            camp.campaign.fields = fields;
            if (!checkValid)
                return null;
            return camp;
        }
    }
}