using BloggerAgent.Domain.Commons;
using MongoDB.Driver;
using VigilAgent.Api.Commons;
using VigilAgent.Api.Data;
using VigilAgent.Api.IRepositories;

namespace VigilAgent.Api.Repositories
{
    public class TelemetryRepository<T> : ITelemetryRepository<T> where T : ITelemetryItem
    {
        private readonly IMongoCollection<T> _collection;

        public TelemetryRepository(MongoDbContext context)
        {
            _collection = context.GetCollection<T>(CollectionType.GetCollectionName(typeof(T)));
        }

        public async Task AddAsync(T item) => await _collection.InsertOneAsync(item);

        public async Task<List<T>> GetLastNAsync(int n)
        {
            return await _collection.Find(_ => true)
                                    .SortByDescending(t => t.Timestamp)
                                    .Limit(n)
                                    .ToListAsync();
        }
    }
}
