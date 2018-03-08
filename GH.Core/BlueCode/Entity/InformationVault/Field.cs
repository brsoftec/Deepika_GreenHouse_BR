using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace GH.Core.BlueCode.Entity.InformationVault
{

    public class InfomationVault
    {
        public InfomationVault()
        {
            Values = new List<GroupVault>();
        }
        [BsonId]
        public int Id { get; set; }
        [BsonElement("userId")]
        public string UserId { set; get; }
        [BsonElement("dateUpdate")]
        public DateTime DateUpdate { set; get; }
        [BsonElement("version")]
        public string Version { set; get; }
        [BsonElement("status")]
        public string Status { set; get; }
        [BsonElement("value")]
        public List<GroupVault> Values { get; set; }
    }

    // Type Group: financial Name: Financial
    public class GroupVault
    {
        [BsonId(IdGenerator = typeof(CounterIdGenerator))]
        [BsonElement("id")]
        public int Id { get; set; }
        [BsonElement("name")]
        public string Name { set; get; }
        [BsonElement("type")]
        public string Type { set; get; }
        [BsonElement("value")]
        public List<FormVault> Values { set; get; }
    }
    // Type Form: currentAddress Name: Current Address 1
    public class FormVault
    {
        [BsonId(IdGenerator = typeof(CounterIdGenerator))]
        [BsonElement("id")]
        public int Id { get; set; }
        [BsonElement("name")]
        public string Name { set; get; }
        [BsonElement("type")]
        public string Type { set; get; }

        [BsonElement("value")]
        public List<FieldVault> Values { set; get; }
        [BsonElement("default")]
        public bool IsDefault { get; set; }
    }
    public class FieldVault
    {
        [BsonId(IdGenerator = typeof(CounterIdGenerator))]
        [BsonElement("id")]
        public int Id { get; set; }
        [BsonElement("name")]
        public string Name { set; get; }
        [BsonElement("type")]
        public string Type { set; get; }
        [BsonElement("value")]
        public string Value { set; get; }
        public List<ValueKey> Values { set; get; }
        public List<ValueKey> Options { set; get; }
        [BsonElement("order")]
        public int Order { get; set; }
    }
    public class ValueKey
    {
        [BsonElement("id")]
        public string Id { get; set; }
        [BsonElement("name")]
        public string Name { get; set; }

    }

    public class CounterIdGenerator : IIdGenerator
    {
        private static int _counter = 0;
        public object GenerateId(object container, object document)
        {
            return _counter++;
        }

        public bool IsEmpty(object id)
        {
            return id.Equals(default(int));
        }
    }
 
    public sealed class EnumTypeGroup
    {
        public static readonly string Basic = "information";
        public static readonly string Contact = "contact";
        public static readonly string Address = "address";
        public static readonly string Financial = "financial";
        public static readonly string Government = "governmentId";
        public static readonly string Family = "family";
        public static readonly string Pet = "pet";
        public static readonly string Membership = "membership";
        public static readonly string Employment = "employment";
        public static readonly string Education = "education";
        public static readonly string Others = "others";
        public static readonly string Document = "document";
    }
    public sealed class EnumTypeField
    {
     
        public static readonly string Text = "Text";
        public static readonly string Number = "Number";
        public static readonly string Option = "Opotions";
        public static readonly string DateTime = "Date time";
        public static readonly string Location = "Location";
        public static readonly string Address = "Address";

    }

}