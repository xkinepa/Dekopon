using System;
using System.Collections.Concurrent;
using DaiDai.Miscs;

namespace DaiDai.Transaction
{
    public abstract class TransactionAwareResourceManager<T> : IResourceManager<T>, IDisposable where T : IDisposable
    {
        private volatile bool _disposed = false;
        private readonly ConcurrentDictionary<string, T> _container = new ConcurrentDictionary<string, T>();

        private readonly ITransactionManager _transactionManager;

        private readonly Lazy<T> _resourceWithoutTransaction;

        protected TransactionAwareResourceManager(ITransactionManager transactionManager)
        {
            _resourceWithoutTransaction = new Lazy<T>(() => CreateResource(null));
            _transactionManager = transactionManager;
        }

        public T GetResource()
        {
            Assertion.IsTrue(!_disposed, $"already disposed");
            var transaction = _transactionManager?.Current;
            return transaction != null ? GetOrAdd(transaction) : _resourceWithoutTransaction.Value;
        }

        public virtual void Dispose()
        {
            _disposed = true;

            if (_resourceWithoutTransaction.IsValueCreated)
            {
                _resourceWithoutTransaction.Value.Dispose();
            }

            foreach (var disposable in _container.Values)
            {
                disposable.Dispose();
            }
        }

        protected abstract T CreateResource(System.Transactions.Transaction transaction = null);

        private T GetOrAdd(System.Transactions.Transaction transaction)
        {
            Assertion.NotNull(transaction, $"transaction should be specified");

            var identifier = GetIdentifier(transaction);
            return _container.GetOrAdd(identifier, i =>
            {
                transaction.TransactionCompleted += (sender, args) =>
                {
                    if (_container.TryRemove(i, out var resource))
                    {
                        resource.Dispose();
                    }
                };

                return CreateResource(transaction);
            });
        }

        private string GetIdentifier(System.Transactions.Transaction transaction)
        {
            return transaction.TransactionInformation.LocalIdentifier;
        }
    }

    public interface IResourceManager<out T> where T : IDisposable
    {
        T GetResource();
    }
}