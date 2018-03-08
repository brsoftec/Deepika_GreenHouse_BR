
using MongoDB.Bson.Serialization.Attributes;

namespace GH.Core.Models
{
    public class Resource
    {
        [BsonId]
        public string Id { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public string Path { get; set; }
        public string[] Paths { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Permissions Permissions { get; set; }
    }
    
    public class Permissions
    {
        public string Admin { get; set; }
        public string Editor { get; set; }
        public string Approver { get; set; }
    }

}