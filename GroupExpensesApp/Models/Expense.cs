using System;

namespace GroupExpensesApp.Models; 

public class Expense {

    public readonly Guid Id = Guid.NewGuid();
    public User PaidBy { get; set; }
    public string Name { get; set; }
    public double Amount { get; set; }
}
