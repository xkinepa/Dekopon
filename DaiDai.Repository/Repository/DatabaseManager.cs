using System;
using System.Data;
using System.Data.Common;
using System.Transactions;
using Daidai.QueryBuilder;
using Daidai.Transaction;
using Microsoft.EntityFrameworkCore;

namespace Daidai.Repository
{
    public class DatabaseManager : TransactionAwareResourceManager<DbConnection>, IDatabaseResourceManager, IDatabaseManager, IDisposable
    {
        private readonly Func<DbContext> _databaseFactory;
        private readonly IEntityQueryBuilder _entityQueryBuilder;

        public DatabaseManager(DbContextOptions dbContextOptions, ITransactionManager transactionManager = null, IEntityQueryBuilder entityQueryBuilder = null)
            : this(() => new DbContext(dbContextOptions), transactionManager, entityQueryBuilder)
        {
        }

        public DatabaseManager(Func<DbContext> databaseFactory, ITransactionManager transactionManager = null, IEntityQueryBuilder entityQueryBuilder = null)
            : base(transactionManager)
        {
            _databaseFactory = databaseFactory;
            _entityQueryBuilder = entityQueryBuilder;
        }

        public virtual IDbConnection GetConnection() => GetResource();

        public IEntityQueryBuilder GetQueryBuilder()
        {
            return _entityQueryBuilder;
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