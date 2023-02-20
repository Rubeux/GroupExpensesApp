using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GroupExpensesApp.Models;

//TODO Add Interface
public class Group
{

    public readonly Guid Id = Guid.NewGuid();
    public string Name { get; set; }
    public ReadOnlyCollection<User> Users => _users.AsReadOnly();
    public ReadOnlyCollection<Expense> Expenses => _expenses.Values.ToList().AsReadOnly();
    public ReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    private readonly List<User> _users;
    private readonly ConcurrentDictionary<Guid, Expense> _expenses = new(); //This is thread safe
    private readonly List<Transaction> _transactions = new();
    private readonly Balance _balance;
    private bool _readyForSettlement = false;

    private readonly object _lock = new();


    public Group(string name, List<User> users)
    {
        Name = name;
        _users = users;
        _balance = new Balance(this);
    }


    //TODO make these methods to return something in case of exception
    public void AddExpense(Expense expense)
    {
        //TODO throw Exception if _readyForSettlement == true
        if (expense != null && !_readyForSettlement)
        {
            lock (_lock)
            {
                _expenses.AddOrUpdate(expense.Id, expense, (key, value) => expense);
                _balance.Update();
            }
        }
    }

    public void RemoveExpense(Expense expense)
    {
        //TODO throw Exception if _readyForSettlement == true
        if (expense != null && !_readyForSettlement)
        {
            lock (_lock)
            {
                _expenses.Remove(expense.Id, out _);
                _balance.Update();
            }
        }
    }

    public void AddSettlementTransaction(Payment payment)
    {
        lock (_lock)
        {
            _readyForSettlement = true;
            var availableSettlementPayments = GetSettlementPayments();
            if (payment != null && availableSettlementPayments[payment.Payer].Contains(payment))
            {
                _transactions.Add(new Transaction(payment));
                _balance.Update();
            }
        }
    }

    public ReadOnlyDictionary<User, List<Payment>> GetSettlementPayments()
    {
        return _balance.SettlePaymentsPerUser;
    }

}
