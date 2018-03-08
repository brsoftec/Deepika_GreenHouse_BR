using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GH.Core.Models
{
    public class Country
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Code3 { get; set; }
        public string NumCode { get; set; }
        public string PhoneCode { get; set; }
    }

    public class City
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string CountryCode { get; set; }
        public string RegionCode { get; set; }
        public string Name { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }

    public class Region
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string CountryCode { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public int Cities { get; set; }
    }
}