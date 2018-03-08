using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.Common;
using GH.Core.BlueCode.Entity.InformationVault;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using GH.Core.Models;
using System.Globalization;
using GH.Core.Services;
using NLog;
using GH.Core.Adapters;
using GH.Core.ViewModels;
using GH.Core.BlueCode.Entity.ManualHandshake;
using System.Web.Hosting;
using System.Configuration;
using System.IO;

namespace GH.Core.BlueCode.BusinessLogic
{
    public class InfomationVaultBusinessLogic : IInfomationVaultBusinessLogic
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

        IInfomationVaultRepository infomationVaultRepository;

        public InfomationVaultBusinessLogic(IInfomationVaultRepository infomationVaultRepository)
        {
            this.infomationVaultRepository = infomationVaultRepository;
        }

        public InfomationVaultBusinessLogic()
        {
            this.infomationVaultRepository = new InfomationVaultRepository();
        }

        public void AddInformationVault(string userId)
        {
            var informationVaultTemplate = GetInformationVaultTemplate();
            if (informationVaultTemplate.Contains("_id"))
            {
                informationVaultTemplate.Remove("_id");
            }

            String json = informationVaultTemplate.ToJson();
            var document = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(json);
            document["userId"] = userId;
            infomationVaultRepository.GetCollection("InformationVault").InsertOne(document);
        }

        public void AddInformationVaultWithJson(string jsonvault, string userId)
        {
            String json = jsonvault;
            var document = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(json);
            document["userId"] = userId;
            infomationVaultRepository.GetCollection("InformationVault").InsertOne(document);
        }

        private BsonDocument GetInformationVaultTemplate()
        {
            var settings = infomationVaultRepository.GetCollection("Settings");
            var filter = Builders<BsonDocument>.Filter.Eq("key", "informationVault");
            var informationVaultTemplate = settings.Find(filter).FirstOrDefault();

            return informationVaultTemplate["value"] as BsonDocument;
        }

