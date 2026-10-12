# ControllerPass.cs

**Project:** Budget.Api
**Layer:** Controller (HTTP Entry Point)

## What it does

Exposes the pass generate and submit endpoints — the core functionality of the app.

| Method | Route | Permission | Description |
|--------|-------|-----------|-------------|
| POST | `api/pass/generate` | Read | Generate payments for a time window |
| POST | `api/pass/submit/payment` | Write | Submit a batch of payments |

## POST /api/pass/generate

```csharp
[HttpPost("generate")]
public async Task<IActionResult> Generate([FromBody] PassGenerateRequest request)
    => Ok(await _service.GeneratePassAsync(request));
```

Takes a `{ StartUTC, EndUTC }` body, returns `PassGenerateResponse` with Standard/Extra/Optional payment buckets. The Angular pass component calls this when the user opens the pass page.

## POST /api/pass/submit/payment

```csharp
[HttpPost("submit/payment")]
[Permission(EPermission.Write)]
public async Task<IActionResult> SubmitPayments([FromBody] SubmitPaymentRequest request)
    => Ok(await _submitService.SubmitPaymentsAsync(request));
```

Takes a batch of payments (with temp IDs) and returns `PaymentBindingId[]` for the NGXS store to update local state.

## Why POST for both?

Both operations modify or create data:
- Generate may INSERT new Pending payment rows.
- Submit UPDATEs or INSERTs payment rows.

GET requests should be idempotent (safe to repeat, no side effects). Since generate has side effects (DB writes), it's a POST even though it "feels" like a read.

## eMAR parallel

`ControllerMedPass` in eMAR has the exact same shape: `POST /generate` and `POST /submit`. The controllers are intentionally minimal — all logic is in the service layer.
