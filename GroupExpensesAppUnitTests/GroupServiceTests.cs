using GroupExpensesApp.Exceptions;
using GroupExpensesApp.Models;
using GroupExpensesApp.Services;
using GroupExpensesAppUnitTests;

namespace GroupExpensesApp.Tests;

public class GroupServiceTests
{

    private readonly IGroupService _groupService;

    public GroupServiceTests()
    {
        _groupService = new GroupService();
    }


    [Fact]
    public void TestSettlementPaymentsStatementExample()
    {
      
        var group = _groupService.CreateGroup("Trip", new() { TestData.John, TestData.Peter, TestData.Mary });

        _groupService.AddExpense(group.Id, new Expense { Name = "Hotel", Amount = 500, PaidBy = TestData.John });
        _groupService.AddExpense(group.Id, new Expense { Name = "Restaurant", Amount = 150, PaidBy = TestData.Mary });
        _groupService.AddExpense(group.Id, new Expense { Name = "Sightseeing", Amount = 100, PaidBy = TestData.Peter });

        var payments = _groupService.GetSettlementPaymentsPerUser(group.Id);
        Assert.NotNull(payments);
        Assert.Equal(expected: 2, payments.Count);

        Assert.Contains(payments, payment => payment.Key.Equals(TestData.Mary));
        Assert.Contains(payments, payment => payment.Key.Equals(TestData.Peter));

        Assert.Contains(payments, payment =>
        {
            return payment.Key.Equals(TestData.Mary) && payment.Value.Sum(s => s.Amount) == 100d;
        });

        Assert.Contains(payments, payment =>
        {
            return payment.Key.Equals(TestData.Peter) && payment.Value.Sum(s => s.Amount) == 150d;
        });
    }

    [Fact]
    public void TestSettlementPayments_WithOnly1Payer()
    {
        var group = _groupService.CreateGroup("Trip", new() { TestData.John, TestData.Peter, TestData.Mary });

        _groupService.AddExpense(group.Id, new Expense { Name = "Hotel", Amount = 500, PaidBy = TestData.John });
        _groupService.AddExpense(group.Id, new Expense { Name = "Restaurant", Amount = 150, PaidBy = TestData.John });
        _groupService.AddExpense(group.Id, new Expense { Name = "Sightseeing", Amount = 100, PaidBy = TestData.John });

        var payments = _groupService.GetSettlementPaymentsPerUser(group.Id);
        Assert.NotNull(payments);
        Assert.Equal(expected: 2, payments.Count);

        Assert.Contains(payments, payment => payment.Key.Equals(TestData.Mary));
        Assert.Contains(payments, payment => payment.Key.Equals(TestData.Peter));

        Assert.Contains(payments, payment =>
        {
            return payment.Key.Equals(TestData.Mary) && payment.Value.Sum(s => s.Amount) == 250d;
        });

        Assert.Contains(payments, payment =>
        {
            return payment.Key.Equals(TestData.Peter) && payment.Value.Sum(s => s.Amount) == 250d;
        });
    }


    [Fact]
    public void TestGroupIdUniqueness_MembersDistinctOrder()
    {
        var group1 = _groupService.CreateGroup("Trip", new() { TestData.John, TestData.Peter, TestData.Mary });

        var group2 = _groupService.CreateGroup("Trip", new() { TestData.Peter, TestData.Mary, TestData.John });

        Assert.Equal(expected: group1.Id, group2.Id);
        Assert.Equal(expected: group1, group2);
    }

    [Fact]
    public void TestSettlementPayments_AllTransactions_Success()
    {

        var group = _groupService.CreateGroup("Trip", new() { TestData.John, TestData.Peter, TestData.Mary });

        _groupService.AddExpense(group.Id, new Expense { Name = "Hotel", Amount = 500, PaidBy = TestData.John });
        _groupService.AddExpense(group.Id, new Expense { Name = "Restaurant", Amount = 150, PaidBy = TestData.John });
        _groupService.AddExpense(group.Id, new Expense { Name = "Sightseeing", Amount = 100, PaidBy = TestData.John });

        var payments = _groupService.GetSettlementPaymentsPerUser(group.Id);
        Assert.NotNull(payments);
        Assert.NotEmpty(payments);


        var paymentsFromMary = payments[TestData.Mary];
        var paymentsFromPeter = payments[TestData.Peter];

        paymentsFromMary.ForEach(payment => _groupService.AddPayment(group.Id, payment));
        paymentsFromPeter.ForEach(payment => _groupService.AddPayment(group.Id, payment));

        Assert.Equal(expected: 2, group.Transactions.Count);

        var paymentsAfterSettlement = _groupService.GetSettlementPaymentsPerUser(group.Id);
        Assert.NotNull(paymentsAfterSettlement);
        Assert.Empty(paymentsAfterSettlement);
    }

    [Fact]
    public void TestGroupNotFound_AddExpense_ThrowsGroupNotFoundException()
    {

        //We test with a random GUID that does not belong to any group
        Assert.Throws<GroupNotFoundException>(() => _groupService.AddExpense(Guid.NewGuid(), new Expense { Name = "Hotel", Amount = 500, PaidBy = TestData.John }));
    }

