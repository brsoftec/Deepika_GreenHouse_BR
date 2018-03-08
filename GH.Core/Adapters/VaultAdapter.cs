using GH.Core.BlueCode.Entity.InformationVault;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.Adapters
{
    public class VaultAdapter
    {
        public FormVault BasicBsonToForm(BsonDocument bsonVault)
        {

            var listField = new List<FieldVault>();
            //var f = new FieldVault();
            //f.Name = "Basic Information";
            //f.Type = "basicInformation";

            //listField.Add(f);

            var formVault = new FormVault();
            formVault.Name = EnumVaultGroup.BasicName;
            formVault.Type = EnumVaultGroup.Basic;
            formVault.Values = listField;

            var json = bsonVault["value"].AsBsonDocument;
            var i = 1;
         foreach(var s in EnumListJsPathVaultForm.Basic)
            {
                var field = new FieldVault();
                field.Id = i;
                field.Name = json[s]["label"].AsString;
                field.Value = json[s]["value"].AsString;
                listField.Add(field);
            }
            return formVault;
        }

        //
     

    }

    public class Item
    {
      

        [JsonExtensionData]
        public Dictionary<string, object> CustomFields
        {
            get
            {
                if (_customFields == null)
                    _customFields = new Dictionary<string, object>();
                return _customFields;
            }
            private set
            {
                _customFields = value;
            }
        }
        private Dictionary<string, object> _customFields;
    }
  

    public class CrazyStringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            // Reverse the string just for fun
            return new string(token.ToString().Reverse().ToArray());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}