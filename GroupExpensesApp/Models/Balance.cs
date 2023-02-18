using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GroupExpensesApp.Models;

public class Balance
{
    private readonly Group _group;

    public Balance(Group group)
    {
        _group = group ?? throw new ArgumentNullException(nameof(group));
    }

    public ReadOnlyDictionary<User, List<Payment>> SettlePaymentsPerUser => new(_settlePaymentsPerUser);

    private Dictionary<User, List<Payment>> _settlePaymentsPerUser = new();

    public void Update()
    {
        var sumExpenses = _group.Expenses.Sum(x => x.Amount);
        var amountUsers = _group.Users.Count;
        var avgExpensePerUser = amountUsers > 0 ? sumExpenses / amountUsers : 0;

        var expensesPaidPerUser = _group.Expenses.GroupBy(expense => expense.PaidBy).ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());
        var totalPaidPerUser = expensesPaidPerUser.ToDictionary(pair => pair.Key, pair => pair.Value.Sum(x => x.Amount));

        //Now we find out those that have not paid anything and we add them to 'totalPaidPerUser' with amount 0.
        var debtorsWithoutPayments = _group.Users.Where(user => !totalPaidPerUser.ContainsKey(user)).ToHashSet();
        debtorsWithoutPayments.ToList().ForEach(debtor => totalPaidPerUser.Add(debtor, 0));


        var allDebtors = _group.Users.Where(user => totalPaidPerUser[user] < avgExpensePerUser).ToHashSet();
        var borrowers = _group.Users.Except(allDebtors).ToHashSet(); //These users are those that have borrowed money besides paying their share of expenses.


        var totalOwedPerDebtor = allDebtors.ToDictionary(debtor => debtor, debtor => avgExpensePerUser - totalPaidPerUser[debtor]); //The owed amount will be positive
        var totalLentPerBorrower = borrowers.ToDictionary(borrower => borrower, borrower => totalPaidPerUser[borrower] - avgExpensePerUser); //The lent amount will be positive

        //We try to minimize transactions by sorting the debtors.
        var debtorsListDescendingAmount = totalOwedPerDebtor.OrderByDescending(pair => pair.Value).Select(pair => pair.Key).ToList();
        var borrowersListDescendingAmount = totalLentPerBorrower.OrderByDescending(pair => pair.Value).Select(pair => pair.Key).ToList();


        var payments = new List<Payment>();


        //Now we split the bill between those that owe and the borrowers.
        foreach (var borrower in borrowersListDescendingAmount)
        {
            var borrowedAmount = totalLentPerBorrower[borrower];
            while (borrowedAmount > 0.0001)
            {
                foreach (var debtor in debtorsListDescendingAmount)
                {
                    var owedAmount = totalOwedPerDebtor[debtor];

                    while(owedAmount > 0.0001 && borrowedAmount > 0.0001)
                    {
                        var paymentAmount = borrowedAmount > owedAmount ? owedAmount : borrowedAmount;
                        var payment = new Payment
                        {
                            Amount = paymentAmount,
                            Payer = debtor,
                            Receiver = borrower
                        };
                        payments.Add(payment);

                        borrowedAmount -= paymentAmount;
                        totalLentPerBorrower[borrower] = borrowedAmount;

                        owedAmount -= paymentAmount;
                        totalOwedPerDebtor[debtor] = owedAmount;
                    }

                    if (borrowedAmount < 0.0001) break;
                }
            }
        }

        //At the end we update the internal variable _settlePaymentsPerUser

        _settlePaymentsPerUser = payments.GroupBy(payment => payment.Payer).ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());
    }
}