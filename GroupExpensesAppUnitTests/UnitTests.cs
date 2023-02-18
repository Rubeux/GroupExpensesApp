using GroupExpensesApp.Models;
using System.Linq;
using Xunit;

namespace GroupExpensesApp.Tests;

public class UnitTests
{

    [Fact]
    public void TestSettlementPaymentsExample()
    {
        var John = new User
        {
            Name = "John",
        };

        var Peter = new User
        {
            Name = "Peter",
        };

        var Mary = new User
        {
            Name = "Mary",
        };

        var group = new Group("Trip", new() { John, Peter, Mary });

        group.AddExpense(new Expense { Name = "Hotel", Amount = 500, PaidBy = John });
        group.AddExpense(new Expense { Name = "Restaurant", Amount = 150, PaidBy = Mary });
        group.AddExpense(new Expense { Name = "Sightseeing", Amount = 100, PaidBy = Peter });

        var payments = group.GetSettlementPayments();
        Assert.NotNull(payments);
        Assert.Equal(2, payments.Count);

        Assert.Contains(payments, payment => payment.Key.Equals(Mary));
        Assert.Contains(payments, payment => payment.Key.Equals(Peter));

        Assert.Contains(payments, payment =>
        {
            return payment.Key.Equals(Mary) && payment.Value.Sum(s => s.Amount) == 100d ;
        });

        Assert.Contains(payments, payment =>
        {
            return payment.Key.Equals(Peter) && payment.Value.Sum(s => s.Amount) == 150d;
        });
    }
}
