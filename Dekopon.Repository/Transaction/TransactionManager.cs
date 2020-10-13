using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Dekopon.Miscs;

namespace Dekopon.Transaction
{
    public class TransactionManager : TransactionManagerBase
    {
        public TransactionManager()
            : base(new ThreadLocalInstanceHolder<Stack<TransactionSupport>>())
        {
        }
    }

    public class FlowableTransactionManager : TransactionManagerBase
    {
        public FlowableTransactionManager()
            : base(new AsyncLocalInstanceHolder<Stack<TransactionSupport>>())
        {
        }
    }

    public abstract class TransactionManagerBase : ITransactionManager
    {
        private readonly IInstanceHolder<Stack<TransactionSupport>> _transactionSupportsIsolator;

        protected TransactionManagerBase() : this(new ThreadLocalInstanceHolder<Stack<TransactionSupport>>())
        {
        }

        protected TransactionManagerBase(IInstanceHolder<Stack<TransactionSupport>> transactionSupportsIsolator)
        {
            _transactionSupportsIsolator = transactionSupportsIsolator;
        }

        public ITransactionSupport Begin(
            TransactionScopeOption propagation = TransactionScopeOption.Required,
            IsolationLevel isolation = IsolationLevel.ReadCommitted)
        {
            var support = CreateTransactionSupport(propagation, isolation);
            EnsureTransactionSupportsCreated().Push(support);
            return support;
        }

        public System.Transactions.Transaction Current => CurrentTransactionSupport?.TransactionStatus.Transaction;

        private Stack<TransactionSupport> TransactionSupports
        {
            get => _transactionSupportsIsolator.Value;
            set => _transactionSupportsIsolator.Value = value;
        }

        internal TransactionSupport CurrentTransactionSupport
        {
            get
            {
                var stack = TransactionSupports;
                if (stack == null || !stack.Any())
                {
                    return null;
                }

                return stack.Peek();
            }
        }

        internal TransactionSupport PopTransactionSupport(TransactionSupport current)
        {
            var stack = TransactionSupports;
            Assertion.IsTrue(CurrentTransactionSupport == current, $"could not pop from other transactionSupport");
            return stack.Pop();
        }

        public void Dispose()
        {
            // do nothing
        }

        private Stack<TransactionSupport> EnsureTransactionSupportsCreated()
        {
            return TransactionSupports ?? (TransactionSupports = new Stack<TransactionSupport>(1));
        }

        private TransactionSupport CreateTransactionSupport(TransactionScopeOption propagation, IsolationLevel isolation)
        {
            switch (propagation)
            {
                case TransactionScopeOption.Required when CurrentTransactionSupport != null:
                    return TransactionSupport.CreateFrom(this, isolation, CurrentTransactionSupport);
                case TransactionScopeOption.Required:
                case TransactionScopeOption.RequiresNew:
                    return TransactionSupport.CreateNew(this, isolation);
                case TransactionScopeOption.Suppress:
                    return TransactionSupport.CreateSuppressed(this);
                default:
                    throw new ArgumentOutOfRangeException(nameof(propagation), propagation, null);
            }
        }
    }

    public class TransactionSupport : ITransactionSupport, IDisposable
    {
        private readonly TransactionManagerBase _transactionManager;

        public static TransactionSupport CreateSuppressed(TransactionManagerBase transactionManager)
        {
            return new TransactionSupport(transactionManager, new TransactionStatus());
        }

        public static TransactionSupport CreateNew(TransactionManagerBase transactionManager, IsolationLevel isolation)
        {
            return new TransactionSupport(transactionManager, new TransactionStatus(new CommittableTransaction(new TransactionOptions
            {
                IsolationLevel = isolation,
            })));
        }

        public static TransactionSupport CreateFrom(TransactionManagerBase transactionManager, IsolationLevel isolation, TransactionSupport parent)
        {
            Assertion.NotNull(parent, $"{nameof(parent)} should be specified");
            Assertion.IsFalse(parent.Completed, $"{nameof(parent)} already completed");

            if (parent.TransactionStatus.Transaction == null)
            {
                return CreateNew(transactionManager, isolation);
            }

            Assertion.IsTrue(isolation == parent.TransactionStatus.Transaction.IsolationLevel,
                $"conflict isolation detected");

            return new TransactionSupport(transactionManager, parent.TransactionStatus, false);
        }

        private TransactionSupport(TransactionManagerBase transactionManager, TransactionStatus transactionStatus, bool isNewTransaction = true)
        {
            _transactionManager = transactionManager;

            TransactionStatus = transactionStatus;
            IsNewTransaction = isNewTransaction;
        }

        public bool IsNewTransaction { get; }

        public string TransactionId => TransactionStatus.Transaction?.TransactionInformation.LocalIdentifier;

        public void Complete()
        {
            Assertion.IsFalse(Completed, $"already completed");
            Assertion.IsTrue(_transactionManager.CurrentTransactionSupport == this, $"not current transaction stack");
            Assertion.IsFalse(TransactionStatus.RollbackOnly, $"can only complete");

            if (IsNewTransaction)
            {
                // Assertion.IsTrue(TransactionStatus.Transaction is CommittableTransaction,
                //     $"{nameof(TransactionStatus.Transaction)} is not committable");
                (TransactionStatus.Transaction as CommittableTransaction)?.Commit();
            }

            Completed = true;
        }

        public void Rollback()
        {
            Assertion.IsFalse(Completed, $"already completed");
            Assertion.IsTrue(_transactionManager.CurrentTransactionSupport == this, $"not current transaction stack");

            if (IsNewTransaction)
            {
                TransactionStatus.Transaction?.Rollback();
            }

            TransactionStatus.RollbackOnly = true;
            Completed = true;
        }

        public void Dispose()
        {
            Assertion.IsTrue(_transactionManager.CurrentTransactionSupport == this, $"not current transaction stack");

            if (!Completed)
            {
                Rollback();
            }

            if (IsNewTransaction)
            {
                TransactionStatus.Transaction?.Dispose();
            }

            _transactionManager.PopTransactionSupport(this);
        }

        internal TransactionStatus TransactionStatus { get; }

        internal bool Completed { get; private set; }
    }

    public class TransactionStatus
    {
        public TransactionStatus()
        {
        }

        public TransactionStatus(System.Transactions.Transaction transaction)
        {
            Transaction = transaction;
        }

        public System.Transactions.Transaction Transaction { get; }

        public bool RollbackOnly { get; internal set; } = false;
    }
}