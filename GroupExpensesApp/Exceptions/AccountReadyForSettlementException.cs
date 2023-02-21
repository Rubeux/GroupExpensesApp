using System;

namespace GroupExpensesApp.Exceptions;

[Serializable]
public class AccountReadyForSettlementException : Exception
{
    public AccountReadyForSettlementException() { }

    public AccountReadyForSettlementException(string message)
        : base(message) { }

    public AccountReadyForSettlementException(string message, Exception inner)
        : base(message, inner) { }
}