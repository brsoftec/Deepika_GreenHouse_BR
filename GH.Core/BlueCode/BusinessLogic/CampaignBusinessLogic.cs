using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.Campaign;
using GH.Core.BlueCode.Entity.Common;
using GH.Util;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using GH.Core.Services;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Web;
using GH.Core.BlueCode.Entity.Post;
using GH.Core.BlueCode.Entity.Profile;
using NLog;
using GH.Core.ViewModels;
using GH.Core.BlueCode.Entity.InformationVault;
using Microsoft.AspNet.Identity;

namespace GH.Core.BlueCode.BusinessLogic
{
    public class CampaignBusinessLogic : ICampaignBusinessLogic
    {
        private readonly IBusinessMemberLogic _businessMemberService = new BusinessMemberLogic();
        private readonly MongoRepository<BusinessMember> _businessMemberRepos = new MongoRepository<BusinessMember>();
        private static Logger Log = LogManager.GetCurrentClassLogger();

        public DataList<CampaignListItem> GetCampaignByBusinessUser(string businessUserId, string campaignType = "All",
            string campaignStatus = "All", bool withoutDraft = false, int pageIndex = 1, int pageSize = 10,
            bool withoutTemplate = false,
            bool withoutSRFI = false, string keysearch = "", bool isnotcasebbusid = false,
            bool withoutPustToVault = false,
            bool withoutHandshake = false, List<string> busids = null, bool isbothPushtovaultandhandshake = false)
        {
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Campaign");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("userId", businessUserId);
            criteria = criteria & !filter.Eq("campaign.status", "Remove");
            if (busids != null && busids.Count > 0)
            {
                busids.Add(businessUserId);
                criteria = filter.In("_id", busids);
            }
            if (isnotcasebbusid)
                criteria = filter.Empty;
            if (!string.IsNullOrEmpty(campaignType) && !campaignType.ToLower().Equals("all"))
            {
                criteria = criteria & filter.Eq("campaign.type", campaignType);
            }
            if (!string.IsNullOrEmpty(campaignStatus) && !campaignStatus.ToLower().Equals("all"))
            {
                criteria = criteria & filter.Eq("campaign.status", campaignStatus);
            }

            if (withoutDraft)
            {
                criteria = criteria & filter.Ne("campaign.status", EnumCampaignStatus.Draft);
            }

            if (withoutTemplate)
            {
                criteria = criteria & filter.Ne("campaign.status", EnumCampaignStatus.Template);
            }
            if (withoutSRFI)
            {
                criteria = criteria & filter.Ne("campaign.type", EnumCampaignType.SRFI);
            }
            if (withoutHandshake)
            {
                criteria = criteria & filter.Ne("campaign.type", EnumCampaignType.HandShake);
            }
            if (isbothPushtovaultandhandshake)
            {
                criteria = criteria & (filter.Eq("campaign.type", EnumCampaignType.HandShake) |
                                       filter.Eq("campaign.type", EnumCampaignType.PushToVault));
            }
            if (withoutPustToVault)
            {
                criteria = criteria & filter.Ne("campaign.type", EnumCampaignType.PushToVault);
            }
            var criterianame = filter.Regex("campaign.name",
                BsonRegularExpression.Create(new Regex(".*" + keysearch + ".*", RegexOptions.IgnoreCase)));
            var criteriadescription = filter.Regex("campaign.description",
                BsonRegularExpression.Create(new Regex(".*" + keysearch + ".*", RegexOptions.IgnoreCase)));

            if (!string.IsNullOrEmpty(keysearch))
            {
                criteria = criteria & (criteriadescription | criterianame);
            }

            var totalList = campaignCollection.Find(criteria).ToEnumerable();

            var enumerable = totalList as BsonDocument[] ?? totalList.ToArray();

            var listOfCurrentPage = BsonDocumentToCampaignList(enumerable);
            listOfCurrentPage = listOfCurrentPage.Where(x =>
                busids == null || busids.Any(y => y != x.BusinessUserId) ||
                (busids.Any(y => y == x.BusinessUserId) && x.CampaignType == "Handshake") &&
                !x.Status.Equals(EnumCampaignStatus.Remove)).ToList();

            var totalItems = listOfCurrentPage.Count();

            try
            
            {
                var accountId = HttpContext.Current.User.Identity.GetUserId();
                foreach (CampaignListItem c in listOfCurrentPage)
                {
                    
                    c.Following = new InteractionService().IsFollowing(accountId, c.BusinessUserId);
                    var participants = c.Participants;
                    if (c.CampaignType == "Handshake")
                    {
                        participants =
                            new PostHandShakeBusinessLogic().GetCountUserhoidHandshakebycmapaignid(c.Id) +
                            new PostHandShakeBusinessLogic().GetCountUserInvitedPending(c.Id);
                    }
                    else
                    {
                        participants = new InteractionService().CountParticipants(c.Id);
                    }
                    c.Participants = participants;
                }
            }
            catch (Exception e)
            {
                Log.Debug("Error counting: " + e.Message + e.StackTrace);
            }

            var dataList = new DataList<CampaignListItem>(
                listOfCurrentPage.ToList().OrderByDescending(x => x.indexoder).Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize).ToList(), totalItems, pageIndex, pageSize);


            return dataList;
        }

