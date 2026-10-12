using Budget.Application.Contracts.Enums;
using Budget.Pass.Generate.Model;
using Budget.Pass.Generate.Repository;
using Budget.Pass.Generate.Service;
using Budget.Shared.Account.Model;
using Budget.Shared.Bill.Model;
using Budget.Shared.Payment.Model;
using FluentAssertions;
using Moq;
using System.Reflection;

namespace Budget.Test.Unit.Pass;

/// <summary>
/// Tests for schedule expansion logic in ServicePassGenerate.
/// The schedule expansion is the most complex piece — handling month-end, leap years,
/// biweekly anchors. These test the same edge cases eMAR's medpass generation must handle.
/// </summary>
public class ServicePassGenerateTests
{
    private readonly Mock<IRepositoryPassGenerate> _repositoryMock = new();
    private readonly ServicePassGenerate _service;

    public ServicePassGenerateTests()
    {
        _service = new ServicePassGenerate(_repositoryMock.Object);
    }

    [Fact]
    public async Task Generate_Should_CreatePendingPayments_When_NoneExist()
    {
        // Arrange: one monthly bill, no existing payments
        var bills = new List<FBill>
        {
            new() { BillId = 1, AccountId = 1, CategoryId = 1, Name = "Netflix",
                    ExpectedAmount = 15.99m, ScheduleType = EScheduleType.Monthly,
                    DayOfMonth = 15, IsActive = true }
        };
        var request = new PassGenerateRequest
        {
            StartUTC = new DateTimeOffset(2024, 10, 1, 0, 0, 0, TimeSpan.Zero),
            EndUTC   = new DateTimeOffset(2024, 11, 1, 0, 0, 0, TimeSpan.Zero)
        };

        _repositoryMock.Setup(r => r.SelectActiveBillsAsync()).ReturnsAsync(bills);
        _repositoryMock.Setup(r => r.SelectActiveAccountsAsync()).ReturnsAsync(new List<FAccount>());
        _repositoryMock.Setup(r => r.SelectExistingPaymentsAsync(request.StartUTC, request.EndUTC))
                       .ReturnsAsync(new List<FPayment>());
        _repositoryMock.Setup(r => r.InsertPaymentAsync(It.IsAny<FPayment>())).ReturnsAsync(101);
        _repositoryMock.Setup(r => r.SelectAdherenceFlagsAsync()).ReturnsAsync(new List<AdherenceFlag>());
        _repositoryMock.Setup(r => r.SelectOverdueCountAsync()).ReturnsAsync(0);

        // Act
        var result = await _service.GeneratePassAsync(request);

        // Assert
        result.Standard.Payments.Should().HaveCount(1);
        var payment = result.Standard.Payments.First();
        payment.BillId.Should().Be(1);
        payment.Status.Should().Be(EPaymentStatus.Pending);
        payment.DueAtUTC.Should().Be(new DateTimeOffset(2024, 10, 15, 0, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public async Task Generate_Should_ReuseExistingPayment_When_AlreadyPaid()
    {
        // Arrange: bill with existing PAID payment
        var dueDate = new DateTimeOffset(2024, 10, 15, 0, 0, 0, TimeSpan.Zero);
        var bills = new List<FBill>
        {
            new() { BillId = 1, AccountId = 1, CategoryId = 1, Name = "Rent",
                    ExpectedAmount = 1200m, ScheduleType = EScheduleType.Monthly,
                    DayOfMonth = 15, IsActive = true }
        };
        var existingPayment = new FPayment
        {
            PaymentId = 99, BillId = 1, AccountId = 1, CategoryId = 1,
            DueAtUTC = dueDate, Status = EPaymentStatus.Paid, Amount = 1200m,
            ModifiedBy = 1, ModifiedUTC = DateTimeOffset.UtcNow
        };
        var request = new PassGenerateRequest
        {
            StartUTC = new DateTimeOffset(2024, 10, 1, 0, 0, 0, TimeSpan.Zero),
            EndUTC   = new DateTimeOffset(2024, 11, 1, 0, 0, 0, TimeSpan.Zero)
        };

        _repositoryMock.Setup(r => r.SelectActiveBillsAsync()).ReturnsAsync(bills);
        _repositoryMock.Setup(r => r.SelectActiveAccountsAsync()).ReturnsAsync(new List<FAccount>());
        _repositoryMock.Setup(r => r.SelectExistingPaymentsAsync(request.StartUTC, request.EndUTC))
                       .ReturnsAsync(new List<FPayment> { existingPayment });
        _repositoryMock.Setup(r => r.SelectAdherenceFlagsAsync()).ReturnsAsync(new List<AdherenceFlag>());
        _repositoryMock.Setup(r => r.SelectOverdueCountAsync()).ReturnsAsync(0);

        // Act
        var result = await _service.GeneratePassAsync(request);

        // Assert: existing paid payment is reused, NOT re-inserted
        result.Standard.Payments.Should().HaveCount(1);
        result.Standard.Payments.First().PaymentId.Should().Be(99);
        result.Standard.Payments.First().Status.Should().Be(EPaymentStatus.Paid);
        _repositoryMock.Verify(r => r.InsertPaymentAsync(It.IsAny<FPayment>()), Times.Never);
    }

    [Fact]
    public async Task Generate_Should_HandleMonthEnd_When_DayExceedsDaysInMonth()
    {
        // Bill due on day 31 in February — should land on Feb 28
        var bills = new List<FBill>
        {
            new() { BillId = 1, AccountId = 1, CategoryId = 1, Name = "Day 31 Bill",
                    ExpectedAmount = 100m, ScheduleType = EScheduleType.Monthly,
                    DayOfMonth = 31, IsActive = true }
        };
        var request = new PassGenerateRequest
        {
            StartUTC = new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero),  // 2024 is leap year
            EndUTC   = new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero)
        };

        _repositoryMock.Setup(r => r.SelectActiveBillsAsync()).ReturnsAsync(bills);
        _repositoryMock.Setup(r => r.SelectActiveAccountsAsync()).ReturnsAsync(new List<FAccount>());
        _repositoryMock.Setup(r => r.SelectExistingPaymentsAsync(request.StartUTC, request.EndUTC))
                       .ReturnsAsync(new List<FPayment>());
        _repositoryMock.Setup(r => r.InsertPaymentAsync(It.IsAny<FPayment>())).ReturnsAsync(1);
        _repositoryMock.Setup(r => r.SelectAdherenceFlagsAsync()).ReturnsAsync(new List<AdherenceFlag>());
        _repositoryMock.Setup(r => r.SelectOverdueCountAsync()).ReturnsAsync(0);

        var result = await _service.GeneratePassAsync(request);

        result.Standard.Payments.Should().HaveCount(1);
        result.Standard.Payments.First().DueAtUTC!.Value.Day.Should().Be(29);  // 2024 is leap year
    }

    [Fact]
    public async Task Generate_Should_ExpandBiweeklyBill_Correctly()
    {
        // Biweekly bill anchored on Oct 1 — should appear Oct 1 and Oct 15 in October
        var anchor = new DateTimeOffset(2024, 10, 1, 0, 0, 0, TimeSpan.Zero);
        var bills = new List<FBill>
        {
            new() { BillId = 1, AccountId = 1, CategoryId = 1, Name = "Paycheck deduction",
                    ExpectedAmount = 50m, ScheduleType = EScheduleType.Biweekly,
                    AnchorDateUTC = anchor, IsActive = true }
        };
        var request = new PassGenerateRequest
        {
            StartUTC = new DateTimeOffset(2024, 10, 1, 0, 0, 0, TimeSpan.Zero),
            EndUTC   = new DateTimeOffset(2024, 11, 1, 0, 0, 0, TimeSpan.Zero)
        };

        _repositoryMock.Setup(r => r.SelectActiveBillsAsync()).ReturnsAsync(bills);
        _repositoryMock.Setup(r => r.SelectActiveAccountsAsync()).ReturnsAsync(new List<FAccount>());
        _repositoryMock.Setup(r => r.SelectExistingPaymentsAsync(request.StartUTC, request.EndUTC))
                       .ReturnsAsync(new List<FPayment>());
        _repositoryMock.Setup(r => r.InsertPaymentAsync(It.IsAny<FPayment>())).ReturnsAsync(1);
        _repositoryMock.Setup(r => r.SelectAdherenceFlagsAsync()).ReturnsAsync(new List<AdherenceFlag>());
        _repositoryMock.Setup(r => r.SelectOverdueCountAsync()).ReturnsAsync(0);

        var result = await _service.GeneratePassAsync(request);

        // Oct 1 + Oct 15 + Oct 29 = 3 instances in October (Oct 29 < Nov 1, so included)
        result.Standard.Payments.Should().HaveCount(3);
        var dueDates = result.Standard.Payments.Select(p => p.DueAtUTC!.Value.Day).OrderBy(d => d).ToList();
        dueDates.Should().BeEquivalentTo(new[] { 1, 15, 29 });
    }
}
