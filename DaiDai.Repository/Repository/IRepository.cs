using System.Collections.Generic;

namespace Daidai.Repository
{
    public interface IRepository<T> : IRepository
    {
        IList<T> FindAll();
    }

    public interface IRepository
    {
    }
}