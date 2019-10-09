using System.Collections.Generic;

namespace Dekopon.Repository
{
    public interface IRepository<T> : IRepository
    {
        IList<T> FindAll();
    }

    public interface IRepository
    {
    }
}