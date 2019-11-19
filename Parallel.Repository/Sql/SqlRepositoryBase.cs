using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Parallel.Repository.Sql
{
    public class SqlRepositoryBase<T> : IRepository<T> where T: class, IEntity
    {
        private readonly DbSet<T> _set;

        public SqlRepositoryBase(DbSet<T> set)
        {
            _set = set;
        }

        public IAsyncEnumerable<T> GetAllAsync()
        {
            return _set.AsAsyncEnumerable();
        }

        public IEnumerable<T> GetAll()
        {
            return _set;
        }

        public T GetByKey(params object[] key)
        {
            return _set.Find(key);
        }

        public Task<T> GetByKeyAsync(params object[] key)
        {
            return _set.FindAsync(key).AsTask();
        }

        public T Add(T item)
        {
            return _set.Add(item).Entity;
        }

        public async Task<T> AddAsync(T item)
        {
            var result = await _set.AddAsync(item);
            return result.Entity;
        }

        public T Update(T updatedItem)
        {
            return _set.Update(updatedItem).Entity;
        }

        public Task<T> UpdateAsync(T updatedItem)
        {
            return Task.Run(() => Update(updatedItem));
        }
    }
}