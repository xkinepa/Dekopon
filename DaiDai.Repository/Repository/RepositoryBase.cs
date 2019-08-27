using System.Collections.Generic;
using System.Data;
using System.Linq;
using DaiDai.Entity;
using DaiDai.QueryBuilder;
using Dapper;

namespace DaiDai.Repository
{
    public abstract class RepositoryBase<T> : RepositoryBase, IRepository<T>
    {
        protected EntityDefinition EntityDefinition { get; }

        protected string TableName => EntityQueryBuilder.Table(EntityDefinition);
        protected string AllColumnNames => EntityQueryBuilder.Columns(EntityDefinition);

        protected RepositoryBase(IDatabaseManager databaseManager) : base(databaseManager)
        {
            EntityDefinition = EntityDefinitionContainer.Instance.Get(typeof(T));
        }

        public virtual IList<T> FindAll()
        {
            var (query, @params) = EntityQueryBuilder.FindAll(EntityDefinition);
            return Conn.Query<T>(query, @params).ToList();
        }
    }

    public abstract class RepositoryBase : IRepository
    {
        private readonly IDatabaseManager _databaseManager;

        protected IEntityQueryBuilder EntityQueryBuilder { get; }

        protected RepositoryBase(IDatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;

            EntityQueryBuilder = databaseManager.GetQueryBuilder();
        }

        protected IDbConnection Conn => _databaseManager.GetConnection();
    }
}