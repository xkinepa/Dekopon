using System;

namespace Daidai.Transaction
{
    public interface ITransactionSupport : IDisposable
    {
        void Complete();

        [Obsolete]
        void Rollback();
    }
}