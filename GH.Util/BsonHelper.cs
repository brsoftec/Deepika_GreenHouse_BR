using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GH.Util
{
    public class BsonHelper
    {
        public static BsonDocument ConvertVaultJson2Bson(string jsonString)
        {
            var bsonDoc = BsonDocument.Parse(jsonString);
            bsonDoc["_id"] = ObjectId.Parse(bsonDoc["_id"].RawValue as string);

            return bsonDoc;
        }

        public static string ConvertVaultBson2Json(BsonDocument bson)
        {
            bson["_id"] = bson["_id"].AsObjectId.ToString();
            return bson.ToJson();
        }

        public static string GenerateObjectIdString()
        {
            return ObjectId.GenerateNewId().ToString().Replace("ObjectId(","").Replace(")","");
        }

        public static XDocument ConvertBsonToXDocument(BsonValue bson)
        {
            XDocument xDoc = JsonConvert.DeserializeXNode(bson.ToJson(), "root");
            return xDoc;
        }

        public static string ConvertXDocumentToJson(XDocument xDoc)
        {
            return JsonConvert.SerializeXNode(xDoc);
        }

        public static BsonDocument ConvertXDocumentToBson(XDocument xDoc)
        {
            string json = JsonConvert.SerializeXNode(xDoc);
            var bsonDoc = BsonDocument.Parse(json);
            return bsonDoc;
        }

        public static string GetvaluestringFromObject(BsonValue bson)
        {
            try
            {
                if (bson.IsBsonNull)
                    return "";
                return bson.ToString();
            }
            catch {

            }
            return "";
        }
        public static int GetvalueIntFromObjectOnelevel(BsonValue bson, string namelevelone)
        {
            try
            {
                if (bson[namelevelone].IsBsonNull)
                    return 0;
                return bson[namelevelone].AsInt32;
            }
            catch
            {

            }
            return 0;
        }
        public static string GetvaluestringFromObjectOnelevel(BsonValue bson,string namelevelone)
        {
            try
            {
                if (bson[namelevelone].IsBsonNull)
                    return "";
                return bson[namelevelone].ToString();
            }
            catch
            {

            }
            return "";
        }



    }
}
