using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace blueprint.modules.blueprintlog.database
{
    [BsonIgnoreExtraElements]
    public class log_model
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string blueprint_id { get; set; }
        public string process_id { get; set; }
        public string node_id { get; set; }
        public string type { get; set; }
        public string message { get; set; }
        public DateTime createDateTime { get; set; }
    }
}
