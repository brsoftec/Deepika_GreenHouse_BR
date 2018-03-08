namespace GH.Core.Extensions
{
    using MongoDB.Bson;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public static class ExtensionBsonDocument
    {
        public static T GHValue<T>(this BsonDocument bson, string fieldName)
        {
            if (!bson.Contains(fieldName)) return default(T);

            return (T)Convert.ChangeType(bson.GetElement(fieldName).Value.ToString(), typeof(T));
        }
       

    }
   


}