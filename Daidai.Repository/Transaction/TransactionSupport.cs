using System;
using System.Transactions;

namespace Daidai.Transaction
{
    public class TransactionSupport : ITransactionSupport, IDisposable
    {
        private readonly TransactionScope _scope;

        public TransactionSupport(TransactionScopeOption propagation, IsolationLevel isolation) =>
            _scope = new TransactionScope(propagation, new TransactionOptions
            {
                IsolationLevel = isolation,
            });

        public void Complete() => _scope.Complete();

        public void Rollback() => Dispose();

        public void Dispose() => _scope.Dispose();
    }
}