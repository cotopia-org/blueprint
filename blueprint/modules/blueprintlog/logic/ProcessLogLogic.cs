using blueprint.core;
using blueprint.modules.account.database;
using blueprint.modules.account.response;
using blueprint.modules.database.logic;
using blueprint.modules.drive.logic;
using blueprint.modules.blueprintlog.database;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

using srtool;
using blueprint.modules.blueprintlog.response;
using blueprint.modules.blueprint.core;
using blueprint.modules.blueprint;

namespace blueprint.modules.blueprintlog.logic
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
        public async void AddLog(string blueprint_id, string process_id, string node_id, string type, string message)
        {
            try
            {
                await dbContext.InsertOneAsync(
                    new database.log_model()
                    {
                        _id = ObjectId.GenerateNewId().ToString(),
                        blueprint_id = blueprint_id,
                        process_id = process_id,
                        node_id = node_id,
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

        public async Task<PaginationResponse<LogResponse>> List(string id, Pagination pagination, string fromAccountId = null)
        {
            var result = new PaginationResponse<LogResponse>();
            var query = dbContext.AsQueryable();

            var blueprint = await BlueprintModule.Instance.Get(id, fromAccountId);

            query = query.Where(i => i.blueprint_id == blueprint.id);

            var dbItems = await query.OrderByDescending(i => i.createDateTime).Skip(pagination.Skip).Take(pagination.Take).ToListAsync();

            result.items = await List(dbItems);
            result.page = pagination.Page;
            result.perPage = pagination.PerPage;
            result.total = await query.LongCountAsync();
            return result;
        }
        public async Task<LogResponse> Get(string id, string fromAccountId = null)
        {
            var item = await dbContext.AsQueryable().Where(i => i._id == id).FirstOrDefaultAsync();
            if (item == null)
                throw new AppException(System.Net.HttpStatusCode.NotFound);

            return (await List(new List<log_model>() { item })).FirstOrDefault();
        }
        public async Task DeleteBlueprintLogs(string id, string fromAccountId = null)
        {
            await dbContext.DeleteManyAsync(Builders<log_model>.Filter.Eq(i => i.blueprint_id, id));
        }

        public async Task DeleteLog(string id, string fromAccountId = null)
        {
            await dbContext.DeleteManyAsync(Builders<log_model>.Filter.Eq(i => i._id, id));
        }
        public async Task DeleteLogId(string id, string fromAccountId = null)
        {
            await dbContext.DeleteManyAsync(Builders<log_model>.Filter.Eq(i => i._id, id));
        }
        public async Task<List<LogResponse>> List(List<string> ids, string fromAccountId = null)
        {
            if (ids == null)
                return new List<LogResponse>();

            var _ids = ids.Select(i => i.ToObjectId()).Distinct().ToList();
            var dbAccounts = await dbContext.Find_Cache("_id", ids.Select(i => i.ToObjectId()).ToList());

            return await List(dbAccounts, fromAccountId);
        }
        public async Task<List<LogResponse>> List(List<log_model> dbAccounts, string fromAccountId = null)
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
