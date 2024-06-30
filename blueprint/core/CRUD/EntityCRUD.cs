using Microsoft.ClearScript.JavaScript;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using srtool;
using System.Linq;
using System.Linq.Expressions;

namespace blueprint.core.CRUD
{
    public class EntityCRUD<T> where T : DBItemBase
    {
        public TimeSpan CacheTime { get; set; }
        public IMongoCollection<T> DbContext { get; private set; }
        private string PREFIX_CACHE_KEY { get; set; }
        public EntityCRUD(IMongoCollection<T> dbContext)
        {
            DbContext = dbContext;
            PREFIX_CACHE_KEY = $"EntityCRUD_{GetHashCode()}";
            CacheTime = TimeSpan.FromSeconds(0);
        }

        public async Task<T> Upsert(T item)
        {
            var _dbItem = (DBItemBase)item;
            var result = await DbContext.ReplaceOneAsync(i => i._id == _dbItem._id, item, new ReplaceOptions { IsUpsert = true });
            if (result.IsAcknowledged && result.ModifiedCount == 0 && result.UpsertedId != BsonValue.Create(ObjectId.Empty))
            {
                _dbItem._id = result.UpsertedId.AsObjectId;
                _dbItem.createDateTime = DateTime.UtcNow;
            }
            else
            if (result.IsAcknowledged && result.ModifiedCount == 1)
            {
                _dbItem.updateDateTime = DateTime.UtcNow;
            }

            SuperCache.Set<T>(item, new CacheSetting() { timeLife = CacheTime, key = $"{PREFIX_CACHE_KEY}_{_dbItem._id}" });

            return item;
        }
        public async Task<T> Read(ObjectId id)
        {
            if (!SuperCache.Exist($"{PREFIX_CACHE_KEY}_{id}"))
            {
                var item = await DbContext.Find(i => i._id == id).FirstOrDefaultAsync();
                SuperCache.Set<T>(item, new CacheSetting() { timeLife = CacheTime, key = $"{PREFIX_CACHE_KEY}_{id}" });
                return item;
            }
            else
            {
                var item = SuperCache.Get<T>($"{PREFIX_CACHE_KEY}_{id}");
                return item;
            }
        }
        public async Task<PaginationResponse<T>> Read(Expression<Func<T, bool>> whereExpression, int page, int perPage)
        {
            await DbContext.AsQueryable().Where(whereExpression).ToListAsync();
            return new PaginationResponse<T>();
        }

        public async Task<List<T>> Read(IList<ObjectId> ids)
        {
            var _notExist = new List<ObjectId>();
            var items = new List<T>();
            foreach (var id in ids)
            {
                if (!SuperCache.Exist($"{PREFIX_CACHE_KEY}_{id}"))
                {
                    _notExist.Add(id);
                }
                else
                {
                    var _cacheItem = SuperCache.Get<T>($"{PREFIX_CACHE_KEY}_{id}");
                    if (_cacheItem != null)
                        items.Add(_cacheItem);
                }
            }
            if (_notExist.Count > 0)
            {
                var _newItems = await DbContext.Find(i => _notExist.Contains(i._id)).ToListAsync();

                foreach (var item in _newItems)
                {
                    items.Add(item);
                    SuperCache.Set<T>(item, new CacheSetting() { timeLife = CacheTime, key = $"{PREFIX_CACHE_KEY}_{item._id}" });
                }
            }

            return ids.Select(i => items.FirstOrDefault(j => j._id == i)).Where(i => i != null).ToList();
        }
        public async Task<bool> Delete(ObjectId id)
        {
            var result = await DbContext.DeleteOneAsync(i => i._id == id);
            if (result.IsAcknowledged && result.DeletedCount > 0)
            {
                SuperCache.Remove($"{PREFIX_CACHE_KEY}_{id}");
                return true;
            }
            else
            {
                return false;
            }
        }
        public async Task<int> Delete(IList<ObjectId> ids)
        {
            var result = await DbContext.DeleteManyAsync(i => ids.Contains(i._id));
            if (result.IsAcknowledged && result.DeletedCount > 0)
            {
                foreach (var id in ids)
                {
                    SuperCache.Remove($"{PREFIX_CACHE_KEY}_{id}");
                }
                return (int)result.DeletedCount;
            }
            else
                return 0;
        }
    }
}
