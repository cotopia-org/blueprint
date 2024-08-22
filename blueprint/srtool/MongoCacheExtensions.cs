using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using srtool;
using System.Reflection;
public static class MongoCacheExtensions
{
    public static async Task<T> Find_Cache<T>(this IMongoCollection<T> collection, string fieldName, string value)
    {
        var result = new List<KeyValuePair<string, T>>();
        var prefixCacheKey = $"mongo_cache>{collection.CollectionNamespace.FullName}>CacheFind";


        var k = $"{prefixCacheKey}:{fieldName}:{value}";
        if (SuperCache.Exist(k))
        {
            return SuperCache.Get<T>(k);
        }
        else
        {
            var item = await collection.Find(Builders<T>.Filter.Eq(fieldName, value)).FirstOrDefaultAsync();
            SuperCache.Set(item, new CacheSetting(k, TimeSpan.FromHours(1)));

            return item;
        }
    }
    public static async Task<T> Find_Cache<T>(this IMongoCollection<T> collection, string fieldName, ObjectId value)
    {
        var result = new List<KeyValuePair<string, T>>();
        var prefixCacheKey = $"mongo_cache>{collection.CollectionNamespace.FullName}>CacheFind";


        var k = $"{prefixCacheKey}:{fieldName}:{value}";
        if (SuperCache.Exist(k))
        {
            return SuperCache.Get<T>(k);
        }
        else
        {
            var item = await collection.Find(Builders<T>.Filter.Eq(fieldName, value)).FirstOrDefaultAsync();
            SuperCache.Set(item, new CacheSetting(k, TimeSpan.FromHours(1)));

            return item;
        }
    }
    public static async Task<List<T>> Find_Cache<T>(this IMongoCollection<T> collection, string fieldName, List<ObjectId> values)
    {
        var result = new List<KeyValuePair<string, T>>();
        var prefixCacheKey = $"mongo_cache>{collection.CollectionNamespace.FullName}>CacheFind";

        var _ids = values.Distinct();

        var _need_to_feach_ids = new List<ObjectId>();

        foreach (var id in _ids)
        {
            var k = $"{prefixCacheKey}:{fieldName}:{id}";
            if (SuperCache.Exist(k))
            {
                result.Add(new KeyValuePair<string, T>(id.ToString(), SuperCache.Get<T>(k)));
            }
            else
            {
                _need_to_feach_ids.Add(id);
            }
        }
        if (_need_to_feach_ids.Count > 0)
        {
            var items = await collection.Find(Builders<T>.Filter.In(fieldName, _need_to_feach_ids)).ToListAsync();

            var property = typeof(T).GetProperty(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var item in items)
            {
                var id = property.GetValue(item);

                result.Add(new KeyValuePair<string, T>(id.ToString(), item));

                var k = $"{prefixCacheKey}:{fieldName}:{id}";
                SuperCache.Set(item, new CacheSetting(k, TimeSpan.FromHours(1)));
            }
        }
        return values.Select(i => result.FirstOrDefault(j => j.Key == i.ToString()).Value).Where(i=>i != null).ToList();
    }
    public static void CacheFind_remove<T>(this IMongoCollection<T> collection, string fieldName, string value)
    {
        var prefixCacheKey = $"mongo_cache>{collection.CollectionNamespace.FullName}>CacheFind";
        SuperCache.Remove($"{prefixCacheKey}:{fieldName}:{value}");
    }
    public static void CacheFind_remove<T>(this IMongoCollection<T> collection, string fieldName, ObjectId value)
    {
        CacheFind_remove(collection, fieldName, value.ToString());
    }
}