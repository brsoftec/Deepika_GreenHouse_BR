using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.BlueCode.Entity.InformationVault
{
    public class DocumentVault
    {
        public DocumentVault()
        {
            Documents = new List<Document>();
        }
        public ObjectId Id { get; set; }
        public string UserId { set; get; }
        public string Name { set; get; }
        public string Label { set; get; }
        public string Default { set; get; }
        public string Privacy { set; get; }

        public List<Document> Documents { set; get; }

    }
    public class Document
    {
        public int Id { get; set; }
        public string FileName { set; get; }
        public string SaveName { set; get; }
        public string Type { set; get; }
        public string Category { set; get; }
        public string UploadDate { set; get; }
        public string ExpiredDate { set; get; }
        public string Path { set; get; }
        public bool NoSearch { set; get; }
        public string Privacy { set; get; }
    }
}
