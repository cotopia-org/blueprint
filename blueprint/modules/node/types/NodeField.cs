using MongoDB.Bson.Serialization.Attributes;

namespace blueprint.modules.node.types
{
    public class NodeField
    {
        public string name { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public FieldType fieldType { get; set; }
        public bool required { get; set; }
        public string defaultValue { get; set; }
        public List<NodeField> fields { get; set; }
        public string meta { get; set; }

    }
}
