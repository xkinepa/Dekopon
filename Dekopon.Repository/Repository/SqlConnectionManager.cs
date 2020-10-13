using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Dekopon.Miscs;
using Dekopon.Profiler;
using Dekopon.QueryBuilder;
using Dekopon.Transaction;

namespace Dekopon.Repository
{
    public class SqlConnectionManager : TransactionAwareResourceManager<DbConnection>, IDatabaseManager, IDisposable
    {
        private readonly IConnectionProvider _connectionProvider;
        private readonly string _connectionString;

        private readonly IDbProfiler _dbProfiler;
        private readonly IQueryBuilder _queryBuilder;
        private readonly Action<IDbConnection> _afterCreatedAction;

        public static Builder NewBuilder(string connectionString)
        {
            return new Builder(connectionString);
        }

        public SqlConnectionManager(string connectionString, IConnectionProvider connectionProvider = null,
            ITransactionManager transactionManager = null, IDbProfiler dbProfiler = null, IQueryBuilder queryBuilder = null,
            Action<IDbConnection> afterCreatedAction = null)
            : base(transactionManager)
        {
            _connectionProvider = connectionProvider ?? new SqlConnectionProvider();
            _connectionString = connectionString;

            _dbProfiler = dbProfiler;
            _queryBuilder = queryBuilder ?? new SqlServerQueryBuilder();
            _afterCreatedAction = afterCreatedAction ?? (_ => { });
        }

        public virtual IDbConnection GetConnection() => GetResource();

        public IQueryBuilder GetQueryBuilder() => _queryBuilder;

        protected override DbConnection CreateResource(System.Transactions.Transaction transaction = null)
        {
            var connection = _connectionProvider.CreateConnection(_connectionString);
            connection.EnlistTransaction(transaction);
            if (_dbProfiler != null)
            {
                connection = _dbProfiler.Profile(connection, transaction);
            }

            _afterCreatedAction?.Invoke(connection);
            return connection;
        }

        public class Builder : AbstractDatabaseManagerBuilder<Builder>
        {
            private readonly string _connectionString;
            private IConnectionProvider _connectionProvider;

            public Builder(string connectionString)
            {
                _connectionString = connectionString;
            }

            public Builder SetConnectionProvider(IConnectionProvider connectionProvider)
            {
                _connectionProvider = connectionProvider;
                return this;
            }

            public override IDatabaseManager Build()
            {
                return new SqlConnectionManager(_connectionString, _connectionProvider, TransactionManager, DbProfiler, QueryBuilder, AfterCreatedAction);
            }
        }
    }

    public class SqlConnectionProvider : IConnectionProvider
    {
        public SqlConnectionProvider()
        {
        }

        public DbConnection CreateConnection(string connectionString)
        {
            Assertion.HasLength(connectionString);

            var conn = new SqlConnection(connectionString);
            conn.Open();

            return conn;
        }
    }

    public interface IConnectionProvider
    {
        DbConnection CreateConnection(string connectionString);
    }
}