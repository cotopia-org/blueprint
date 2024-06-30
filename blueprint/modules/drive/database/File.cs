using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;

namespace blueprint.modules.drive.database
{
    [BsonIgnoreExtraElements]
    public class File
    {
        [BsonId]
        public ObjectId _id { get; set; }
        public ObjectId account_id { get; set; }
        public string name { get; set; }
        public string uniqueName { get; set; }

        public string title { get; set; }
        public string extension { get; set; }
        public long size { get; set; }
        public DateTime createDateTime { get; set; }
    }
}
