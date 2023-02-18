using System;

namespace GroupExpensesApp.Models;

public class Payment
{
    public readonly Guid Id = Guid.NewGuid();

    public User Payer { get; set; }

    public User Receiver { get; set; }

    public double Amount { get; set; }
}
