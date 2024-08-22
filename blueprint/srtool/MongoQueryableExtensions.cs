using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace srtool
{
    public static class MongoQueryableExtensions
    {

        public static IMongoQueryable<TDocument> NearSphere<TDocument>(
        this IMongoQueryable<TDocument> source,
        string field, double x, double y, double? maxDistance, double? minDistance)
        {
            var regexFilter = Builders<TDocument>.Filter.NearSphere(field, x, y, maxDistance, minDistance);
            return source.Where(document => regexFilter.Inject());
        }

        public static IMongoQueryable<TDocument> WhereRegex<TDocument>(
            this IMongoQueryable<TDocument> source,
            string fieldName,
            string pattern
            )
        {
            var regexFilter = Builders<TDocument>.Filter.Regex(fieldName, new BsonRegularExpression(pattern));
            return source.Where(document => regexFilter.Inject());
        }

        public static IMongoQueryable<TDocument> WhereRegex<TDocument>(
          this IMongoQueryable<TDocument> source,
          string fieldName1,
          string fieldName2,
          string pattern
          )
        {
            var regexFilter1 = Builders<TDocument>.Filter.Regex(fieldName1, new BsonRegularExpression(pattern));
            var regexFilter2 = Builders<TDocument>.Filter.Regex(fieldName2, new BsonRegularExpression(pattern));

            return source.Where(document => regexFilter1.Inject() || regexFilter2.Inject());
        }

        public static IMongoQueryable<TDocument> WhereTextContains<TDocument>(
    this IMongoQueryable<TDocument> source,
    string searchTerm)
        {
            var filter = Builders<TDocument>.Filter.Text(searchTerm);
            return source.Where(document => filter.Inject());
        }
    }
}
