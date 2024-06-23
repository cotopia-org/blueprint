using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace blueprint.modules.scheduler.database
{
    [BsonIgnoreExtraElements]
    public class schedule
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string category { get; set; }
        public string key { get; set; }
        public string payload { get; set; }
        public bool repeat { get; set; }
        public DateTime createDateTime { get; set; }
        public DateTime invokeTime { get; set; }
    }
}
