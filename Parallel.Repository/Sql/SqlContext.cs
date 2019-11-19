using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Parallel.Repository.Sql
{
    public class SqlContext : DbContext, IDatabaseContext
    {
        public SqlContext(DbContextOptions<SqlContext> options):base(options){}
        public new void Dispose()
        {
            base.Dispose();
        }

        public IRepository<T> GetSet<T>() where T : class, IEntity
        {
            return new SqlRepositoryBase<T>(Set<T>());
        }

        public new Task<int> SaveChanges()
        {
            return SaveChangesAsync();
        }
    }
}