using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace blueprint.modules.schedule.database
{
    [BsonIgnoreExtraElements]
    public class Schedule
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string category { get; set; }
        public string key { get; set; }
        public string expression { get; set; }
        public string payload { get; set; }
        public bool repeat { get; set; }
        public DateTime nextOccurrenceTime { get; set; }
        public DateTime checkinTime { get; set; }
        public DateTime updateTime { get; set; }
    }
}
