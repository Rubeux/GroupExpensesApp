using System;
using System.Collections.Generic;

namespace GroupExpensesApp.Models; 

public class Expense {

    public readonly Guid Id = Guid.NewGuid();
    public User PaidBy { get; set; }
    public string Name { get; set; }
    public double Amount { get; set; }

    public override bool Equals(object obj)
    {
        return obj is Expense expense &&
               Id.Equals(expense.Id) &&
               EqualityComparer<User>.Default.Equals(PaidBy, expense.PaidBy) &&
               Name == expense.Name &&
               Amount == expense.Amount;
    }
}
