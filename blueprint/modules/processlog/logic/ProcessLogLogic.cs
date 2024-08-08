using blueprint.core;
using blueprint.modules.database.logic;
using blueprint.modules.processlog.database;
using MongoDB.Bson;
using MongoDB.Driver;
using srtool;

namespace blueprint.modules.processlog.logic
{
    public class ProcessLogLogic : Module<ProcessLogLogic>
    {
        public IMongoCollection<log> dbContext { get; private set; }
        public string WebRootPath { get; set; }
        public override async Task RunAsync()
        {
            await base.RunAsync();
            dbContext = DatabaseModule.Instance.database.GetCollection<log>("processlog");
        }
        public async void AddLog(string blueprint_id, string process_id, string type, string message)
        {
            try
            {
                await dbContext.InsertOneAsync(
                    new log()
                    {
                        _id = ObjectId.GenerateNewId().ToString(),
                        blueprint_id = blueprint_id,
                        process_id = process_id,
                        type = type,
                        message = message,
                        createDateTime = DateTime.UtcNow
                    }
                    );
            }
            catch (Exception e)
            {
                Debug.Error(e);
            }
        }
    }
}
