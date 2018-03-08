using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GH.Core.BlueCode.DataAccess;

using MongoDB.Driver;
using GH.Core.Models;
using GH.Core.ViewModels;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using NLog;

namespace GH.Core.Services
{
    public static class VaultService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static readonly IMongoCollection<VaultField> VaultFieldCollection =
            MongoDBConnection.Database.GetCollection<VaultField>("VaultField");

        public static async Task<List<UserVaultField>> ListFieldsAsync()
        {
            var builder = Builders<VaultField>.Filter;
            var filter = !builder.Exists("Branch");

            var fields = await VaultFieldCollection.Find(filter).ToListAsync();
            var userFields = new List<UserVaultField>();
            foreach (var f in fields)
            {
                if (f.Type == "range" || f.Type == "doc") continue;
                var field = new UserVaultField
                {
                    Path = f.Id,
                    Title = f.Title,
                    Type = f.Type,
                    Options = f.Options ?? new object[] {} 
                };
                userFields.Add(field);
            }

            return userFields;

        }

        public static UserVaultField GetField(string path)
        {
            var builder = Builders<VaultField>.Filter;
            var filter = builder.Eq("Id",path);

            var f = VaultFieldCollection.Find(filter).FirstOrDefault();
            if (f == null) return null;
            return new UserVaultField 
            {
                Path = f.Id,
                Title = f.Title,
                Type = f.Type,
                Options = f.Options ?? new object[] {} 
            };
        }
        public static async Task<UserVaultField> GetFieldAsync(string path)
        {
            var builder = Builders<VaultField>.Filter;
            var filter = builder.Eq("Id",path);

            var f = await VaultFieldCollection.Find(filter).FirstOrDefaultAsync();
            if (f == null) return null;
            return new UserVaultField 
            {
                Path = f.Id,
                Title = f.Title,
                Type = f.Type,
                Options = f.Options ?? new object[] {} 
            };
        }
        
        
        public static async Task<FuncResult> UpdateVaultFields(IEnumerable<UserField> fields, string accountId)
        {
            var vaultCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("InformationVault");
            var filter = Builders<BsonDocument>.Filter.Eq("userId", accountId);
            BsonDocument vaultDoc = vaultCollection.Find(filter).SingleOrDefault();
            vaultDoc.Remove("_id");

//            JObject vault = JObject.Parse(vaultDoc.ToJson());
            JObject vault = JObject.FromObject(vaultDoc.ToDictionary());

            var fieldUpdates = new List<FieldUpdate>();

            foreach (var field in fields)
            {
                var path = field.Field.Path;
                var type = field.Field.Type;
                var value = field.Data.Value;

                var pathTokens = path.Trim('.').Split('.');

                if (pathTokens.Length < 2)
                    return new FuncResult(false, "fieldpath.invalid");

                string token = null, token2 = null, addFormToken = null, addFormDefaultToken = null;
                object value2 = value;
                object addFormItem = null;

                var bucket = pathTokens[0];


                switch (bucket)
                {
                    case "contact":
                    {
                        var contact = vault.SelectToken($"contact.value.{pathTokens[1]}");
                        var defaultValue = (string) contact.SelectToken("default");
                        var index = contact.SelectToken("value").Children().FirstOrDefault(
                                            c => (string) c.SelectToken("value") == defaultValue)?
                                        .BeforeSelf().Count() ?? 0;
                        token = $"contact.value.{pathTokens[1]}.value.{index}.value";
                        token2 = $"contact.value.{pathTokens[1]}.default";
                        break;
                    }

                    case "education":
                    case "employment":
                    case "membership":
                    {
                        var prop = pathTokens[1];
                        var node = vault.SelectToken($"{bucket}");
                        var defaultDesc = (string) node.SelectToken("default");
                        var index = 0;

                        var items = (JArray) node.SelectToken("value");

                        if (items.Count == 0)
                        {
                            var item = new
                            {
                                _id = 1,
                                privacy = string.Empty,
                                description = string.Empty,
                                note = string.Empty,
                                _default = true
                            };

                            addFormToken = $"{bucket}.value";
                            addFormItem = item;

                            addFormDefaultToken = $"{bucket}.default";
                        }
                        else
                        {
                            index = items.FirstOrDefault(a => a.SelectToken("description").ToString() == defaultDesc)?
                                        .BeforeSelf().Count() ?? 0;
                        }

                        switch (prop)
                        {
                            default:
                                token = $"{bucket}.value.{index}.{prop}";
                                break;
                        }

                        break;
                    }

                    case "address":
                    case "financial":
                    case "governmentID":
                    {
                        bucket = bucket == "address" ? "groupAddress" :
                            bucket == "financial" ? "groupFinancial" : "groupGovernmentID";
                        var prop = pathTokens[2];
                        var node = vault.SelectToken($"{bucket}.value.{pathTokens[1]}");
                        if (node == null) return new ErrResult("vault.error");
                        var defaultDesc = node.SelectToken("default")?.ToString() ?? "";
                        var index = 0;
                        var items = (JArray) node.SelectToken("value");
                        if (items.Count == 0)
                        {
                            var item = new
                            {
                                _id = 1,
                                privacy = string.Empty,
                                description = string.Empty,
                                note = string.Empty,
                                _default = true
                            };

                            addFormToken = $"{bucket}.value.{pathTokens[1]}.value";
                            addFormItem = item;

                            addFormDefaultToken = $"{bucket}.value.{pathTokens[1]}.default";
                        }
                        else
                        {
                            index = items.FirstOrDefault(a => a.SelectToken("description").ToString() == defaultDesc)?
                                        .BeforeSelf().Count() ?? 0;
                        }

                        switch (prop)
                        {
                            case "address":
                                prop = "addressLine";
                                break;
                            case "location":
                                var location = JObject.FromObject(value);
                                var subPathTokens = pathTokens.Take(pathTokens.Length - 1).ToArray();
                                token = $"{bucket}.value.{pathTokens[1]}.value.{index}.country";
                                value = location.SelectToken("country")?.ToString();
                                token2 = $"{bucket}.value.{pathTokens[1]}.value.{index}.city";
                                value2 = location.SelectToken("city")?.ToString();
                                break;
                        }

                        if (token == null)
                            token = $"{bucket}.value.{pathTokens[1]}.value.{index}.{prop}";
                        break;
                    }

                    default:
                    {
                        var prop = pathTokens[pathTokens.Length - 1];
                        switch (prop)
                        {
                            case "location":
                                var location = JObject.FromObject(value);
                                var subPathTokens = pathTokens.Take(pathTokens.Length - 1).ToArray();
                                token = string.Join(".value.", subPathTokens) + ".value.country.value";
                                value = location.SelectToken("country")?.ToString();
                                token2 = string.Join(".value.", subPathTokens) + ".value.city.value";
                                value2 = location.SelectToken("city")?.ToString();
                                break;
                            case "height":
                            case "weight":
                            case "neck":
                            case "arm":
                            case "inner":
                            case "biceps":
                            case "shoes":
                            case "ring":
                                var num = JObject.FromObject(value);
                                token = string.Join(".value.", pathTokens) + ".value";
                                token2 = string.Join(".value.", pathTokens) + ".unit";
                                value = num.SelectToken("amount")?.ToString();
                                value2 = num.SelectToken("unit")?.ToString();
                                break;
                            default:
                                token = string.Join(".value.", pathTokens) + ".value";
                                break;
                        }

                        break;
                    }
                }

                if (!string.IsNullOrEmpty(addFormToken))
                {
                    var formUpdate = Builders<BsonDocument>.Update.AddToSet(addFormToken, addFormItem);
                    var formResult = await vaultCollection.UpdateOneAsync(filter, formUpdate);
                    formUpdate = Builders<BsonDocument>.Update.Set(addFormDefaultToken, string.Empty);
                    formResult = await vaultCollection.UpdateOneAsync(filter, formUpdate);
                }

                var update = Builders<BsonDocument>.Update.Set(token, value);
                var result = await vaultCollection.UpdateOneAsync(filter, update);
                var fieldUpdate = new FieldUpdate
                {
                    Path = path,
                    Timestamp = DateTimeOffset.UtcNow
                };
                if (!result.IsAcknowledged || result.ModifiedCount == 0)
                {
                    fieldUpdate.Success = false;
                    fieldUpdate.Status = "notUpdated";
                }
                else
                {
                    fieldUpdate.Success = true;
                    fieldUpdate.Status = "updated";
                }

                fieldUpdates.Add(fieldUpdate);
                if (!string.IsNullOrEmpty(token2))
                {
                    update = Builders<BsonDocument>.Update.Set(token2, value2);
                    await vaultCollection.UpdateOneAsync(filter, update);
                }
            }

            return new FuncResult(true, "vault.updated", fieldUpdates);
        }
   }
}