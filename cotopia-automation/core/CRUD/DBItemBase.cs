using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace blueprint.core.CRUD
{
    [BsonIgnoreExtraElements]
    public class DBItemBase
    {
        [BsonId]
        public ObjectId _id { get; set; }
        public DateTime createDateTime { get; set; }
        public DateTime updateDateTime { get; set; }
    }
}
