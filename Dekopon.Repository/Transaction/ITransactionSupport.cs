using System;

namespace Dekopon.Transaction
{
    public interface ITransactionSupport : IDisposable
    {
        string TransactionId { get; }

        void Complete();
        void Rollback();
    }
}