using System.Collections.Generic;

namespace DaiDai.Repository
{
    public interface ICrudRepository<T> : IRepository<T>
    {
        IList<T> FindAll(IList<T> entities);
        T Get(T entity);

        long Add(T entity);
        int AddAll(IList<T> entities, int chunk = 100);

        int Update(T entity);
        int UpdateAll(IList<T> entities, int chunk = 100);

        int Delete(T entity);
        int DeleteAll(IList<T> entities);

        IList<T> FindByIdIn(IList<long> ids);
        T GetById(long id);
        int DeleteById(long id);
        int DeleteByIdIn(IList<long> ids);
    }
}