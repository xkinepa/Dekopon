using System.Collections.Generic;

namespace DaiDai.Repository
{
    public interface IRepository<T> : IRepository
    {
        IList<T> FindAll();
    }

    public interface IRepository
    {
    }
}