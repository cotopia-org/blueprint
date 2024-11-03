using blueprint.core;
using blueprint.modules.config;
using MongoDB.Driver;

namespace blueprint.modules.database.logic
{
    public class DatabaseModule : Module<DatabaseModule>
    {
        public MongoClient client { get; private set; }
        public IMongoDatabase database { get; private set; }
        public override async Task RunAsync()
        {
            var connection = ConfigModule.GetString("mongodb.db-connection");
            client = new MongoClient(connection);
            database = client.GetDatabase(ConfigModule.GetString("mongodb.db-name"));
            await base.RunAsync();

        }
    }
}
