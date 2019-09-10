using System.Data;
using Daidai.QueryBuilder;

namespace Daidai.Repository
{
    public interface IDatabaseManager
    {
        IDbConnection GetConnection();

        IEntityQueryBuilder GetQueryBuilder();
    }
}