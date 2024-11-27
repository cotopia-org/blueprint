using MongoDB.Bson.Serialization.Attributes;

namespace blueprint.modules.node.types
{
    public class NodeField
    {
        public string name { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public FieldType type { get; set; }
        public bool required { get; set; }
        public string defaultValue { get; set; }
        public List<EnumValue> listValue { get; set; }
        public List<NodeField> fields { get; set; }
        public string meta { get; set; }

    }
    public class EnumValue
    {
        public string value { get; set; }
        public string display { get; set; }
    }
}
