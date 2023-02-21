using GroupExpensesApp.Exceptions;
using GroupExpensesApp.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Group = GroupExpensesApp.Models.Group;

namespace GroupExpensesApp.Services;

public class GroupService : IGroupService
{
    private readonly ConcurrentDictionary<Guid, Group> _groups = new();

    public GroupService() { }

    public bool AddExpense(Guid groupId, Expense expense)
    {
        var group = GetGroup(groupId);
        if (group == null) throw new GroupNotFoundException($"The group with Id {groupId} was not found");

        group.AddExpense(expense);
        return true;
    }

    public bool RemoveExpense(Guid groupId, Expense expense)
    {
        var group = GetGroup(groupId);
        if (group == null) throw new GroupNotFoundException($"The group with Id {groupId} was not found");

        group.RemoveExpense(expense);
        return true;
    }

    public bool AddPayment(Guid groupId, Payment payment)
    {
        var group = GetGroup(groupId);
        if (group == null) throw new GroupNotFoundException($"The group with Id {groupId} was not found");

        group.AddSettlementTransaction(payment);
        return true;
    }

    public Group CreateGroup(string name, List<User> users)
    {
        var group = new Group(name, users);
        var existingGroup = GetGroup(group.Id);
        if (existingGroup != null)
        {
            return existingGroup;
        }
        
        return _groups.AddOrUpdate(group.Id, group, (key, value) => group); 
    }

    public Group GetGroup(Guid groupId)
    {
       _groups.TryGetValue(groupId, out var group);
        return group;
    }

    public ReadOnlyDictionary<User, List<Payment>> GetSettlementPaymentsPerUser(Guid groupId)
    {
        var group = GetGroup(groupId);
        if (group == null) throw new GroupNotFoundException($"The group with Id {groupId} was not found");

        return group.GetSettlementPayments();
    }
}
