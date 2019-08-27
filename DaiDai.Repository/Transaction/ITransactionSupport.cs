using System;

namespace DaiDai.Transaction
{
    public interface ITransactionSupport : IDisposable
    {
        void Complete();

        [Obsolete]
        void Rollback();
    }
}