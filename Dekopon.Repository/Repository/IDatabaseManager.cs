using System.Data;
using Dekopon.QueryBuilder;

namespace Dekopon.Repository
{
    public interface IDatabaseManager
    {
        IDbConnection GetConnection();

        IEntityQueryBuilder GetQueryBuilder();
    }
}