﻿using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Daidai.Miscs;
using Daidai.QueryBuilder;
using Daidai.Transaction;

namespace Daidai.Repository
{
    public class SqlConnectionManager : TransactionAwareResourceManager<DbConnection>, IDatabaseManager, IDisposable
    {
        private readonly IConnectionProvider _connectionProvider = new SqlConnectionProvider();
        private readonly string _connectionString;

        public SqlConnectionManager(string connectionString, ITransactionManager transactionManager = null)
            : base(transactionManager)
        {
            _connectionString = connectionString;
        }

        public virtual IDbConnection GetConnection() => GetResource();

        public IEntityQueryBuilder GetQueryBuilder()
        {
            return new SqlServerEntityQueryBuilder();
        }

        protected override DbConnection CreateResource(System.Transactions.Transaction transaction = null)
        {
            var dbConnection = _connectionProvider.CreateConnection(_connectionString);
            dbConnection.EnlistTransaction(transaction);
            return dbConnection;
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