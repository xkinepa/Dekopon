﻿using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dekopon.Entity;
using Dekopon.QueryBuilder;
using Dapper;

namespace Dekopon.Repository
{
    public abstract class RepositoryBase<T> : RepositoryBase, IRepository<T>
    {
        protected EntityDefinition EntityDefinition { get; }

        protected IEntityQueryBuilder QueryBuilder { get; }

        protected string TableName => QueryBuilder.Table(EntityDefinition);
        protected string AllColumnNames => QueryBuilder.Columns(EntityDefinition);
        protected string WhereClause => QueryBuilder.Where(EntityDefinition);

        protected RepositoryBase(IDatabaseManager databaseManager) : base(databaseManager)
        {
            EntityDefinition = EntityDefinitionContainer.Instance.Get(typeof(T));
            QueryBuilder = (IEntityQueryBuilder) databaseManager.GetQueryBuilder();
        }

        public virtual IList<T> FindAll()
        {
            var (query, @params) = QueryBuilder.FindAll(EntityDefinition);
            return Conn.Query<T>(query, @params).ToList();
        }
    }

    public abstract class RepositoryBase : IRepository
    {
        private readonly IDatabaseManager _databaseManager;

        protected RepositoryBase(IDatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        protected IDbConnection Conn => _databaseManager.GetConnection();
    }
}