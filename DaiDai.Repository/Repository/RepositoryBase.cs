﻿using System.Collections.Generic;
using System.Data;
using System.Linq;
using Daidai.Entity;
using Daidai.QueryBuilder;
using Dapper;

namespace Daidai.Repository
{
    public abstract class RepositoryBase<T> : RepositoryBase, IRepository<T>
    {
        protected EntityDefinition EntityDefinition { get; }
        protected IEntityQueryBuilder EntityQueryBuilder { get; }

        protected string TableName => EntityQueryBuilder.Table(EntityDefinition);
        protected string AllColumnNames => EntityQueryBuilder.Columns(EntityDefinition);

        protected RepositoryBase(IDatabaseManager databaseManager) : base(databaseManager)
        {
            EntityDefinition = EntityDefinitionContainer.Instance.Get(typeof(T));
            EntityQueryBuilder = databaseManager.GetQueryBuilder();
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

        protected RepositoryBase(IDatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        protected IDbConnection Conn => _databaseManager.GetConnection();
    }
}