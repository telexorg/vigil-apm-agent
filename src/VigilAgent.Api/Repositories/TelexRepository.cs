using VigilAgent.Api.Data;
using VigilAgent.Api.IRepositories;
using VigilAgent.Api.Models;

namespace VigilAgent.Api.Repositories
{
    public class TelexRepository<T> : ITelexRepository<T> where T : IEntity
    {

        private readonly TelexDbContext _context;

        public TelexRepository(TelexDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateAsync(T document)
        {
            var response = await _context.AddAsync<T>(document);
            return response.Status == "success";
        }

        public async Task<Document<T>> GetByIdAsync(string id)
        {
            var result = await _context.GetSingle<T>(id);
            return result.Status == "success" ? result.Data : null;
        }

        public async Task<List<Document<T>>> GetAllAsync(object filter = null)
        {
            var result = await _context.GetAll<T>(filter);

            return result.Status == "success" ? result.Data : new List<Document<T>>();
        }

        public async Task<List<Document<T?>>> FilterAsync(object filter)
        {
            var result = await _context.GetAll<T>(filter);
            return result.Status == "success" ? result.Data : new List<Document<T>>();
        }

        public async Task<bool> UpdateAsync(string id, T document)
        {
            var response = await _context.UpdateAsync<T>(id, document);
            return response.Status == "success";
        }
                
        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _context.DeleteAsync<T>(id);
            return response.Status == "success";
        }

    }

}
