using System;

namespace GroupExpensesApp.Models;

public class Payment
{
    public User Payer { get; set; }

    public User Receiver { get; set; }

    public double Amount { get; set; }
}
