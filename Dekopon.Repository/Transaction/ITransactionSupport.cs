using System;

namespace Dekopon.Transaction
{
    public interface ITransactionSupport : IDisposable
    {
        void Complete();

        [Obsolete]
        void Rollback();
    }
}