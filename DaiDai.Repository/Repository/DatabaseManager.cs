using System;
using System.Data;
using System.Data.Common;
using System.Transactions;
using DaiDai.QueryBuilder;
using DaiDai.Transaction;
using Microsoft.EntityFrameworkCore;

namespace DaiDai.Repository
{
    public class DatabaseManager : TransactionAwareResourceManager<DbConnection>, IDatabaseResourceManager, IDatabaseManager, IDisposable
    {
        private readonly Func<DbContext> _databaseFactory;

        public DatabaseManager(DbContextOptions dbContextOptions, ITransactionManager transactionManager = null)
            : this(() => new DbContext(dbContextOptions), transactionManager)
        {
        }

        public DatabaseManager(Func<DbContext> databaseFactory, ITransactionManager transactionManager = null)
            : base(transactionManager)
        {
            _databaseFactory = databaseFactory;
        }

        public virtual IDbConnection GetConnection() => GetResource();

        public IEntityQueryBuilder GetQueryBuilder()
        {
            return new SqlServerEntityQueryBuilder();
        }

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