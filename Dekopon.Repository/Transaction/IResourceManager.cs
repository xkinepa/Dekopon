using System;
using System.Collections.Concurrent;
using System.Transactions;
using Dekopon.Miscs;

namespace Dekopon.Transaction
{
    public interface IResourceManager<out T> where T : IDisposable
    {
        T GetResource();
    }

    public abstract class TransactionAwareResourceManager<T> : IResourceManager<T>, IDisposable where T : IDisposable
    {
        private readonly ConcurrentDictionary<string, T> _resourceContainer = new ConcurrentDictionary<string, T>();
        private readonly Lazy<T> _resourceWithoutTransaction;

        private readonly ITransactionManager _transactionManager;

        private bool _disposed = false;

        protected TransactionAwareResourceManager(ITransactionManager transactionManager)
        {
            _resourceWithoutTransaction = new Lazy<T>(() => CreateResource());
            _transactionManager = transactionManager;
        }

        public T GetResource()
        {
            Assertion.IsFalse(_disposed, $"already disposed");

            var transaction = _transactionManager?.Current;
            return transaction != null ? GetOrAdd(transaction) : _resourceWithoutTransaction.Value;
        }

        public virtual void Dispose()
        {
            _disposed = true;

            foreach (var disposable in _resourceContainer.Values)
            {
                disposable.Dispose();
            }

            if (_resourceWithoutTransaction.IsValueCreated)
            {
                _resourceWithoutTransaction.Value.Dispose();
            }

            _resourceContainer.Clear();
        }

        protected abstract T CreateResource(System.Transactions.Transaction transaction = null);

        private T GetOrAdd(System.Transactions.Transaction transaction)
        {
            Assertion.NotNull(transaction, $"{nameof(transaction)} should be specified");

            var identifier = GetIdentifier(transaction);
            return _resourceContainer.GetOrAdd(identifier, i =>
            {
                transaction.TransactionCompleted += (sender, args) =>
                {
                    if (_resourceContainer.TryRemove(i, out var resource))
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
}