using System;
using System.Data;
using System.Data.Common;
using System.Transactions;
using Dekopon.QueryBuilder;
using Dekopon.Transaction;
using Microsoft.EntityFrameworkCore;

namespace Dekopon.Repository
{
    public class DatabaseManager : TransactionAwareResourceManager<DbConnection>, IDatabaseResourceManager, IDatabaseManager, IDisposable
    {
        private readonly Func<DbContext> _databaseFactory;
        private readonly IQueryBuilder _queryBuilder;

        public DatabaseManager(DbContextOptions dbContextOptions, ITransactionManager transactionManager = null, IQueryBuilder queryBuilder = null)
            : this(() => new DbContext(dbContextOptions), transactionManager, queryBuilder)
        {
        }

        public DatabaseManager(Func<DbContext> databaseFactory, ITransactionManager transactionManager = null, IQueryBuilder queryBuilder = null)
            : base(transactionManager)
        {
            _databaseFactory = databaseFactory;
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
            return dbContext.Database.GetDbConnection();
        }
    }

    public interface IDatabaseResourceManager : IResourceManager<DbConnection>, IDatabaseManager
    {
    }
}