        public String GetJsonFromInformationvaultId(string id)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", new BsonObjectId(ObjectId.Parse(id)));
            BsonDocument vaultinformation = infomationVaultRepository.GetCollection("InformationVault").Find(filter)
                .SingleOrDefault();
            var m = vaultinformation.ToJson();
            return m;
        }

        public String GetInformationVaultJsonByUserId(string userId)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("userId", userId);
            BsonDocument vaultinformation = infomationVaultRepository.GetCollection("InformationVault").Find(filter)
                .SingleOrDefault();
            var m = vaultinformation.ToJson();
            return m;
        }

        public BsonDocument GetInformationVaultByUserId(string userId)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("userId", userId);
            BsonDocument vaultinformation = infomationVaultRepository.GetCollection("InformationVault").Find(filter)
                .SingleOrDefault();
            return vaultinformation;
        }

        public void SaveInformationVault(string informationVaultId, string informationVaultJson)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", new BsonObjectId(ObjectId.Parse(informationVaultId)));
            var _accountService = new AccountService();
            var accountid = new InfomationVaultBusinessLogic().GetInformationVaultById(informationVaultId)["userId"]
                .AsString;
            CheckManualHandshake(accountid, informationVaultId, informationVaultJson);

            informationVaultJson = informationVaultJson.Replace("\"" + informationVaultId + "\"",
                "ObjectId(\"" + informationVaultId + "\")");
            BsonDocument newvaultinformation =
                MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(informationVaultJson);

            infomationVaultRepository.GetCollection("InformationVault").ReplaceOne(filter, newvaultinformation);
            //Call postHandShake
            new PostHandShakeBusinessLogic().TaskCheckUpdateVaultHandshake(accountid);
        }


        public BsonDocument GetInformationVaultById(string vaultId)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", new BsonObjectId(ObjectId.Parse(vaultId)));
            BsonDocument vaultinformation = infomationVaultRepository.GetCollection("InformationVault").Find(filter)
                .SingleOrDefault();
            return vaultinformation;
        }

        public void UpdateInformationVaultById(string accountId, List<KeyValue> keyValues)
        {
            var informationVault = GetInformationVaultByUserId(accountId);
            if (informationVault != null)
            {
                var jsonString = informationVault.ToJson();
                jsonString = jsonString.Replace("ObjectId(", "").Replace(")", "");
                foreach (var keyValue in keyValues)
                {
                    var searchPattern = "\"" + keyValue.Key + "\", \"value\" : (\\\")*(.*?)(\\})";
                    var replacement = "\"" + keyValue.Key + "\", \"value\" : \"" + keyValue.Value + "\"}";
                    var match = Regex.Match(jsonString, searchPattern);
                    jsonString = Regex.Replace(jsonString, searchPattern, replacement);
                }
                string vaultId = informationVault["_id"].AsObjectId.ToString();
                SaveInformationVault(vaultId, jsonString);
            }
        }

        public List<string> Getaccountidsfromkeyworkinvault(List<string> keywords)
        {
            List<string> listaccountids = new List<string>();
            var InformationvaultCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("InformationVault");
            var builder = Builders<BsonDocument>.Filter;
            var filter1 = builder.AnyIn("others.value.preference.value.food.value", keywords.ToArray());
            var filter2 = builder.AnyIn("others.value.preference.value.seat.value", keywords.ToArray());
            var filter3 = builder.AnyIn("others.value.preference.value.interesting.value", keywords.ToArray());
            var filter4 = builder.AnyIn("others.value.preference.value.season.value", keywords.ToArray());
            var filter5 = builder.AnyIn("others.value.favourite.value.colour.value", keywords.ToArray());
            var filter6 = builder.AnyIn("others.value.favourite.value.holiday.value", keywords.ToArray());
            var filter7 = builder.AnyIn("others.value.favourite.value.movie.value", keywords.ToArray());
            var filter8 = builder.AnyIn("others.value.favourite.value.music_type.value", keywords.ToArray());
            var filter9 = builder.AnyIn("others.value.favourite.value.song.value", keywords.ToArray());
            var filter10 = builder.AnyIn("others.value.favourite.value.tv_show.value", keywords.ToArray());

            var filter = filter1 | filter2 | filter3 | filter4 | filter5 | filter6 | filter7 | filter8 | filter9 |
                         filter10;

            var projection = Builders<BsonDocument>.Projection.Include("userId");
            listaccountids = InformationvaultCollection.Find(filter).Project(projection).ToEnumerable()
                .Select(x => x["userId"].AsString).ToList();

            return listaccountids;
        }

        public FieldinformationVault GetvalueFromFieldinformationVault(FieldinformationVault fieldinformationVault,
            JToken node)
        {
            try
            {
                switch (fieldinformationVault.type)
                {
                    case "doc":
                        return fieldinformationVault;
                    case "location":

                        if (fieldinformationVault.jsPath.StartsWith(".basicInformation"))
                        {
                            if (node["value"]["country"]["value"] != null)
                                fieldinformationVault.model = node["value"]["country"]["value"].ToString();
                            if (node["value"]["city"]["value"] != null)
                                fieldinformationVault.unitModel = node["value"]["city"]["value"].ToString();
                        }
                        else
                        {
                            fieldinformationVault.model = node["country"].ToString();
                            fieldinformationVault.unitModel = node["city"].ToString();
                        }
                        return fieldinformationVault;
                    case "address":
                        fieldinformationVault.model = node["addressLine"].ToString();
                        return fieldinformationVault;
                    case "numinput":
                        fieldinformationVault.model = node["value"].ToString();
                        fieldinformationVault.unitModel = node["unit"].ToString();

                        return fieldinformationVault;
                    case "tagsinput":
                    case "smartinput":
                        if (node["value"] is JArray)
                        {
                            fieldinformationVault.model =
                                String.Join(",", ((JArray) node["value"]).ToObject<List<string>>());
                        }
                        else
                            fieldinformationVault.model = node["value"].ToString();
                        return fieldinformationVault;
                    default:

                        if (fieldinformationVault.jsPath.StartsWith(".basicInformation") ||
                            fieldinformationVault.jsPath.StartsWith(".others.preference") ||
                            fieldinformationVault.jsPath.StartsWith(".others.body") ||
                            fieldinformationVault.jsPath.StartsWith(".others.favourite"))
                            fieldinformationVault.model = node["value"].ToString();
                        else
                        {
                            string nodename =
                                fieldinformationVault.jsPath.Trim('.').Split('.')[
                                    fieldinformationVault.jsPath.Trim('.').Split('.').Length - 1];
                            fieldinformationVault.model = node[nodename].ToString();
                        }

                        //if (fieldinformationVault.type == "date" || fieldinformationVault.type=="datecombo")
                        //{
                        //    fieldinformationVault.model = Convert.ToDateTime(fieldinformationVault.model).ToString("dd/MM/yyyy");
                        //}
                        break;
                }
            }
            catch
            {
            }
            return fieldinformationVault;
        }

        public FieldinformationVault GetvalueFromFieldinformationVault(FieldinformationVault fieldinformationVault,
            JObject jObect)
        {
            List<string> listpath = fieldinformationVault.jsPath.Trim('.').Split('.').ToList();
            string jsonpathreal = string.Join(".value.", listpath);
            var node = jObect.SelectToken(jsonpathreal);
            switch (fieldinformationVault.type)
            {
                case "location":
                    if (listpath.Count() > 1)
                    {
                        listpath.RemoveAt(listpath.Count() - 1);
                        jsonpathreal = string.Join(".value.", listpath);
                        node = jObect.SelectToken(jsonpathreal);
                        fieldinformationVault = GetvalueFromFieldinformationVault(fieldinformationVault, node);
                    }
                    return fieldinformationVault;
                case "address":
                    try
                    {
                        if (listpath.Count() > 1)
                        {
                            listpath.RemoveAt(listpath.Count() - 1);
                            jsonpathreal = string.Join(".value.", listpath);
                            node = jObect.SelectToken(jsonpathreal);
                            fieldinformationVault = GetvalueFromFieldinformationVault(fieldinformationVault, node);
                        }
                    }
                    catch
                    {
                    }
                    return fieldinformationVault;
                case "numinput":
                    fieldinformationVault = GetvalueFromFieldinformationVault(fieldinformationVault, node);
                    return fieldinformationVault;
                case "tagsinput":
                case "smartinput":
                    fieldinformationVault = GetvalueFromFieldinformationVault(fieldinformationVault, node);
                    return fieldinformationVault;
                default:
                    fieldinformationVault = GetvalueFromFieldinformationVault(fieldinformationVault, node);
                    break;
            }

            return fieldinformationVault;
        }

        public FieldinformationVault GetvalueFromFieldinformationVault(BsonDocument bsonvault,
            FieldinformationVault fieldinformationVault, BsonDocument field,
            List<FieldinformationVault> fieldsregisdata, bool ischart = false)
        {
            var userid = bsonvault["userId"].AsString;
            var jsonString = bsonvault.ToJson();
            jsonString = jsonString.Replace("ObjectId(", "").Replace(")", "");
            JObject jObect = JObject.Parse(jsonString);
            if (field["options"] is BsonArray)
            {
                List<string> list = new List<string>();
                foreach (var bsonValue1 in field["options"].AsBsonArray)
                {
                    var field1 = bsonValue1;
                    list.Add(field1.AsString);
                }
                fieldinformationVault.options = list;
            }
            else
            {
                fieldinformationVault.options = field["options"] is BsonNull ? "" : field["options"].AsString;
            }

            string jsonpath = field["jsPath"].AsString;
            List<string> listpath = field["jsPath"].AsString.Trim('.').Split('.').ToList();
            string jsonpathreal = string.Join(".value.", listpath);
            var node = jObect.SelectToken(jsonpathreal);
            int? nullint = null;
            switch (jsonpath)
            {
                case ".contact.fax":
                case ".contact.office":
                case ".contact.mobile":
                case ".contact.home":
                    try
                    {
                        var nodecontact = jObect.SelectToken("contact");
                        var varluedefault = nodecontact["default"];
                        fieldinformationVault.model = varluedefault.ToString();
                    }
                    catch
                    {
                        fieldinformationVault.model = "";
                    }
                    return fieldinformationVault;
                case ".contact.email":
                    try
                    {
                        var nodeemail = jObect.SelectToken("contact.value.email");
                        var varluedefault = nodeemail["default"];
                        fieldinformationVault.model = varluedefault.ToString();
                    }
                    catch
                    {
                        fieldinformationVault.model = "";
                    }
                    return fieldinformationVault;
                case ".contact.officeEmail":
                    try
                    {
                        var nodeemail = jObect.SelectToken("contact.value.officeEmail");
                        var varluedefault = nodeemail["default"];
                        fieldinformationVault.model = varluedefault.ToString();
                    }
                    catch
                    {
                        fieldinformationVault.model = "";
                    }
                    return fieldinformationVault;
                case ".basicInformation.age":
                    try
                    {
                        var noderoot = jObect.SelectToken("basicInformation");
                        string strdate = noderoot["value"]["dob"]["value"].ToString();
                        strdate = string.Join("-",
                            strdate.Split('-').Select(x => x.Length == 1 ? "0" + x : x).ToList());
                        DateTime dobtimevault = DateTime.Now;
                        dobtimevault = Convert.ToDateTime(strdate);

                        var age = GH.Util.Common.CalculateAge(dobtimevault);
                        fieldinformationVault.model = age.ToString();
                        return fieldinformationVault;
                    }
                    catch
                    {
                        fieldinformationVault.model = "";
                    }
                    break;
                case ".basicInformation.ageRange":
                    // fieldinformationVault.unitModel = field["unitModel"].AsString;
                    try
                    {
                        fieldinformationVault.modelarrays = field["model"].AsBsonArray.Select(x =>
                            x.AsBsonArray.Select(y => y is BsonNull ? nullint : y.AsInt32).ToList()).ToList();
                        fieldinformationVault.options = field["options"].AsString;
                        if (ischart)
                        {
                            var fieldinformationVaultage =
                                fieldsregisdata.FirstOrDefault(x => x.jsPath == fieldinformationVault.jsPath);
                            var modelarrays =
                                JsonConvert.DeserializeObject<List<List<int?>>>(fieldinformationVaultage
                                    .modelarraysstr);
                            ;
                            var rangeselect = modelarrays[Convert.ToInt32(fieldinformationVaultage.model)];
                            int min = rangeselect[0].HasValue ? rangeselect[0].Value : 0;


                            int max = rangeselect[1].HasValue ? rangeselect[1].Value : 0;

                            if (min > 0 && max > 0)
                                fieldinformationVault.model = min + "-" + max.ToString();
                            else if (max == 0)
                                fieldinformationVault.model = "> " + min.ToString();
                            else if (min == 0)
                                fieldinformationVault.model = "< " + max.ToString();
                            else if (min == 0 && max == 0)
                                fieldinformationVault.model = "> " + min.ToString();
                        }
                        else
                        {
                            var noderoot = jObect.SelectToken("basicInformation");
                            string strdate = noderoot["value"]["dob"]["value"].ToString();

                            strdate = string.Join("-",
                                strdate.Split('-').Select(x => x.Length == 1 ? "0" + x : x).ToList());

                            DateTime dobtimevault = DateTime.Now;
                            dobtimevault = DateTime.ParseExact(strdate, "dd-MM-yyyy", CultureInfo.InvariantCulture);

                            var age = Util.Common.CalculateAge(dobtimevault);
                            int index = -1;
                            var modelarrays = (List<List<int?>>) fieldinformationVault.modelarrays;
                            for (index = 0; index < modelarrays.Count(); index++)
                            {
                                int min = modelarrays[index][0].HasValue ? modelarrays[index][0].Value : 0;

                                int max = modelarrays[index][1].HasValue ? modelarrays[index][1].Value : int.MaxValue;

                                if (min <= age && max >= age)
                                {
                                    break;
                                }
                            }
                            if (index >= modelarrays.Count())
                                index = -1;
                            fieldinformationVault.model = index.ToString();
                        }
                    }
                    catch
                    {
                        fieldinformationVault.model = "-1";
                    }
                    // fieldinformationVault.model = varluedefault.ToString();

                    return fieldinformationVault;
                case ".employment.salaryRange":
                    // fieldinformationVault.unitModel = field["unitModel"].AsString;
                    try
                    {
                        fieldinformationVault.modelarrays = field["model"].AsBsonArray.Select(x =>
                            x.AsBsonArray.Select(y => y is BsonNull ? nullint : y.AsInt32).ToList()).ToList();
                        fieldinformationVault.options = field["options"].AsString;

                        fieldinformationVault.unitModel = field["unitModel"].AsString;
                        if (ischart)
                        {
                            var fieldinformationVaultage =
                                fieldsregisdata.FirstOrDefault(x => x.jsPath == fieldinformationVault.jsPath);
                            var modelarrays =
                                JsonConvert.DeserializeObject<List<List<int?>>>(fieldinformationVaultage
                                    .modelarraysstr);
                            ;
                            var rangeselect = modelarrays[Convert.ToInt32(fieldinformationVaultage.model)];
                            int min = rangeselect[0].HasValue ? rangeselect[0].Value : 0;


                            int max = rangeselect[1].HasValue ? rangeselect[1].Value : 0;

                            if (min > 0 && max > 0)
                                fieldinformationVault.model = min + "-" + max.ToString();
                            else if (max == 0)
                                fieldinformationVault.model = "> " + min.ToString();
                            else if (min == 0)
                                fieldinformationVault.model = "< " + max.ToString();
                            else if (min == 0 && max == 0)
                                fieldinformationVault.model = "> " + min.ToString();

                            fieldinformationVault.model += fieldinformationVault.unitModel;
                        }
                        else
                        {
                            var noderoot = jObect.SelectToken("employment");
                            string strdefault = noderoot["default"].ToString();
                            var employmentdefault = ((JArray) noderoot["value"])
                                .Where(x => x["description"].ToString() == strdefault).FirstOrDefault();
                            int salary = GH.Util.Common.ConvertToInt(employmentdefault["salary"].ToString());

                            int index = -1;
                            var modelarrays = (List<List<int?>>) fieldinformationVault.modelarrays;
                            for (index = 0; index < modelarrays.Count(); index++)
                            {
                                int min = modelarrays[index][0].HasValue ? modelarrays[index][0].Value : 0;

                                int max = modelarrays[index][1].HasValue ? modelarrays[index][1].Value : int.MaxValue;

                                if (min <= salary && max >= salary)
                                {
                                    break;
                                }
                            }
                            if (index >= modelarrays.Count())
                                index = -1;
                            fieldinformationVault.model = index.ToString();
                        }
                    }
                    catch
                    {
                        fieldinformationVault.model = "-1";
                    }
                    return fieldinformationVault;
                case ".address.addressHistory":
                    // fieldinformationVault.unitModel = field["unitModel"].AsString;
                    try
                    {
                        if (ischart)
                        {
                            fieldinformationVault.model = "";
                        }
                        else
                        {
                            fieldinformationVault.model = field["model"].AsInt32.ToString();
                            var nodegroupAddresscurrent = jObect.SelectToken("groupAddress.value.currentAddress");
                            int yearpast = GH.Util.Common.ConvertToInt(fieldinformationVault.model);
                            var arraysaddress = (JArray) nodegroupAddresscurrent["value"];

                            var currentaddress = arraysaddress.Where(x => !string.IsNullOrEmpty(x["endDate"].ToString())
                                                                          && (Convert.ToDateTime(
                                                                                  x["endDate"].ToString()) >=
                                                                              DateTime.Now.AddYears(0 - yearpast)))
                                .Select(x => x["description"].ToString()).ToList();
                            fieldinformationVault.modelarrays = currentaddress;
                        }
                    }
                    catch
                    {
                        fieldinformationVault.modelarrays = new List<string>();
                    }
                    return fieldinformationVault;
                case ".basicInformation.emergencyContact":
                    List<string> listEmergency = new List<string>();
                    try
                    {
                        listEmergency = new NetworkService().GetEmergencyContactByUserid(userid)
                            .Select(x => x.DisplayName).ToList();
                    }
                    catch
                    {
                        listEmergency = new List<string>();
                    }
                    if (listEmergency.Count > 0)
                        fieldinformationVault.model = listEmergency[0];
                    fieldinformationVault.options = listEmergency;
                    return fieldinformationVault;
            }

            fieldinformationVault = GetvalueFromFieldinformationVault(fieldinformationVault, jObect);
            return fieldinformationVault;
        }

        public List<FieldinformationVault> getInformationvaultforDRFI(string userid, string jsonfields)
        {
            List<FieldinformationVault> listFieldinformationVault = new List<FieldinformationVault>();
            var fieldsregisdata = new List<FieldinformationVault>();
            var fieldsOfRegistration = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonArray>(jsonfields);
            var informationVault = GetInformationVaultByUserId(userid);
            return getInformationvaultfromfieldsvault(informationVault, fieldsOfRegistration, fieldsregisdata);
        }

        public List<FieldinformationVault> getInformationvaultforcampaign(BsonDocument vault, string campaignId)
        {
            List<FieldinformationVault> listFieldinformationVault = new List<FieldinformationVault>();
            var informationVault = vault;
            CampaignBusinessLogic campaignBusinessLogic = new CampaignBusinessLogic();
            var campaign = campaignBusinessLogic.GetCampaignById(campaignId);
            var userid = informationVault["userId"].AsString;
            var fieldsregisdata = new PostBusinessLogic().GetDataRegisofUser(campaignId, userid);
            var fieldsOfRegistration = new BsonArray();
            if (campaign != null)
            {
                fieldsOfRegistration =
                    (campaign["campaign"].ToBsonDocument().Names.FirstOrDefault(x => x.Contains("fields")) == null)
                        ? null
                        : campaign["campaign"]["fields"].AsBsonArray;
            }
            if (fieldsOfRegistration == null)
                return listFieldinformationVault;

            return getInformationvaultfromfieldsvault(vault, fieldsOfRegistration, fieldsregisdata);
        }


        public List<FieldinformationVault> getInformationvaultfromfieldsvault(BsonDocument vault,
            BsonArray fieldsOfRegistration, List<FieldinformationVault> fieldsregisdata)
        {
            List<FieldinformationVault> listFieldinformationVault = new List<FieldinformationVault>();
            var informationVault = vault;
            if (informationVault != null)
            {
                var userid = informationVault["userId"].AsString;
                var jsonString = informationVault.ToJson();
                jsonString = jsonString.Replace("ObjectId(", "").Replace(")", "");
                JObject jObect = JObject.Parse(jsonString);

                if (fieldsOfRegistration == null)
                    return listFieldinformationVault;

                try
                {
                    var listqa = fieldsOfRegistration.Where(x => x["jsPath"].AsString.StartsWith("Custom.")).ToList();
                    foreach (var item in listqa)
                    {
                        var field = (BsonDocument) item;
                        FieldinformationVault fieldinformationVault = new FieldinformationVault();
                        fieldinformationVault.choices = field["choices"].AsBoolean;
                        fieldinformationVault.qa = field["qa"].AsBoolean;
                        fieldinformationVault.optional = field["optional"].AsBoolean;

                        fieldinformationVault.jsPath = field["path"].AsString;
                        fieldinformationVault.type = (field.GetValue("type", "")??"").ToString();
                        try
                        {
                            if (fieldinformationVault.type == "doc")
                            {
                                fieldinformationVault.pathfile = "Content/vault/documents/" + userid + "/";
                                var nodeducument = jObect.SelectToken("document");
                                var listdocument = ((JArray) nodeducument["value"])
                                    .Where(x => x["jsPath"].ToString() == fieldinformationVault.jsPath)
                                    .Select(y => y["name"].ToString()).ToList();

                                fieldinformationVault.modelarrays = listdocument;
                            }
                            else if (fieldinformationVault.type == "select")
                            {
                                fieldinformationVault.options =
                                    field["options"].AsBsonArray.Select(x => x.ToString()).ToList();
                            }
                        }
                        catch
                        {
                        }

                        fieldinformationVault.displayName = field.GetValue("displayName", "").ToString();

                        var model = field.AsBsonDocument.GetValue("model", null);
                        if (model != null)
                        {
                            fieldinformationVault.model = "";
                            fieldinformationVault.modelarrays = model.AsBsonArray.Select(y => new
                            {
                                //                            value = y["value"].AsString
                                value = y.AsBsonDocument.GetValue("value", string.Empty).AsString
                            }).ToList();
                        }
                        else
                        {
                            fieldinformationVault.model = null;
                        }
                        listFieldinformationVault.Add(fieldinformationVault);
                    }
                }
                catch
                {
                }
                try
                {
                    var groupaddress = fieldsOfRegistration
                        .Where(x => x["jsPath"].AsString.StartsWith(".governmentID."))
                        .Select(x => x["jsPath"].AsString.Trim('.').Split('.')[1]).Distinct().ToList();
                    foreach (var namecurrentaddress in groupaddress)
                    {
                        try
                        {
                            var listaddressfields = fieldsOfRegistration.Where(x =>
                                x["jsPath"].AsString.StartsWith(".governmentID." + namecurrentaddress)).ToList();
                            if (listaddressfields.Count() > 0)
                            {
                                var nodeaddress = jObect.SelectToken("groupGovernmentID");
                                var labelgroupGovernmentID = nodeaddress["label"].ToString();

                                var nodeaddresscurrent = nodeaddress["value"][namecurrentaddress];
                                var labelcurrent = nodeaddresscurrent["label"].ToString();
                                if (labelcurrent == "National Identity Card")
                                {
                                    labelcurrent = "National ID";
                                }

                                var defaultcurrentaddress = nodeaddresscurrent["default"].ToString();
                                JArray arrayaddress = (JArray) nodeaddresscurrent["value"];

                                var listaddressdefault = arrayaddress
                                    .Where(x => x["description"].ToString() == defaultcurrentaddress).ToList();
                                if (listaddressdefault.Count > 0)
                                {
                                    var defaultaddress = listaddressdefault.FirstOrDefault();

                                    foreach (var item in listaddressfields)
                                    {
                                        var addressnodename = item["jsPath"].AsString.Trim('.').Split('.').ToList()[2];
                                        var field = (BsonDocument) item;
                                        FieldinformationVault fieldinformationVault = new FieldinformationVault();
                                        fieldinformationVault.jsPath = field["jsPath"].AsString;
                                        fieldinformationVault.label = field.GetValue("label", "").ToString();
                                        fieldinformationVault.optional = field["optional"].AsBoolean;
                                        fieldinformationVault.type = (field.GetValue("type", "")??"").ToString();
                                        fieldinformationVault.displayName = field.GetValue("displayName", "").ToString();

                                        try
                                        {
                                            fieldinformationVault.options = field["options"].AsString;
                                            if (fieldinformationVault.type == "doc")
                                            {
                                                fieldinformationVault.pathfile =
                                                    "Content/vault/documents/" + userid + "/";
                                                fieldinformationVault.pathddocument =
                                                    "/" + labelgroupGovernmentID + "/" + labelcurrent + "/" +
                                                    defaultaddress["_id"];
                                                var nodeducument = jObect.SelectToken("document");
                                                var listdocument = ((JArray) nodeducument["value"])
                                                    .Where(x => x["jsPath"].ToString() ==
                                                                fieldinformationVault.pathddocument)
                                                    .Select(y => y["name"].ToString()).ToList();
                                                fieldinformationVault.modelarrays = listdocument;
                                            }
                                            else if (fieldinformationVault.type == "select")
                                            {
                                                fieldinformationVault.options = field["options"].AsBsonArray
                                                    .Select(x => x.ToString()).ToList();
                                            }
                                        }
                                        catch
                                        {
                                        }

                                        fieldinformationVault =
                                            GetvalueFromFieldinformationVault(fieldinformationVault, defaultaddress);
                                        listFieldinformationVault.Add(fieldinformationVault);
                                    }
                                }
                                else
                                {
                                    foreach (var item in listaddressfields)
                                    {
                                        var field = (BsonDocument) item;
                                        FieldinformationVault fieldinformationVault = new FieldinformationVault();
                                        fieldinformationVault.jsPath = field["jsPath"].AsString;
                                        fieldinformationVault.label = field.GetValue("label", "").ToString();
                                        fieldinformationVault.optional = field["optional"].AsBoolean;
                                        fieldinformationVault.type = (field.GetValue("type", "") ?? "").ToString();
                                        fieldinformationVault.displayName = field.GetValue("displayName", "").ToString();
                                        fieldinformationVault.model = "";
                                        if (fieldinformationVault.type == "select")
                                        {
                                            fieldinformationVault.options = field["options"]
                                                .AsBsonArray.Select(x => x.ToString())
                                                .ToList();
                                        }
                                        else
                                        {
                                            fieldinformationVault.options = field["options"].ToString();
                                        }

                                        listFieldinformationVault.Add(fieldinformationVault);
                                    }
                                }
                            }
                        }
                        catch
                        {
                            
                        }
                    }
                    //eeeee
                } 
                catch
                {
                }


                try
                {
                    //contact
                    var groupaddress = fieldsOfRegistration.Where(x => x["jsPath"].AsString.StartsWith(".contact."))
                        .Select(x => x["jsPath"].AsString.Trim('.').Split('.')[1]).Distinct().ToList();
                    foreach (var namecurrentaddress in groupaddress)
                    {
                        try
                        {
                            var listaddressfields = fieldsOfRegistration
                                .Where(x => x["jsPath"].AsString.Equals(".contact." + namecurrentaddress)).ToList();
                            if (listaddressfields.Count() > 0)
                            {
                                var nodeaddress = jObect.SelectToken("contact");
                                var nodeaddresscurrent = nodeaddress["value"][namecurrentaddress];
                                string node = namecurrentaddress != "email" && namecurrentaddress != "officeEmail"
                                    ? "phone"
                                    : "";
                                JArray arrayaddress = (JArray) nodeaddresscurrent["value"];
                                if (namecurrentaddress == "mobile" && arrayaddress.Count == 0)
                                {
                                    foreach (var item in listaddressfields)
                                    {
                                        var field = (BsonDocument) item;
                                        FieldinformationVault fieldinformationVault = new FieldinformationVault();
                                        fieldinformationVault.jsPath = field["jsPath"].AsString;
                                        fieldinformationVault.label = field.GetValue("label","").ToString();
                                        fieldinformationVault.optional = field["optional"].AsBoolean;
                                        fieldinformationVault.type = (field.GetValue("type", "")??"").ToString();
                                        fieldinformationVault.options = node;
                                        fieldinformationVault.displayName = field.GetValue("displayName", "").ToString();
                                        fieldinformationVault.model =
                                            nodeaddress["value"]["profilePhone"]["value"].ToString();
                                        listFieldinformationVault.Add(fieldinformationVault);
                                    }
                                    continue;
                                }
                                if (namecurrentaddress == "email" && arrayaddress.Count == 0)
                                {
                                    foreach (var item in listaddressfields)
                                    {
                                        var field = (BsonDocument) item;
                                        FieldinformationVault fieldinformationVault = new FieldinformationVault();
                                        fieldinformationVault.jsPath = field["jsPath"].AsString;
                                        fieldinformationVault.label = field.GetValue("label","").ToString();
                                        fieldinformationVault.optional = field["optional"].AsBoolean;
                                        fieldinformationVault.type = (field.GetValue("type", "")??"").ToString();
                                        fieldinformationVault.options = node;
                                        fieldinformationVault.displayName = field.GetValue("displayName", "").ToString();
                                        fieldinformationVault.model =
                                            nodeaddress["value"]["profileEmail"]["value"].ToString();
                                        listFieldinformationVault.Add(fieldinformationVault);
                                    }
                                    continue;
                                }
                                if (namecurrentaddress == "officeEmail" && arrayaddress.Count == 0)
                                {
                                    foreach (var item in listaddressfields)
                                    {
                                        var field = (BsonDocument) item;
                                        FieldinformationVault fieldinformationVault = new FieldinformationVault();
                                        fieldinformationVault.jsPath = field["jsPath"].AsString;
                                        fieldinformationVault.label = field.GetValue("label","").ToString();
                                        fieldinformationVault.optional = field["optional"].AsBoolean;
                                        fieldinformationVault.type = (field.GetValue("type", "")??"").ToString();
                                        fieldinformationVault.options = node;
                                        fieldinformationVault.displayName = field.GetValue("displayName", "").ToString();
                                        fieldinformationVault.model =
                                            nodeaddress["value"]["profileEmail"]["value"].ToString();
                                        listFieldinformationVault.Add(fieldinformationVault);
                                    }
                                    continue;
                                }
                                var defaultcurrentaddress = "";
                                try
                                {
                                    defaultcurrentaddress = nodeaddresscurrent["default"].ToString();
                                }
                                catch (Exception ex)
                                {
                                }

                                var listaddressdefault = arrayaddress
                                    .Where(x => x["value"].ToString() == defaultcurrentaddress).ToList();
                                if (listaddressdefault.Count > 0)
                                {
                                    var defaultaddress = listaddressdefault.FirstOrDefault();

                                    foreach (var item in listaddressfields)
                                    {
                                        var field = (BsonDocument) item;
                                        FieldinformationVault fieldinformationVault = new FieldinformationVault();
                                        fieldinformationVault.jsPath = field["jsPath"].AsString;
                                        fieldinformationVault.label = field.GetValue("label","").ToString();
                                        fieldinformationVault.optional = field["optional"].AsBoolean;
                                        fieldinformationVault.type = (field.GetValue("type", "")??"").ToString();
                                        fieldinformationVault.options = node;
                                        fieldinformationVault.displayName = field.GetValue("displayName", "").ToString();
                                        fieldinformationVault.model = defaultaddress["value"].ToString();
                                        listFieldinformationVault.Add(fieldinformationVault);
                                    }
                                }
                                else
                                {
                                    foreach (var item in listaddressfields)
                                    {
                                        var field = (BsonDocument) item;
                                        FieldinformationVault fieldinformationVault = new FieldinformationVault();
                                        fieldinformationVault.jsPath = field["jsPath"].AsString;
                                        fieldinformationVault.label = field.GetValue("label","").ToString();
                                        fieldinformationVault.optional = field["optional"].AsBoolean;
                                        fieldinformationVault.type = (field.GetValue("type", "")??"").ToString();
                                        fieldinformationVault.displayName = field.GetValue("displayName", "").ToString();
                                        fieldinformationVault.model = "";
                                        fieldinformationVault.options = node;
                                        listFieldinformationVault.Add(fieldinformationVault);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
                catch
                {
                }
                try
                {
                    var groupaddress = fieldsOfRegistration
                        .Where(x => x["jsPath"].AsString.StartsWith(".address.") &&
                                    !x["jsPath"].AsString.StartsWith(".address.addressHistory"))
                        .Select(x => x["jsPath"].AsString.Trim('.').Split('.')[1]).Distinct().ToList();
                    foreach (var namecurrentaddress in groupaddress)
                    {
                        var listaddressfields = fieldsOfRegistration
                            .Where(x => x["jsPath"].AsString.StartsWith(".address." + namecurrentaddress)).ToList();
                        if (listaddressfields.Count() > 0)
                        {
                            var nodeaddress = jObect.SelectToken("groupAddress");
                            var nodeaddresscurrent = nodeaddress["value"][namecurrentaddress];
                            var defaultcurrentaddress = nodeaddresscurrent["default"].ToString();
                            JArray arrayaddress = (JArray) nodeaddresscurrent["value"];

                            var listaddressdefault = arrayaddress
                                .Where(x => x["description"].ToString() == defaultcurrentaddress).ToList();
                            if (listaddressdefault.Count > 0)
                            {
                                var defaultaddress = listaddressdefault.FirstOrDefault();

                                foreach (var item in listaddressfields)
                                {
                                    var addressnodename = item["jsPath"].AsString.Trim('.').Split('.').ToList()[2];
                                    var field = (BsonDocument) item;
                                    FieldinformationVault fieldinformationVault = new FieldinformationVault();
                                    fieldinformationVault.jsPath = field["jsPath"].AsString;
                                    fieldinformationVault.label = field.GetValue("label","").AsString;
                                    fieldinformationVault.optional = field["optional"].AsBoolean;
                                    fieldinformationVault.type = (field.GetValue("type", "")??"").ToString();
                                    fieldinformationVault.displayName = field.GetValue("displayName", "").ToString();
                                    fieldinformationVault =
                                        GetvalueFromFieldinformationVault(fieldinformationVault, defaultaddress);
                                    listFieldinformationVault.Add(fieldinformationVault);
                                }
                            }
                            else
                            {
                                foreach (var item in listaddressfields)
                                {
                                    var field = (BsonDocument) item;
                                    FieldinformationVault fieldinformationVault = new FieldinformationVault();
                                    fieldinformationVault.jsPath = field["jsPath"].AsString;
                                    fieldinformationVault.label = field.GetValue("label","").AsString;
                                    fieldinformationVault.optional = field["optional"].AsBoolean;
                                    fieldinformationVault.type = (field.GetValue("type", "")??"").ToString();
                                    fieldinformationVault.displayName = field.GetValue("displayName", "").ToString();
                                    fieldinformationVault.model = "";
                                    listFieldinformationVault.Add(fieldinformationVault);
                                }
                            }
                        }
                    }
                }
                catch
                {
                }
                try
                {
                    var listeducationfields =
                        fieldsOfRegistration.Where(x => x["jsPath"].AsString.StartsWith(".education"));
                    if (listeducationfields.Count() > 0)
                    {
                        var nodeeducation = jObect.SelectToken("education");
                        JArray arrayeducation = (JArray) nodeeducation["value"];
                        var defaultName = nodeeducation["default"].ToString();
                        if (arrayeducation.Where(x => (x["description"].ToString() == defaultName)).Count() > 0)
                        {
                            var educationpedit = arrayeducation.Where
                                (x => x["description"].ToString() == defaultName).FirstOrDefault();

                            foreach (var item in listeducationfields)
                            {
                                var field = (BsonDocument) item;
                                FieldinformationVault fieldinformationVault = new FieldinformationVault();
                                fieldinformationVault.jsPath = field["jsPath"].AsString;
                                fieldinformationVault.label = field.GetValue("label","").AsString;
                                fieldinformationVault.optional = field["optional"].AsBoolean;
                                fieldinformationVault.type = (field.GetValue("type", "")??"").ToString();
                                try
                                {
                                    if (fieldinformationVault.type == "doc")
                                    {
                                        fieldinformationVault.pathfile = "Content/vault/documents/" + userid + "/";
                                        fieldinformationVault.pathddocument =
                                            "/" + "Education" + "/" + educationpedit["_id"];
                                        var nodeducument = jObect.SelectToken("document");
                                        var listdocument = ((JArray) nodeducument["value"])
                                            .Where(x => x["jsPath"].ToString() == fieldinformationVault.pathddocument)
                                            .Select(y => y["name"].ToString()).ToList();
                                        fieldinformationVault.modelarrays = listdocument;
                                    }
                                    else if (fieldinformationVault.type == "select")
                                    {
                                        fieldinformationVault.options = field["options"].AsBsonArray
                                            .Select(x => x.ToString()).ToList();
                                    }
                                }
                                catch
                                {
                                }
                                fieldinformationVault.displayName = field.GetValue("displayName", "").ToString();
                                fieldinformationVault =
                                    GetvalueFromFieldinformationVault(fieldinformationVault, educationpedit);
                                listFieldinformationVault.Add(fieldinformationVault);
                            }
                        }
                        else
                        {
                            foreach (var item in listeducationfields)
                            {
                                var field = (BsonDocument) item;
                                FieldinformationVault fieldinformationVault = new FieldinformationVault();
                                fieldinformationVault.jsPath = field["jsPath"].AsString;
                                fieldinformationVault.label = field.GetValue("label","").AsString;
                                fieldinformationVault.optional = field["optional"].AsBoolean;
                                fieldinformationVault.type = (field.GetValue("type", "")??"").ToString();
                                fieldinformationVault.displayName = field.GetValue("displayName", "").ToString();
                                fieldinformationVault.model = "";
                                listFieldinformationVault.Add(fieldinformationVault);
                            }
                        }
                    }
                }
                catch
                {
                }
                try
                {
                    var listeducationfields =
                        fieldsOfRegistration.Where(x => x["jsPath"].AsString.StartsWith(".employment"));
                    if (listeducationfields.Count() > 0)
                    {
                        var nodeeducation = jObect.SelectToken("employment");
                        JArray arrayeducation = (JArray) nodeeducation["value"];
                        var defaultName = nodeeducation["default"].ToString();
                        if (arrayeducation.Where(x => (x["description"].ToString() == defaultName)).Count() > 0)
                        {
                            var educationpedit = arrayeducation.Where
                                (x => x["description"].ToString() == defaultName).FirstOrDefault();

                            foreach (var item in listeducationfields)
                            {
                                var field = (BsonDocument) item;
                                FieldinformationVault fieldinformationVault = new FieldinformationVault();
                                fieldinformationVault.jsPath = field["jsPath"].AsString;
                                fieldinformationVault.label = field.GetValue("label","").AsString;
                                fieldinformationVault.optional = field["optional"].AsBoolean;
                                fieldinformationVault.type = (field.GetValue("type", "")??"").ToString();
                                try
                                {
                                    if (fieldinformationVault.type == "doc")
                                    {
                                        fieldinformationVault.pathfile = "Content/vault/documents/" + userid + "/";
                                        fieldinformationVault.pathddocument =
                                            "/" + "Employment" + "/" + educationpedit["_id"];
                                        var nodeducument = jObect.SelectToken("document");
                                        var listdocument = ((JArray) nodeducument["value"])
                                            .Where(x => x["jsPath"].ToString() == fieldinformationVault.pathddocument)
                                            .Select(y => y["name"].ToString()).ToList();
                                        fieldinformationVault.modelarrays = listdocument;
                                    }
                                    else if (fieldinformationVault.type == "select")
                                    {
                                        fieldinformationVault.options = field["options"].AsBsonArray
                                            .Select(x => x.ToString()).ToList();
                                    }
                                }
                                catch
                                {
                                }
                                fieldinformationVault.displayName = field.GetValue("displayName", "").ToString();
                                fieldinformationVault =
                                    GetvalueFromFieldinformationVault(fieldinformationVault, educationpedit);
                                listFieldinformationVault.Add(fieldinformationVault);
                            }
                        }
                        else
                        {
                            foreach (var item in listeducationfields)
                            {
                                var field = (BsonDocument) item;
                                FieldinformationVault fieldinformationVault = new FieldinformationVault();
                                fieldinformationVault.jsPath = field["jsPath"].AsString;
                                fieldinformationVault.label = field.GetValue("label","").AsString;
                                fieldinformationVault.optional = field["optional"].AsBoolean;
                                fieldinformationVault.type = (field.GetValue("type", "")??"").ToString();
                                fieldinformationVault.displayName = field.GetValue("displayName", "").ToString();
                                fieldinformationVault.model = "";
                                listFieldinformationVault.Add(fieldinformationVault);
                            }
                        }
                    }
                }
                catch
                {
                }
                try
                {
                    var listmemberhsipfields =
                        fieldsOfRegistration.Where(x => x["jsPath"].AsString.StartsWith(".membership"));
                    if (listmemberhsipfields.Count() > 0)
                    {
                        var nodemembership = jObect.SelectToken("membership");
                        JArray arraymembsership = (JArray) nodemembership["value"];
                        var businessName =
                            listmemberhsipfields.FirstOrDefault(x => x["jsPath"].AsString == ".membership.businessName")
                                ["options"].AsString;
                        var membershipProgramName =
                            listmemberhsipfields.FirstOrDefault(x =>
                                x["jsPath"].AsString == ".membership.membershipProgramName")["options"].AsString;
                        if (arraymembsership.Where(x =>
                                (x["businessName"].ToString() == businessName) &&
                                (x["membershipProgramName"].ToString() == membershipProgramName)).Count() > 0)
                        {
                            var membershipedit = arraymembsership.Where(x =>
                                (x["businessName"].ToString() == businessName) &&
                                (x["membershipProgramName"].ToString() == membershipProgramName)).FirstOrDefault();

                            foreach (var item in listmemberhsipfields)
                            {
                                var membershipnodename = item["jsPath"].AsString.Trim('.').Split('.').ToList()[1];
                                var field = (BsonDocument) item;
                                FieldinformationVault fieldinformationVault = new FieldinformationVault();
                                fieldinformationVault.jsPath = field["jsPath"].AsString;
                                fieldinformationVault.label = field.GetValue("label","").AsString;
                                fieldinformationVault.optional = field["optional"].AsBoolean;
                                fieldinformationVault.type = (field.GetValue("type", "")??"").ToString();
                                fieldinformationVault.displayName = field.GetValue("displayName", "").ToString();
                                fieldinformationVault.membership = "true";
                                fieldinformationVault.model = membershipedit[membershipnodename].ToString();
                                listFieldinformationVault.Add(fieldinformationVault);
                            }
                        }
                        else
                        {
                            foreach (var item in listmemberhsipfields)
                            {
                                var membershipnodename = item["jsPath"].AsString.Trim('.').Split('.').ToList()[1];
                                var field = (BsonDocument) item;
                                FieldinformationVault fieldinformationVault = new FieldinformationVault();
                                fieldinformationVault.jsPath = field["jsPath"].AsString;
                                fieldinformationVault.label = field.GetValue("label","").AsString;
                                fieldinformationVault.optional = field["optional"].AsBoolean;
                                fieldinformationVault.type = (field.GetValue("type", "")??"").ToString();
                                fieldinformationVault.displayName = field.GetValue("displayName", "").ToString();
                                fieldinformationVault.membership = "true";
                                fieldinformationVault.model = "";
                                if (fieldinformationVault.jsPath == ".membership.businessName" ||
                                    fieldinformationVault.jsPath == ".membership.membershipProgramName")
                                    fieldinformationVault.model = field["options"].AsString;
                                listFieldinformationVault.Add(fieldinformationVault);
                            }
                        }
                    }
                }
                catch
                {
                }
                try
                {
                    var fields = fieldsOfRegistration.Where(x =>
                        !x["jsPath"].AsString.StartsWith(".employment") &&
                        !x["jsPath"].AsString.StartsWith(".education") &&
                        !x["jsPath"].AsString.StartsWith(".governmentID")
                        && !x["jsPath"].AsString.StartsWith("Custom.") &&
                        !x["jsPath"].AsString.StartsWith(".membership") &&
                        !x["jsPath"].AsString.StartsWith(".contact") &&
                        (!x["jsPath"].AsString.StartsWith(".address") ||
                         x["jsPath"].AsString.Equals(".address.addressHistory"))).ToList();
                    foreach (var bsonValue in fields)
                    {
                        try
                        {
                            var field = bsonValue.AsBsonDocument;
                            var type = field.GetValue("type","");
                            if (type.IsBsonNull) type = null;
                            var label = field.GetValue("displayName","");
                            if (label.IsBsonNull) label = null;
                            FieldinformationVault fieldinformationVault = new FieldinformationVault();
                            fieldinformationVault.jsPath = field["jsPath"].AsString;
                            fieldinformationVault.label = fieldinformationVault.displayName = label?.ToString();
                            fieldinformationVault.type = type?.ToString();
                            
                            listFieldinformationVault.Add(GetvalueFromFieldinformationVault(informationVault,
                                fieldinformationVault, field, fieldsregisdata));
                        }
                        catch
                        {
                        }
                    }
                }
                catch
                {
                }
            }

            foreach (var field in listFieldinformationVault)
            {
                var vaultField = VaultService.GetField(field.jsPath);
                if (vaultField == null) continue;
                if (field.type == null) field.type = vaultField.Type;
                if (field.label == null) field.label = vaultField.Title;
                if (field.displayName == null) field.displayName = vaultField.Title;
                if (field.options == null) field.options = JsonConvert.SerializeObject(vaultField.Options);
            }

            return listFieldinformationVault;
        }

        #region GetValue

        public List<FieldinformationVault> getOthers(List<FieldinformationVault> fields)
        {
            var rs = new List<FieldinformationVault>();
            try
            {
                var lstField = fields.Where(x => x.jsPath.ToLower().StartsWith(".others")).ToList();
                foreach (var field in lstField)
                {
                    rs.Add(field);
                }
            }
            catch (Exception ex)
            {
                Log.Error("getOthers: " + ex.ToString());
            }
            return rs;
        }

        public List<FieldinformationVault> getBasic(List<FieldinformationVault> fields)
        {
            var rs = new List<FieldinformationVault>();
            try
            {
                var lstField = fields.Where(x => x.jsPath.ToLower().StartsWith(".basicinformation".ToLower())).ToList();
                foreach (var field in lstField)
                {
                    if (field.type == "datecombo")
                    {
                        field.model = Convert.ToDateTime(field.model).ToString("yyyy-MM-dd");
                    }
                    rs.Add(field);
                }
            }
            catch (Exception ex)
            {
                Log.Error("getBasic: " + ex.ToString());
            }
            return rs;
        }

        public List<FieldinformationVault> getContact(List<FieldinformationVault> fields)
        {
            var rs = new List<FieldinformationVault>();
            try
            {
                var lstField = fields.Where(x => x.jsPath.StartsWith(".contact")).ToList();
                foreach (var field in lstField)
                {
                    rs.Add(field);
                }
            }
            catch (Exception ex)
            {
                Log.Error("getContact: " + ex.ToString());
            }
            return rs;
        }

        public List<FieldinformationVault> getQA(List<FieldinformationVault> campaignFields, string userId)
        {
            var rs = new List<FieldinformationVault>();
            try
            {
                var fields = campaignFields.Where(x => x.qa).ToList();
                foreach (var field in fields)
                {
                    if (field.type == "doc")
                    {
                        field.pathfile = "/Content/vault/documents/" + userId + "/";
                        field.modelarrays = JsonConvert.DeserializeObject<List<string>>(field.modelarraysstr);
                    }
                    rs.Add(field);
                }
            }
            catch (Exception ex)
            {
                Log.Error("getQA: " + ex.ToString());
            }
            return rs;
        }

        public List<FieldinformationVault> getEducation(BsonArray BsonArrayPostfields, JObject jObject, string userId)
        {
            var rs = new List<FieldinformationVault>();
            try
            {
                var fields = BsonArrayPostfields.Where(x => x["jsPath"].AsString.StartsWith(".education"));
                if (fields.Count() > 0)
                {
                    var nodeeducation = jObject.SelectToken("education");
                    JArray arrayEducation = (JArray) nodeeducation["value"];
                    var defaultName = nodeeducation["default"].ToString();
                    if (arrayEducation.Where(x => (x["description"].ToString() == defaultName)).Count() > 0)
                    {
                        var educationpedit = arrayEducation.Where
                            (x => x["description"].ToString() == defaultName).FirstOrDefault();
                        foreach (var item in fields)
                        {
                            var field = (BsonDocument) item;
                            FieldinformationVault fieldinformationVault = new FieldinformationVault();
                            fieldinformationVault.jsPath = field["jsPath"].AsString;
                            fieldinformationVault.label = field["label"].AsString;
                            fieldinformationVault.optional = field["optional"].AsBoolean;
                            fieldinformationVault.type = (field.GetValue("type", "")??"").ToString();
                            try
                            {
                                if (fieldinformationVault.type == "doc")
                                {
                                    fieldinformationVault.pathfile = "Content/vault/documents/" + userId + "/";
                                    fieldinformationVault.pathddocument =
                                        "/" + "Education" + "/" + educationpedit["_id"];
                                    var nodeducument = jObject.SelectToken("document");
                                    var listdocument = ((JArray) nodeducument["value"])
                                        .Where(x => x["jsPath"].ToString() == fieldinformationVault.pathddocument)
                                        .Select(y => y["name"].ToString()).ToList();
                                    fieldinformationVault.modelarrays = listdocument;
                                }
                                else if (fieldinformationVault.type == "select")
                                {
                                    fieldinformationVault.options =
                                        field["options"].AsBsonArray.Select(x => x.ToString()).ToList();
                                }
                            }
                            catch
                            {
                            }
                            fieldinformationVault.displayName = field.GetValue("displayName", "").ToString();
                            fieldinformationVault =
                                GetvalueFromFieldinformationVault(fieldinformationVault, educationpedit);
                            rs.Add(fieldinformationVault);
                        }
                    }
                    else
                    {
                        foreach (var item in fields)
                        {
                            var field = (BsonDocument) item;
                            FieldinformationVault fieldVault = new FieldinformationVault();
                            fieldVault.jsPath = field["jsPath"].AsString;
                            fieldVault.label = field["label"].AsString;
                            fieldVault.optional = field["optional"].AsBoolean;
                            fieldVault.type = field["type"].AsString;
                            fieldVault.displayName = field["displayName"].AsString;
                            fieldVault.model = "";
                            rs.Add(fieldVault);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("getEducation: " + ex.ToString());
                throw ex;
            }
            return rs;
        }

        public List<FieldinformationVault> getEmployment(BsonArray BsonArrayPostfields, JObject jObject, string userId)
        {
            var rs = new List<FieldinformationVault>();
            try
            {
                var fields = BsonArrayPostfields.Where(x => x["jsPath"].AsString.StartsWith(".employment"));
                if (fields.Count() > 0)
                {
                    var node = jObject.SelectToken("employment");
                    JArray arrayEducation = (JArray) node["value"];
                    var defaultName = node["default"].ToString();

                    if (arrayEducation.Where(x => (x["description"].ToString() == defaultName)).Count() > 0)
                    {
                        var educationpedit = arrayEducation.Where
                            (x => x["description"].ToString() == defaultName).FirstOrDefault();

                        foreach (var item in fields)
                        {
                            var field = (BsonDocument) item;
                            FieldinformationVault fieldVault = new FieldinformationVault();
                            fieldVault.jsPath = field["jsPath"].AsString;
                            fieldVault.label = field["label"].AsString;
                            fieldVault.optional = field["optional"].AsBoolean;
                            fieldVault.type = field["type"].AsString;
                            try
                            {
                                if (fieldVault.type == "doc")
                                {
                                    fieldVault.pathfile = "/Content/vault/documents/" + userId + "/";
                                    fieldVault.pathddocument = "/" + "Employment" + "/" + educationpedit["_id"];
                                    var nodeducument = jObject.SelectToken("document");
                                    var listdocument = ((JArray) nodeducument["value"])
                                        .Where(x => x["jsPath"].ToString() == fieldVault.pathddocument)
                                        .Select(y => y["name"].ToString()).ToList();
                                    fieldVault.modelarrays = listdocument;
                                }
                                else if (fieldVault.type == "select")
                                {
                                    fieldVault.options =
                                        field["options"].AsBsonArray.Select(x => x.ToString()).ToList();
                                }
                            }
                            catch
                            {
                            }
                            fieldVault.displayName = field["displayName"].AsString;
                            fieldVault = GetvalueFromFieldinformationVault(fieldVault, educationpedit);
                            rs.Add(fieldVault);
                        }
                    }
                    else
                    {
                        foreach (var item in fields)
                        {
                            var field = (BsonDocument) item;
                            FieldinformationVault fieldVault = new FieldinformationVault();
                            fieldVault.jsPath = field["jsPath"].AsString;
                            //fieldinformationVault.id = field["id"] is BsonNull?"": field["id"].AsString;
                            fieldVault.label = field["label"].AsString;
                            fieldVault.optional = field["optional"].AsBoolean;
                            fieldVault.type = field["type"].AsString;
                            fieldVault.displayName = field["displayName"].AsString;
                            fieldVault.model = "";
                            rs.Add(fieldVault);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("getEmployment: " + ex.ToString());
                throw ex;
            }

            return rs;
        }

        public List<FieldinformationVault> getGovernmentID(BsonArray BsonArrayPostfields, JObject jObject,
            string userId)
        {
            var rs = new List<FieldinformationVault>();
            try
            {
                var group = BsonArrayPostfields.Where(x => x["jsPath"].AsString.StartsWith(".governmentID."))
                    .Select(x => x["jsPath"].AsString.Trim('.').Split('.')[1]).Distinct().ToList();
                foreach (var formName in group)
                {
                    var fields = BsonArrayPostfields
                        .Where(x => x["jsPath"].AsString.StartsWith(".governmentID." + formName)).ToList();
                    if (fields.Count() > 0)
                    {
                        var node = jObject.SelectToken("groupGovernmentID");
                        var nodeForm = node["value"][formName];
                        var defaultFormName = nodeForm["default"].ToString();
                        JArray formValue = (JArray) nodeForm["value"];
                        var labelGroup = node["label"].ToString();
                        var labelForm = nodeForm["label"].ToString();
                        var listFormDefault = formValue.Where(x => x["description"].ToString() == defaultFormName)
                            .ToList();
                        if (listFormDefault.Count > 0)
                        {
                            var defaultForm = listFormDefault.FirstOrDefault();
                            foreach (var item in fields)
                            {
                                var addressnodename = item["jsPath"].AsString.Trim('.').Split('.').ToList()[2];
                                var field = (BsonDocument) item;
                                FieldinformationVault fieldVault = new FieldinformationVault();
                                fieldVault.jsPath = field["jsPath"].AsString;
                                fieldVault.label = field["label"].AsString;
                                fieldVault.optional = field["optional"].AsBoolean;
                                fieldVault.type = field["type"].AsString;
                                if (fieldVault.type == "doc")
                                {
                                    fieldVault.pathfile = "/Content/vault/documents/" + userId + "/";
                                    fieldVault.pathddocument =
                                        "/" + labelGroup + "/" + labelForm + "/" + defaultForm["_id"];
                                    var nodeducument = jObject.SelectToken("document");
                                    var listdocument = ((JArray) nodeducument["value"])
                                        .Where(x => x["jsPath"].ToString() == fieldVault.pathddocument)
                                        .Select(y => y["name"].ToString()).ToList();
                                    fieldVault.modelarrays = listdocument;
                                }

                                else if (fieldVault.type == "select")
                                {
                                    fieldVault.options =
                                        field["options"].AsBsonArray.Select(x => x.ToString()).ToList();
                                }
                                fieldVault.displayName = field["displayName"].AsString;
                                fieldVault = GetvalueFromFieldinformationVault(fieldVault, defaultForm);
                                rs.Add(fieldVault);
                            }
                        }
                        else
                        {
                            foreach (var item in fields)
                            {
                                var field = (BsonDocument) item;
                                FieldinformationVault fieldVault = new FieldinformationVault();
                                fieldVault.jsPath = field["jsPath"].AsString;
                                fieldVault.label = field["label"].AsString;
                                fieldVault.optional = field["optional"].AsBoolean;
                                fieldVault.type = field["type"].AsString;
                                if (fieldVault.type == "doc")
                                    break;
                                else if (fieldVault.type == "select")
                                {
                                    fieldVault.options =
                                        field["options"].AsBsonArray.Select(x => x.ToString()).ToList();
                                }
                                fieldVault.displayName = field["displayName"].AsString;
                                fieldVault.model = "";
                                rs.Add(fieldVault);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("getGovernmentID: " + ex.ToString());
                throw ex;
            }
            return rs;
        }

        public List<FieldinformationVault> getMembership(BsonArray BsonArrayPostfields, JObject jObject, string userId)
        {
            var rs = new List<FieldinformationVault>();
            try
            {
                var fields = BsonArrayPostfields.Where(x => x["jsPath"].AsString.StartsWith(".membership"));
                if (fields.Count() > 0)
                {
                    var node = jObject.SelectToken("membership");
                    JArray arrayForm = (JArray) node["value"];
                    var businessName =
                        fields.FirstOrDefault(x => x["jsPath"].AsString == ".membership.businessName")["value"]
                            .AsString;
                    var membershipProgramName =
                        fields.FirstOrDefault(x => x["jsPath"].AsString == ".membership.membershipProgramName")["value"]
                            .AsString;
                    if (arrayForm.Where(x =>
                            (x["businessName"].ToString() == businessName) &&
                            (x["membershipProgramName"].ToString() == membershipProgramName)).Count() > 0)
                    {
                        var form = arrayForm.Where(x =>
                            (x["businessName"].ToString() == businessName) &&
                            (x["membershipProgramName"].ToString() == membershipProgramName)).FirstOrDefault();
                        foreach (var item in fields)
                        {
                            var formName = item["jsPath"].AsString.Trim('.').Split('.').ToList()[1];
                            var field = (BsonDocument) item;
                            FieldinformationVault fieldVault = new FieldinformationVault();
                            fieldVault.jsPath = field["jsPath"].AsString;
                            fieldVault.label = field["label"].AsString;
                            fieldVault.optional = field["optional"].AsBoolean;
                            fieldVault.type = field["type"].AsString;
                            fieldVault.displayName = field["displayName"].AsString;
                            fieldVault.model = form[formName].ToString();
                            fieldVault.membership = "true";
                            rs.Add(fieldVault);
                        }
                    }
                    else
                    {
                        foreach (var item in fields)
                        {
                            var membershipnodename = item["jsPath"].AsString.Trim('.').Split('.').ToList()[1];
                            var field = (BsonDocument) item;
                            FieldinformationVault fieldVault = new FieldinformationVault();
                            fieldVault.jsPath = field["jsPath"].AsString;
                            fieldVault.label = field["label"].AsString;
                            fieldVault.optional = field["optional"].AsBoolean;
                            fieldVault.type = field["type"].AsString;
                            fieldVault.displayName = field["displayName"].AsString;
                            fieldVault.model = "";
                            fieldVault.membership = "true";
                            if (fieldVault.jsPath == ".membership.businessName" ||
                                fieldVault.jsPath == ".membership.membershipProgramName")
                                fieldVault.model = field["value"].AsString;
                            rs.Add(fieldVault);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("getMembership: " + ex.ToString());
                throw ex;
            }
            return rs;
        }

        public List<FieldinformationVault> getAddressHistory(List<FieldinformationVault> fields)
        {
            var rs = new List<FieldinformationVault>();
            try
            {
                var lstField = fields.Where(x =>
                    x.jsPath.StartsWith(".address.") && !x.jsPath.StartsWith(".address.addressHistory")).ToList();
                foreach (var fieldinformationVault in lstField)
                {
                    rs.Add(fieldinformationVault);
                }
            }
            catch (Exception ex)
            {
                Log.Error("getAddressHistory: " + ex.ToString());
                throw ex;
            }
            return rs;
        }

        #endregion GetValue

        public List<FieldinformationVault> getInformationvaultforcampaign(BsonDocument vault, BsonArray fieldsregis,
            List<FieldinformationVault> fieldsregisdata)
        {
            List<FieldinformationVault> listFieldinformationVault = new List<FieldinformationVault>();
            var informationVault = vault;
            if (informationVault != null)
            {
                var userid = informationVault["userId"].AsString;
                var jsonString = informationVault.ToJson();
                jsonString = jsonString.Replace("ObjectId(", "").Replace(")", "");
                JObject jObect = JObject.Parse(jsonString);
                CampaignBusinessLogic campaignBusinessLogic = new CampaignBusinessLogic();
                var fieldsOfRegistration = fieldsregis;
                if (fieldsOfRegistration == null)
                    return listFieldinformationVault;
                //Other
                var getFields = getOthers(fieldsregisdata);
                if (getFields != null)
                    listFieldinformationVault.AddRange(getFields);
                //Basic
                getFields = getBasic(fieldsregisdata);
                if (getFields != null)
                    listFieldinformationVault.AddRange(getFields);
                //Contact
                getFields = getContact(fieldsregisdata);
                if (getFields != null)
                    listFieldinformationVault.AddRange(getFields);
                //QA
                getFields = getQA(fieldsregisdata, userid);
                if (getFields != null)
                    listFieldinformationVault.AddRange(getFields);

                //Education
                getFields = getEducation(fieldsOfRegistration, jObect, userid);
                if (getFields != null)
                    listFieldinformationVault.AddRange(getFields);
                //Employment
                getFields = getEmployment(fieldsOfRegistration, jObect, userid);
                if (getFields != null)
                    listFieldinformationVault.AddRange(getFields);

                //GovernmentID
                getFields = getGovernmentID(fieldsOfRegistration, jObect, userid);
                if (getFields != null)
                    listFieldinformationVault.AddRange(getFields);
                //Membership
                getFields = getMembership(fieldsOfRegistration, jObect, userid);
                if (getFields != null)
                    listFieldinformationVault.AddRange(getFields);
                //GovernmentID
                getFields = getAddressHistory(fieldsregisdata);
                if (getFields != null)
                    listFieldinformationVault.AddRange(getFields);


                foreach (var bsonValue in fieldsOfRegistration.Where(x =>
                    !x["jsPath"].AsString.ToLower().StartsWith(".others") &&
                    !x["jsPath"].AsString.ToLower().StartsWith(".basicinformation") &&
                    !x["jsPath"].AsString.StartsWith(".contact") && !x["jsPath"].AsString.StartsWith(".employment") &&
                    !x["jsPath"].AsString.StartsWith(".education")
                    && !x["jsPath"].AsString.StartsWith(".governmentID") && !x["path"].AsString.StartsWith("Custom") &&
                    !x["jsPath"].AsString.StartsWith(".membership") &&
                    (!x["jsPath"].AsString.StartsWith(".address") ||
                     x["jsPath"].AsString.Equals(".address.addressHistory"))).ToList())
                {
                    try
                    {
                        var field = (BsonDocument) bsonValue;
                        FieldinformationVault fieldinformationVault = new FieldinformationVault();
                        fieldinformationVault.jsPath = field["jsPath"].AsString;
                        //fieldinformationVault.id = field["id"] is BsonNull?"": field["id"].AsString;
                        fieldinformationVault.label = field["label"].AsString;
                        fieldinformationVault.optional = field["optional"].AsBoolean;
                        fieldinformationVault.type = (field.GetValue("type", "")??"").ToString();
                        fieldinformationVault.displayName = field.GetValue("displayName", "").ToString();
                        listFieldinformationVault.Add(GetvalueFromFieldinformationVault(informationVault,
                            fieldinformationVault, field, fieldsregisdata, true));
                    }
                    catch
                    {
                    }
                }
            }
            var newlist = fieldsregis
                .Select(x => listFieldinformationVault.FirstOrDefault(y => y.jsPath == x["jsPath"].AsString)).ToList();
            return newlist;
        }

        public List<FieldinformationVault> getInformationvaultforcampaign(string campaignId)
        {
            var listFieldinformationVault = new List<FieldinformationVault>();
            CampaignBusinessLogic campaignBusinessLogic = new CampaignBusinessLogic();
            var campaign = campaignBusinessLogic.GetCampaignById(campaignId);
            var businessUserId = campaign["userId"].AsString;
            var vaultinformationtemplate = GetInformationVaultTemplate();
            return getInformationvaultforcampaign(vaultinformationtemplate, campaignId);
        }

        public List<FieldinformationVault> getInformationvaultforcampaign(string accountId, string campaignId)
        {
            var informationVault = GetInformationVaultByUserId(accountId);
            return getInformationvaultforcampaign(informationVault, campaignId);
        }

        public void UpdateInformationVaultById(JToken node, FieldinformationVault field)
        {
            switch (field.type)
            {
                case "location":
                    if (field.jsPath.StartsWith(".basicInformation"))
                    {
                        node["value"]["country"]["value"] = field.model;
                        if (field.options.ToString() != "nocity")
                            node["value"]["city"]["value"] = field.unitModel;
                    }
                    else
                    {
                        node["country"] = field.model;
                        if (field.options == null || field.options.ToString() != "nocity")
                            node["city"] = field.unitModel;
                    }
                    break;
                case "address":
                    node["addressLine"] = field.model + "" + field.unitModel;

                    break;

                case "numinput":
                    node["value"] = field.model;
                    node["unit"] = field.unitModel;
                    break;
                case "tagsinput":
                case "smartinput":
                    if (!string.IsNullOrEmpty(field.model))
                        node["value"] = JToken.Parse(JsonConvert.SerializeObject(field.model.Split(',').ToArray()));
                    break;

                default:
                    if (field.type == "date" || field.type == "datecombo")
                    {
                        if (!string.IsNullOrEmpty(field.model))
                            field.model = Convert.ToDateTime(field.model).ToString("yyyy-MM-dd");
                        else
                            field.model = "";
                    }

                    if (field.jsPath.StartsWith(".basicInformation") || field.jsPath.StartsWith(".others"))
                        node["value"] = field.model;
                    else
                    {
                        string nodename =
                            field.jsPath.Trim('.').Split('.')[field.jsPath.Trim('.').Split('.').Length - 1];
                        node[nodename] = field.model;
                    }

                    break;
            }
        }

        public void UpdateInformationVaultById(string accountId, List<FieldinformationVault> keyValues)
        {
            var informationVault = GetInformationVaultByUserId(accountId);
            if (informationVault != null)
            {
                var jsonString = informationVault.ToJson();
                jsonString = jsonString.Replace("ObjectId(", "").Replace(")", "");
                JObject jObect = JObject.Parse(jsonString);

                try
                {
                    if (keyValues.Where(x =>
                            x.jsPath.StartsWith(".governmentID") || x.jsPath.StartsWith(".groupGovernmentID")).Count() >
                        0)
                    {
                        var groupaddress = keyValues
                            .Where(x => x.jsPath.StartsWith(".governmentID.") ||
                                        x.jsPath.StartsWith(".groupGovernmentID"))
                            .Select(x => x.jsPath.Trim('.').Split('.')[1]).Distinct().ToList();
                        foreach (var namecurrentaddress in groupaddress)
                        {
                            var nodeaddress = jObect.SelectToken("groupGovernmentID");
                            var labelgroupGovernmentID = nodeaddress["label"].ToString();
                            nodeaddress["name"] = "GovernmentID";
                            var nodeaddresscurrent = nodeaddress["value"][namecurrentaddress];
                            var labelcurrent = nodeaddresscurrent["label"].ToString();
                            if (labelcurrent == "National Identity Card")
                                labelcurrent = "National ID";
                            nodeaddresscurrent["name"] = namecurrentaddress;
                            var defaultcurrentaddress = nodeaddresscurrent["default"].ToString();
                            JArray arrayaddress = (JArray) nodeaddresscurrent["value"];
                            var listaddressdefault = arrayaddress
                                .Where(x => x["description"].ToString() == defaultcurrentaddress).ToList();
                            if (listaddressdefault.Count() > 0)
                            {
                                var defaultaddress = listaddressdefault.FirstOrDefault();
                                foreach (var item in keyValues.Where(x =>
                                    x.jsPath.StartsWith(".governmentID." + namecurrentaddress) ||
                                    x.jsPath.StartsWith(".groupGovernmentID." + namecurrentaddress)))
                                {
                                    switch (item.type)
                                    {
                                        case "doc":
                                            try
                                            {
                                                var pathddocument =
                                                    "/" + labelgroupGovernmentID + "/" + labelcurrent + "/" +
                                                    defaultaddress["_id"];
                                                var nodedocument = jObect.SelectToken("document");
                                                var arraydocscurrent = ((JArray) nodedocument["value"]);
                                                var countarraydocs = arraydocscurrent.Count();
                                                var arraydocs = new JArray();
                                                foreach (var item1 in nodedocument["value"]
                                                    .Where(x => x["jsPath"].ToString() != pathddocument))
                                                    arraydocs.Add(item1);
                                                var list = ((JArray) item.modelarrays).Select(x => x.ToString())
                                                    .ToList();
                                                var i = 0;

                                                foreach (var fname in list)
                                                {
                                                    JToken js = JToken.Parse(JsonConvert.SerializeObject(new
                                                    {
                                                        _id = (countarraydocs + i + 1).ToString() + "",
                                                        name = fname,
                                                        description = "file" + (countarraydocs + i + 1).ToString(),
                                                        type = "image/jpeg",
                                                        category = labelcurrent,
                                                        jsPath = pathddocument,
                                                        nosearch = false
                                                    }));
                                                    arraydocs.Add(js);
                                                    i++;
                                                }
                                                nodedocument["value"] = arraydocs;
                                            }
                                            catch (Exception ex)
                                            {
                                            }
                                            break;
                                        default:
                                            UpdateInformationVaultById(defaultaddress, item);
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                bool ishasdescription = false;
                                var valuedefault = "";
                                List<string> arraystringjsonnewaddress = new List<string>();
                                foreach (var item in keyValues.Where(x =>
                                    x.jsPath.StartsWith(".governmentID." + namecurrentaddress) ||
                                    x.jsPath.StartsWith(".groupGovernmentID." + namecurrentaddress)))
                                {
                                    var addressnodename = item.jsPath.Trim('.').Split('.').ToList()[2];

                                    switch (addressnodename)
                                    {
                                        case "photo":
                                            try
                                            {
                                                var pathddocument =
                                                    "/" + labelgroupGovernmentID + "/" + labelcurrent + "/" +
                                                    (arrayaddress.Count + 1).ToString();
                                                var nodedocument = jObect.SelectToken("document");
                                                var arraydocscurrent = ((JArray) nodedocument["value"]);
                                                var countarraydocs = arraydocscurrent.Count();
                                                var arraydocs = new JArray();
                                                foreach (var item1 in nodedocument["value"]
                                                    .Where(x => x["jsPath"].ToString() != pathddocument))
                                                    arraydocs.Add(item1);

                                                var list = ((JArray) item.modelarrays).Select(x => x.ToString())
                                                    .ToList();


                                                var i = 0;
                                                foreach (var fname in list)
                                                {
                                                    JToken js = JToken.Parse(JsonConvert.SerializeObject(new
                                                    {
                                                        _id = (countarraydocs + i + 1).ToString() + "",
                                                        name = fname,
                                                        description = "file" + (countarraydocs + i + 1).ToString(),
                                                        type = "image/jpeg",
                                                        category = labelcurrent,
                                                        jsPath = pathddocument,
                                                        nosearch = false
                                                    }));
                                                    arraydocs.Add(js);
                                                    i++;
                                                }
                                                nodedocument["value"] = arraydocs;
                                            }
                                            catch
                                            {
                                            }
                                            break;
                                        case "location":
                                            arraystringjsonnewaddress.Add(
                                                "\"" + "country" + "\"" + ":" + "\"" + item.model + "\"");
                                            arraystringjsonnewaddress.Add(
                                                "\"" + "city" + "\"" + ":" + "\"" + item.unitModel + "\"");
                                            break;
                                        case "description":
                                            valuedefault = item.model;
                                            arraystringjsonnewaddress.Add(
                                                "\"" + addressnodename + "\"" + ":" + "\"" + item.model + "\"");
                                            ishasdescription = true;
                                            break;
                                        default:
                                            arraystringjsonnewaddress.Add(
                                                "\"" + addressnodename + "\"" + ":" + "\"" + item.model + "\"");
                                            break;
                                    }
                                }

                                nodeaddresscurrent["default"] = valuedefault;
                                arraystringjsonnewaddress.Add("\"" + "_id" + "\"" + ":" + "\"" +
                                                              (arrayaddress.Count + 1).ToString() + "\"");
                                if (!ishasdescription)
                                    arraystringjsonnewaddress.Add("\"" + "description" + "\"" + ":" + "\"" + "" + "\"");
                                JToken jsaddress =
                                    JToken.Parse("{" + string.Join(",", arraystringjsonnewaddress) + "}");
                                arrayaddress.Add(jsaddress);
                            }
                        }
                    }
                }
                catch
                {
                }

                try
                {
                    if (keyValues.Where(x => x.jsPath.StartsWith(".contact")).Count() > 0)
                    {
                        var groupaddress = keyValues.Where(x => x.jsPath.StartsWith(".contact."))
                            .Select(x => x.jsPath.Trim('.').Split('.')[1]).Distinct().ToList();
                        foreach (var namecurrentaddress in groupaddress)
                        {
                            try
                            {
                                var nodeaddress = jObect.SelectToken("contact");
                                nodeaddress["name"] = "Contact";
                                var nodeaddresscurrent = nodeaddress["value"][namecurrentaddress];
                                nodeaddresscurrent["name"] = namecurrentaddress;
                                var defaultcurrentaddress = "";
                                try
                                {
                                    defaultcurrentaddress = nodeaddresscurrent["default"].ToString();
                                }
                                catch (Exception ex)
                                {
                                }
                                JArray arrayaddress = (JArray) nodeaddresscurrent["value"];
                                var listaddressdefault = arrayaddress
                                    .Where(x => x["description"].ToString() == defaultcurrentaddress).ToList();
                                if (listaddressdefault.Count() > 0)
                                {
                                    var defaultaddress = listaddressdefault.FirstOrDefault();
                                    defaultaddress["value"] = keyValues
                                        .Where(x => x.jsPath.StartsWith(".contact." + namecurrentaddress))
                                        .FirstOrDefault().model;
                                }
                                else
                                {
                                    var valuedefault = keyValues
                                        .Where(x => x.jsPath.StartsWith(".contact." + namecurrentaddress))
                                        .FirstOrDefault().model;
                                    List<string> arraystringjsonnewaddress = new List<string>();
                                    arraystringjsonnewaddress.Add(
                                        "\"" + "value" + "\"" + ":" + "\"" + valuedefault + "\"");

                                    nodeaddresscurrent["default"] = valuedefault;
                                    JToken jsaddress =
                                        JToken.Parse("{" + string.Join(",", arraystringjsonnewaddress) + "}");
                                    arrayaddress.Add(jsaddress);
                                }
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                }
                catch
                {
                }


                try
                {
                    if (keyValues.Where(x => x.jsPath.StartsWith(".address")).Count() > 0)
                    {
                        var groupaddress = keyValues.Where(x => x.jsPath.StartsWith(".address."))
                            .Select(x => x.jsPath.Trim('.').Split('.')[1]).Distinct().ToList();
                        foreach (var namecurrentaddress in groupaddress)
                        {
                            var nodeaddress = jObect.SelectToken("groupAddress");
                            nodeaddress["name"] = "Address";
                            var nodeaddresscurrent = nodeaddress["value"][namecurrentaddress];
                            nodeaddresscurrent["name"] = namecurrentaddress;
                            var defaultcurrentaddress = nodeaddresscurrent["default"].ToString();
                            JArray arrayaddress = (JArray) nodeaddresscurrent["value"];
                            var listaddressdefault = arrayaddress
                                .Where(x => x["description"].ToString() == defaultcurrentaddress).ToList();
                            if (listaddressdefault.Count() > 0)
                            {
                                var defaultaddress = listaddressdefault.FirstOrDefault();

                                foreach (var item in keyValues.Where(x =>
                                    x.jsPath.StartsWith(".address." + namecurrentaddress)))
                                {
                                    try
                                    {
                                        UpdateInformationVaultById(defaultaddress, item);
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                            else
                            {
                                bool ishasdescription = false;
                                var valuedefault = "";
                                List<string> arraystringjsonnewaddress = new List<string>();
                                foreach (var item in keyValues.Where(x =>
                                    x.jsPath.StartsWith(".address." + namecurrentaddress)))
                                {
                                    var addressnodename = item.jsPath.Trim('.').Split('.').ToList()[2];

                                    switch (addressnodename)
                                    {
                                        case "address":
                                            arraystringjsonnewaddress.Add(
                                                "\"" + "addressLine" + "\"" + ":" + "\"" + item.model + "\"");
                                            break;
                                        case "location":
                                            arraystringjsonnewaddress.Add(
                                                "\"" + "country" + "\"" + ":" + "\"" + item.model + "\"");
                                            arraystringjsonnewaddress.Add(
                                                "\"" + "city" + "\"" + ":" + "\"" + item.unitModel + "\"");
                                            break;
                                        case "description":
                                            valuedefault = item.model;
                                            arraystringjsonnewaddress.Add(
                                                "\"" + addressnodename + "\"" + ":" + "\"" + item.model + "\"");
                                            ishasdescription = true;
                                            break;
                                        default:
                                            arraystringjsonnewaddress.Add(
                                                "\"" + addressnodename + "\"" + ":" + "\"" + item.model + "\"");
                                            break;
                                    }
                                }

                                nodeaddresscurrent["default"] = valuedefault;
                                if (!ishasdescription)
                                    arraystringjsonnewaddress.Add("\"" + "description" + "\"" + ":" + "\"" + "" + "\"");
                                JToken jsaddress =
                                    JToken.Parse("{" + string.Join(",", arraystringjsonnewaddress) + "}");
                                arrayaddress.Add(jsaddress);
                            }
                        }
                    }
                }
                catch
                {
                }

                try
                {
                    if (keyValues.Where(x => x.membership == "true").Count() > 0)
                    {
                        var nodemembership = jObect.SelectToken("membership");
                        nodemembership["name"] = "Membership";
                        JArray arraymembsership = (JArray) nodemembership["value"];
                        var businessName = keyValues.FirstOrDefault(x => x.jsPath == ".membership.businessName").model;
                        var membershipProgramName = keyValues
                            .FirstOrDefault(x => x.jsPath == ".membership.membershipProgramName").model;

                        if (arraymembsership.Where(x =>
                                (x["businessName"].ToString() == businessName) &&
                                (x["membershipProgramName"].ToString() == membershipProgramName)).Count() > 0)
                        {
                            var membershipedit = arraymembsership.Where(x =>
                                (x["businessName"].ToString() == businessName) &&
                                (x["membershipProgramName"].ToString() == membershipProgramName)).FirstOrDefault();

                            foreach (var item in keyValues.Where(x => x.membership == "true"))
                            {
                                var membershipnodename = item.jsPath.Trim('.').Split('.').ToList()[1];
                                membershipedit[membershipnodename] = item.model;
                            }
                        }
                        else
                        {
                            List<string> arraystringjsonnewmembership = new List<string>();
                            foreach (var item in keyValues.Where(x => x.membership == "true"))
                            {
                                var membershipnodename = item.jsPath.Trim('.').Split('.').ToList()[1];
                                arraystringjsonnewmembership.Add(
                                    "\"" + membershipnodename + "\"" + ":" + "\"" + item.model + "\"");
                            }
                            JToken jsmembership =
                                JToken.Parse("{" + string.Join(",", arraystringjsonnewmembership) + "}");
                            arraymembsership.Add(jsmembership);
                        }
                    }
                }
                catch
                {
                }


                try
                {
                    if (keyValues.Where(x => x.jsPath.StartsWith(".education")).Count() > 0)
                    {
                        var nodeeducation = jObect.SelectToken("education");
                        nodeeducation["name"] = "Education";
                        nodeeducation["label"] = "Education";

                        var defaultcurrenteducation = nodeeducation["default"].ToString();
                        JArray arrayeducation = (JArray) nodeeducation["value"];
                        var listeducationdefault = arrayeducation
                            .Where(x => x["description"].ToString() == defaultcurrenteducation).ToList();
                        if (listeducationdefault.Count() > 0)
                        {
                            var defaulteducation = listeducationdefault.FirstOrDefault();

                            foreach (var item in keyValues.Where(x => x.jsPath.StartsWith(".education")))
                            {
                                switch (item.type)
                                {
                                    case "doc":
                                        try
                                        {
                                            var pathddocument = "/" + "Education" + "/" + defaulteducation["_id"];
                                            var nodedocument = jObect.SelectToken("document");
                                            var arraydocscurrent = ((JArray) nodedocument["value"]);
                                            var countarraydocs = arraydocscurrent.Count();
                                            var arraydocs = new JArray();
                                            foreach (var item1 in nodedocument["value"]
                                                .Where(x => x["jsPath"].ToString() != pathddocument))
                                                arraydocs.Add(item1);

                                            var list = ((JArray) item.modelarrays).Select(x => x.ToString()).ToList();

                                            var i = 0;

                                            foreach (var fname in list)
                                            {
                                                JToken js = JToken.Parse(JsonConvert.SerializeObject(new
                                                {
                                                    _id = (countarraydocs + i + 1).ToString() + "",
                                                    name = fname,
                                                    description = "file" + (countarraydocs + i + 1).ToString(),
                                                    type = "image/jpeg",
                                                    category = "Education",
                                                    jsPath = pathddocument,
                                                    nosearch = false
                                                }));
                                                arraydocs.Add(js);
                                                i++;
                                            }
                                            nodedocument["value"] = arraydocs;
                                        }
                                        catch (Exception ex)
                                        {
                                        }
                                        break;
                                    default:
                                        UpdateInformationVaultById(defaulteducation, item);
                                        break;
                                }
                            }
                        }
                        else
                        {
                            bool ishasdescription = false;
                            var valuedefault = "";
                            List<string> arraystringjsonnewaddress = new List<string>();
                            foreach (var item in keyValues.Where(x => x.jsPath.StartsWith(".education")))
                            {
                                var addressnodename = item.jsPath.Trim('.').Split('.').ToList()[1];

                                switch (addressnodename)
                                {
                                    case "photo":
                                        try
                                        {
                                            var pathddocument =
                                                "/" + "Education" + "/" + (arrayeducation.Count + 1).ToString();
                                            var nodedocument = jObect.SelectToken("document");
                                            var arraydocscurrent = ((JArray) nodedocument["value"]);
                                            var countarraydocs = arraydocscurrent.Count();
                                            var arraydocs = new JArray();
                                            foreach (var item1 in nodedocument["value"]
                                                .Where(x => x["jsPath"].ToString() != pathddocument))
                                                arraydocs.Add(item1);

                                            var list = ((JArray) item.modelarrays).Select(x => x.ToString()).ToList();


                                            var i = 0;
                                            foreach (var fname in list)
                                            {
                                                JToken js = JToken.Parse(JsonConvert.SerializeObject(new
                                                {
                                                    _id = (countarraydocs + i + 1).ToString() + "",
                                                    name = fname,
                                                    description = "file" + (countarraydocs + i + 1).ToString(),
                                                    type = "image/jpeg",
                                                    category = "Education",
                                                    jsPath = pathddocument,
                                                    nosearch = false
                                                }));
                                                arraydocs.Add(js);
                                                i++;
                                            }
                                            nodedocument["value"] = arraydocs;
                                        }
                                        catch
                                        {
                                        }
                                        break;
                                    case "description":
                                        valuedefault = item.model;
                                        arraystringjsonnewaddress.Add(
                                            "\"" + addressnodename + "\"" + ":" + "\"" + item.model + "\"");
                                        ishasdescription = true;
                                        break;
                                    default:
                                        arraystringjsonnewaddress.Add(
                                            "\"" + addressnodename + "\"" + ":" + "\"" + item.model + "\"");
                                        break;
                                }
                            }

                            nodeeducation["default"] = valuedefault;
                            arraystringjsonnewaddress.Add("\"" + "_id" + "\"" + ":" + "\"" +
                                                          (arrayeducation.Count + 1).ToString() + "\"");
                            if (!ishasdescription)
                                arraystringjsonnewaddress.Add("\"" + "description" + "\"" + ":" + "\"" + "" + "\"");
                            JToken jsaddress = JToken.Parse("{" + string.Join(",", arraystringjsonnewaddress) + "}");
                            arrayeducation.Add(jsaddress);
                        }
                    }
                }
                catch
                {
                }


                try
                {
                    if (keyValues.Where(x => x.jsPath.StartsWith(".employment")).Count() > 0)
                    {
                        var nodeeducation = jObect.SelectToken("employment");
                        nodeeducation["name"] = "Employment";
                        nodeeducation["label"] = "Employment";

                        var defaultcurrentemployment = nodeeducation["default"].ToString();
                        JArray arrayeemployment = (JArray) nodeeducation["value"];
                        var listeducationdefault = arrayeemployment
                            .Where(x => x["description"].ToString() == defaultcurrentemployment).ToList();
                        if (listeducationdefault.Count() > 0)
                        {
                            var defaulteducation = listeducationdefault.FirstOrDefault();

                            foreach (var item in keyValues.Where(x => x.jsPath.StartsWith(".employment")))
                            {
                                switch (item.type)
                                {
                                    case "doc":
                                        try
                                        {
                                            var pathddocument = "/" + "Employment" + "/" + defaulteducation["_id"];
                                            var nodedocument = jObect.SelectToken("document");
                                            var arraydocscurrent = ((JArray) nodedocument["value"]);
                                            var countarraydocs = arraydocscurrent.Count();
                                            var arraydocs = new JArray();
                                            foreach (var item1 in nodedocument["value"]
                                                .Where(x => x["jsPath"].ToString() != pathddocument))
                                                arraydocs.Add(item1);

                                            var list = ((JArray) item.modelarrays).Select(x => x.ToString()).ToList();

                                            var i = 0;

                                            foreach (var fname in list)
                                            {
                                                JToken js = JToken.Parse(JsonConvert.SerializeObject(new
                                                {
                                                    _id = (countarraydocs + i + 1).ToString() + "",
                                                    name = fname,
                                                    description = "file" + (countarraydocs + i + 1).ToString(),
                                                    type = "image/jpeg",
                                                    category = "Employment",
                                                    jsPath = pathddocument,
                                                    nosearch = false
                                                }));
                                                arraydocs.Add(js);
                                                i++;
                                            }
                                            nodedocument["value"] = arraydocs;
                                        }
                                        catch (Exception ex)
                                        {
                                        }
                                        break;
                                    default:
                                        UpdateInformationVaultById(defaulteducation, item);
                                        break;
                                }
                            }
                        }
                        else
                        {
                            bool ishasdescription = false;
                            var valuedefault = "";
                            List<string> arraystringjsonnewaddress = new List<string>();
                            foreach (var item in keyValues.Where(x => x.jsPath.StartsWith(".employment")))
                            {
                                var addressnodename = item.jsPath.Trim('.').Split('.').ToList()[1];

                                switch (addressnodename)
                                {
                                    case "photo":
                                        try
                                        {
                                            var pathddocument =
                                                "/" + "Employment" + "/" + (arrayeemployment.Count + 1).ToString();
                                            var nodedocument = jObect.SelectToken("document");
                                            var arraydocscurrent = ((JArray) nodedocument["value"]);
                                            var countarraydocs = arraydocscurrent.Count();
                                            var arraydocs = new JArray();
                                            foreach (var item1 in nodedocument["value"]
                                                .Where(x => x["jsPath"].ToString() != pathddocument))
                                                arraydocs.Add(item1);

                                            var list = ((JArray) item.modelarrays).Select(x => x.ToString()).ToList();


                                            var i = 0;
                                            foreach (var fname in list)
                                            {
                                                JToken js = JToken.Parse(JsonConvert.SerializeObject(new
                                                {
                                                    _id = (countarraydocs + i + 1).ToString() + "",
                                                    name = fname,
                                                    description = "file" + (countarraydocs + i + 1).ToString(),
                                                    type = "image/jpeg",
                                                    category = "Employment",
                                                    jsPath = pathddocument,
                                                    nosearch = false
                                                }));
                                                arraydocs.Add(js);
                                                i++;
                                            }
                                            nodedocument["value"] = arraydocs;
                                        }
                                        catch
                                        {
                                        }
                                        break;
                                    case "address":
                                        arraystringjsonnewaddress.Add(
                                            "\"" + "addressLine" + "\"" + ":" + "\"" + item.model + "\"");
                                        break;
                                    case "location":
                                        arraystringjsonnewaddress.Add(
                                            "\"" + "country" + "\"" + ":" + "\"" + item.model + "\"");
                                        arraystringjsonnewaddress.Add(
                                            "\"" + "city" + "\"" + ":" + "\"" + item.unitModel + "\"");
                                        break;
                                    case "description":
                                        valuedefault = item.model;
                                        arraystringjsonnewaddress.Add(
                                            "\"" + addressnodename + "\"" + ":" + "\"" + item.model + "\"");
                                        ishasdescription = true;
                                        break;
                                    default:
                                        arraystringjsonnewaddress.Add(
                                            "\"" + addressnodename + "\"" + ":" + "\"" + item.model + "\"");
                                        break;
                                }
                            }

                            nodeeducation["default"] = valuedefault;
                            arraystringjsonnewaddress.Add("\"" + "_id" + "\"" + ":" + "\"" +
                                                          (arrayeemployment.Count + 1).ToString() + "\"");
                            if (!ishasdescription)
                                arraystringjsonnewaddress.Add("\"" + "description" + "\"" + ":" + "\"" + "" + "\"");
                            JToken jsaddress = JToken.Parse("{" + string.Join(",", arraystringjsonnewaddress) + "}");
                            arrayeemployment.Add(jsaddress);
                        }
                    }
                }
                catch
                {
                }

                foreach (FieldinformationVault field in keyValues.Where(x =>
                    !x.jsPath.StartsWith(".education") && !x.jsPath.StartsWith(".education") &&
                    !x.jsPath.StartsWith(".governmentID") && !(x.qa) && x.membership != "true" &&
                    !x.jsPath.StartsWith(".address")).ToList())
                {
                    try
                    {
                        string jsonpath = field.jsPath;
                        List<string> listpath = field.jsPath.Trim('.').Split('.').ToList();
                        string jsonpathreal = string.Join(".value.", listpath);
                        var node = jObect.SelectToken(jsonpathreal);

                        //upload node parent
                        try
                        {
                            var nodeparent = jObect.SelectToken("." + listpath[0]);
                            nodeparent["name"] = listpath[0];
                            var nodesubparent = jObect.SelectToken("." + listpath[0] + ".value." + listpath[1]);
                            nodesubparent["name"] = listpath[1];
                        }
                        catch
                        {
                        }
                        switch (jsonpath)
                        {
                            case ".contact.home":
                                var nodecontact = jObect.SelectToken("contact.value.home");
                                var varluedefault = nodecontact["default"];
                                var nodecontactmobile = jObect.SelectToken("contact.value.home");
                                JArray array = (JArray) nodecontactmobile["value"];

                                if (!string.IsNullOrEmpty(varluedefault.ToString()) && array.Count > 0)
                                {
                                    if (array.Where(x => x["value"].ToString() == varluedefault.Value<string>())
                                            .Count() > 0)
                                    {
                                        var find = array
                                            .Where(x => x["value"].ToString() == varluedefault.Value<string>())
                                            .FirstOrDefault();
                                        find["value"] = field.model;
                                    }
                                    else
                                    {
                                        JToken js = JToken.Parse(
                                            JsonConvert.SerializeObject(
                                                new vaultkeyvalue {value = field.model, id = ""}));
                                        array.Add(js);
                                    }
                                }
                                else
                                {
                                    array = new JArray();
                                    JToken js = JToken.Parse(
                                        JsonConvert.SerializeObject(new vaultkeyvalue {value = field.model, id = ""}));
                                    array.Add(js);
                                    nodecontactmobile["value"] = array;
                                }

                                nodecontact["default"] = field.model;
                                continue;
                            case ".contact.office":
                                nodecontact = jObect.SelectToken("contact.value.office");
                                varluedefault = nodecontact["default"];
                                nodecontactmobile = jObect.SelectToken("contact.value.office");
                                array = (JArray) nodecontactmobile["value"];

                                if (!string.IsNullOrEmpty(varluedefault.ToString()) && array.Count > 0)
                                {
                                    if (array.Where(x => x["value"].ToString() == varluedefault.Value<string>())
                                            .Count() > 0)
                                    {
                                        var find = array
                                            .Where(x => x["value"].ToString() == varluedefault.Value<string>())
                                            .FirstOrDefault();
                                        find["value"] = field.model;
                                    }
                                    else
                                    {
                                        JToken js = JToken.Parse(
                                            JsonConvert.SerializeObject(
                                                new vaultkeyvalue {value = field.model, id = ""}));
                                        array.Add(js);
                                    }
                                }
                                else
                                {
                                    array = new JArray();
                                    JToken js = JToken.Parse(
                                        JsonConvert.SerializeObject(new vaultkeyvalue {value = field.model, id = ""}));
                                    array.Add(js);
                                    nodecontactmobile["value"] = array;
                                }

                                nodecontact["default"] = field.model;
                                continue;
                            case ".contact.fax":
                                nodecontact = jObect.SelectToken("contact.value.fax");
                                varluedefault = nodecontact["default"];
                                nodecontactmobile = jObect.SelectToken("contact.value.fax");
                                array = (JArray) nodecontactmobile["value"];

                                if (!string.IsNullOrEmpty(varluedefault.ToString()) && array.Count > 0)
                                {
                                    if (array.Where(x => x["value"].ToString() == varluedefault.Value<string>())
                                            .Count() > 0)
                                    {
                                        var find = array
                                            .Where(x => x["value"].ToString() == varluedefault.Value<string>())
                                            .FirstOrDefault();
                                        find["value"] = field.model;
                                    }
                                    else
                                    {
                                        JToken js = JToken.Parse(
                                            JsonConvert.SerializeObject(
                                                new vaultkeyvalue {value = field.model, id = ""}));
                                        array.Add(js);
                                    }
                                }
                                else
                                {
                                    array = new JArray();
                                    JToken js = JToken.Parse(
                                        JsonConvert.SerializeObject(new vaultkeyvalue {value = field.model, id = ""}));
                                    array.Add(js);
                                    nodecontactmobile["value"] = array;
                                }

                                nodecontact["default"] = field.model;
                                continue;
                            case ".contact.mobile":
                                nodecontact = jObect.SelectToken("contact.value.mobile");
                                varluedefault = nodecontact["default"];
                                nodecontactmobile = jObect.SelectToken("contact.value.mobile");
                                array = (JArray) nodecontactmobile["value"];

                                if (!string.IsNullOrEmpty(varluedefault.ToString()) && array.Count > 0)
                                {
                                    if (array.Where(x => x["value"].ToString() == varluedefault.Value<string>())
                                            .Count() > 0)
                                    {
                                        var find = array
                                            .Where(x => x["value"].ToString() == varluedefault.Value<string>())
                                            .FirstOrDefault();
                                        find["value"] = field.model;
                                    }
                                    else
                                    {
                                        JToken js = JToken.Parse(
                                            JsonConvert.SerializeObject(
                                                new vaultkeyvalue {value = field.model, id = ""}));
                                        array.Add(js);
                                    }
                                }
                                else
                                {
                                    array = new JArray();
                                    JToken js = JToken.Parse(
                                        JsonConvert.SerializeObject(new vaultkeyvalue {value = field.model, id = ""}));
                                    array.Add(js);
                                    nodecontactmobile["value"] = array;
                                }

                                nodecontact["default"] = field.model;
                                continue;
                            case ".contact.email":
                                nodecontact = jObect.SelectToken("contact.value.email");
                                varluedefault = nodecontact["default"];
                                nodecontactmobile = jObect.SelectToken("contact.value.email");
                                array = (JArray) nodecontactmobile["value"];

                                if (!string.IsNullOrEmpty(varluedefault.ToString()) && array.Count > 0)
                                {
                                    if (array.Where(x => x["value"].ToString() == varluedefault.Value<string>())
                                            .Count() > 0)
                                    {
                                        var find = array
                                            .Where(x => x["value"].ToString() == varluedefault.Value<string>())
                                            .FirstOrDefault();
                                        find["value"] = field.model;
                                    }
                                    else
                                    {
                                        JToken js = JToken.Parse(
                                            JsonConvert.SerializeObject(
                                                new vaultkeyvalue {value = field.model, id = ""}));
                                        array.Add(js);
                                    }
                                }
                                else
                                {
                                    array = new JArray();
                                    JToken js = JToken.Parse(
                                        JsonConvert.SerializeObject(new vaultkeyvalue {value = field.model, id = ""}));
                                    array.Add(js);
                                    nodecontactmobile["value"] = array;
                                }

                                nodecontact["default"] = field.model;
                                continue;
                            case ".contact.officeEmail":
                                nodecontact = jObect.SelectToken("contact.value.officeEmail");
                                varluedefault = nodecontact["default"];
                                nodecontactmobile = jObect.SelectToken("contact.value.officeEmail");
                                array = (JArray) nodecontactmobile["value"];

                                if (!string.IsNullOrEmpty(varluedefault.ToString()) && array.Count > 0)
                                {
                                    if (array.Where(x => x["value"].ToString() == varluedefault.Value<string>())
                                            .Count() > 0)
                                    {
                                        var find = array
                                            .Where(x => x["value"].ToString() == varluedefault.Value<string>())
                                            .FirstOrDefault();
                                        find["value"] = field.model;
                                    }
                                    else
                                    {
                                        JToken js = JToken.Parse(
                                            JsonConvert.SerializeObject(
                                                new vaultkeyvalue {value = field.model, id = ""}));
                                        array.Add(js);
                                    }
                                }
                                else
                                {
                                    array = new JArray();
                                    JToken js = JToken.Parse(
                                        JsonConvert.SerializeObject(new vaultkeyvalue {value = field.model, id = ""}));
                                    array.Add(js);
                                    nodecontactmobile["value"] = array;
                                }

                                nodecontact["default"] = field.model;
                                continue;
                            case ".basicInformation.age":
                                var nodedob = jObect.SelectToken("basicInformation.value.dob");
                                string strdate = nodedob["value"].ToString();
                                strdate = string.Join("-",
                                    strdate.Split('-').Select(x => x.Length == 1 ? "0" + x : x).ToList());
                                DateTime dobtimevault = DateTime.Now;
                                dobtimevault = DateTime.ParseExact(strdate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                                var newyear = GH.Util.Common.ConvertToInt(field.model);
                                if (newyear > 0)
                                    dobtimevault =
                                        dobtimevault.AddYears(0 - (newyear - (DateTime.Now.Year - dobtimevault.Year)));
                                nodedob["value"] = dobtimevault.ToString("dd-MM-yyyy");
                                break;
                            case ".basicInformation.ageRange":
                                continue;
                            case ".employment.salaryRange":
                                continue;
                            case ".address.addressHistory":
                                continue;
                            case ".basicInformation.emergencyContact":
                                continue;
                        }

                        switch (field.type)
                        {
                            case "location":
                                if (listpath.Count() > 1)
                                {
                                    listpath.RemoveAt(listpath.Count() - 1);
                                    jsonpathreal = string.Join(".value.", listpath);
                                    node = jObect.SelectToken(jsonpathreal);
                                    UpdateInformationVaultById(node, field);
                                }
                                continue;
                            case "address":
                                if (listpath.Count() > 1)
                                {
                                    listpath.RemoveAt(listpath.Count() - 1);
                                    jsonpathreal = string.Join(".value.", listpath);
                                    node = jObect.SelectToken(jsonpathreal);
                                    UpdateInformationVaultById(node, field);
                                }

                                continue;
                            case "numinput":
                                UpdateInformationVaultById(node, field);
                                continue;
                            case "tagsinput":
                            case "smartinput":
                                UpdateInformationVaultById(node, field);
                                continue;
                            default:
                                UpdateInformationVaultById(node, field);
                                continue;
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                var objectjson = jObect.ToObject<object>();
                string output = JsonConvert.SerializeObject(objectjson);
                string vaultId = informationVault["_id"].AsObjectId.ToString();
                SaveInformationVault(vaultId, output);
            }
        }

        public void UpdateInfoFieldById(string accountId, InfoField field)
        {
            var informationVault = GetInformationVaultByUserId(accountId);
            if (informationVault != null)
            {
                var jsonString = informationVault.ToJson();
                jsonString = jsonString.Replace("ObjectId(", "").Replace(")", "");
                JObject jObect = JObject.Parse(jsonString);

                string jsonpath = field.jsPath;

                List<string> listpath = field.jsPath.Trim('.').Split('.').ToList();
                string jsonpathreal = string.Join(".value.", listpath);
                var node = jObect.SelectToken(jsonpathreal);
                var valuecontinue = true;
                switch (field.Type)
                {
                    case "location":
                        if (listpath.Count() > 1)
                        {
                            listpath.RemoveAt(listpath.Count() - 1);
                            jsonpathreal = string.Join(".value.", listpath);
                            node = jObect.SelectToken(jsonpathreal);
                            node["value"]["country"]["value"] = field.Value;
                            node["value"]["city"]["value"] = field.Unit;
                        }
                        valuecontinue = false;
                        break;
                    case "address":
                        if (listpath.Count() > 1)
                        {
                            listpath.RemoveAt(listpath.Count() - 1);
                            jsonpathreal = string.Join(".value.", listpath);
                            node = jObect.SelectToken(jsonpathreal);
                            node["value"]["addressLine1"]["value"] = field.Value;
                            node["value"]["addressLine2"]["value"] = field.Unit;
                        }
                        valuecontinue = false;
                        break;

                    case "numinput":
                        node["value"] = field.Value;
                        node["unit"] = string.IsNullOrEmpty(field.Unit) ? "" : field.Unit;
                        valuecontinue = false;
                        break;
                    case "tagsinput":
                    case "smartinput":
                        if (!string.IsNullOrEmpty(field.Value))
                            node["value"] = JToken.Parse(JsonConvert.SerializeObject(field.Value.Split(',').ToArray()));
                        valuecontinue = false;
                        break;
                    case "date":
                    case "datecombo":
                        if (!string.IsNullOrEmpty(field.Value))
                        {
                            node["default"] = Convert.ToDateTime(field.Value).ToString("yyyy-MM-dd");
                        }
                        valuecontinue = false;
                        break;
                }
                if (valuecontinue)
                    switch (jsonpath)
                    {
                        case ".contact.phone":
                            var nodecontact = jObect.SelectToken("contact");
                            var varluedefault = nodecontact["default"];
                            var nodecontactmobile = jObect.SelectToken("contact.value.mobile");
                            JArray array = (JArray) nodecontactmobile["value"];

                            if (!string.IsNullOrEmpty(varluedefault.ToString()) && array.Count > 0)
                            {
                                if (array.Where(x => x["value"].ToString() == varluedefault.Value<string>()).Count() >
                                    0)
                                {
                                    var find = array.Where(x => x["value"].ToString() == varluedefault.Value<string>())
                                        .FirstOrDefault();
                                    find["value"] = field.Value;
                                }
                                else
                                {
                                    JToken js = JToken.Parse(
                                        JsonConvert.SerializeObject(new vaultkeyvalue {value = field.Value, id = ""}));
                                    array.Add(js);
                                }
                            }
                            else
                            {
                                array = new JArray();
                                array.Add(new vaultkeyvalue {value = field.Value, id = ""});
                                ;
                            }

                            nodecontact["default"] = field.Value;
                            break;
                        default:
                            node["value"] = field.Value;
                            break;
                    }


                var objectjson = jObect.ToObject<object>();
                string output = JsonConvert.SerializeObject(objectjson);
                string vaultId = informationVault["_id"].AsObjectId.ToString();
                SaveInformationVault(vaultId, output);
            }
        }

        public void UpdateListInfoFieldById(string accountId, List<InfoField> fields)
        {
            var informationVault = GetInformationVaultByUserId(accountId);
            if (informationVault != null)
            {
                var jsonString = informationVault.ToJson();
                jsonString = jsonString.Replace("ObjectId(", "").Replace(")", "");
                JObject jObect = JObject.Parse(jsonString);

                foreach (InfoField field in fields)
                {
                    string jsonpath = field.jsPath;
                    List<string> listpath = field.jsPath.Trim('.').Split('.').ToList();
                    string jsonpathreal = string.Join(".value.", listpath);
                    var node = jObect.SelectToken(jsonpathreal);
                    node["value"] = (field.Value == null ? "" : field.Value.ToString());
                }

                var objectjson = jObect.ToObject<object>();
                string output = JsonConvert.SerializeObject(objectjson);
                string vaultId = informationVault["_id"].AsObjectId.ToString();
                SaveInformationVault(vaultId, output);
            }
        }

        #region version2

        public void StartByAccountId(string accountId, BasicProfile basicPro)
        {
            var basic = GetFormVaultByAccountId(accountId, EnumVaultGroup.Basic);
            var bsBasic = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(basic);
            if (!string.IsNullOrEmpty(basicPro.FirstName))
                bsBasic["value"]["firstName"]["value"] = basicPro.FirstName;

            if (!string.IsNullOrEmpty(basicPro.LastName))
                bsBasic["value"]["lastName"]["value"] = basicPro.LastName;
            if (!string.IsNullOrEmpty(basicPro.Gender))
                bsBasic["value"]["gender"]["value"] = basicPro.Gender;

            if (basicPro.BirthDay != null)
            {
                try
                {
                    bsBasic["value"]["lastName"]["value"] = basicPro.BirthDay;
                }
                catch
                {
                }
            }

            if (!string.IsNullOrEmpty(basicPro.Country))
                bsBasic["value"]["country"]["value"] = basicPro.Country;

            if (!string.IsNullOrEmpty(basicPro.City))
                bsBasic["value"]["city"]["value"] = basicPro.City;
            UpdateFormBsonByAccountId(accountId, EnumVaultGroup.Basic, bsBasic);

            var contact = GetFormVaultByAccountId(accountId, EnumVaultGroup.Contact);
            var bsContact = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(contact);
            if (!string.IsNullOrEmpty(basicPro.Phone))
            {
                BsonArray dataPhone = new BsonArray
                {
                    new BsonDocument
                    {
                        {"id", 0},
                        {"value", basicPro.Phone}
                    }
                };
                bsContact["value"]["mobile"]["default"] = basicPro.Phone;
                bsContact["value"]["mobile"]["value"] = dataPhone;
            }

            if (!string.IsNullOrEmpty(basicPro.Email))
            {
                BsonArray dataMail = new BsonArray
                {
                    new BsonDocument
                    {
                        {"id", 0},
                        {"value", basicPro.Email}
                    }
                };
                bsContact["value"]["email"]["default"] = basicPro.Email;
                bsContact["value"]["email"]["value"] = dataMail;
            }


            UpdateFormBsonByAccountId(accountId, EnumVaultGroup.Contact, bsContact);
        }

        public void UpdateFormBsonByAccountId(string accountId, string formName, BsonDocument formString)
        {
            var vaultCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("InformationVault");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("userId", accountId);
            //BsonDocument bsformString = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(formString);
            var update = Builders<BsonDocument>.Update.Set(formName, formString);
            vaultCollection.UpdateOne(criteria, update);
        }

        public string GetFormVaultByAccountId(string accountId, string formName)
        {
            var vaultCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("InformationVault");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("userId", accountId);

            var vault = vaultCollection.Find(criteria).FirstOrDefault();
            var vaultForm = vault[formName];
            return vaultForm.ToJson();
        }

        public void UpdateFormByAccountId(string accountId, string formName, string formString)
        {
            var vaultCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("InformationVault");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("userId", accountId);
            BsonDocument bsformString = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(formString);
            var update = Builders<BsonDocument>.Update.Set(formName, bsformString);
            vaultCollection.UpdateOne(criteria, update);
        }

        //GetVaultByUserId
        public string GetVaultJsonByUserId(string userId)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("userId", userId);
            BsonDocument vaultinformation = infomationVaultRepository.GetCollection("InformationVault").Find(filter)
                .SingleOrDefault();
            var m = vaultinformation.ToJson();
            return m;
        }

        public void SaveVault(string informationVaultId, string informationVaultJson)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", new BsonObjectId(ObjectId.Parse(informationVaultId)));
            informationVaultJson = informationVaultJson.Replace("\"" + informationVaultId + "\"",
                "ObjectId(\"" + informationVaultId + "\")");
            BsonDocument newvaultinformation =
                MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(informationVaultJson);
            infomationVaultRepository.GetCollection("InformationVault").ReplaceOne(filter, newvaultinformation);
        }

        public DocumentVault GetDocumentByAccountId(string accountId)
        {
            var vaultCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("InformationVault");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("userId", accountId);

            var vault = vaultCollection.Find(criteria).FirstOrDefault();
            var vaultDocument = vault["document"];
            var document = new DocumentVault();
            return document;
        }

        public Document InsertDocumentFieldByAccountId(string accountId, Document document)
        {
            var vaultCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("InformationVault");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("userId", accountId);
            var vault = vaultCollection.Find(criteria).FirstOrDefault();
            var vaultDocument = vault["document"];
            var documentBsonArray = new BsonArray();
            try
            {
                documentBsonArray = vault["document"]["value"].AsBsonArray;
            }
            catch
            {
            }
            ;
            var lstDoc = new List<Document>();
            foreach (var item in documentBsonArray)
            {
                var doc = new Document();

                try
                {
                    doc.Id = item["_id"].AsInt32;
                    if (!string.IsNullOrEmpty(item["name"].AsString))
                        doc.SaveName = item["name"].AsString;
                    if (!string.IsNullOrEmpty(item["description"].AsString))
                        doc.FileName = item["description"].AsString;
                    if (!string.IsNullOrEmpty(item["category"].AsString))
                        doc.Category = item["category"].AsString;
                    if (!string.IsNullOrEmpty(item["uploadDate"].AsString))
                    {
                        doc.UploadDate = item["uploadDate"].AsString;
                    }
                    if (!string.IsNullOrEmpty(item["expiryDate"].AsString))
                    {
                        doc.ExpiredDate = item["expiryDate"].AsString;
                    }
                    if (!string.IsNullOrEmpty(item["jsPath"].AsString))
                        doc.Path = item["jsPath"].AsString;
                    if (!string.IsNullOrEmpty(item["nosearch"].AsString))
                    {
                        doc.NoSearch = item["nosearch"].AsBoolean;
                    }
                }
                catch
                {
                }
                if (doc != null)
                    lstDoc.Add(doc);
            }
            try
            {
                var i = 0;
                while (lstDoc.Exists(value => value.Id == i))
                {
                    i = i + 1;
                }
                document.Id = i;
                //
                var tempSaveName = document.SaveName;
                var count = 1;
                while (lstDoc.Exists(value => value.SaveName == tempSaveName))
                {
                    tempSaveName = document.SaveName;
                    int idx = tempSaveName.LastIndexOf('.');
                    if (idx >= 0)
                    {
                        string name = tempSaveName.Substring(0, idx) + "_" + count.ToString() + '.' +
                                      tempSaveName.Substring(idx + 1, tempSaveName.Length - idx - 1);
                        tempSaveName = name;
                    }
                    count++;
                }
                document.SaveName = tempSaveName;

                lstDoc.Add(document);

                BsonArray bArray = new BsonArray();
                foreach (var term in lstDoc)
                {
                    if (string.IsNullOrEmpty(term.NoSearch.ToString()))
                    {
                        term.NoSearch = true;
                    }

                    var bs = new BsonDocument
                    {
                        {"_id", term.Id},
                        {"name", term.SaveName == null ? "" : term.SaveName},
                        {"description", term.FileName == null ? "" : term.FileName},
                        {"category", term.Category == null ? "" : term.Category},
                        {"uploadDate", term.UploadDate == null ? "" : term.UploadDate},
                        {"expiryDate", term.ExpiredDate == null ? "" : term.ExpiredDate},
                        {"jsPath", term.Path == null ? "" : term.Path},
                        {"nosearch", term.NoSearch}
                    };
                    bArray.Add(bs);
                }


                vaultDocument["value"] = bArray;
                var formName = "document";
                var update = Builders<BsonDocument>.Update.Set(formName, vaultDocument);
                vaultCollection.UpdateOne(criteria, update);
            }
            catch
            {
            }

            return document;
        }

        public string CheckFileNameDocument(string accountId, string fileName)
        {
            var vaultCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("InformationVault");
            var filter = Builders<BsonDocument>.Filter;
            var criteria = filter.Eq("userId", accountId);

            var vault = vaultCollection.Find(criteria).FirstOrDefault();
            var vaultDocument = vault["document"];

            var rs = fileName;
            try
            {
                var listDocument = vault["document"]["value"].AsBsonArray;
                var lstFileName = new List<string>();
                foreach (var item in listDocument)
                {
                    var fName = item["name"].AsString;
                    lstFileName.Add(fName);
                }
                var i = 1;

                while (lstFileName.Exists(value => value == rs))
                {
                    int idx = rs.LastIndexOf('.');

                    if (idx >= 0)
                    {
                        string name = rs.Substring(0, idx) + i.ToString() + '.' +
                                      rs.Substring(idx + 1, rs.Length - idx - 1);
                        rs = name;
                    }
                    i++;
                }
            }
            catch
            {
            }

            return rs;
        }

        public BsonDocument GetVaultByUserId(string userId)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("userId", userId);
            BsonDocument vault = infomationVaultRepository.GetCollection("InformationVault").Find(filter)
                .SingleOrDefault();

            return vault;
        }

        public FormVault GetBasicFormVaultByUserId(string userId)
        {
            var rs = new FormVault();
            var filter = Builders<BsonDocument>.Filter.Eq("userId", userId);
            BsonDocument vault = infomationVaultRepository.GetCollection("InformationVault").Find(filter)
                .SingleOrDefault();
            var bs = vault["basicInformation"].AsBsonDocument;
            var vaultAdapter = new VaultAdapter();
            rs = vaultAdapter.BasicBsonToForm(bs);
            return rs;
        }

        public List<FieldinformationVault> getValueFieldInformationvault(string accountId,
            List<FieldinformationVault> fields, bool defaultForm = true)
        {
            var rs = new List<FieldinformationVault>();
            var vault = GetVaultByUserId(accountId);
            foreach (var field in fields)
            {
                var f = new FieldinformationVault();
                if (field.jsPath.StartsWith(EnumJsPathVaultForm.JsBasic))
                {
                    f = BasicField(field, vault[EnumVaultGroup.Basic].AsBsonDocument);
                }
                else if (field.jsPath.StartsWith(EnumJsPathVaultForm.JsContact))
                {
                }
                else if (field.jsPath.StartsWith(EnumJsPathVaultForm.JsAddress))
                {
                }
                else if (field.jsPath.StartsWith(EnumJsPathVaultForm.JsFinancial))
                {
                }
                else if (field.jsPath.StartsWith(EnumJsPathVaultForm.JsGovernmentID))
                {
                }
                else if (field.jsPath.StartsWith(EnumJsPathVaultForm.JsMembership))
                {
                }
                else if (field.jsPath.StartsWith(EnumJsPathVaultForm.JsFamily))
                {
                }
                else if (field.jsPath.StartsWith(EnumJsPathVaultForm.JsPet))
                {
                }
                else if (field.jsPath.StartsWith(EnumJsPathVaultForm.JsEmployment))
                {
                }
                else if (field.jsPath.StartsWith(EnumJsPathVaultForm.JsEducation))
                {
                }
                else if (field.jsPath.StartsWith(EnumJsPathVaultForm.JsOthers))
                {
                }
                else if (field.jsPath.StartsWith(EnumJsPathVaultForm.JsDocument))
                {
                }
                if (f != null)
                    rs.Add(f);
            }

            return rs;
        }

        public FieldinformationVault BasicField(FieldinformationVault field, BsonDocument form)
        {
            var rs = new FieldinformationVault();
            var json = form["value"].AsBsonDocument;
            foreach (var s in EnumListJsPathVaultForm.Basic)
            {
                try
                {
                    if (field.label == json[s]["label"].AsString)
                    {
                        field.model = json[s]["value"].AsString;
                        break;
                    }
                }
                catch
                {
                    Log.Error("Vault field: " + field.label);
                }
            }

            return rs;
        }

        #endregion version2

        #region Manual Handshake

        public void CheckManualHandshake(string accountId, string informationVaultId, string informationVaultJson)
        {
            var rs = new List<ChangeValue>();
            try
            {
                informationVaultJson = informationVaultJson.Replace("\"" + informationVaultId + "\"",
                    "ObjectId(\"" + informationVaultId + "\")");
                BsonDocument newVault =
                    MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(informationVaultJson);

                var oldVault = GetVaultByUserId(accountId);

                var newCurrentAddress = newVault["groupAddress"]["value"]["currentAddress"]["value"].AsBsonArray;
                var oldCurrentAddress = oldVault["groupAddress"]["value"]["currentAddress"]["value"].AsBsonArray;
                var currentAddress = GetManualHandshakeAddress(oldCurrentAddress, newCurrentAddress);
                if (currentAddress != null)
                {
                    currentAddress.Name = "Current Address";
                    rs.Add(currentAddress);
                }

                var newMailingAddress = newVault["groupAddress"]["value"]["mailingAddress"]["value"].AsBsonArray;
                var oldMailingAddress = oldVault["groupAddress"]["value"]["mailingAddress"]["value"].AsBsonArray;
                var mailingAddress = GetManualHandshakeAddress(oldMailingAddress, newMailingAddress);
                if (mailingAddress != null)
                {
                    mailingAddress.Name = "Mailing Address";
                    rs.Add(mailingAddress);
                }

                // Mobile
                var newMobile = newVault["contact"]["value"]["mobile"]["default"].AsString;
                var oldMobile = oldVault["contact"]["value"]["mobile"]["default"].AsString;
                var mobile = new ChangeValue();
                if (newMobile != oldMobile)
                {
                    mobile.Name = "Mobile Number";
                    mobile.OldValue = oldMobile;
                    mobile.NewValue = newMobile;
                    rs.Add(mobile);
                }
                var newOffice = newVault["contact"]["value"]["office"]["default"].AsString;
                var oldOffice = oldVault["contact"]["value"]["office"]["default"].AsString;

                var office = new ChangeValue();
                if (newOffice != oldOffice)
                {
                    office.Name = "Office Number";
                    office.OldValue = oldOffice;
                    office.NewValue = newOffice;
                    rs.Add(office);
                }
                // Mail
                var newPersonal = newVault["contact"]["value"]["email"]["default"].AsString;
                var oldPersonal = oldVault["contact"]["value"]["email"]["default"].AsString;

                var personal = new ChangeValue();
                if (newPersonal != oldPersonal)
                {
                    personal.Name = "Personal Email";
                    personal.OldValue = oldPersonal;
                    personal.NewValue = newPersonal;
                    rs.Add(personal);
                }

                var newOfficeMail = newVault["contact"]["value"]["officeEmail"]["default"].AsString;
                var oldOfficeMail = oldVault["contact"]["value"]["officeEmail"]["default"].AsString;

                var officeMail = new ChangeValue();
                if (newOfficeMail != oldOfficeMail)
                {
                    officeMail.Name = "Work Email";
                    officeMail.OldValue = oldOfficeMail;
                    officeMail.NewValue = newOfficeMail;
                    rs.Add(officeMail);
                }
                if (rs.Count > 0)
                {
                    var manualHandShakeBus = new ManualHandshakeBusinessLogic();
                    var manualHandShakes = manualHandShakeBus.GetActiveListByAccountId(accountId);
                    if (manualHandShakes.Count > 0)
                    {
                        foreach (var item in manualHandShakes)
                        {
                            var bodyEmail = "";
                            if (rs.Count > 0)
                            {
                                var sendData = item.notifyFormat == EnumManualHandshake.SendData ? true : false;
                                bodyEmail = ExportToHtml(rs, sendData);
                            }

                            if (item.status == EnumManualHandshake.Active)
                            {
                                var account = new AccountService().GetByAccountId(item.accountId);
                                SendEmailManualHandshakeChange(account?.Profile.DisplayName, item.toName, item.toEmail, item.description,
                                    bodyEmail);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
            }
        }
        public void CheckManualHandshakes(string accountId, BsonDocument oldVault)
        {
            var rs = new List<ChangeValue>();
            try
            {
                var newVault = GetVaultByUserId(accountId);

                var newCurrentAddress = newVault["groupAddress"]["value"]["currentAddress"]["value"].AsBsonArray;
                var oldCurrentAddress = oldVault["groupAddress"]["value"]["currentAddress"]["value"].AsBsonArray;
                var currentAddress = GetManualHandshakeAddress(oldCurrentAddress, newCurrentAddress);
                if (currentAddress != null)
                {
                    currentAddress.Name = "Current Address";
                    rs.Add(currentAddress);
                }

                var newMailingAddress = newVault["groupAddress"]["value"]["mailingAddress"]["value"].AsBsonArray;
                var oldMailingAddress = oldVault["groupAddress"]["value"]["mailingAddress"]["value"].AsBsonArray;
                var mailingAddress = GetManualHandshakeAddress(oldMailingAddress, newMailingAddress);
                if (mailingAddress != null)
                {
                    mailingAddress.Name = "Mailing Address";
                    rs.Add(mailingAddress);
                }

                // Mobile
                var newMobile = newVault["contact"]["value"]["mobile"]["default"].AsString;
                var oldMobile = oldVault["contact"]["value"]["mobile"]["default"].AsString;
                var mobile = new ChangeValue();
                if (newMobile != oldMobile)
                {
                    mobile.Name = "Mobile Number";
                    mobile.OldValue = oldMobile;
                    mobile.NewValue = newMobile;
                    rs.Add(mobile);
                }
                var newOffice = newVault["contact"]["value"]["office"]["default"].AsString;
                var oldOffice = oldVault["contact"]["value"]["office"]["default"].AsString;

                var office = new ChangeValue();
                if (newOffice != oldOffice)
                {
                    office.Name = "Office Number";
                    office.OldValue = oldOffice;
                    office.NewValue = newOffice;
                    rs.Add(office);
                }
                // Mail
                var newPersonal = newVault["contact"]["value"]["email"]["default"].AsString;
                var oldPersonal = oldVault["contact"]["value"]["email"]["default"].AsString;

                var personal = new ChangeValue();
                if (newPersonal != oldPersonal)
                {
                    personal.Name = "Personal Email";
                    personal.OldValue = oldPersonal;
                    personal.NewValue = newPersonal;
                    rs.Add(personal);
                }

                var newOfficeMail = newVault["contact"]["value"]["officeEmail"]["default"].AsString;
                var oldOfficeMail = oldVault["contact"]["value"]["officeEmail"]["default"].AsString;

                var officeMail = new ChangeValue();
                if (newOfficeMail != oldOfficeMail)
                {
                    officeMail.Name = "Work Email";
                    officeMail.OldValue = oldOfficeMail;
                    officeMail.NewValue = newOfficeMail;
                    rs.Add(officeMail);
                }
                if (rs.Count > 0)
                {
                    var manualHandShakeBus = new ManualHandshakeBusinessLogic();
                    var manualHandShakes = manualHandShakeBus.GetActiveListByAccountId(accountId);
                    if (manualHandShakes.Count > 0)
                    {
                        foreach (var item in manualHandShakes)
                        {
                            var bodyEmail = "";
                            if (rs.Count > 0)
                            {
                                var sendData = item.notifyFormat == EnumManualHandshake.SendData ? true : false;
                                bodyEmail = ExportToHtml(rs, sendData);
                            }

                            if (item.status == EnumManualHandshake.Active)
                            {
                                var account = new AccountService().GetByAccountId(item.accountId);
                                SendEmailManualHandshakeChange(account?.Profile.DisplayName, item.toName, item.toEmail, item.description,
                                    bodyEmail);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
            }
        }

        private ChangeValue GetManualHandshakeAddress(BsonArray oldAddress, BsonArray newAddress)
        {
            var newAddressDefault = new BsonDocument();
            var oldAddressDefault = new BsonDocument();
            foreach (var item in newAddress)
            {
                // Son fix
                //if (item["_default"] == true)
                var doc = item.AsBsonDocument;
                if (doc.GetValue("_default", true).AsBoolean == true)
                {
                    newAddressDefault = new BsonDocument
                    {
                        {"addressLine", doc.GetValue("addressLine", string.Empty).AsString},
                        {"country", doc.GetValue("country", string.Empty).AsString},
                        {"city", doc.GetValue("city", string.Empty).AsString},
                        {"zipCode", doc.GetValue("zipCode", string.Empty).AsString},
                    };
                    break;
                }
            }
            foreach (var item in oldAddress)
            {
                // Son fix
                var doc = item.AsBsonDocument;
                //if (item["_default"] == true)
                if (doc.GetValue("_default", true).AsBoolean == true)
                {
                    oldAddressDefault = new BsonDocument
                    {
                        {"addressLine", doc.GetValue("addressLine", string.Empty).AsString},
                        {"country", doc.GetValue("country", string.Empty).AsString},
                        {"city", doc.GetValue("city", string.Empty).AsString},
                        {"zipCode", doc.GetValue("zipCode", string.Empty).AsString},
                    };
                    break;
                }
            }

            var rs = new ChangeValue();
            if (!oldAddressDefault.Equals(newAddressDefault))
            {
                rs.NewValue = GetAddressVaulue(newAddressDefault);
                rs.OldValue = GetAddressVaulue(oldAddressDefault);
            }
            else
            {
                rs = null;
            }

            return rs;
        }

        private string GetAddressVaulue(BsonDocument address)
        {
            var rs = "";
            try
            {
                var description = address.GetValue("description", BsonString.Empty).AsString;
                var startDate = address.GetValue("startDate", BsonString.Empty).AsString;
                var endDate = address.GetValue("endDate", BsonString.Empty).AsString;
                var addressLine = address.GetValue("addressLine", BsonString.Empty).AsString;
                var instruction = address.GetValue("instruction", BsonString.Empty).AsString;
                var country = address.GetValue("country", BsonString.Empty).AsString;
                var city = address.GetValue("city", BsonString.Empty).AsString;
                var zipCode = address.GetValue("zipCode", BsonString.Empty).AsString;
                if (!string.IsNullOrEmpty(description))
                    rs += "Description: " + description;
                if (!string.IsNullOrEmpty(addressLine))
                    rs += "<br/>Address line: " + addressLine;
                if (!string.IsNullOrEmpty(country))
                    rs += "<br/>Country: " + country;
                if (!string.IsNullOrEmpty(city))
                    rs += "<br/>City: " + city;
                if (!string.IsNullOrEmpty(zipCode))
                    rs += "<br/>ZIP / Postal Code: " + zipCode;
                if (!string.IsNullOrEmpty(instruction))
                    rs += "<br/>Additional instructions: " + zipCode;
                if (!string.IsNullOrEmpty(startDate))
                    rs += "<br/>From date: " + Convert.ToDateTime(startDate).ToShortDateString();
                if (!string.IsNullOrEmpty(endDate))
                    rs += "<br/>To date: " + Convert.ToDateTime(endDate).ToShortDateString();
            }
            catch
            {
            }


            return rs;
        }

        private BsonDocument GetAddressDefault(BsonArray address)
        {
            foreach (var item in address)
                if (item["_default"] == true)
                {
                    return item.AsBsonDocument;
                }
            return null;
        }

        public string EmailInviteManualHandshake(ManualHandshake manualHandshake)
        {
            var vault = GetVaultByUserId(manualHandshake.accountId);
            var mailHandshake = new ChangeViewModel();
            if (manualHandshake.fields.Count > 0)
            {
                foreach (var item in manualHandshake.fields)
                {
                    if (item.label == EnumManualHandshake.CurrentAddress && item.selected == true)
                    {
                        var addressArray = vault["groupAddress"]["value"]["currentAddress"]["value"].AsBsonArray;
                        var value = GetAddressVaulueByBsonArray(addressArray);
                        var changeValue = new ChangeValue();
                        changeValue.Name = EnumManualHandshake.CurrentAddress;
                        if (!string.IsNullOrEmpty(value))
                        {
                            changeValue.NewValue = value;
                        }

                        mailHandshake.Values.Add(changeValue);
                    }
                    else if (item.label == EnumManualHandshake.MailingAddress && item.selected == true)
                    {
                        var addressArray = vault["groupAddress"]["value"]["mailingAddress"]["value"].AsBsonArray;
                        var value = GetAddressVaulueByBsonArray(addressArray);
                        var changeValue = new ChangeValue();
                        changeValue.Name = EnumManualHandshake.MailingAddress;
                        if (!string.IsNullOrEmpty(value))
                        {
                            changeValue.NewValue = value;
                        }

                        mailHandshake.Values.Add(changeValue);
                    }
                    else if (item.label == EnumManualHandshake.MobileNumber && item.selected == true)
                    {
                        var value = vault["contact"]["value"]["mobile"]["default"].AsString;
                        var changeValue = new ChangeValue();
                        changeValue.Name = EnumManualHandshake.MobileNumber;
                        if (!string.IsNullOrEmpty(value))
                        {
                            changeValue.NewValue = value;
                        }

                        mailHandshake.Values.Add(changeValue);
                    }
                    else if (item.label == EnumManualHandshake.OfficeNumber && item.selected == true)
                    {
                        var value = vault["contact"]["value"]["office"]["default"].AsString;
                        var changeValue = new ChangeValue();
                        changeValue.Name = EnumManualHandshake.OfficeNumber;
                        if (!string.IsNullOrEmpty(value))
                        {
                            changeValue.NewValue = value;
                        }

                        mailHandshake.Values.Add(changeValue);
                    }
                    else if (item.label == EnumManualHandshake.PersonalMail && item.selected == true)
                    {
                        var value = vault["contact"]["value"]["email"]["default"].AsString;
                        var changeValue = new ChangeValue();
                        changeValue.Name = EnumManualHandshake.PersonalMail;
                        if (!string.IsNullOrEmpty(value))
                        {
                            changeValue.NewValue = value;
                        }

                        mailHandshake.Values.Add(changeValue);
                    }
                    else if (item.label == EnumManualHandshake.WorkMail && item.selected == true)
                    {
                        var value = vault["contact"]["value"]["officeEmail"]["default"].AsString;
                        var changeValue = new ChangeValue();
                        changeValue.Name = EnumManualHandshake.WorkMail;
                        if (!string.IsNullOrEmpty(value))
                        {
                            changeValue.NewValue = value;
                        }

                        mailHandshake.Values.Add(changeValue);
                    }
                }
            }
            var bodyEmail = "";
            var sendData = manualHandshake.notifyFormat == EnumManualHandshake.SendData ? true : false;
            if (mailHandshake.Values.Count > 0)
            {
                bodyEmail = ExportToHtml(mailHandshake.Values, sendData);
            }
            SendEmailManualHandshake(manualHandshake.name, manualHandshake.toName, manualHandshake.toEmail,
                manualHandshake.description, bodyEmail);

            return manualHandshake.toEmail;
        }

        private string GetAddressVaulueByBsonArray(BsonArray addressArray)
        {
            var rs = "";
            if (addressArray == null)
                return null;
            try
            {
                var address = new BsonDocument();
                if (addressArray.Count > 0)
                {
                    address = addressArray[0].AsBsonDocument;
                    for (int i = 0; i < addressArray.Count; i++)
                    {
                        var checkDefault = addressArray[i].AsBsonDocument;
                        if (checkDefault.GetValue("_default", false).AsBoolean)
                        {
                            address = addressArray[i].AsBsonDocument;
                            break;
                        }
                    }
                }


                var description = address.GetValue("description", BsonString.Empty).AsString;
                var startDate = address.GetValue("startDate", BsonString.Empty).AsString;
                var endDate = address.GetValue("endDate", BsonString.Empty).AsString;
                var addressLine = address.GetValue("addressLine", BsonString.Empty).AsString;
                var instruction = address.GetValue("instruction", BsonString.Empty).AsString;
                var country = address.GetValue("country", BsonString.Empty).AsString;
                var city = address.GetValue("city", BsonString.Empty).AsString;
                var zipCode = address.GetValue("zipCode", BsonString.Empty).AsString;
                //if (!string.IsNullOrEmpty(description))
                //    rs += "Description: " + description;
                if (!string.IsNullOrEmpty(addressLine))
                    rs += "<br/>Address line: " + addressLine;
                if (!string.IsNullOrEmpty(country))
                    rs += "<br/>Country: " + country;
                if (!string.IsNullOrEmpty(city))
                    rs += "<br/>City: " + city;
                if (!string.IsNullOrEmpty(zipCode))
                    rs += "<br/>ZIP / Postal Code: " + zipCode;

                //if (!string.IsNullOrEmpty(instruction))
                //    rs += "<br/>Additional instructions: " + instruction;

                //if (!string.IsNullOrEmpty(startDate))
                //    rs += "<br/>From date: " + Convert.ToDateTime(startDate).ToShortDateString();
                //if (!string.IsNullOrEmpty(endDate))
                //    rs += "<br/>To date: " + Convert.ToDateTime(endDate).ToShortDateString();
            }
            catch
            {
            }
            return rs;
        }

        private string ExportToHtml(List<ChangeValue> lstField, bool sendData = false)
        {
            var ContentHtml = "";
            if (sendData)
            {
                ContentHtml =
                    "<table border=1 width=\"100%\"><tr><th>Field Name</th><th>Old value</th><th>New Value</th> </tr>";
                for (int i = 0; i < lstField.Count; i++)
                {
                    if (!string.IsNullOrEmpty(lstField[i].NewValue) || !string.IsNullOrEmpty(lstField[i].OldValue))
                    {
                        ContentHtml += "<tr valign='top'><td>" + lstField[i].Name + "</td><td>" + lstField[i].OldValue +
                                       "</td> <td>" + lstField[i].NewValue + "</td> </tr>";
                    }
                }
                ContentHtml += "</table>";
            }
            else
            {
                for (int i = 0; i < lstField.Count; i++)
                {
                    if (!string.IsNullOrEmpty(lstField[i].Name))
                    {
                        ContentHtml += " - " + lstField[i].Name + "<br />";
                    }
                }
            }

            return ContentHtml;
        }

        private void SendEmailManualHandshake(string name, string toName, string toEmail, string description,
            string contentEmail)
        {
            var emailTemplate = string.Empty;
            emailTemplate = HostingEnvironment.MapPath("~/Content/EmailTemplates/email_template_ManualHandshake.html");
            string emailContent = string.Empty;
            var baseUrl = ConfigurationManager.AppSettings["baseUrl"];
            var begin = "Hello,";
            if (!string.IsNullOrEmpty(toName))
                begin = "Dear " + toName + ",";

            if (File.Exists(emailTemplate))
            {
                emailContent = File.ReadAllText(emailTemplate);
                emailContent = emailContent.Replace("[begin]", begin);
                emailContent = emailContent.Replace("[name]", name);
                emailContent = emailContent.Replace("[description]", description);
                emailContent = emailContent.Replace("[contentEmail]", contentEmail);
                emailContent = emailContent.Replace("[callbacklink]", baseUrl);

                IMailService mailService = new MailService();
                mailService.SendMailAsync(new NotificationContent
                {
                    Title = "Notification from Regit",
                    Body = string.Format(emailContent, ""),
                    SendTo = new[] {toEmail}
                });
            }
        }

        private void SendEmailManualHandshakeChange(string name, string toName, string toEmail, string description,
            string contentEmail)
        {
            var emailTemplate = string.Empty;
            emailTemplate =
                HostingEnvironment.MapPath("~/Content/EmailTemplates/email_template_ManualHandshakeChange.html");
            string emailContent = string.Empty;
            var baseUrl = ConfigurationManager.AppSettings["baseUrl"];
            var begin = "Hello,";
            if (!string.IsNullOrEmpty(toName))
                begin = "Dear " + toName + ",";

            if (File.Exists(emailTemplate))
            {
                emailContent = File.ReadAllText(emailTemplate);
                emailContent = emailContent.Replace("[begin]", begin);
                emailContent = emailContent.Replace("[toName]", toName);
                emailContent = emailContent.Replace("[name]", name);
                emailContent = emailContent.Replace("[description]", description);
                emailContent = emailContent.Replace("[contentEmail]", contentEmail);
                emailContent = emailContent.Replace("[callbacklink]", baseUrl);

                IMailService mailService = new MailService();
                var title = "Notification from " + name;

                mailService.SendMailAsync(new NotificationContent
                {
                    Title = title,
                    Body = string.Format(emailContent, ""),
                    SendTo = new[] {toEmail}
                });
            }
        }

        #endregion Manual Handshake
    }
}