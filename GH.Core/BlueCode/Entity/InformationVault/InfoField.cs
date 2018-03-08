using MongoDB.Bson;


namespace GH.Core.BlueCode.Entity.InformationVault
{
    public class InfoField
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Value { get; set; }
        public string Option { get; set; }
        public string Type { get; set; }
        public string Privacy { get; set; }
        public string Unit { get; set; }
        public string Default { get; set; }
        public string jsPath { set; get; }
   
    }
}