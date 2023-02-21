using System;

namespace GroupExpensesApp.Exceptions;

[Serializable]
public class InvalidSettlementTransactionException : Exception
{
    public InvalidSettlementTransactionException() { }

    public InvalidSettlementTransactionException(string message)
        : base(message) { }

    public InvalidSettlementTransactionException(string message, Exception inner)
        : base(message, inner) { }
}