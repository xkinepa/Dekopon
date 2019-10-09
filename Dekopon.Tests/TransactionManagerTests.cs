using System;
using System.Transactions;
using Dekopon.Transaction;
using Xunit;
using TransactionManager = Dekopon.Transaction.TransactionManager;

namespace Dekopon
{
    public class TransactionManagerTests : IDisposable
    {
        private readonly ITransactionManager _transactionManager;
        private readonly ResourceManager _resourceManager;

        public TransactionManagerTests()
        {
            _transactionManager = new TransactionManager();
            _resourceManager = new ResourceManager(_transactionManager);
        }

        public void Dispose()
        {
            _resourceManager.Dispose();
            _transactionManager.Dispose();
        }

        [Fact]
        public void InitialState()
        {
            Assert.Null(_transactionManager.Current);
            Assert.Null(_resourceManager.GetResource().Transaction);
        }

        [Fact]
        public void SimpleUsage()
        {
            using (var transactionSupport = _transactionManager.Begin())
            {
                var resource = _resourceManager.GetResource();
                Assert.NotNull(resource.Transaction);

                var anotherResource = _resourceManager.GetResource();
                Assert.Equal(resource.Transaction, anotherResource.Transaction);

                transactionSupport.Complete();
            }
        }

        [Fact]
        public void SharingTransactionManager()
        {
            using (var transactionSupport = _transactionManager.Begin())
            {
                var resource = _resourceManager.GetResource();

                using (var anotherResourceManager = new ResourceManager(_transactionManager))
                {
                    var anotherResource = anotherResourceManager.GetResource();
                    Assert.NotNull(anotherResource.Transaction);

                    Assert.Equal(resource.Transaction, anotherResource.Transaction);
                }

                transactionSupport.Complete();
            }
        }

        [Fact]
        public void ResourceManagerWithoutTransaction()
        {
            using (var transactionSupport = _transactionManager.Begin())
            using (var resourceManager = new ResourceManager(/* empty */))
            {
                var resource = resourceManager.GetResource();
                Assert.Null(resource.Transaction);

                transactionSupport.Complete();
            }
        }

        [Fact]
        public void TransactionScopeDisposed()
        {
            Resource resource;
            using (_transactionManager.Begin())
            {
                resource = _resourceManager.GetResource();
                Assert.False(resource.Disposed);
            }

            Assert.True(resource.Disposed);

            var anotherResource = _resourceManager.GetResource();
            Assert.Null(anotherResource.Transaction);
            Assert.NotEqual(resource.Transaction, anotherResource.Transaction);
        }

        [Fact]
        public void TransactionScopeRequired()
        {
            using (_transactionManager.Begin())
            {
                var resource = _resourceManager.GetResource();

                using (_transactionManager.Begin(TransactionScopeOption.Required))
                {
                    var anotherResource = _resourceManager.GetResource();
                    Assert.Equal(resource.Transaction, anotherResource.Transaction);
                }
            }
        }

        [Fact]
        public void TransactionScopeRequiresNew()
        {
            using (_transactionManager.Begin())
            {
                var resource = _resourceManager.GetResource();

                using (_transactionManager.Begin(TransactionScopeOption.RequiresNew))
                {
                    var anotherResource = _resourceManager.GetResource();
                    Assert.NotEqual(resource.Transaction, anotherResource.Transaction);
                }
            }
        }

        [Fact]
        public void TransactionScopeRequiredWithDifferentIsolationLevelThrowException()
        {
            using (_transactionManager.Begin(TransactionScopeOption.Required, IsolationLevel.ReadCommitted))
            {
                Assert.Throws<ArgumentException>(() =>
                {
                    _transactionManager.Begin(TransactionScopeOption.Required, IsolationLevel.RepeatableRead);
                });
            }
        }

        [Fact]
        public void TransactionScopeRequiresNewWithDifferentIsolationLevel()
        {
            using (_transactionManager.Begin(TransactionScopeOption.Required, IsolationLevel.ReadCommitted))
            using (_transactionManager.Begin(TransactionScopeOption.RequiresNew, IsolationLevel.RepeatableRead))
            {
                var resource = _resourceManager.GetResource();
                Assert.NotNull(resource.Transaction);
            }
        }

        internal class ResourceManager : TransactionAwareResourceManager<Resource>, IResourceManager<Resource>
        {
            public ResourceManager() : this(null)
            {
            }

            public ResourceManager(ITransactionManager transactionManager) : base(transactionManager)
            {
            }

            protected override Resource CreateResource(System.Transactions.Transaction transaction = null)
            {
                return new Resource(transaction);
            }
        }

        internal class Resource : IDisposable
        {
            public bool Disposed = false;
            public System.Transactions.Transaction Transaction { get; }

            public Resource(System.Transactions.Transaction transaction)
            {
                Transaction = transaction;
            }

            public void Dispose()
            {
                Disposed = true;
            }
        }
    }
}
