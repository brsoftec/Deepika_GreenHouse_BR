using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GH.Core.BlueCode.BusinessLogic;
using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.InformationVault;
using GH.Core.BlueCode.Entity.Interaction;
using GH.Core.BlueCode.Entity.Message;
using MongoDB.Driver;
using GH.Core.Models;
using GH.Core.ViewModels;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace GH.Core.Services
{
    public static class MessagingService
    {
       // private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly IMongoCollection<Conversation> ConversationCollection =
            MongoDBConnection.Database.GetCollection<Conversation>("Conversation");
        private static readonly IPersonalMessageService MessageService = new PersonalMessageService();

        public static FuncResult GetMessage(string messageId)
        {
            if (!ObjectId.TryParse(messageId, out var objId))
                return new ErrResult("message.invalid");
            var builder = Builders<Conversation>.Filter;
            var filterMessage = Builders<PersonalMessage>.Filter.Eq("Id", objId);
            var filter = builder.ElemMatch("Messages", filterMessage);

            var conversation = ConversationCollection.Find(filter).FirstOrDefault();
            if (conversation == null)
            return new ErrResult("message.notFound");
            var message = conversation.Messages.FirstOrDefault(m => m.Id == objId);
            if (message == null)
            return new ErrResult("message.notFound");
            return new OkResult("message.ok", message);
        }
        public static FuncResult SetMessageStatus(string messageId, string status)
        {
            if (!ObjectId.TryParse(messageId, out var objId))
                return new ErrResult("message.invalid");
            var builder = Builders<Conversation>.Filter;
            var filterMessage = Builders<PersonalMessage>.Filter.Eq("Id", objId);
            var filter = builder.ElemMatch("Messages", filterMessage);
/*            var conversation = ConversationCollection.Find(filter).FirstOrDefault();
            if (conversation == null)
            return new ErrResult("message.notFound");*/
            var result = ConversationCollection.UpdateOne(filter, Builders<Conversation>.Update.Set("Messages.$.Status", status));
            if (!result.IsAcknowledged)
                return new ErrResult("message.update.not");
            return new OkResult("message.update.ok");
        }       
        public static async Task<FuncResult> AcceptDrfiMessageAsync(PersonalMessage message, IList<UserFieldData> fields, 
            Account account, Account fromAccount)
        {
            var legacyFields = new List<FieldinformationVault>();
            var updateFields = new List<UserField>();

            foreach (var field in fields)
            {
                var vaultFields = await VaultService.ListFieldsAsync();

                var iField = vaultFields.FirstOrDefault(f => f.Path == field.Path);
                if (iField == null)
                    return new ErrResult("field.invalid", field.Path);
                
                var fieldFullName = $"{iField.Title} ({iField.Path})";
                
                var type = iField.Type;
                if (type == "static" || type == "qa")
                    return new ErrResult("field.type.invalid", fieldFullName);

                if (field.Value == null)
                    return new FuncResult(false, "field.null", fieldFullName);

                var value = field.Value;

                //    Validate field values
                switch (type)
                {
                    case "textbox":
                    case "radio":
                    case "checkbox":
                    case "select":
                    case "qa":
                    case "address":
                        if (!(value is string stringValue))
                            return new ErrResult("field.invalid", $"{fieldFullName} <{type}> expects string");
                        if (string.IsNullOrEmpty(stringValue))
                            return new ErrResult("field.null", $"{fieldFullName} empty string");
                        if (type == "textbox" && iField.Options.Equals("phone"))
                        {
                            value = "+" + stringValue.Trim('+');
                        }
                        break;
                    case "range":
                        if (!(value is string stringVal))
                            return new ErrResult("field.invalid", $"{fieldFullName} <select> expects string");
                        if (string.IsNullOrEmpty(stringVal))
                            return new ErrResult("field.null", $"{fieldFullName} empty string");

                            if (!Int32.TryParse(stringVal, out var index))
                            {
                                string rangeValue = null;
                                for (var i = 0; i < iField.Options.Length; i++)
                                {
                                    var rangeArray = iField.Options;
                                    if (value.ToString() == rangeArray[0] + "-" + rangeArray[1])
                                    {
                                        rangeValue = i.ToString();
                                    }
                                }

                                if (rangeValue == null)
                                    return new ErrResult("field.invalid.value",
                                        $"{fieldFullName} <select> value not in pre-defined list");
                                value = index.ToString();
                            }
                            else if (index >= iField.Options.Length)
                                return new ErrResult("field.invalid.value",
                                    $"{fieldFullName} <range> value out of bound");
                        break;
                    case "smartinput":
                    case "tagsinput":
                        if (!(value is JArray arrayValue))
                        {
                            if (!(value is string listStr))
                                return new ErrResult("field.invalid",
                                    $"{fieldFullName} <{type}> expects array or JSON");
                            try
                            {
                                arrayValue = JArray.Parse(listStr);
                            }
                            catch
                            {
                                return new ErrResult("field.invalid",
                                    $"{fieldFullName} <{type}> expects valid JSON array");
                            }
                        }

                        if (arrayValue.Count == 0)
                            return new ErrResult("field.null", $"{fieldFullName} empty list");

                        value = arrayValue;
                        break;
                    case "date":
                    case "datecombo":
                        if (!(value is string dateStr))
                            return new ErrResult("field.invalid",
                                $"{fieldFullName} <{type}> expects 'YYYY-MM-DD' string");
                        if (string.IsNullOrEmpty(dateStr) && iField.Options.ToString() != "indef")
                            return new ErrResult("field.null", $"{fieldFullName} empty date");
                        if (!string.IsNullOrEmpty(dateStr) && !Regex.IsMatch(dateStr, @"\d\d\d\d-\d\d-\d\d"))
                            return new ErrResult("field.invalid.format",
                                $"{fieldFullName} <{type}> expects 'YYYY-MM-DD' string");
                        var dateParts = dateStr.Split('-');
                        if (Convert.ToInt32(dateParts[0]) < 1900 || Convert.ToInt32(dateParts[1]) > 12 ||
                            Convert.ToInt32(dateParts[2]) > 31)
                            return new ErrResult("field.invalid.value", $"{fieldFullName} invalid date");
                        break;
                    case "location":
                        if (!(value is JObject locationObj))
                        {
                            if (!(value is string locationStr))
                                return new ErrResult("field.invalid",
                                    $"{fieldFullName} <location> expects object or JSON");
                            try
                            {
                                locationObj = JObject.Parse(locationStr);
                            }
                            catch
                            {
                                return new ErrResult("field.invalid",
                                    $"{fieldFullName} <location> expects valid JSON object");
                            }
                        }

                        if (string.IsNullOrEmpty(locationObj.SelectToken("country")?.ToString()))
                            return new ErrResult("field.invalid.format",
                                $"{fieldFullName} <location> requires {{ country }}");
                        value = locationObj;
                        break;
                    case "numinput":
                        if (!(value is JObject numObject))
                        {
                            if (!(value is string numStr))
                                return new ErrResult("field.invalid",
                                    $"{fieldFullName} <numinput> expects object or JSON");
                            try
                            {
                                numObject = JObject.Parse(numStr);
                            }
                            catch
                            {
                                return new ErrResult("field.invalid",
                                    $"{fieldFullName} <numinput> expects valid JSON object");
                            }
                        }

                        if (string.IsNullOrEmpty(numObject.SelectToken("amount")?.ToString()))
                            return new ErrResult("field.invalid.format",
                                $"{fieldFullName} <numinput> requires {{ amount }}");
                        value = numObject;
                        break;
                    case "doc":
                        if (!(value is JArray docArray))
                        {
                            if (!(value is string listStr))
                                return new ErrResult("field.invalid", $"{fieldFullName} <doc> expects array or JSON");
                            try
                            {
                                docArray = JArray.Parse(listStr);
                            }
                            catch
                            {
                                return new ErrResult("field.invalid",
                                    $"{fieldFullName} <doc> expects valid JSON array");
                            }
                        }

                        if (docArray.Count == 0)
                            return new ErrResult("field.null", $"{fieldFullName} empty document list");
                        var docs = new ArrayList();
                        foreach (var doc in docArray)
                        {

                            docs.Add(doc);
                        }

                        if (docs.Count == 0) docs.Add(docArray[0]);
                        value = docs;
                        break;
                }

                var source = FieldSource.VaultCurrentValue;

                var json = JsonConvert.SerializeObject(value);
                var bsDoc = BsonSerializer.Deserialize<object>(json);
                value = bsDoc;
                
                if (field.Source == "newValue" && iField.Type != "range")
                {
                    source = FieldSource.NewValue;
                    updateFields.Add(new UserField
                    {
                        Field = new UserFormField
                        {
                            Path = field.Path
                        },
                        Data = new UserFieldData
                        {
                            Value = value
                        }
                    });
                }


                // Construct Legacy Post fields

                var optionsJson = JsonConvert.SerializeObject(iField.Options);
//                var model = field.Value.ToString();
                var model = value.ToString();

                string modelarraystr = "";

                switch (type)
                {
                    case "doc":
                        var modelarrays = (JArray.FromObject(value)).Select(d =>
                            d.SelectToken("fileName")).ToList();
                        modelarraystr = JsonConvert.SerializeObject(modelarrays);
                        model = "";
                        break;

                    case "smartinput":
                    case "tagsinput":
                        if (value is List<object> list)
                        {
                            model = string.Join(",", list);
                        }

                        break;
                }

                var lField = new FieldinformationVault
                {
                    jsPath = field.Path,
                    displayName = iField.Title,
                    type = iField.Type,
                    optional = false,
                    options = iField.Options,
                    model = model,
                    modelarraysstr = modelarraystr
                };

                switch (type)
                {
                    case "doc":
                        lField.pathfile = (JArray.FromObject(value)).FirstOrDefault()?.SelectToken("filePath")
                            ?.ToString();
                        break;

                    case "location":
                    {
                        JObject location = JObject.FromObject(value);
                        lField.model = location.SelectToken("country")?.ToString();
                        lField.unitModel = location.SelectToken("city")?.ToString();
                        break;
                    }
                    case "numinput":
                    {
                        JObject num = JObject.FromObject(value);
                        lField.model = num.SelectToken("amount")?.ToString();
                        lField.unitModel = num.SelectToken("unit")?.ToString();
                        break;
                    }
                }

                legacyFields.Add(lField);
            }

            var messageId = message.Id.ToString();

            var response = new PersonalMessage
            {
                Id = ObjectId.GenerateNewId(),
                Text = "[DRFI Response]",
                Type = "drfiresponseaccepted",
                FromAccountId = account.AccountId,
                ToReceiverId = fromAccount.Id.ToString(),
                ConversationId = message.ConversationId,
                JsonFieldsdrfi = JsonConvert.SerializeObject(legacyFields),
                DrfiRequestId = messageId
            };
            await MessageService.AddPersonalMessageAsync(response, true);

            var result = MessagingService.SetMessageStatus(messageId, "accepted");
            if (!result.Success)
            {
               // return Request.CreateApiErrorResponse("Error accepting DRFI request", error: "drfi.accept.error");
            }
            
            //    Save current vault
            var vaultCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("InformationVault");
            var filterVault = Builders<BsonDocument>.Filter.Eq("userId", account.AccountId);
            BsonDocument vaultDoc = vaultCollection.Find(filterVault).SingleOrDefault();
            vaultDoc?.Remove("_id");

            var updateResult = await VaultService.UpdateVaultFields(updateFields, account.AccountId);
            List<FieldUpdate> fieldUpdates = (List<FieldUpdate>) updateResult.Data;
            if (!updateResult.Success || fieldUpdates.Count == 0)
            {
            }
            else
            {
                await new InteractionService().SyncHandshakeFields(fieldUpdates, legacyFields, account.AccountId);

                foreach (var field in fieldUpdates)
                {
                    var path = field.Path;
                    if (path.Contains(".currentAddress") || path.Contains(".currentAddress")
                                                         || path.Contains(".contact.mobile") ||
                                                         path.Contains(".contact.office")
                                                         || path.Contains(".contact.email") ||
                                                         path.Contains(".contact.officeEmail"))
                    {
                        var vaultService = new InfomationVaultBusinessLogic();
                        vaultService.CheckManualHandshakes(account.AccountId, vaultDoc);
                        break;
                    }
                }
            }

            return new OkResult("drfi.accept.ok",response.Id.ToString());
        }

    }
}