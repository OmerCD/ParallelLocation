using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace Parallel.Repository
{
    public interface IRepository<T> where T : class, IEntity
    {
        IAsyncEnumerable<T> GetAllAsync();
        IEnumerable<T> GetAll();
        T GetByKey(params object[] key);
        Task<T> GetByKeyAsync(params object[] key);
        T Add(T item);
        Task<T> AddAsync(T item);
        T Update(T updatedItem);
        Task<T> UpdateAsync(T updatedItem);
    }
}