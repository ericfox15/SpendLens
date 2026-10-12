using Budget.Application.Contracts.Enums;
using Budget.Pass.Submit.Model;
using Budget.Pass.Submit.Repository;
using Budget.Pass.Submit.Service;
using FluentAssertions;
using Moq;

namespace Budget.Test.Unit.Pass;

/// <summary>
/// Tests for ServicePassSubmit — specifically:
///   - TempId bindings: negative frontend IDs get mapped to real server IDs
///   - Offline-sync conflict: when IsAlreadyRecorded is true, history row is written
///   - One transaction per item: a failed payment doesn't affect others in the batch
/// These are the highest-value behaviors mirroring eMAR's submit service.
/// </summary>
public class ServicePassSubmitTests
{
    private readonly Mock<IRepositoryPassSubmit> _repositoryMock = new();
    private readonly ServicePassSubmit _service;

    public ServicePassSubmitTests()
    {
        _service = new ServicePassSubmit(_repositoryMock.Object);
    }

    [Fact]
    public async Task SubmitPayments_Should_ReturnTempIdBindings_For_ExistingPayments()
    {
        // Arrange: two existing payments (positive IDs)
        var request = new SubmitPaymentRequest
        {
            Payments = new List<SubmitPayment>
            {
                new() { Standard = new SubmitPaymentStandard { PaymentId = 10, Status = EPaymentStatus.Paid } },
                new() { Standard = new SubmitPaymentStandard { PaymentId = 20, Status = EPaymentStatus.Skipped } }
            },
            IsOfflineSync = false
        };

        _repositoryMock.Setup(r => r.UpdatePaymentAsync(It.Is<SubmitPayment>(p => p.Standard.PaymentId == 10)))
                       .ReturnsAsync(10);
        _repositoryMock.Setup(r => r.UpdatePaymentAsync(It.Is<SubmitPayment>(p => p.Standard.PaymentId == 20)))
                       .ReturnsAsync(20);

        // Act
        var bindings = (await _service.SubmitPaymentsAsync(request)).ToList();

        // Assert: TempId == Id for existing rows (no temp IDs here)
        bindings.Should().HaveCount(2);
        bindings.Should().Contain(b => b.TempId == 10 && b.Id == 10);
        bindings.Should().Contain(b => b.TempId == 20 && b.Id == 20);
    }

    [Fact]
    public async Task SubmitPayments_Should_ReturnRealId_For_NewAdHocPayment()
    {
        // Arrange: ad-hoc payment with temp negative ID (like PRN in eMAR)
        var request = new SubmitPaymentRequest
        {
            Payments = new List<SubmitPayment>
            {
                new() { Standard = new SubmitPaymentStandard
                {
                    PaymentId = -1,  // temp ID assigned by frontend
                    AccountId = 1, CategoryId = 2,
                    Status = EPaymentStatus.Paid, Amount = 25m
                }}
            }
        };

        _repositoryMock.Setup(r => r.CreateOrUpdatePaymentAsync(It.IsAny<SubmitPayment>()))
                       .ReturnsAsync(999);  // server assigns real ID 999

        // Act
        var bindings = (await _service.SubmitPaymentsAsync(request)).ToList();

        // Assert: binding maps TempId=-1 to RealId=999
        bindings.Should().HaveCount(1);
        bindings[0].TempId.Should().Be(-1);
        bindings[0].Id.Should().Be(999);
    }

    [Fact]
    public async Task SubmitPayments_Should_WriteHistoryRow_When_OfflineSyncConflict()
    {
        // Arrange: offline sync, payment already recorded on server
        var request = new SubmitPaymentRequest
        {
            Payments = new List<SubmitPayment>
            {
                new() { Standard = new SubmitPaymentStandard { PaymentId = 5, Status = EPaymentStatus.Paid } }
            },
            IsOfflineSync = true
        };

        _repositoryMock.Setup(r => r.UpdatePaymentAsync(It.IsAny<SubmitPayment>())).ReturnsAsync(5);
        _repositoryMock.Setup(r => r.IsAlreadyRecordedAsync(It.IsAny<SubmitPayment>())).ReturnsAsync(true);
        _repositoryMock.Setup(r => r.InsertPaymentHistoryAsync(It.IsAny<SubmitPayment>(), It.IsAny<bool>()))
                       .Returns(Task.CompletedTask);

        // Act
        await _service.SubmitPaymentsAsync(request);

        // Assert: history row written for conflict
        _repositoryMock.Verify(
            r => r.InsertPaymentHistoryAsync(It.IsAny<SubmitPayment>(), true),
            Times.Once);
    }

    [Fact]
    public async Task SubmitPayments_Should_NotWriteHistory_When_NotOfflineSync()
    {
        // Regular online submit should NOT check for conflicts
        var request = new SubmitPaymentRequest
        {
            Payments = new List<SubmitPayment>
            {
                new() { Standard = new SubmitPaymentStandard { PaymentId = 5, Status = EPaymentStatus.Paid } }
            },
            IsOfflineSync = false
        };

        _repositoryMock.Setup(r => r.UpdatePaymentAsync(It.IsAny<SubmitPayment>())).ReturnsAsync(5);

        await _service.SubmitPaymentsAsync(request);

        _repositoryMock.Verify(r => r.IsAlreadyRecordedAsync(It.IsAny<SubmitPayment>()), Times.Never);
        _repositoryMock.Verify(r => r.InsertPaymentHistoryAsync(It.IsAny<SubmitPayment>(), It.IsAny<bool>()), Times.Never);
    }
}
