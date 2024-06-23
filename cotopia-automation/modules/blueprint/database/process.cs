using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace blueprint.modules.blueprint.database
{
    [BsonIgnoreExtraElements]
    public class process
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string blueprint_id { get; set; }
        public string snapshot { get; set; }
        public bool end { get; set; }
        public DateTime createDateTime { get; set; }
    }
}
