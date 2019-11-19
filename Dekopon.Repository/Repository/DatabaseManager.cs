using System;
using System.Data;
using System.Data.Common;
using System.Transactions;
using Dekopon.Profiler;
using Dekopon.QueryBuilder;
using Dekopon.Transaction;
using Microsoft.EntityFrameworkCore;

namespace Dekopon.Repository
{
    public class DatabaseManager : TransactionAwareResourceManager<DbConnection>, IDatabaseResourceManager, IDatabaseManager, IDisposable
    {
        private readonly Func<DbContext> _databaseFactory;
        private readonly IDbProfiler _dbProfiler;
        private readonly IQueryBuilder _queryBuilder;

        public static Builder NewBuilder(DbContextOptions dbContextOptions)
        {
            return new Builder(dbContextOptions);
        }

        public static Builder NewBuilder(Func<DbContext> databaseFactory)
        {
            return new Builder(databaseFactory);
        }

        public DatabaseManager(Func<DbContext> databaseFactory,
            ITransactionManager transactionManager = null, IDbProfiler dbProfiler = null, IQueryBuilder queryBuilder = null)
            : base(transactionManager)
        {
            _databaseFactory = databaseFactory;
            _dbProfiler = dbProfiler;
            _queryBuilder = queryBuilder;
        }

        public virtual IDbConnection GetConnection() => GetResource();

        public IQueryBuilder GetQueryBuilder() => _queryBuilder;

        public override void Dispose()
        {
            base.Dispose();
        }

        protected override DbConnection CreateResource(System.Transactions.Transaction transaction = null)
        {
            var dbContext = _databaseFactory.Invoke();
            dbContext.Database.OpenConnection(); // open then enlist
            dbContext.Database.EnlistTransaction(transaction); // check null
            var connection = dbContext.Database.GetDbConnection();
            return _dbProfiler != null ? _dbProfiler.Profile(connection) : connection;
        }

        public class Builder : AbstractDatabaseManagerBuilder<Builder>
        {
            private readonly Func<DbContext> _databaseFactory;

            public Builder(DbContextOptions dbContextOptions)
                : this(() => new DbContext(dbContextOptions))
            {
            }

            public Builder(Func<DbContext> databaseFactory)
            {
                _databaseFactory = databaseFactory;
            }

            public override IDatabaseManager Build()
            {
                return new DatabaseManager(_databaseFactory, TransactionManager, DbProfiler, QueryBuilder);
            }
        }
    }

    public interface IDatabaseResourceManager : IResourceManager<DbConnection>, IDatabaseManager
    {
    }
}