using Microsoft.Extensions.Options;
using MongoDB.Driver;
using VigilAgent.Api.Commons;
using VigilAgent.Api.Models;

namespace VigilAgent.Api.Data
{
    public class MongoDbContext
    {
        public IMongoDatabase Database { get; set; }  

        public MongoDbContext(IOptions<MongoOptions> config)
        {
                       
            var client = new MongoClient(config.Value.ConnectionString);
            Database = client.GetDatabase(config.Value.DatabaseName);
        }

        public IMongoCollection<T> GetCollection<T>(in string name) => Database.GetCollection<T>(name);

    }
}
