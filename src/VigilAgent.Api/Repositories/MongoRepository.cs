using BloggerAgent.Domain.Commons;
using MongoDB.Driver;
using System.Linq.Expressions;
using VigilAgent.Api.Data;
using VigilAgent.Api.IRepositories;
using VigilAgent.Api.Models;

namespace VigilAgent.Api.Repositories
{
    public class MongoRepository<T> : IMongoRepository<T>
    {
        private readonly IMongoCollection<T> _collection;

        public MongoRepository(MongoDbContext context)
        {
            var collectionName = CollectionType.GetCollectionName(typeof(T));


            if (collectionName == null)
            {
                throw new ArgumentException($"No collection name defined for type {typeof(T).Name}");
            }

            _collection = context.GetCollection<T>(CollectionType.GetCollectionName(typeof(T)));
        }


        public async Task<T> Create(T entity)
        {
            await _collection.InsertOneAsync(entity);
            return entity;
        }


        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>> filter)
        {
            var result = await _collection.FindAsync(filter);
            return await result.ToListAsync();
        }


        public async Task<List<T>> GetLastNAsync(Expression<Func<T, bool>> filter, int limit, string sortField = "_id")
        {
            return await _collection
                .Find(filter)
                .Sort(Builders<T>.Sort.Descending(sortField))
                .Limit(limit)
                .ToListAsync();
        }


        public Task<T?> FindOneAsync(Expression<Func<T, bool>> filter)
        {
            return _collection.Find(filter).FirstOrDefaultAsync();
        }

        
        public Task<List<T>> FindManyAsync(Expression<Func<T, bool>> filter)
        {
            return _collection.Find(filter).ToListAsync();
        }

       
        public Task<List<T>> FindAllAsync()
        {
            return _collection.Find(_ => true).ToListAsync();
        }

       
        public async Task<bool> UpdateAsync(Expression<Func<T, bool>> filter, T entity)
        {
            var result = await _collection.ReplaceOneAsync(filter, entity);
            return result.ModifiedCount > 0;
        }

        
        public async Task<long> DeleteAsync(Expression<Func<T, bool>> filter)
        {
            var result = await _collection.DeleteManyAsync(filter);
            return result.DeletedCount;
        }

        
        public Task<long> CountAsync(Expression<Func<T, bool>>? filter = null)
        {
            if (filter == null)
                return _collection.CountDocumentsAsync(_ => true);

            return _collection.CountDocumentsAsync(filter);
        }

    }
}
