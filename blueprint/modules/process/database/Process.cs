using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace blueprint.modules.blueprintProcess.database
{
    [BsonIgnoreExtraElements]
    public class Process
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string blueprint_id { get; set; }
        public string snapshot { get; set; }
        public DateTime createDateTime { get; set; }
    }
}
