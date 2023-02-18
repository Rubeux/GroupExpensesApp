using System;

namespace GroupExpensesApp.Models;

public class Transaction : Payment
{
    public readonly DateTime TransactionDate = DateTime.Now;
}
