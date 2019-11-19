using System;
using System.Threading.Tasks;

namespace Parallel.Repository
{
    public interface IDatabaseContext : IDisposable
    {
        IRepository<T> GetSet<T>() where T : class,IEntity;
        Task<int> SaveChanges();
    }
}