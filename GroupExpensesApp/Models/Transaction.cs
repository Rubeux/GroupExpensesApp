using System;

namespace GroupExpensesApp.Models;

public class Transaction
{
    protected Guid Id = Guid.NewGuid();

    public readonly DateTime TransactionDate = DateTime.Now;

    public readonly Payment Payment;

    public Transaction(Payment payment)
    {
        Payment = payment;
    }
}
