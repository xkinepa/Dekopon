using System;
using System.Data;
using System.Data.Common;
using Daidai.Repository;
using Daidai.Transaction;
using Microsoft.EntityFrameworkCore;
using StackExchange.Profiling.Data;

namespace Daidai
{
    public class ProfiledDatabaseManager : DatabaseManager
    {
        private readonly IDbProfiler _dbProfiler;

        public ProfiledDatabaseManager(DbContextOptions dbContextOptions, IDbProfiler dbProfiler, ITransactionManager transactionManager = null)
            : base(dbContextOptions, transactionManager)
        {
            _dbProfiler = dbProfiler;
        }

        public ProfiledDatabaseManager(Func<DbContext> databaseFactory, IDbProfiler dbProfiler, ITransactionManager transactionManager = null)
            : base(databaseFactory, transactionManager)
        {
            _dbProfiler = dbProfiler;
        }

        public override IDbConnection GetConnection()
        {
            var rawConnection = (DbConnection) base.GetConnection();
            return new ProfiledDbConnection(rawConnection, _dbProfiler);
        }
    }
}