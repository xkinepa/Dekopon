using System;
using System.Data;
using Dekopon.Profiler;
using Dekopon.QueryBuilder;
using Dekopon.Transaction;

namespace Dekopon.Repository
{
    public interface IDatabaseManager
    {
        IDbConnection GetConnection();

        IQueryBuilder GetQueryBuilder();
    }

    public abstract class AbstractDatabaseManagerBuilder<TBuilder> where TBuilder : AbstractDatabaseManagerBuilder<TBuilder>
    {
        protected ITransactionManager TransactionManager;
        protected IDbProfiler DbProfiler;
        protected IQueryBuilder QueryBuilder;
        protected Action<IDbConnection> AfterCreatedAction;

        public abstract IDatabaseManager Build();

        public TBuilder SetTransactionManager(ITransactionManager transactionManager)
        {
            TransactionManager = transactionManager;
            return (TBuilder)this;
        }

        public TBuilder SetDbProfiler(IDbProfiler dbProfiler)
        {
            DbProfiler = dbProfiler;
            return (TBuilder)this;
        }

        public TBuilder SetQueryBuilder(IQueryBuilder queryBuilder)
        {
            QueryBuilder = queryBuilder;
            return (TBuilder)this;
        }

        public TBuilder AfterCreated(Action<IDbConnection> afterCreatedAction)
        {
            AfterCreatedAction = afterCreatedAction;
            return (TBuilder)this;
        }
    }
}