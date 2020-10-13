using System;
using System.Transactions;

namespace Dekopon.Transaction
{
    public class DtcTransactionManager : ITransactionManager, IDisposable
    {
        private static readonly Lazy<TransactionManager> TransactionManagerHolder = new Lazy<TransactionManager>(() => new TransactionManager());

        public static TransactionManager Instance => TransactionManagerHolder.Value;

        public ITransactionSupport Begin(
            TransactionScopeOption propagation = TransactionScopeOption.Required,
            IsolationLevel isolation = IsolationLevel.ReadCommitted)
        {
            return new DtcTransactionSupport(propagation, isolation);
        }

        public System.Transactions.Transaction Current => System.Transactions.Transaction.Current;

        public void Dispose()
        {
        }
    }

    public class DtcTransactionSupport : ITransactionSupport, IDisposable
    {
        private readonly TransactionScope _scope;

        public DtcTransactionSupport(TransactionScopeOption propagation, IsolationLevel isolation)
        {
            _scope = new TransactionScope(propagation, new TransactionOptions
            {
                IsolationLevel = isolation,
            });
            TransactionId = System.Transactions.Transaction.Current?.TransactionInformation.LocalIdentifier;
        }

        public string TransactionId { get; }

        public void Complete() => _scope.Complete();

        public void Rollback() => Dispose();

        public void Dispose() => _scope.Dispose();
    }
}