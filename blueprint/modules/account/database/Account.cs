using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace blueprint.modules.account.database
{
    [BsonIgnoreExtraElements]
    public class Account
    {
        [BsonId]
        public ObjectId _id { get; set; }
        public string username { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string hashedPassword { get; set; }
        public  string saltPassword{get; set; }
        public ObjectId? avatar_fileId { get; set; }
        public List<string> roles { get; set; }

        public DateTime signupDateTime { get; set; }
        public DateTime activeDateTime { get; set; }
    }
}
