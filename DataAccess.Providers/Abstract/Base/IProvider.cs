using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DataAccess.Providers.Abstract.Base
{
    public interface IProvider<TEntity, in TId>
        where TEntity : class
    {
        Task<List<TEntity>> GetAll(int take = int.MaxValue, int skip = 0);
        Task<TEntity> GetById(TId id);
        Task<List<TEntity>> Get(Func<TEntity, bool> predicate, int take = int.MaxValue, int skip = 0);
        Task<int> GetCount();
        Task<TEntity> FirstOrDefault(Expression<Func<TEntity, bool>> predicate);
        Task Add(TEntity added);
        Task AddRange(IEnumerable<TEntity> added);
        Task Edit(TEntity edited);
        Task EditRange(IEnumerable<TEntity> edited);
        Task Remove(TEntity removed);
    }

}