    [Fact]
    public void TestGroupNotFound_RemoveExpense_ThrowsGroupNotFoundException()
    {

        //We test with a random GUID that does not belong to any group
        Assert.Throws<GroupNotFoundException>(() => _groupService.RemoveExpense(Guid.NewGuid(), new Expense { Name = "Hotel", Amount = 500, PaidBy = TestData.John }));
    }

    [Fact]
    public void TestGroupNotFound_AddPayment_ThrowsGroupNotFoundException()
    {

        //We test with a random GUID that does not belong to any group, it does not matter to try with a random invalid payment
        Assert.Throws<GroupNotFoundException>(() => _groupService.AddPayment(Guid.NewGuid(), new Payment { Payer = TestData.John, Receiver = TestData.John, Amount = 0d })); 
    }

    [Fact]
    public void TestGroupNotFound_GetSettlements_ThrowsGroupNotFoundException()
    {
        //We test with a random GUID that does not belong to any group
        Assert.Throws<GroupNotFoundException>(() => _groupService.GetSettlementPaymentsPerUser(Guid.NewGuid()));
    }

    [Fact]
    public void TestAddOrRemoveExpenseAfterPayment_ThrowsAccountReadyForSettlementExceptionException()
    {

        var group = _groupService.CreateGroup("Trip", new() { TestData.John, TestData.Peter, TestData.Mary });

        var hotelExpense = new Expense { Name = "Hotel", Amount = 500, PaidBy = TestData.John };
        var restaurantExpense = new Expense { Name = "Restaurant", Amount = 150, PaidBy = TestData.John };
        var sightseeingExpense = new Expense { Name = "Sightseeing", Amount = 100, PaidBy = TestData.John };

        _groupService.AddExpense(group.Id, hotelExpense);
        _groupService.AddExpense(group.Id, restaurantExpense);
        _groupService.AddExpense(group.Id, sightseeingExpense);

        var payments = _groupService.GetSettlementPaymentsPerUser(group.Id);
        Assert.NotNull(payments);
        Assert.NotEmpty(payments);

        var paymentsFromMary = payments[TestData.Mary];
        paymentsFromMary.ForEach(payment => _groupService.AddPayment(group.Id, payment));

        //We check that we cannot add more expenses after adding some settlement payments.
        Assert.Throws<AccountReadyForSettlementException>(() => _groupService.AddExpense(group.Id, new Expense { Name = "Shopping", Amount = 100, PaidBy = TestData.John }));

        //We check that we cannot remove any previous expenses either.
        Assert.Throws<AccountReadyForSettlementException>(() => _groupService.RemoveExpense(group.Id, hotelExpense));
    }

    [Fact]
    public void TestAddInvalidPayment_InvalidAmount_ThrowsInvalidSettlementTransactionException()
    {

        var group = _groupService.CreateGroup("Trip", new() { TestData.John, TestData.Peter, TestData.Mary });

        var hotelExpense = new Expense { Name = "Hotel", Amount = 500, PaidBy = TestData.John };
        var restaurantExpense = new Expense { Name = "Restaurant", Amount = 150, PaidBy = TestData.John };
        var sightseeingExpense = new Expense { Name = "Sightseeing", Amount = 100, PaidBy = TestData.John };

        _groupService.AddExpense(group.Id, hotelExpense);
        _groupService.AddExpense(group.Id, restaurantExpense);
        _groupService.AddExpense(group.Id, sightseeingExpense);

        //The following payment is not the full amount for a settlement, so it is not valid.
        var ex = Assert.Throws<InvalidSettlementTransactionException>(() => _groupService.AddPayment(group.Id, new Payment { Payer = TestData.Mary, Receiver = TestData.John, Amount = 30d }));
        Assert.Equal("The transaction is not valid for settlement", ex.Message);
    }

    [Fact]
    public void TestAddInvalidPayment_InvalidPayer_ThrowsInvalidSettlementTransactionException()
    {

        var group = _groupService.CreateGroup("Trip", new() { TestData.John, TestData.Peter, TestData.Mary });

        var hotelExpense = new Expense { Name = "Hotel", Amount = 500, PaidBy = TestData.John };
        var restaurantExpense = new Expense { Name = "Restaurant", Amount = 150, PaidBy = TestData.John };
        var sightseeingExpense = new Expense { Name = "Sightseeing", Amount = 100, PaidBy = TestData.John };

        _groupService.AddExpense(group.Id, hotelExpense);
        _groupService.AddExpense(group.Id, restaurantExpense);
        _groupService.AddExpense(group.Id, sightseeingExpense);

        //It is not John who needs to pay the others, since he is the borrower. The following transaction is not valid.
        var ex = Assert.Throws<InvalidSettlementTransactionException>(() => _groupService.AddPayment(group.Id, new Payment { Payer = TestData.John, Receiver = TestData.Mary, Amount = 30d }));
        Assert.Equal("There are not any available payments for that payer", ex.Message);
    }
}
