using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GroupExpensesApp.Models;

public class Balance
{
    private const double RELATIVE_ZER0 = 0.0001;
    private readonly Group _group;

    public Balance(Group group)
    {
        _group = group ?? throw new ArgumentNullException(nameof(group));
    }

    public ReadOnlyDictionary<User, List<Payment>> SettlePaymentsPerUser => new(_settlePaymentsPerUser);

    private Dictionary<User, List<Payment>> _settlePaymentsPerUser = new();

    public void Update()
    {
        double avgSpentPerUser = GetAvgSpentPerUser(_group.Expenses, _group.Users.Count);
        var totalPaidPerUser = GetTotalPaidPerUser(_group.Expenses, _group.Users);

        var allDebtors = _group.Users.Where(user => totalPaidPerUser[user] < avgSpentPerUser).ToHashSet();
        var borrowers = _group.Users.Except(allDebtors).ToHashSet(); //These users are those that have borrowed money besides paying their share of expenses.

        var totalOwedPerDebtor = allDebtors.ToDictionary(debtor => debtor, debtor => avgSpentPerUser - totalPaidPerUser[debtor]); //The owed amount will be positive
        var totalLentPerBorrower = borrowers.ToDictionary(borrower => borrower, borrower => totalPaidPerUser[borrower] - avgSpentPerUser); //The lent amount will be positive

        //Now we split the bill between those that owe and the borrowers.
        var payments = GetSettlePayments(totalOwedPerDebtor, totalLentPerBorrower);

        //We remove from the payments the settlement transactions (if any)
        if (_group.Transactions.Count > 0)
        {
            var paymentsFromTransactions = _group.Transactions.Select(transaction => transaction.Payment).ToList();
            payments.RemoveAll(payment => paymentsFromTransactions.Contains(payment));
        }

        _settlePaymentsPerUser = payments.GroupBy(payment => payment.Payer).ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());
    }

    private static Dictionary<User, double> GetTotalPaidPerUser(ICollection<Expense> expenses, ICollection<User> users)
    {
        var expensesPaidPerUser = expenses.GroupBy(expense => expense.PaidBy).ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());

        var totalPaidPerUser = expensesPaidPerUser.ToDictionary(pair => pair.Key, pair => pair.Value.Sum(x => x.Amount));

        //Now we find out those that have not paid anything and we add them to 'totalPaidPerUser' with amount 0.
        var debtorsWithoutPayments = users.Where(user => !totalPaidPerUser.ContainsKey(user)).ToHashSet();
        debtorsWithoutPayments.ToList().ForEach(debtor => totalPaidPerUser.Add(debtor, 0));
        return totalPaidPerUser;
    }

    private static List<Payment> GetSettlePayments(Dictionary<User, double> totalOwedPerDebtor, Dictionary<User, double> totalLentPerBorrower)
    {
        var payments = new List<Payment>();
        //We try to minimize transactions by sorting the debtors.
        var debtorsListDescendingAmount = totalOwedPerDebtor.OrderByDescending(pair => pair.Value).Select(pair => pair.Key).ToList();
        var borrowersListDescendingAmount = totalLentPerBorrower.OrderByDescending(pair => pair.Value).Select(pair => pair.Key).ToList();

        foreach (var borrower in borrowersListDescendingAmount)
        {
            var borrowedAmount = totalLentPerBorrower[borrower];
            while (borrowedAmount > RELATIVE_ZER0)
            {
                foreach (var debtor in debtorsListDescendingAmount)
                {
                    var owedAmount = totalOwedPerDebtor[debtor];

                    while (owedAmount > RELATIVE_ZER0 && borrowedAmount > RELATIVE_ZER0)
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

                    if (borrowedAmount < RELATIVE_ZER0) break;
                }
            }
        }

        return payments;
    }

    private static double GetAvgSpentPerUser(ICollection<Expense> expenses, int amountUsers)
    {
        var sumExpenses = expenses.Sum(x => x.Amount);
        var avgExpensePerUser = amountUsers > 0 ? sumExpenses / amountUsers : 0;
        return avgExpensePerUser;
    }
}