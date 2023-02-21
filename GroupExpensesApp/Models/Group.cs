using GroupExpensesApp.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;

namespace GroupExpensesApp.Models;

public class Group
{

    public readonly Guid Id;
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
        Id = GetGuidForGroup(name, users);
        _users = users;
        _balance = new Balance(this);
    }


    public void AddExpense(Expense expense)
    {
        if (_readyForSettlement)
        {
            throw new AccountReadyForSettlementException("It is not possible to add any expense since there has been done some settlement payments already.");
        }

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
        if (_readyForSettlement)
        {
            throw new AccountReadyForSettlementException("It is not possible to remove any expense since there has been done some settlement payments already.");
        }

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
        if (payment == null)
            throw new ArgumentNullException(nameof(payment));

        lock (_lock)
        {
            var availableSettlementPayments = GetSettlementPayments();
            if (!availableSettlementPayments.ContainsKey(payment.Payer))
                throw new InvalidSettlementTransactionException("There are not any available payments for that payer");

            if (!availableSettlementPayments[payment.Payer].Contains(payment))
                throw new InvalidSettlementTransactionException("The transaction is not valid for settlement");

            _transactions.Add(new Transaction(payment));
            _balance.Update();
            _readyForSettlement = true;
        }
    }

    public ReadOnlyDictionary<User, List<Payment>> GetSettlementPayments()
    {
        return _balance.SettlePaymentsPerUser;
    }

    private static Guid GetGuidForGroup(string name, List<User> users, string salt = "example12")
    {
        var text = string.Join("", name, users.OrderByDescending(x => x.Id).Select(e => e.Id));

        if (string.IsNullOrEmpty(text))
        {
            return Guid.NewGuid();
        }

        // Uses MD5 to create a 16 bytes hash
        using (var md5 = MD5.Create())
        {
            // Convert the string to a byte array first, to be processed
            byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(text + salt);
            byte[] hashBytes = md5.ComputeHash(textBytes);

            var result = new Guid(hashBytes);

            return result;
        }
    }
}
