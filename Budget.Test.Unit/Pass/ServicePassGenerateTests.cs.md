# ServicePassGenerateTests.cs

**Project:** Budget.Test.Unit
**Layer:** Unit Tests — Pass Generate

## What it tests

Four unit tests for `ServicePassGenerate` covering the core scheduling logic.

## Tests

### Generate_Should_CreatePendingPayments_When_NoneExist

**Scenario:** Generate a pass for October, no existing payments.
**Verifies:** A new Pending payment is inserted for each bill that falls in the window.

```csharp
// Setup:
_mockRepo.Setup(r => r.SelectExistingPayments(...)).ReturnsAsync(new List<FPayment>());

// Assert:
_mockRepo.Verify(r => r.InsertPayment(It.IsAny<FPayment>()), Times.Exactly(expectedCount));
```

### Generate_Should_ReuseExistingPayment_When_AlreadyPaid

**Scenario:** Payment already exists and is Paid.
**Verifies:** No new INSERT; the existing Paid payment is returned unchanged.

This tests idempotency — regenerating a pass for a period that was already partially completed must not wipe out prior work.

### Generate_Should_HandleMonthEnd_When_DayExceedsDaysInMonth

**Scenario:** Monthly bill with `DayOfMonth = 31`, generating for February 2024 (leap year → 29 days).
**Verifies:** The generated payment has `DueAt.Day == 29`, not 31 (which doesn't exist).

Tests the `Math.Min(DayOfMonth, daysInMonth)` clamping logic.

### Generate_Should_ExpandBiweeklyBill_Correctly

**Scenario:** Biweekly bill, anchor = Oct 1 2024, window = Oct 1 – Nov 1.
**Verifies:** Three payments generated for Oct 1, Oct 15, Oct 29.

This test failed initially because the expected count was 2 (Oct 1, Oct 15) — the fix was to count the window properly: Oct 1 + 14 = Oct 15 + 14 = Oct 29 (still < Nov 1 → included).

## Mocking with Moq

```csharp
var _mockRepo = new Mock<IRepositoryPassGenerate>();
_mockRepo.Setup(r => r.SelectActiveBills()).ReturnsAsync(new List<FBill> { testBill });
var svc = new ServicePassGenerate(_mockRepo.Object);
```

`Mock<T>` creates a fake implementation of the interface. `.Setup()` configures what it returns when called. `.Verify()` asserts it was called with specific arguments.

## FluentAssertions

```csharp
result.Standard.Should().HaveCount(3);
result.Standard[0].DueAtUTC.Day.Should().Be(1);
```

FluentAssertions reads like English and provides much better failure messages than plain `Assert.Equal`.
