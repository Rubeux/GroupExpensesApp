using System;
using System.Collections.Generic;

namespace GroupExpensesApp.Models;

public class Payment
{
    public User Payer { get; set; }

    public User Receiver { get; set; }

    public double Amount { get; set; }

    public override bool Equals(object obj)
    {
        return obj is Payment payment &&
               EqualityComparer<User>.Default.Equals(Payer, payment.Payer) &&
               EqualityComparer<User>.Default.Equals(Receiver, payment.Receiver) &&
               Amount == payment.Amount;
    }
}
