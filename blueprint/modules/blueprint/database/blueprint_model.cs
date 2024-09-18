using blueprint.core.CRUD;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace blueprint.modules.blueprint.database
{
    [BsonIgnoreExtraElements]
    public class blueprint_model
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string? account_id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public bool active { get; set; }
        public string data_snapshot { get; set; }

        public List<string> index_tokens { get; set; }

        public long exec_counter { get; set; }

        public DateTime createDateTime { get; set; }
        public DateTime updateDateTime { get; set; }
    }
}
