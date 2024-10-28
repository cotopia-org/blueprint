using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api_server.modules.auth.database
{
    [BsonIgnoreExtraElements]
    public class SigninSession
    {
        [BsonId]
        public ObjectId _id { get; set; }
        public ObjectId account_id { get; set; }
        public string sessionName { get; set; }
        public string refreshToken { get; set; }
        public DateTime loginDateTime { get; set; }
    }
}