        public List<CampaignListItem> BsonDocumentToCampaignList(BsonDocument[] enumerable)
        {
            var count = 0;
            var listOfCurrentPage = new List<CampaignListItem>();
            foreach (var c in enumerable)
            {
                var cp = c["campaign"].AsBsonDocument;
                var type = cp.GetValue("type","").AsString;
                var rs = new CampaignListItem();
                try
                {
                    if (type == "PushToVault")
                    {
                        try
                        {
                            rs = new CampaignListItem
                            {
                                Name = c["campaign"]["name"].AsString,
                                CampaignName = c["campaign"]["name"].AsString,
                                Type = cp.GetValue("type","").AsString,
                                CampaignType = c["campaign"]["type"].AsString,
                                Description = c["campaign"]["description"].AsString,
                                Status = c["campaign"]["status"].AsString,
                                Id = c["_id"].AsString,
                                indexoder = count
                            };
                        }
                        catch
                        {
                            Log.Error("Campaign Name: " + c["campaign"]["name"].AsString);
                        }
                    }
                    else
                    {
                        try
                        {
                            var participants = 0;
                            var countinvitedhandshake = 0;
                            var p = cp.GetValue("participants", new BsonString("0")).AsString;

                            int.TryParse(p, out participants);
                            try
                            {
                                var postlogic = new PostBusinessLogic();
                                var userList = postlogic.GetFollowersList(c["_id"].AsString);
                                if (userList.DataOfCurrentPage != null)
                                    participants = userList.DataOfCurrentPage.Count();
                            }
                            catch
                            {
                            }
                            try
                            {
                                if (type == "Handshake")
                                {
                                    participants =
                                        new PostHandShakeBusinessLogic().GetCountUserhoidHandshakebycmapaignid(
                                            c["_id"].AsString);
                                    countinvitedhandshake =
                                        new PostHandShakeBusinessLogic().GetCountUserInvitedPending(c["_id"].AsString);
                                }
                            }
                            catch (Exception ex)
                            {
                            }

                            string imageurl = cp.GetValue("image", BsonString.Empty).AsString;
                            string targetLink = cp.GetValue("targetLink", BsonString.Empty).AsString;

                            var boost = cp.GetValue("boostAdvertising", BsonString.Empty).AsString;
                            string usercodetype = "";
                            string usercode = "";
                            string usercodecurrentcy = "";

                                if (type == "Event" || type == "Registration")
                                {
                                    usercodetype = cp.GetValue("usercodetype", new BsonString("Free")).AsString;
                                    usercode = cp.GetValue("usercode", new BsonString(string.Empty)).ToString();
                                    usercodecurrentcy = cp.GetValue("usercodecurrentcy", new BsonString(string.Empty))
                                        .AsString;
                                }

                                var timing = cp["criteria"]["spend"].AsBsonDocument;
                                string timetype = timing.GetValue("type", new BsonString("Daily")).AsString;
                                string from = timing.GetValue("effectiveDate", new BsonString(string.Empty)).AsString;
                                string until = timing.GetValue("endDate", new BsonString(string.Empty)).AsString;
                                var id = c.GetValue("_id").ToString();
                            
 

                            var businessAccountId = c["userId"].AsString;
                            var userId = HttpContext.Current.User.Identity.GetUserId();
                            var account = new AccountService().GetByAccountId(userId);
                            var businessAccount = new AccountService().GetByAccountId(businessAccountId);

                            rs = new CampaignListItem()
                            {
                                indexoder = count,
                                usercodetype = usercodetype,
                                usercode = usercode,
                                usercodecurrentcy = usercodecurrentcy,
                                Id = id,
                                CampaignId = id,
                                TimeType = timetype,
                                Name = c["campaign"]["name"].AsString,
                                CampaignName = c["campaign"]["name"].AsString,
                                Type = c["campaign"]["type"].AsString,
                                CampaignType = c["campaign"]["type"].AsString,
                                BusinessId = businessAccount?.Id.ToString(),
                                BusinessUserId = c["userId"].AsString,
                                Description = c["campaign"]["description"].AsString,
                                Status = c["campaign"]["status"].AsString,
                                EnDate = until,
                                SpendEffectDate =
                                (c["campaign"]["criteria"]["spend"]["effectiveDate"] != null &&
                                 !string.IsNullOrEmpty(c["campaign"]["criteria"]["spend"]["effectiveDate"].AsString))
                                    ? Convert.ToDateTime(c["campaign"]["criteria"]["spend"]["effectiveDate"].AsString)
                                    : DateTime.Now.Date,
                                SpendEndDate =
                                (c["campaign"]["criteria"]["spend"]["endDate"] != null &&
                                 !string.IsNullOrEmpty(c["campaign"]["criteria"]["spend"]["endDate"].AsString))
                                    ? Convert.ToDateTime(c["campaign"]["criteria"]["spend"]["endDate"].AsString)
                                    : DateTime.Now.Date,
                                Image = imageurl,
                                TargetLink = targetLink,
                                SocialShare = cp.GetValue("socialShare", "all").AsString,
                                Participants = participants,
                                Verb = cp.GetValue("verb", "register").AsString,
                                Boost = boost,
                            };
                            if (type == "Event")
                            {
                                BsonDocument e = cp["event"].AsBsonDocument;
                                rs.startdate = e.GetValue("startdate", BsonString.Empty).AsString;
                                rs.starttime = e.GetValue("starttime", BsonString.Empty).AsString;
                                rs.enddate = e.GetValue("enddate", BsonString.Empty).AsString;
                                rs.endtime = e.GetValue("endtime", BsonString.Empty).AsString;
                                rs.location = e.GetValue("location", BsonString.Empty).AsString;
                                rs.theme = e.GetValue("theme", BsonString.Empty).AsString;
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Debug("Error loading interaction " + c["campaign"]["name"].AsString + ": " + e.Message + e.StackTrace);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Debug(e.Message);
                }
                if (rs != null)
                {
                    count++;
                    listOfCurrentPage.Add(rs);
                }
            }
            // end
            return listOfCurrentPage;
        }


        private FilterDefinition<BsonDocument> GetFilterVault(BsonValue bsonvaultfilter,
            FilterDefinitionBuilder<BsonDocument> filter)
        {
            FilterDefinition<BsonDocument> criteria = null;
            if (bsonvaultfilter.IsBsonArray)
                criteria = filter.AnyIn("campaign.criteria.keywords",
                    bsonvaultfilter.AsBsonArray.Select(x => x.AsString).ToArray());
            else
                criteria = filter.AnyIn("campaign.criteria.keywords", new string[] { });
            return criteria;
        }


        public long CountInteractionsUserId(string userId)
        {
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Campaign");
            var filter = Builders<BsonDocument>.Filter;

            var criteria = filter.Eq("userId", userId);

            criteria = criteria & filter.Eq("campaign.status", "Active");

            criteria = criteria & (filter.Eq("campaign.type", "Event") | filter.Eq("campaign.type", "Registration") |
                                   filter.Eq("campaign.type", "Advertising"));

            return campaignCollection.Find(criteria).Count();
        }

        public List<CampaignItemForHomeFeed> GetActiveCampaignsForUser(string userId, string campaignpublicurl = "",
            string campaignType = "All", bool withoutSRFI = true, string keysearch = "", bool withoutPustToVault = true,
            bool withoutHandshake = true)
        {
            //var profileBus = new ProfileBusinessLogic();
            //var profile = profileBus.GetProfileFromId(userId);


            var activeCampaigns = new List<CampaignItemForHomeFeed>();
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Campaign");

            var businesses = _businessMemberRepos.Many(m => m.Members != null
                                                            && m.Members.Any(f =>
                                                                f.UserId.Equals(userId) &&
                                                                f.Status == EnumFollowType.Following)).ToList();
            if (businesses.Count() <= 0)
            {
                return activeCampaigns;
            }

            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("campaign.status", EnumCampaignStatus.Active);
            List<string> businessIds = new List<string>();
            businesses.ForEach(b => businessIds.Add(b.BusinessUserId));
            var types = new List<string>
            {
                EnumCampaignType.Advertising,
                EnumCampaignType.Registration,
                EnumCampaignType.Event
            };
            bool isFeed = !string.IsNullOrEmpty(campaignType) && campaignType.ToLower().Equals("feed");
            if (withoutSRFI || isFeed)
            {
                criteria &= filter.And(filter.In("userId", businessIds),
                    filter.Ne("campaign.type", EnumCampaignType.SRFI),
                    filter.Ne("campaign.type", EnumCampaignType.HandShake));
            }
            else
            {
                criteria &= filter.Or(filter.In("userId", businessIds));
                types.Add(EnumCampaignType.SRFI);
            }


            IEnumerable<BsonDocument> list = campaignCollection.Find(criteria).ToEnumerable();

            var criteriacampaignpublicid = filter.Eq("_id", "dds");

            if (isFeed)
            {
                criteria = criteria & filter.In("campaign.type", types);
            }

            else if (!string.IsNullOrEmpty(campaignType) && !campaignType.ToLower().Equals("all"))
            {
                criteria = criteria & filter.Eq("campaign.type", campaignType);
            }
            if (withoutSRFI)
            {
                criteria = criteria & filter.Ne("campaign.type", EnumCampaignType.SRFI);
            }
            if (withoutPustToVault)
            {
                criteria = criteria & filter.Ne("campaign.type", EnumCampaignType.PushToVault);
            }

            if (withoutHandshake && campaignType.ToLower() != "handshake")
            {
                criteria = criteria & filter.Ne("campaign.type", EnumCampaignType.HandShake);
            }

            //criteria = criteria & filter.Eq("campaign.addToBusinessPage", "Yes");

            try
            {
                var criterianame = filter.Regex("campaign.name",
                    BsonRegularExpression.Create(new Regex(".*" + keysearch + ".*", RegexOptions.IgnoreCase)));
                var criteriadescription = filter.Regex("campaign.description",
                    BsonRegularExpression.Create(new Regex(".*" + keysearch + ".*", RegexOptions.IgnoreCase)));

                if (!string.IsNullOrEmpty(keysearch))
                {
                    criteria = criteria & (criteriadescription | criterianame);
                }
            }
            catch
            {
            }

            int userAge = 0;
            string userGender = "";
            string userCountry = "";
            string userCity = "";


            if (!string.IsNullOrEmpty(userId))
            {
                var accountService = new AccountService();
                var account = accountService.GetByAccountId(userId);
                InfomationVaultBusinessLogic infomationVaultBusinessLogic = new InfomationVaultBusinessLogic();
                var informationVault = infomationVaultBusinessLogic.GetInformationVaultByUserId(userId);
                userAge = account.Profile.Birthdate.HasValue
                    ? (DateTime.Now.Year - account.Profile.Birthdate.Value.Year)
                    : 0;
                userGender = account.Profile.Gender;
                userCountry = account.Profile.Country;
                userCity = account.Profile.City;
                try
                {
                    var criteriafood =
                        GetFilterVault(informationVault["others"]["value"]["preference"]["value"]["food"]["value"],
                            filter);
                    var criteriaseat =
                        GetFilterVault(informationVault["others"]["value"]["preference"]["value"]["seat"]["value"],
                            filter);
                    var criteriainteresting =
                        GetFilterVault(informationVault["others"]["value"]["preference"]["value"]["interests"]["value"],
                            filter);
                    var criteriaseason =
                        GetFilterVault(informationVault["others"]["value"]["preference"]["value"]["season"]["value"],
                            filter);

                    var criteriacolour =
                        GetFilterVault(informationVault["others"]["value"]["favourite"]["value"]["colour"]["value"],
                            filter);
                    var criteriaholiday =
                        GetFilterVault(informationVault["others"]["value"]["favourite"]["value"]["holiday"]["value"],
                            filter);
                    var criteriamovie =
                        GetFilterVault(informationVault["others"]["value"]["favourite"]["value"]["movie"]["value"],
                            filter);
                    var criteriamusic_type =
                        GetFilterVault(informationVault["others"]["value"]["favourite"]["value"]["music_type"]["value"],
                            filter);
                    var criteriasong =
                        GetFilterVault(informationVault["others"]["value"]["favourite"]["value"]["song"]["value"],
                            filter);
                    var criteriatv_show =
                        GetFilterVault(informationVault["others"]["value"]["favourite"]["value"]["tv_show"]["value"],
                            filter);
                    var criteriaempty = filter.Size("campaign.criteria.keywords", 0);
                    list = campaignCollection.Find(criteria & (criteriafood | criteriaseat | criteriainteresting |
                                                               criteriaseason
                                                               | criteriacolour | criteriaholiday | criteriamusic_type |
                                                               criteriasong | criteriatv_show | criteriaempty |
                                                               filter.Exists("campaign.criteria.keywords", false)
                                                   )).ToEnumerable();
                }
                catch
                {
                }

                //string userRegion = profile.RegionName;
            }
            else
                list = campaignCollection.Find(criteria)
                    .ToEnumerable();


            //var list = campaignCollection.Find(new BsonDocument()).ToEnumerable();
            MongoRepository<Entity.Post.Post> postRepos = new MongoRepository<Entity.Post.Post>();

            foreach (var c in list)
            {
                try
                {
                    BsonDocument cp = c["campaign"].AsBsonDocument;
                    string campaigntype = cp["type"].AsString;


                    if (campaigntype == "PushToVault")
                    {
                        var campaign = new CampaignItemForHomeFeed();
                        campaign.Id = c["_id"].ToString();
                        campaign.BusinessUserId = c["userId"].AsString;
                        campaign.CampaignName = c["campaign"]["name"].AsString;
                        campaign.CampaignType = c["campaign"]["type"].AsString;
                        campaign.Description = c["campaign"]["description"].AsString;
                        activeCampaigns.Add(campaign);
                        continue;
                    }

                    bool isMatch = true;

                    int ageMin = 1;
                    int ageMax = 100;
                    string gender = "All";
                    string country = "";
                    string locationType = "";
                    string city = "";

                    if (campaigntype != "SRFI")
                    {
                        var distribute = cp.GetValue("distribute", "public");
                        if (distribute == "profile") continue;

                        BsonDocument cr = cp["criteria"].AsBsonDocument;


                        BsonValue ageMinStr = cr["age"].AsBsonDocument.GetValue("min", BsonNull.Value);
                        if (ageMinStr.IsString)

                        {
                            ageMin = Int32.Parse(ageMinStr.AsString);
                        }
                        else
                        {
                            ageMin = cr["age"].AsBsonDocument.GetValue("min", new BsonInt32(1)).AsInt32;
                        }


                        BsonValue ageMaxStr = cr["age"].AsBsonDocument.GetValue("max", BsonString.Empty);
                        if (ageMinStr.IsString)
                        {
                            ageMax = Int32.Parse(ageMaxStr.AsString);
                        }
                        else
                        {
                            ageMax = cr["age"].AsBsonDocument.GetValue("max", new BsonInt32(100)).AsInt32;
                        }


                        int minAge = ageMin;
                        int maxAge = ageMax;


                        gender = cr.GetValue("gender", new BsonString("All")).AsString;

                        locationType = cr["location"].AsBsonDocument.GetValue("type", BsonString.Empty).AsString;
                        if (string.IsNullOrEmpty(locationType))
                        {
                            locationType = cr.GetValue("locationtype", "Global").AsString;
                        }
                        if (locationType == "all")
                        {
                            locationType = "Global";
                        }


                        var target = c["campaign"].AsBsonDocument.GetValue("target", BsonString.Empty).AsString;
                        if (string.IsNullOrEmpty(target) || target == "criteria")
                        {
                            country = cr["location"].AsBsonDocument.GetValue("country", BsonString.Empty).AsString;
                            city = cr["location"].AsBsonDocument.GetValue("area", BsonString.Empty).AsString;

                            if (campaigntype != "Handshake")
                            {
                                var ageType = cr["age"].AsBsonDocument.GetValue("type", BsonString.Empty).AsString;
                                if (string.IsNullOrEmpty(ageType) && ageMin != 12 && ageMax != 67 || ageType != "all")
                                {
                                    isMatch = minAge <= userAge && userAge <= maxAge;
                                }
                                isMatch = isMatch && ((gender == EnumGender.All) ||
                                                      (!string.IsNullOrEmpty(userGender) && gender.Equals(userGender)));
                            }
                            if (locationType != "Global")
                            {
                                isMatch = isMatch && (!string.IsNullOrEmpty(userCountry) &&
                                                      country.Equals(userCountry));
                                isMatch = isMatch && (string.IsNullOrEmpty(city) ||
                                                      !string.IsNullOrEmpty(userCity) && city.Contains(userCity));
                            }
                        }
                    }


                    if (isMatch)
                    {
                        try
                        {
  /*                          var post = postRepos.Many(p => p.CampaignId.Equals(c["_id"].ToString())).FirstOrDefault();
                            List<String> followerIds = null;
                            if (post != null && post.Followers != null)
                            {
                                followerIds = post.Followers.Select(p => { return p.UserId; }).ToList();
                            }*/
                            var campaign = new CampaignItemForHomeFeed();
                            campaign.Id = c["_id"].ToString();
                            campaign.BusinessUserId = c["userId"].AsString;
                            campaign.CampaignName = c["campaign"]["name"].AsString;
                            campaign.CampaignType = c["campaign"]["type"].AsString;

                            if (campaign.CampaignType == "Event")
                            {
                                BsonDocument e = cp["event"].AsBsonDocument;
                                campaign.startdate = e.GetValue("startdate", BsonString.Empty).AsString;
                                campaign.starttime = e.GetValue("starttime", BsonString.Empty).AsString;
                                var enddate = e.GetValue("enddate", null);
                                campaign.enddate = enddate == BsonNull.Value ? null : enddate.AsString;
                                var endtime = e.GetValue("endtime", null);
                                campaign.endtime = endtime == BsonNull.Value ? null : endtime.AsString;
                                var theme = e.GetValue("theme", null);
                                campaign.theme = theme == BsonNull.Value ? null : theme.AsString;
                                campaign.location = e.GetValue("location", BsonString.Empty).AsString;
                            }

                            if (campaign.CampaignType == "Event" || campaign.CampaignType == "Registration")
                            {
                                bool? paid = cp.GetValue("paid", BsonNull.Value).AsNullableBoolean;
                                decimal price = 0;
                                string priceCurrency = "";
                                if (paid == null)
                                {
                                    string pay = cp.GetValue("usercodetype", "Free").AsString;
                                    paid = pay == "Pay";
                                }
                                if (paid == true)
                                {
                                    string usercode = cp.GetValue("price", BsonString.Empty).AsString;
                                    if (string.IsNullOrEmpty(usercode))
                                    {
                                        usercode = cp.GetValue("usercode", new BsonString("0")).AsString;
                                    }
                                    price = Decimal.Parse(usercode);
                                    priceCurrency = cp.GetValue("priceCurrency", BsonString.Empty).AsString;
                                    if (string.IsNullOrEmpty(priceCurrency))
                                    {
                                        priceCurrency = cp.GetValue("usercodecurrentcy", new BsonString("USD"))
                                            .AsString;
                                    }
                                }


                                campaign.usercodetype = paid == true ? "Paid" : "Free";
                                campaign.usercode = price.ToString();
                                campaign.usercodecurrentcy = priceCurrency;

                                var termsType = cp.GetValue("termsType", new BsonString("file")).AsString;
                                var termsUrl = cp.GetValue("termsUrl", BsonString.Empty).AsString;

                                if (string.IsNullOrEmpty(termsUrl))
                                {
                                    termsUrl = cp.GetValue("termsAndConditionsFile", BsonString.Empty).AsString;
                                }

                                campaign.termsAndConditionsFile = termsUrl;
                            }


                            campaign.Description = c["campaign"]["description"].AsString;
                            campaign.Status = c["campaign"]["status"].AsString;

                            campaign.Image = cp.GetValue("image", BsonString.Empty).AsString;
                            campaign.TargetLink = cp.GetValue("targetLink", BsonString.Empty).AsString;
                            campaign.SocialShare = c["campaign"].AsBsonDocument
                                .GetValue("socialShare", new BsonString("all")).AsString;

                            campaign.Verb = cp.GetValue("verb", "register").AsString;

                            campaign.MaxAge = ageMax;
                            campaign.MinAge = ageMin;

                            campaign.Gender = gender;


                            campaign.LocationType = locationType;


                            campaign.CountryName = country;
                            campaign.CityName = city;

                            campaign.SpendEffectDate =
                            (c["campaign"]["criteria"]["spend"]["effectiveDate"] != null &&
                             !string.IsNullOrEmpty(c["campaign"]["criteria"]["spend"]["effectiveDate"].AsString))
                                ? DateTime.Parse(c["campaign"]["criteria"]["spend"]["effectiveDate"].AsString)
                                : DateTime.Now.Date;
                            campaign.SpendEndDate = (c["campaign"]["criteria"]["spend"]["endDate"] != null &&
                                                     !string.IsNullOrEmpty(c["campaign"]["criteria"]["spend"]["endDate"]
                                                         .AsString))
                                ? DateTime.Parse(c["campaign"]["criteria"]["spend"]["endDate"].AsString)
                                : DateTime.Now.Date;

                            try
                            {
                                campaign.TimeType = c["campaign"]["criteria"]["spend"]["type"].AsString;
                            }
                            catch
                            {
                            }


                            //campaign.FollowerIds = followerIds;

                            campaign.Fields =
                                (c["campaign"].ToBsonDocument().Names.FirstOrDefault(x => x.Contains("fields")) == null)
                                    ? null
                                    : c["campaign"]["fields"];

                            activeCampaigns.Add(campaign);
                        }
                        catch (Exception e)

                        {
                            Log.Debug("Error loading business feed: " + c["campaign"]["name"].AsString + ": " +
                                      e.Message + e.Source);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Debug("Error getting campaigns for user: " + e.Message);
                }
            }
//            activeCampaigns = activeCampaigns.Where(x => (x.SpendEffectDate <= DateTime.Now && (x.TimeType == "Daily" || DateTime.Now <= x.SpendEndDate.AddHours(x.SpendEndDate.Hour < DateTime.Now.Hour ?
//                (DateTime.Now.Hour - x.SpendEndDate.Hour) + 1 : 0))) || x.CampaignType.ToUpper() == "SRFI" || x.CampaignType.ToUpper() == "PushToVault".ToUpper()).ToList();


            activeCampaigns = activeCampaigns.Where(x => (x.SpendEffectDate <= DateTime.Today.AddDays(1) && (x.TimeType == "Daily"
                                                                                                || new DateTime(
                                                                                                    x.SpendEndDate.Year,
                                                                                                    x.SpendEndDate
                                                                                                        .Month,
                                                                                                    x.SpendEndDate.Day,
                                                                                                    0, 0, 0) >= DateTime
                                                                                                    .Today))
                                                         || x.CampaignType.ToUpper() == "SRFI" ||
                                                         x.CampaignType.ToUpper() == "PushToVault".ToUpper())
                .OrderByDescending(c => c.Id).ToList();
            //activeCampaigns.Sort((c1, c2) => c2.Id.CompareTo(c1.Id));
            return activeCampaigns;
        }

        public List<CampaignItemForHomeFeed> GetInteractionFeedFromBusiness(string accountId, string businessAccountId)
        {
            var activeCampaigns = new List<CampaignItemForHomeFeed>();
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Campaign");

            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("userId", businessAccountId);
            criteria = criteria & filter.Eq("campaign.status", EnumCampaignStatus.Active);


            criteria = criteria & filter.In("campaign.type",
                           new List<string>
                           {
                               EnumCampaignType.Advertising,
                               EnumCampaignType.Registration,
                               EnumCampaignType.Event
                           });
            IEnumerable<BsonDocument> list = null;
            list = campaignCollection.Find(criteria).ToEnumerable();


            int userAge = 0;
            string userGender = "";
            string userCountry = "";
            string userCity = "";


            if (!string.IsNullOrEmpty(accountId))
            {
                var accountService = new AccountService();
                var account = accountService.GetByAccountId(accountId);
                InfomationVaultBusinessLogic infomationVaultBusinessLogic = new InfomationVaultBusinessLogic();
                var informationVault = infomationVaultBusinessLogic.GetInformationVaultByUserId(accountId);
                userAge = account.Profile.Birthdate.HasValue
                    ? (DateTime.Now.Year - account.Profile.Birthdate.Value.Year)
                    : 0;
                userGender = account.Profile.Gender;
                userCountry = account.Profile.Country;
                userCity = account.Profile.City;
                try
                {
                    var criteriafood =
                        GetFilterVault(informationVault["others"]["value"]["preference"]["value"]["food"]["value"],
                            filter);
                    var criteriaseat =
                        GetFilterVault(informationVault["others"]["value"]["preference"]["value"]["seat"]["value"],
                            filter);
                    var criteriainteresting =
                        GetFilterVault(informationVault["others"]["value"]["preference"]["value"]["interests"]["value"],
                            filter);
                    var criteriaseason =
                        GetFilterVault(informationVault["others"]["value"]["preference"]["value"]["season"]["value"],
                            filter);

                    var criteriacolour =
                        GetFilterVault(informationVault["others"]["value"]["favourite"]["value"]["colour"]["value"],
                            filter);
                    var criteriaholiday =
                        GetFilterVault(informationVault["others"]["value"]["favourite"]["value"]["holiday"]["value"],
                            filter);
                    var criteriamovie =
                        GetFilterVault(informationVault["others"]["value"]["favourite"]["value"]["movie"]["value"],
                            filter);
                    var criteriamusic_type =
                        GetFilterVault(informationVault["others"]["value"]["favourite"]["value"]["music_type"]["value"],
                            filter);
                    var criteriasong =
                        GetFilterVault(informationVault["others"]["value"]["favourite"]["value"]["song"]["value"],
                            filter);
                    var criteriatv_show =
                        GetFilterVault(informationVault["others"]["value"]["favourite"]["value"]["tv_show"]["value"],
                            filter);
                    var criteriaempty = filter.Size("campaign.criteria.keywords", 0);
                    list = campaignCollection.Find(criteria & (criteriafood | criteriaseat | criteriainteresting |
                                                               criteriaseason
                                                               | criteriacolour | criteriaholiday | criteriamusic_type |
                                                               criteriasong | criteriatv_show | criteriaempty |
                                                               filter.Exists("campaign.criteria.keywords", false)
                                                   )).ToEnumerable();
                }
                catch
                {
                }

                //string userRegion = profile.RegionName;
            }
            else
                list = campaignCollection.Find(criteria)
                    .ToEnumerable();


            //var list = campaignCollection.Find(new BsonDocument()).ToEnumerable();
            MongoRepository<Entity.Post.Post> postRepos = new MongoRepository<Entity.Post.Post>();

            foreach (var c in list)
            {
                try
                {
                    BsonDocument cp = c["campaign"].AsBsonDocument;
                    string campaigntype = cp["type"].AsString;

                    if (campaigntype == "PushToVault")
                    {
                        var campaign = new CampaignItemForHomeFeed();
                        campaign.Id = c["_id"].ToString();
                        campaign.BusinessUserId = c["userId"].AsString;
                        campaign.CampaignName = c["campaign"]["name"].AsString;
                        campaign.CampaignType = c["campaign"]["type"].AsString;
                        campaign.Description = c["campaign"]["description"].AsString;
                        activeCampaigns.Add(campaign);
                        continue;
                    }
                    var distribute = cp.GetValue("distribute", "public");
                    if (distribute == "feed") continue;

                    BsonDocument cr = cp["criteria"].AsBsonDocument;

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


                    int minAge = ageMin;
                    int maxAge = ageMax;
                    string gender = cr.GetValue("gender", new BsonString("All")).AsString;
                    string country = "";
                    string locationType = "";
                    string city = "";

                    locationType = cr["location"].AsBsonDocument.GetValue("type", BsonString.Empty).AsString;
                    if (string.IsNullOrEmpty(locationType))
                    {
                        locationType = cr.GetValue("locationtype", "Global").AsString;
                    }
                    if (locationType == "all")
                    {
                        locationType = "Global";
                    }


                    var isMatch = true;

                    var target = c["campaign"].AsBsonDocument.GetValue("target", BsonString.Empty).AsString;
                    if (isMatch)
                    {
                        try
                        {
                            var post = postRepos.Many(p => p.CampaignId.Equals(c["_id"].AsString)).FirstOrDefault();
                            List<String> followerIds = null;
                            if (post != null && post.Followers != null)
                            {
                                followerIds = post.Followers.Select(p => { return p.UserId; }).ToList();
                            }
                            var campaign = new CampaignItemForHomeFeed();

                            campaign.Id = c["_id"].AsString;
                            campaign.BusinessUserId = c["userId"].AsString;
                            campaign.CampaignName = c["campaign"]["name"].AsString;
                            campaign.CampaignType = c["campaign"]["type"].AsString;

                            if (campaign.CampaignType == "Event")
                            {
                                BsonDocument e = cp.GetValue("event", new BsonDocument()).AsBsonDocument;
                                campaign.startdate = e.GetValue("startdate", BsonString.Empty).AsString;
                                campaign.starttime = e.GetValue("starttime", BsonString.Empty).AsString;
                                var enddate = e.GetValue("enddate", null);
                                campaign.enddate = enddate == BsonNull.Value ? null : enddate.AsString;
                                var endtime = e.GetValue("endtime", null);
                                campaign.endtime = endtime == BsonNull.Value ? null : endtime.AsString;
                                var theme = e.GetValue("theme", null);
                                campaign.theme = theme == BsonNull.Value ? null : theme.AsString;
                                campaign.location = e.GetValue("location", BsonString.Empty).AsString;
                            }

                            if (campaign.CampaignType == "Event" || campaign.CampaignType == "Registration")
                            {
                                bool? paid = cp.GetValue("paid", BsonNull.Value).AsNullableBoolean;
                                decimal price = 0;
                                string priceCurrency = "";
                                if (paid == null)
                                {
                                    string pay = cp.GetValue("usercodetype", "Free").AsString;
                                    paid = pay == "Pay";
                                }
                                if (paid == true)
                                {
                                    string usercode = cp.GetValue("price", BsonString.Empty).AsString;
                                    if (string.IsNullOrEmpty(usercode))
                                    {
                                        usercode = cp.GetValue("usercode", new BsonString("0")).AsString;
                                    }
                                    price = Decimal.Parse(usercode);
                                    priceCurrency = cp.GetValue("priceCurrency", BsonString.Empty).AsString;
                                    if (string.IsNullOrEmpty(priceCurrency))
                                    {
                                        priceCurrency = cp.GetValue("usercodecurrentcy", new BsonString("USD"))
                                            .AsString;
                                    }
                                }


                                campaign.usercodetype = paid == true ? "Paid" : "Free";
                                campaign.usercode = price.ToString();
                                campaign.usercodecurrentcy = priceCurrency;

                                var termsType = cp.GetValue("termsType", new BsonString("file")).AsString;
                                var termsUrl = cp.GetValue("termsUrl", BsonString.Empty).AsString;

                                if (string.IsNullOrEmpty(termsUrl))
                                {
                                    termsUrl = cp.GetValue("termsAndConditionsFile", BsonString.Empty).AsString;
                                }

                                campaign.termsAndConditionsFile = termsUrl;
                            }


                            campaign.Description = c["campaign"]["description"].AsString;
                            campaign.Status = c["campaign"]["status"].AsString;
                            campaign.MaxAge = ageMax;
                            campaign.MinAge = ageMin;
                            campaign.Gender = gender;
                            campaign.LocationType = locationType;
                            campaign.CountryName = country;
                            campaign.CityName = city;

                            campaign.SpendEffectDate =
                            (c["campaign"]["criteria"]["spend"]["effectiveDate"] != null &&
                             !string.IsNullOrEmpty(c["campaign"]["criteria"]["spend"]["effectiveDate"].AsString))
                                ? DateTime.Parse(c["campaign"]["criteria"]["spend"]["effectiveDate"].AsString)
                                : DateTime.Now.Date;
                            campaign.SpendEndDate = (c["campaign"]["criteria"]["spend"]["endDate"] != null &&
                                                     !string.IsNullOrEmpty(c["campaign"]["criteria"]["spend"]["endDate"]
                                                         .AsString))
                                ? DateTime.Parse(c["campaign"]["criteria"]["spend"]["endDate"].AsString)
                                : DateTime.Now.Date;
                            try
                            {
                                campaign.TimeType = c["campaign"]["criteria"]["spend"]["type"].AsString;
                            }
                            catch
                            {
                            }

                            campaign.Image = cp.GetValue("image", BsonString.Empty).AsString;
                            campaign.TargetLink = cp.GetValue("targetLink", BsonString.Empty).AsString;
                            campaign.SocialShare = c["campaign"].AsBsonDocument
                                .GetValue("socialShare", "all").AsString;
                            campaign.Verb = cp.GetValue("verb", "register").AsString;

                            campaign.FollowerIds = followerIds;

                            campaign.Fields =
                                (c["campaign"].ToBsonDocument().Names.FirstOrDefault(x => x.Contains("fields")) == null)
                                    ? null
                                    : c["campaign"]["fields"];


                            activeCampaigns.Add(campaign);
                        }
                        catch (Exception ex)

                        {
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            activeCampaigns = activeCampaigns.Where(x =>
                (x.SpendEffectDate <= DateTime.Today.AddDays(1) && (x.TimeType == "Daily" || DateTime.Now <=
                                                       x.SpendEndDate.AddHours(x.SpendEndDate.Hour < DateTime.Now.Hour
                                                           ? (DateTime.Now.Hour - x.SpendEndDate.Hour) + 1
                                                           : 0))) || x.CampaignType.ToUpper() == "SRFI" ||
                x.CampaignType.ToUpper() == "PushToVault".ToUpper()).OrderByDescending(c => c.Id).ToList();
            return activeCampaigns;
        }

        public CampaignItemForHomeFeed GetCampaignInfor(string campaignId)
        {
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Campaign");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("_id", campaignId);
            var campaign = campaignCollection.Find(criteria).FirstOrDefault();
            var campaignInfor = new CampaignItemForHomeFeed();
            if (campaign != null)
            {
                try
                {
                    BsonDocument cp = campaign["campaign"].AsBsonDocument;
                    campaignInfor.Id = campaign["_id"].AsString;
                    campaignInfor.BusinessUserId = campaign["userId"].AsString;
                    campaignInfor.CampaignName = campaign["campaign"]["name"].AsString;
                    campaignInfor.CampaignType = campaign["campaign"]["type"].AsString;
                    campaignInfor.Status = campaign["campaign"]["status"].AsString;
                    campaignInfor.Description = campaign["campaign"]["description"].AsString;
                    campaignInfor.Image = cp.GetValue("image", BsonString.Empty).AsString;
                    campaignInfor.TargetLink = cp.GetValue("targetLink", BsonString.Empty).AsString;
                    if (campaignInfor.CampaignType == "PushToVault" ||
                        campaignInfor.CampaignType == "ManualPushToVault")
                    {
                        return campaignInfor;
                    }

                    BsonDocument timing = cp["criteria"]["spend"].AsBsonDocument;
                    try
                    {
                        string fr = timing.GetValue("effectiveDate", BsonString.Empty).AsString;
                        campaignInfor.SpendEffectDate = Convert.ToDateTime(fr);
                    }
                    catch (Exception e)
                    {
                        Log.Debug(e.Message);
                    }

                    bool? indefinite = cp.GetValue("indefinite", BsonNull.Value)?.AsNullableBoolean;

                    if (indefinite == null)
                    {
                        campaignInfor.TimeType = timing.GetValue("type", new BsonString("Daily")).AsString;
                    }
                    if (indefinite == true)
                    {
                        campaignInfor.TimeType = "Daily";
                    }
                    else
                    {
                        string to = timing.GetValue("endDate", BsonString.Empty).AsString;
                        campaignInfor.SpendEndDate = Convert.ToDateTime(to);
                    }

                    campaignInfor.termsAndConditionsFile =
                        cp.GetValue("termsAndConditionsFile", BsonString.Empty).AsString;
                    if (string.IsNullOrEmpty(campaignInfor.termsAndConditionsFile))
                        campaignInfor.termsAndConditionsFile = cp.GetValue("termsUrl", BsonString.Empty).AsString;

                    campaignInfor.Verb = cp.GetValue("verb", BsonString.Empty).AsString;


                    campaignInfor.Fields = cp.GetValue("fields", new BsonArray()).AsBsonArray;

                    return campaignInfor;
                }
                catch
                {
                }
            }

            return null;
        }

        public List<CampaignRegistrationFormField> GetUserInformationForCampaign(string accountId, string campaignId)
        {
            var accountService = new AccountService();
            var account = accountService.GetByAccountId(accountId);

            var vaultBus = new InfomationVaultBusinessLogic();
            var bsonInformationVault = vaultBus.GetInformationVaultByUserId(accountId);
            XDocument xdocInformationVault = null;
            IEnumerable<XElement> xNodeList = null;
            if (bsonInformationVault != null)
            {
                if (bsonInformationVault["_id"].IsObjectId)
                {
                    bsonInformationVault["_id"] = bsonInformationVault["_id"].AsObjectId.ToString()
                        .Replace("ObjectId(", "").Replace(")", "");
                }
                xdocInformationVault = BsonHelper.ConvertBsonToXDocument(bsonInformationVault);
                if (xdocInformationVault != null) xNodeList = xdocInformationVault.Descendants();
            }

            var campaign = GetCampaignById(campaignId);
            var businessUserId = campaign["userId"].AsString;
            var fieldsOfRegistration =
                (campaign["campaign"].ToBsonDocument().Names.FirstOrDefault(x => x.Contains("fields")) == null)
                    ? null
                    : campaign["campaign"]["fields"].AsBsonArray;
            var list = new List<CampaignRegistrationFormField>();
            foreach (var bsonValue in fieldsOfRegistration)
            {
                var field = (BsonDocument) bsonValue;
                var id = field["_id"].AsString;
                var xNode = xNodeList.FirstOrDefault(el =>
                    el.Name.LocalName.Equals("_id") && !string.IsNullOrEmpty(el.Value) && el.Value.Equals(id));

                if (xNode != null)
                {
                    var node = new CampaignRegistrationFormField();
                    node.Id = field["_id"].AsString;
                    node.BusinessUserId = businessUserId;
                    node.Name = field["name"].AsString;
                    node.DisplayName = field["displayName"].AsString;
                    node.ControlType = field["type"].AsString;
                    var nodeValue = xNode.Parent.Element("value");
                    if (nodeValue != null)
                    {
                        var value = nodeValue.Value;
                        node.UserInfor = value;
                        list.Add(node);
                    }
                    try
                    {
                        if (field.Contains("values"))
                        {
                            node.Values = new List<string>();
                            foreach (var item in field["values"].AsBsonArray)
                            {
                                node.Values.Add(item.AsString);
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return list;
        }

        public string GetCampaignSRFIId(string businessUserId)
        {
            string campaignSRFIId = "";
            try
            {
                var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Campaign");
                var filter = Builders<BsonDocument>.Filter;
                var criteria = filter.Eq("userId", businessUserId);
                criteria = criteria & filter.Eq("campaign.type", "SRFI");
                var totalList = campaignCollection.Find(criteria).ToEnumerable();
                var enumerable = totalList as BsonDocument[] ?? totalList.ToArray();
                campaignSRFIId = enumerable.FirstOrDefault()["_id"].AsString;
            }
            catch
            {
            }
            return campaignSRFIId;
        }

        public BsonDocument GetCampaignById(string campaignId)
        {
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Campaign");
            var criteria = Builders<BsonDocument>.Filter.Eq("_id", campaignId);
            var campaign = campaignCollection.Find(criteria).FirstOrDefault();
            return campaign;
        }

        public bool CheckEndDateCampaign(string campaignId)
        {
            var rs = false;

            try
            {
                if (!string.IsNullOrEmpty(campaignId))
                {
                    var camp = GetCampaignById(campaignId);
                    var spend = camp["campaign"]["criteria"]["spend"]["endDate"].AsString;
                    DateTime dt = Convert.ToDateTime(spend);
                    // type
                    var typeSpend = camp["campaign"]["criteria"]["spend"]["type"].AsString;
                    if (typeSpend == "Daily")
                    {
                        rs = false;
                    }
                    else
                    {
                        if (DateTime.Now.AddDays(-1) > dt)
                        {
                            rs = true;
                            var hs = new PostHandShakeBusinessLogic();
                            hs.TerminatePostHandshakeByCampaignId(campaignId);
                        }
                    }
                }
            }
            catch
            {
            }
            return rs;
        }

        public bool CheckRemoveCampaign(string campaignId)
        {
            var rs = false;

            try
            {
                if (!string.IsNullOrEmpty(campaignId))
                {
                    var camp = GetCampaignById(campaignId);
                    var remove = camp["campaign"]["status"].AsString;

                    if (remove == "Remove")
                    {
                        rs = true;
                        var hs = new PostHandShakeBusinessLogic();
                        hs.DeletePostHandByCampaign(campaignId);
                        //DeletePostByCampaign
                        var post = new PostBusinessLogic();
                        post.DeletePostByCampaign(campaignId);
                    }
                }
            }
            catch
            {
            }
            return rs;
        }

        public BsonDocument GetCampaignBypublicUrl(string publicURL)
        {
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Campaign");

            var criteria = Builders<BsonDocument>.Filter.Eq("campaign.qrCode.PublicURL", publicURL);
            var campaign = campaignCollection.Find(criteria).FirstOrDefault();
            return campaign;
        }

        public BsonDocument GetCampaignAdvertisingTemplate()
        {
            var settingCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Settings");
            var filter = Builders<BsonDocument>.Filter.Eq("key", "campaignAdvertisingTemplate");
            var campaignTemplate = settingCollection.Find(filter).FirstOrDefault();
            var advertising = campaignTemplate["value"].AsBsonDocument;
            return advertising.ToBsonDocument();
        }

        public BsonDocument GetCampaignTemplate(string name)
        {
            var settingCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Settings");
            var filter = Builders<BsonDocument>.Filter.Eq("key", name);
            var campaignTemplate = settingCollection.Find(filter).FirstOrDefault();
            var advertising = campaignTemplate["value"].AsBsonDocument;
            return advertising.ToBsonDocument();
        }

        public BsonDocument GetCampaignRegistrationTemplate()
        {
            var settingCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Settings");
            var filter = Builders<BsonDocument>.Filter.Eq("key", "campaignRegistrationTemplate");
            var campaignTemplate = settingCollection.Find(filter).FirstOrDefault();
            var advertising = campaignTemplate["value"].AsBsonDocument;
            return advertising.ToBsonDocument();
        }

        public BsonDocument GetCampaignSRFITemplate()
        {
            var settingCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Settings");
            var filter = Builders<BsonDocument>.Filter.Eq("key", "campaignSRFITemplate");
            var campaignTemplate = settingCollection.Find(filter).FirstOrDefault();
            var advertising = campaignTemplate["value"].AsBsonDocument;
            return advertising.ToBsonDocument();
        }

        public BsonDocument GetCampaignEventTemplate()
        {
            var settingCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Settings");
            var filter = Builders<BsonDocument>.Filter.Eq("key", "campaignEventTemplate");
            var campaignTemplate = settingCollection.Find(filter).FirstOrDefault();
            var advertising = campaignTemplate["value"].AsBsonDocument;
            return advertising.ToBsonDocument();
        }

        public BsonDocument GetVaultTreeForRegistration()
        {
            var settingCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Settings");
            var filter = Builders<BsonDocument>.Filter.Eq("key", "vaultTreeForRegistration");
            var campaignTemplate = settingCollection.Find(filter).FirstOrDefault();
            var advertising = campaignTemplate["value"].AsBsonDocument;
            return advertising.ToBsonDocument();
        }

        public BsonDocument GetFormTemplate()
        {
            var settingCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Settings");
            var filter = Builders<BsonDocument>.Filter.Eq("key", "FormTemplate");
            var campaignTemplate = settingCollection.Find(filter).FirstOrDefault();
            var advertising = campaignTemplate["value"].AsBsonDocument;
            return advertising.ToBsonDocument();
        }

        public string InsertCampaign(string businessUserId, BsonDocument campaignContent)
        {
            string campaignid = BsonHelper.GenerateObjectIdString();
            try
            {
                var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Campaign");

                var campaign = new BsonDocument
                {
                    {"_id", campaignid},
                    {"userId", businessUserId},
                    {"campaign", campaignContent}
                };
                campaignCollection.InsertOne(campaign);
            }
            catch (Exception e)
            {
            }

            return campaignid;
        }
        //


        public void UpdateCampaign(string campaignId, BsonDocument campaignContent)
        {
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Campaign");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", campaignId);
            var update = Builders<BsonDocument>.Update.Set("campaign", campaignContent);
            campaignCollection.UpdateOne(filter, update);
        }

        //
        public void ApproveCampaign(string campaignId, string comment)
        {
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Campaign");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", campaignId);
            var campaign = campaignCollection.Find(filter).FirstOrDefault();
            if (campaign != null)
            {
                CultureInfo enUS = new CultureInfo("en-US");
                var dateTimePattern = "yyyy-M-d";
                var effectiveDateString = campaign["campaign"]["criteria"]["spend"]["effectiveDate"].AsString;
                var effectiveDate = DateTime.ParseExact(effectiveDateString, dateTimePattern, enUS);
                var endDateString = campaign["campaign"]["criteria"]["spend"]["endDate"].AsString;
                var endDate = DateTime.ParseExact(endDateString, dateTimePattern, enUS);
                var campaignStatus = EnumCampaignStatus.InActive;

                if (DateTime.Compare(DateTime.Now, effectiveDate) >= 0 && DateTime.Compare(DateTime.Now, endDate) <= 0)
                {
                    campaignStatus = EnumCampaignStatus.Active;
                }
                else if (DateTime.Compare(DateTime.Now, endDate) > 0)
                {
                    campaignStatus = EnumCampaignStatus.Expired;
                }
                campaign["campaign"]["status"] = campaignStatus;
                campaign["campaign"]["commentFromSupervisor"].AsBsonArray.Add(comment);

                campaignCollection.FindOneAndReplace(filter, campaign);

                // Notify to staff
                // TODO

                if (campaignStatus == EnumCampaignStatus.Active)
                {
                    // Trigger the campaign
                    // TODO
                }
            }
        }

        public void DeleteCampaign(string campaignId)
        {
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Campaign");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", campaignId);
            campaignCollection.FindOneAndDelete(filter);
        }

        public void RemoveCampaign(string campaignId)
        {
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Campaign");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", campaignId);
            var update = Builders<BsonDocument>.Update.Set("campaign.status", "Remove");
            campaignCollection.UpdateOne(filter, update);
        }

        public void SaveCampaign(string campaignId, string campaignJson)
        {
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Campaign");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", campaignId);
            //campaignJson = campaignJson.Replace("\"" + campaignId + "\"", "ObjectId(\"" + campaignId + "\")");
            BsonDocument campaign = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(campaignJson);
            campaignCollection.ReplaceOne(filter, campaign);
        }

        public void UpdateCampaignStatus(string campaignId, string campaignStatus)
        {
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Campaign");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", campaignId);
            var update = Builders<BsonDocument>.Update.Set("campaign.status", campaignStatus);
            campaignCollection.UpdateOne(filter, update);
        }

        public void SetBoostAdvertising(string campaignId)
        {
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Campaign");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", campaignId);
            var campaign = campaignCollection.Find(filter).FirstOrDefault();
            var currentCost = int.Parse(campaign["campaign"]["criteria"]["spend"]["money"].AsString);
            var boostCost = 0;
            int.TryParse(ConfigurationManager.AppSettings["BOOST_COST"], out boostCost);
            if (boostCost == 0) boostCost = 4;
            var cost = currentCost + boostCost;
            var update = Builders<BsonDocument>.Update.Set("campaign.boostAdvertising", "true")
                .Set("campaign.criteria.spend.money", cost.ToString());
            campaignCollection.UpdateOne(filter, update);
        }

        #region Version2

        public string UserIdByCampaignId(string campaignId)
        {
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Campaign");
            var criteria = Builders<BsonDocument>.Filter.Eq("_id", campaignId);
            var campaign = campaignCollection.Find(criteria).FirstOrDefault();
            var rs = "";
            try
            {
                return rs = campaign["userId"].AsString;
            }
            catch
            {
            }
            return null;
        }

        public List<CampaignVM> GetCampaignForBusinessByType(string accountId, string campaignType)
        {
            var rs = new List<CampaignVM>();
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Campaign");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("userId", accountId);
            criteria &= filter.Eq("campaign.type", campaignType);

            var camps = campaignCollection.Find(criteria).ToList();
            rs = ObjectToListCampaignViewModel(camps);

            return rs;
        }

        # endregion Version2

        #region CustomQuestion

        public List<FieldinformationVault> UpdateCampaignCustom(string campaignId, string userId,
            List<FieldinformationVault> listFieldInfo)
        {
            var rs = new List<FieldinformationVault>();
            var campaign = GetCampaignById(campaignId);
            var post = new PostBusinessLogic();

            var lstFieldsPost = post.GetDataRegisofUser(campaignId, userId);
            foreach (var fieldInfo in listFieldInfo)
            {
                if (fieldInfo.qa == true)
                {
                    foreach (var fieldPost in lstFieldsPost)
                    {
                        if (fieldInfo.jsPath == fieldPost.jsPath)
                        {
                            fieldInfo.model = fieldPost.model;
                            fieldInfo.pathfile = fieldPost.pathfile;
                            fieldInfo.modelarraysstr = fieldPost.modelarraysstr;
                        }
                    }
                }
                rs.Add(fieldInfo);
            }
            return rs;
        }

        #endregion CustomQuestion

        #region PushVault

        public List<CampaignVM> GetCampaignActiveForBusinessByType(string accountId, string campaignType)
        {
            var rs = new List<CampaignVM>();
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Campaign");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("userId", accountId);
            criteria &= filter.Eq("campaign.type", campaignType);
            criteria &= filter.Eq("campaign.status", EnumCampaignStatus.Active);
            var camps = campaignCollection.Find(criteria).ToList()
                .Where(c =>
                {
                    var cd = c["campaign"].AsBsonDocument;
                    DateTime.TryParse(cd["criteria"]["spend"]["effectiveDate"]?.AsString, out var from);
                    if (from > DateTime.Now) return false;
                    if (cd["criteria"]["spend"]["type"].AsString != "Daily")
                    {
                        DateTime.TryParse(cd["criteria"]["spend"]["endDate"]?.AsString, out var until);
                        if (new DateTime(until.Year, until.Month, until.Day, 0, 0, 0) < DateTime.Today) return false;
                    }
                    return true;

                }).ToList();
            rs = ObjectToListCampaignViewModel(camps);

            return rs;
        }

        public CampaignVM CampaignById(string campaignId)
        {
            var rs = new CampaignVM();
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Campaign");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("_id", campaignId);
            var camps = campaignCollection.Find(criteria).FirstOrDefault();
            rs = ObjectToCampaignViewModel(camps);

            return rs;
        }

        public List<CampaignVM> ObjectToListCampaignViewModel(List<BsonDocument> camps)
        {
            var rs = new List<CampaignVM>();
            if (camps != null)
            {
                foreach (var item in camps)
                {
                    var ps = ObjectToCampaignViewModel(item);
                    if (ps != null)
                        rs.Add(ps);
                }
            }
            return rs;
        }

        public CampaignVM ObjectToCampaignViewModel(BsonDocument item)
        {
            var rs = new CampaignVM();
            if (item != null)
            {
                try
                {
                    BsonDocument cp = item["campaign"].AsBsonDocument;
                    rs.CampaignId = item["_id"].ToString();
                    rs.UserId = item["userId"].AsString;
                    rs.Description = item["campaign"]["description"].AsString;
                    rs.Name = item["campaign"]["name"].AsString;
                    rs.Type = item["campaign"]["type"].AsString;
                    rs.Status = item["campaign"]["status"].AsString;
                    rs.TermsUrl = item["campaign"]["termsUrl"].AsString;
                    rs.Verb = item["campaign"].AsBsonDocument.GetValue("verb", "register").AsString;
                    try
                    {
                        rs.Created = Convert.ToDateTime(item["campaign"]["created"].AsString);
                    }
                    catch
                    {
                    }
                    var fields = item["campaign"]["fields"].AsBsonArray;
                    var lstField = new List<FieldViewModel>();
                    foreach (var field in fields)
                    {
                        var f = new FieldViewModel();
                        var lstOption = new List<string>();

                        try
                        {
                            f.id = field["id"].AsString ?? "";
                            f.displayName = field["displayName"].AsString ?? "";
                            // f.displayName2 = field["displayName2"].AsString ?? "";
                            f.jsPath = field["jsPath"].AsString ?? "";
                            f.label = field["label"].AsString ?? "";
                            f.optional = field["optional"].AsBoolean;
                            // f.optional2 = field["optional2"].AsBoolean ?? false;
                            f.type = field["type"].AsString ?? "";
                        }
                        catch
                        {
                        }
                        try
                        {
                            var options = field["options"].AsBsonArray;
                            foreach (var op in options)
                                lstOption.Add(op.AsString);
                        }
                        catch
                        {
                        }

                        f.options = lstOption;
                        if (f != null)
                            lstField.Add(f);
                    }
                    if (lstField != null)
                        rs.Fields = lstField;
                }
                catch
                {
                }
            }
            return rs;
        }

        public List<PushVaultViewModel> GetAllPushVaultByUser(string accountId)
        {
            var rs = new List<PushVaultViewModel>();
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Campaign");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("userId", accountId);
            criteria &= filter.Eq("campaign.type", EnumCampaignType.PushToVault);
            //criteria &= filter.Eq("campaign.status", EnumCampaignStatus.Active);
            var camps = campaignCollection.Find(criteria).ToList();
            rs = PushVaultToViewModel(camps);

            return rs;
        }

        public List<PushVaultViewModel> GetAllPushVaultActiveByUser(string accountId)
        {
            var rs = new List<PushVaultViewModel>();
            var campaignCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("Campaign");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("userId", accountId);
            criteria &= filter.Eq("campaign.type", EnumCampaignType.PushToVault);
            criteria &= filter.Eq("campaign.status", EnumCampaignStatus.Active);
            var camps = campaignCollection.Find(criteria).ToList();
            rs = PushVaultToViewModel(camps);

            return rs;
        }

        public List<PushVaultViewModel> PushVaultToViewModel(List<BsonDocument> camps)
        {
            var rs = new List<PushVaultViewModel>();
            if (camps != null)
            {
                foreach (var item in camps)
                {
                    var ps = new PushVaultViewModel();
                    try
                    {
                        ps.Created =
                            Convert.ToDateTime(item["campaign"]["criteria"]["spend"]["effectiveDate"].AsString);
                        ps.Created = Convert.ToDateTime(item["campaign"]["created"].AsString);
                    }
                    catch
                    {
                    }
                    try
                    {
                        ps.CampaignId = item["_id"].AsString;
                        ps.Description = item["campaign"]["description"].AsString;
                        ps.Name = item["campaign"]["name"].AsString;
                        ps.Type = item["campaign"]["type"].AsString;
                        ps.Status = item["campaign"]["status"].AsString;

                        var fields = item["campaign"]["fields"].AsBsonArray;
                        var lstField = new List<FieldViewModel>();
                        foreach (var field in fields)
                        {
                            var f = new FieldViewModel();
                            var lstOption = new List<string>();
                            try
                            {
                                f.id = field["id"].AsString ?? "";
                                f.displayName = field["displayName"].AsString ?? "";
                                f.displayName2 = field["displayName2"].AsString ?? "";
                                f.jsPath = field["jsPath"].AsString ?? "";
                                f.label = field["label"].AsString ?? "";
                                f.optional = field["optional"].AsBoolean;
                                f.optional2 = field["optional2"].AsBoolean;
                                f.type = field["type"].AsString ?? "";
                            }
                            catch
                            {
                            }
                            try
                            {
                                var options = field["options"].AsBsonArray;
                                foreach (var op in options)
                                    lstOption.Add(op.AsString);
                            }
                            catch
                            {
                            }

                            f.options = lstOption;
                            if (f != null)
                                lstField.Add(f);
                        }
                        if (lstField != null)
                            ps.Fields = lstField;
                        if (ps != null)
                            rs.Add(ps);
                    }
                    catch
                    {
                    }
                }
            }
            return rs;
        }

        public List<FieldinformationVault> GetListFieldByCampaignId(string campaignId)
        {
            var rs = new List<FieldinformationVault>();
            var campaign = GetCampaignById(campaignId);
            // var fields = new FieldinformationVault();

            var fields = campaign["campaign"]["fields"].AsBsonArray;
            rs = ConvertBsonArrayToListFields(fields);
            return rs;
        }

        public List<FieldinformationVault> ConvertBsonArrayToListFields(BsonArray fields)
        {
            var rs = new List<FieldinformationVault>();
            foreach (var field in fields)
            {
                var f = new FieldinformationVault();

                try
                {
                    f.displayName = field["displayName"].AsString ?? "";
                    //  f.displayName2 = field["displayName2"].AsString ?? "";
                    try
                    {
                        f.jsPath = field["jsPath"].AsString ?? "";

                        //f.label = field["label"].AsString ?? "";
                        f.optional = field["optional"].AsBoolean;
                        // f.optional2 = field["optional2"].AsBoolean;
                        f.type = field["type"].AsString ?? "";
                        f.id = field["id"].AsString ?? "";
                    }
                    catch
                    {
                    }
                    if (f.jsPath == null || f.jsPath == "undefined" || f.jsPath == "")
                    {
                        f.jsPath = field["path"].AsString ?? "";
                    }
                }
                catch
                {
                }

                if (f != null)
                {
                    rs.Add(f);
                }
            }
            return rs;
        }

        #endregion PushVault
    }
}