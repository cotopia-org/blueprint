using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using srtool;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;

namespace blueprint.core.CRUD
{
    public class MongoCRUD_Handler<T>
    {
        private IMongoCollection<T> collection { get; set; }
        private string PREFIX_CACHE_KEY = "A11";
        public MongoCRUD_Handler(IMongoCollection<T> collection)
        {
            this.collection = collection;
        }
        public async Task Create(T item)
        {
            await collection.InsertOneAsync(item);
            string _id = ((dynamic)item)._id;
            SuperCache.Set<T>(item, new CacheSetting() { timeLife = TimeSpan.FromSeconds(60), key = $"{PREFIX_CACHE_KEY}:{_id}" });

        }
        public async Task<bool> Update(T item)
        {
            dynamic _dbItem = item;
            string _id = _dbItem._id;
            var result = await collection.ReplaceOneAsync(Builders<T>.Filter.Eq("_id", _id), item);
            if (result.IsAcknowledged && result.ModifiedCount > 0)
            {
                SuperCache.Set<T>(item, new CacheSetting() { timeLife = TimeSpan.FromSeconds(60), key = $"{PREFIX_CACHE_KEY}:{_id}" });
                return true;
            }
            else
                return false;
        }
        public async Task<T> Read(string id)
        {
            if (!SuperCache.Exist($"{PREFIX_CACHE_KEY}:{id}"))
            {
                var item = await collection.Find(Builders<T>.Filter.Eq("_id", id)).FirstOrDefaultAsync();
                SuperCache.Set<T>(item, new CacheSetting() { timeLife = TimeSpan.FromSeconds(60), key = $"{PREFIX_CACHE_KEY}:{id}" });
                return item;
            }
            else
            {
                var item = SuperCache.Get<T>($"{PREFIX_CACHE_KEY}:{id}");
                return item;
            }
        }
        public async Task<PaginationResponse<T>> Read(Expression<Func<T, bool>> whereExpression, int page, int perPage)
        {
            await collection.AsQueryable().Where(whereExpression).ToListAsync();
            return new PaginationResponse<T>();
        }

        public async Task<List<T>> Read(IEnumerable<string> ids)
        {
            var _notExist = new List<string>();
            var items = new List<T>();
            foreach (var id in ids)
            {
                if (!SuperCache.Exist($"{PREFIX_CACHE_KEY}:{id}"))
                {
                    _notExist.Add(id);
                }
                else
                {
                    var _cacheItem = SuperCache.Get<T>($"{PREFIX_CACHE_KEY}:{id}");
                    if (_cacheItem != null)
                        items.Add(_cacheItem);
                }
            }
            if (_notExist.Count > 0)
            {
                var _newItems = await collection.Find(Builders<T>.Filter.In("_id", _notExist)).ToListAsync();

                foreach (var item in _newItems)
                {
                    items.Add(item);
                    string _id = ((dynamic)item)._id;

                    SuperCache.Set<T>(item, new CacheSetting() { timeLife = TimeSpan.FromSeconds(60), key = $"{PREFIX_CACHE_KEY}:{_id}" });
                }
            }

            return ids.Select(i => items.FirstOrDefault(j => (string)((dynamic)j)._id == i)).Where(i => i != null).ToList();
        }
        public async Task<bool> Delete(ObjectId id)
        {
            var result = await collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", id));
            if (result.IsAcknowledged && result.DeletedCount > 0)
            {
                SuperCache.Remove($"{PREFIX_CACHE_KEY}:{id}");
                return true;
            }
            else
            {
                return false;
            }
        }
        public async Task<int> Delete(IEnumerable<string> ids)
        {
            var result = await collection.DeleteManyAsync(Builders<T>.Filter.In("_id", ids));
            if (result.IsAcknowledged && result.DeletedCount > 0)
            {
                foreach (var id in ids)
                {
                    SuperCache.Remove($"{PREFIX_CACHE_KEY}:{id}");
                }
                return (int)result.DeletedCount;
            }
            else
                return 0;
        }
        public async Task<long> Count()
        {
            if (!SuperCache.Exist($"{PREFIX_CACHE_KEY}:count"))
            {
                long count = await collection.CountDocumentsAsync(Builders<T>.Filter.Empty);
                SuperCache.Set(count, new CacheSetting() { key = $"{PREFIX_CACHE_KEY}:count", timeLife = TimeSpan.FromSeconds(60) });
                return count;
            }
            else
            {
                return SuperCache.Get<long>($"{PREFIX_CACHE_KEY}:count");
            }
        }
    }
}
