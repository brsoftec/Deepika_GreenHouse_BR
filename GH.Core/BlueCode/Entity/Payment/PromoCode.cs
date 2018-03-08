
using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]

public class PromoCode
{
    public string Code { set; get; }
    public string Type { set; get; }
    public bool IsUse { set; get; }
    public bool IsActive { set; get; }

    public int NumberReUse { set; get; }

    public int NumberMonthExpired { set; get; }

}