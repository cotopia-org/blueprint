using blueprint.core;
using blueprint.modules.config;
using MongoDB.Driver;

namespace blueprint.modules.database
{
    public class DatabaseModule : Module<DatabaseModule>
    {
        public MongoClient client { get; private set; }
        public IMongoDatabase database { get; private set; }
        public override async Task RunAsync()
        {
            client = new MongoClient(ConfigModule.GetString("mongodb.db-connection"));
            database = client.GetDatabase(ConfigModule.GetString("mongodb.db-name"));
            await base.RunAsync();

        }
    }
}
