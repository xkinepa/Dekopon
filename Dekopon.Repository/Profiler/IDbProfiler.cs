using System.Data.Common;

namespace Dekopon.Profiler
{
    public interface IDbProfiler
    {
        DbConnection Profile(DbConnection rawConnection);
    }
}