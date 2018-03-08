using GH.Core.ViewModels;
using MongoDB.Bson;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.Adapters
{
    public class InteractionAdapter
    {
        public static BusinessInteractionViewModel BsonToInteractionViewModel(BsonDocument intdoc)
        {
            var interaction = new BusinessInteractionViewModel();
            BsonDocument cp = intdoc["campaign"].AsBsonDocument;
            try
            {
                interaction.id = intdoc["_id"].ToString();
                interaction.created = new DateTime(1970, 1, 1).AddSeconds(new ObjectId(interaction.id).Timestamp).ToUniversalTime();
                interaction.status = cp.GetValue("status", new BsonString("Saved")).AsString;
                interaction.name = cp.GetValue("name", new BsonString(String.Empty)).AsString;
                interaction.type = cp.GetValue("type", new BsonString("Registration")).AsString.ToLower();
                if (interaction.type == "advertising")
                {
                    interaction.type = "broadcast";
                }

                interaction.description = cp.GetValue("description", BsonString.Empty).AsString;

                interaction.image = cp.GetValue("image", BsonString.Empty).AsString;
                interaction.targetUrl = cp.GetValue("targetLink", BsonString.Empty).AsString;
                interaction.termsType = cp.GetValue("termsType", new BsonString("file")).AsString;
                interaction.termsUrl = cp.GetValue("termsUrl", BsonString.Empty).AsString;

                if (string.IsNullOrEmpty(interaction.termsUrl))
                {
                    interaction.termsUrl = cp.GetValue("termsAndConditionsFile", BsonString.Empty).AsString;
                }
                interaction.paid = cp.GetValue("paid", BsonNull.Value).AsNullableBoolean;
                if (interaction.paid == null)
                {
                    string pay = cp.GetValue("usercodetype", "Free").AsString;
                    interaction.paid = pay == "Pay";
                }
                if (interaction.paid == true)
                {
                    string usercode = cp.GetValue("price", BsonString.Empty).AsString;
                    if (string.IsNullOrEmpty(usercode))
                    {
                        usercode = cp.GetValue("usercode", new BsonString("0")).AsString;
                    }
                    interaction.price = Decimal.Parse(usercode);
                    interaction.priceCurrency = cp.GetValue("priceCurrency", BsonString.Empty).AsString;
                    if (string.IsNullOrEmpty(interaction.priceCurrency))
                    {
                        interaction.priceCurrency = cp.GetValue("usercodecurrentcy", new BsonString("USD")).AsString;
                    }
                }
                else
                {
                    interaction.price = 0;
                }

                interaction.verb = cp.GetValue("verb", BsonString.Empty).AsString;
                interaction.boost = cp.GetValue("boostAdvertising", BsonString.Empty).AsString;
                if (string.IsNullOrEmpty(interaction.boost))
                {
                    interaction.boost = cp.GetValue("boost", BsonString.Empty).AsString;
                }
                interaction.distribute = cp.GetValue("distribute", "public").AsString;
                interaction.socialShare = cp.GetValue("socialShare", null).AsString ?? "all";

                interaction.notes = cp.GetValue("notes", BsonString.Empty).AsString;

                if (interaction.type == "event")
                {
                    BsonDocument e = cp["event"].AsBsonDocument;
                    var enddate = e.GetValue("enddate", null);
                    var endtime = e.GetValue("endtime", null);
                    var theme = e.GetValue("theme", null);
                    interaction.eventInfo = new InteractionEventViewModel
                    {
                        fromDate = e.GetValue("startdate", BsonString.Empty).AsString,
                        fromTime = e.GetValue("starttime", BsonString.Empty).AsString,
                        toDate = enddate == BsonNull.Value ? null : enddate.AsString,
                        toTime = endtime == BsonNull.Value ? null : endtime.AsString,
                        location = e.GetValue("location", BsonString.Empty).AsString,
                        theme = theme == BsonNull.Value ? null : theme.AsString
                    };
                }

                BsonDocument cr = cp["criteria"].AsBsonDocument;

                interaction.target = cp.GetValue("target", new BsonString("criteria")).AsString;
                if (interaction.target == "criteria")
                {
                    string ageType = cr["age"].AsBsonDocument.GetValue("type", BsonString.Empty).AsString;
                    if (string.IsNullOrEmpty(ageType))
                    {
                        ageType = "range";
                    }
                    int ageMin = 1;
                    BsonValue ageMinStr = cr["age"].AsBsonDocument.GetValue("min", BsonNull.Value);
                    if (ageMinStr.IsString)

                    {
                        ageMin = Int32.Parse(ageMinStr.AsString);
                    }
                    else
                    {
                        ageMin = cr["age"].AsBsonDocument.GetValue("min", new BsonInt32(1)).AsInt32;
                    }


                    int ageMax = 100;
                    BsonValue ageMaxStr = cr["age"].AsBsonDocument.GetValue("max", BsonString.Empty);
                    if (ageMinStr.IsString)
                    {
                        ageMax = Int32.Parse(ageMaxStr.AsString);
                    }
                    else
                    {
                        ageMax = cr["age"].AsBsonDocument.GetValue("max", new BsonInt32(100)).AsInt32;
                    }


                    string locationType = cr["location"].AsBsonDocument.GetValue("type", BsonString.Empty).AsString;
                    if (string.IsNullOrEmpty(locationType))
                    {
                        locationType = cr.GetValue("locationtype", "all").AsString;
                        if (locationType == "Global")
                        {
                            locationType = "all";
                        }
                    }
                    interaction.criteria = new InteractionCriteria
                    {
                        gender = cr.GetValue("gender", new BsonString("All")).AsString,
                        location = new InteractionLocationCriteria
                        {
                            type = locationType,
                            country = cr["location"].AsBsonDocument.GetValue("country", BsonString.Empty).AsString,
                            area = cr["location"].AsBsonDocument.GetValue("area", BsonString.Empty).AsString
                        },
                        age = new InteractionAgeCriteria
                        {
                            type = ageType,
                            min = ageMin,
                            max = ageMax
                        }
                    };
                }
                else
                {
                    interaction.criteria = null;
                }


                BsonDocument timing = cr["spend"].AsBsonDocument;
                try
                {
                    string fr = timing.GetValue("effectiveDate", BsonString.Empty).AsString;
                    interaction.from = Convert.ToDateTime(fr);
                }
                catch (Exception e)
                {
                   
                }

                interaction.indefinite = cp.GetValue("indefinite", BsonNull.Value)?.AsNullableBoolean;

                if (interaction.indefinite == null)
                {
                    string indef = timing.GetValue("type", new BsonString("Daily")).AsString;
                    interaction.indefinite = indef == "Daily";
                }
                if (interaction.indefinite == true)
                {
                }
                else
                {
                    try
                    {
                        string to = timing.GetValue("endDate", BsonString.Empty).AsString;
                        interaction.until = Convert.ToDateTime(to);
                    }
                    catch
                    {
                        interaction.until = null;
                    }
                    //                    until = timing.GetValue("endDate", BsonNull.Value)?.AsBsonDateTime.ToNullableUniversalTime();
                }
                interaction.notes = cp.GetValue("notes", BsonString.Empty).AsString;


                var cml = cp.GetValue("comments", new BsonArray()).AsBsonArray.ToArray();
                if (cml.Length > 0)
                {
                    interaction.comments = new List<InteractionComment>();
                    foreach (var cm in cml)
                    {
                        var c = cm.AsBsonDocument;
                        interaction.comments.Add(new InteractionComment
                        {
                            type = c.GetValue("type", BsonString.Empty).AsString,
                            category = c.GetValue("category", BsonString.Empty).AsString,
                            creatorId = c.GetValue("creatorId", BsonString.Empty).AsString,
                            text = c.GetValue("text", BsonString.Empty).AsString
                        });
                    }
                }

                interaction.fieldsJson = cp.GetValue("fields", new BsonArray()).AsBsonArray.ToJson();
                interaction.groupsJson = cp.GetValue("groups", new BsonArray()).AsBsonArray.ToJson();

                interaction.participants = cp.GetValue("participants", BsonString.Empty).AsString;
            }
            catch (Exception e)
            {
             
            }
            return interaction;
        }
    }
}