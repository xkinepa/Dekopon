using System;
using System.Transactions;

namespace Dekopon.Transaction
{
    public class TransactionManager : ITransactionManager, IDisposable
    {
        private static readonly Lazy<TransactionManager> TransactionManagerHolder = new Lazy<TransactionManager>(() => new TransactionManager());

        public static TransactionManager Instance => TransactionManagerHolder.Value;

        public ITransactionSupport Begin(
            TransactionScopeOption propagation = TransactionScopeOption.Required,
            IsolationLevel isolation = IsolationLevel.ReadCommitted)
        {
            return new TransactionSupport(propagation, isolation);
        }

        public System.Transactions.Transaction Current => System.Transactions.Transaction.Current;

        public void Dispose()
        {
        }
    }
}