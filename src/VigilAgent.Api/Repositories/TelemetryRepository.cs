using BloggerAgent.Domain.Commons;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using VigilAgent.Api.Commons;
using VigilAgent.Api.Data;
using VigilAgent.Api.Helpers;
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

        public async Task<bool> AddAsync(T item)
        {
            try
            {
                await _collection.InsertOneAsync(item);
                return true; // Success
            }
            catch (Exception ex)
            {
                // Optionally log the exception here
                return false; // Failure
            }


        }

        public async Task<List<T>> GetLastNAsync(int n)
        {
            return await _collection.Find(_ => true)
                                    .SortByDescending(t => t.Timestamp)
                                    .Limit(n)
                                    .ToListAsync();
        }

        public async Task<Dictionary<string, List<T>>> GetLastNPerProjectAsync(int countPerProject)
        {
            var projectGroups = await _collection
                .AsQueryable()
                .GroupBy(x => new { x.OrgId, x.ProjectName })
                .ToListAsync();

            var result = new Dictionary<string, List<T>>();
            foreach (var group in projectGroups)
            {
                var key = CacheKey.For(group.Key.OrgId, group.Key.ProjectName);
                var recentItems = group
                    .OrderByDescending(x => x.Timestamp) // assuming there's a Timestamp field
                    .Take(countPerProject)
                    .ToList();

                result[key] = recentItems;
            }

            return result;
        }
    }
}
