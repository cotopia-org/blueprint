using blueprint.modules.node.types;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace blueprint.modules.node.database
{
    [BsonIgnoreExtraElements]
    public class node
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string name { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? account_id { get; set; }
        public string title { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string? icon_media_id { get; set; }
        public string description { get; set; }
        public List<NodeField> fields { get; set; }
        public List<Component> components { get; set; }
        public string? script { get; set; }
        public DateTime updateDateTime { get; set; }
        public DateTime createDateTime { get; set; }

    }
    public class Component
    {
        public string name { get; set; }
        public string param1 { get; set; }
        public string param2 { get; set; }
    }
}
