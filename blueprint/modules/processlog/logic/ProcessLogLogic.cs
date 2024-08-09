using blueprint.core;
using blueprint.modules.account.database;
using blueprint.modules.account.response;
using blueprint.modules.database.logic;
using blueprint.modules.drive.logic;
using blueprint.modules.log.response;
using blueprint.modules.processlog.database;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

using srtool;

namespace blueprint.modules.processlog.logic
{
    public class ProcessLogLogic : Module<ProcessLogLogic>
    {
        public IMongoCollection<database.log_model> dbContext { get; private set; }
        public string WebRootPath { get; set; }
        public override async Task RunAsync()
        {
            await base.RunAsync();
            dbContext = DatabaseModule.Instance.database.GetCollection<database.log_model>("processlog");
        }
        public async void AddLog(string blueprint_id, string process_id, string type, string message)
        {
            try
            {
                await dbContext.InsertOneAsync(
                    new database.log_model()
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

        public async Task<PaginationResponse<LogResponse>> List(string blueprint_id, Pagination pagination)
        {
            var result = new PaginationResponse<LogResponse>();

            var _q = dbContext.AsQueryable()
                  .Where(i => i.blueprint_id == blueprint_id).OrderByDescending(i => i.createDateTime)
                  .Skip(pagination.Skip)
                  .Take(pagination.Take);

            var dbItems = await _q.ToListAsync();

            result.items = await List(dbItems);
            result.page = pagination.Page;
            result.perPage = pagination.PerPage;
            result.total = -1;
            return result;
        }

        public async Task<List<LogResponse>> List(List<string> ids, string fromAccountId = null)
        {
            if (ids == null)
                return new List<LogResponse>();

            var _ids = ids.Select(i => i.ToObjectId()).Distinct().ToList();
            var dbAccounts = await dbContext.Find_Cache("_id", ids.Select(i => i.ToObjectId()).ToList());

            return await List(dbAccounts);
        }
        public async Task<List<LogResponse>> List(List<log_model> dbAccounts)
        {
            await Task.Yield();
            var results = dbAccounts.Select(i => new
            {
                res = new LogResponse()
                {
                    id = i._id.ToString(),
                    createDateTime = i.createDateTime,
                    blueprint_id = i.blueprint_id,
                    message = i.message,
                    process_id = i.process_id,
                    type = i.type,
                },
            }).ToList();

            return results.Select(i => i.res).ToList();
        }
    }
}
