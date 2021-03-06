﻿using System;
using System.Transactions;

namespace Dekopon.Transaction
{
    public interface ITransactionManager : IDisposable
    {
        ITransactionSupport Begin(
            TransactionScopeOption propagation = TransactionScopeOption.Required,
            IsolationLevel isolation = IsolationLevel.ReadCommitted);

        System.Transactions.Transaction Current { get; }
    }
}