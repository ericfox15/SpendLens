# ServicePassSubmitTests.cs

**Project:** Budget.Test.Unit
**Layer:** Unit Tests — Pass Submit

## What it tests

Four unit tests for `ServicePassSubmit` covering the temp ID binding, ad-hoc payment creation, and offline sync conflict handling.

## Tests

### SubmitPayments_Should_ReturnTempIdBindings_For_ExistingPayments

**Scenario:** Submit a payment with a positive TempId (existing payment row).
**Verifies:** The response `PaymentBindingId` has the same TempId, and RealId matches the existing PaymentId.

### SubmitPayments_Should_ReturnRealId_For_NewAdHocPayment

**Scenario:** Submit a payment with TempId = -1 (new ad-hoc).
**Verifies:**
- `CreateOrUpdatePayment` is called (INSERT path)
- Response has `TempId = -1`, `RealId = 999` (the new DB-assigned ID)

```csharp
_mockRepo.Setup(r => r.CreateOrUpdatePayment(It.IsAny<FPayment>())).ReturnsAsync(999);
var result = await _svc.SubmitPaymentsAsync(request);
result.Single().TempId.Should().Be(-1);
result.Single().RealId.Should().Be(999);
```

### SubmitPayments_Should_WriteHistoryRow_When_OfflineSyncConflict

**Scenario:** `IsOfflineSync = true` AND `IsAlreadyRecorded` returns `true` (payment was processed elsewhere while device was offline).
**Verifies:** `InsertPaymentHistory` is called (conflict recorded in history table).

### SubmitPayments_Should_NotWriteHistory_When_NotOfflineSync

**Scenario:** Normal (online) submission, `IsOfflineSync = false`.
**Verifies:** `InsertPaymentHistory` is **never** called — history rows only exist for offline conflicts.

```csharp
_mockRepo.Verify(r => r.InsertPaymentHistory(It.IsAny<FPayment>()), Times.Never);
```

## TransactionScope in tests

`ServicePassSubmit` uses `TransactionScope`. In tests, there's no real database, so the TransactionScope completes without actually doing a distributed transaction. The mocks still verify call counts correctly.

## What these tests teach

- Moq `.Verify(Times.Never)` — asserting something did NOT happen
- `.ReturnsAsync()` — async mock setup
- Testing the "offline sync conflict" branch without needing a real database
