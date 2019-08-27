using System.Data;
using DaiDai.QueryBuilder;

namespace DaiDai.Repository
{
    public interface IDatabaseManager
    {
        IDbConnection GetConnection();

        IEntityQueryBuilder GetQueryBuilder();
    }
}