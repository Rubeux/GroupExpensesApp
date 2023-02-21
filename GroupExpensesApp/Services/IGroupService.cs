using GroupExpensesApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GroupExpensesApp.Services;

public interface IGroupService
{
    public Group CreateGroup(string name, List<User> Users);

    public Group GetGroup(Guid groupId);

    public bool AddExpense(Guid groupId, Expense expense);

    public bool RemoveExpense(Guid groupId, Expense expense);

    public bool AddPayment(Guid groupId, Payment payment);

    public ReadOnlyDictionary<User, List<Payment>> GetSettlementPaymentsPerUser(Guid groupId);

}
