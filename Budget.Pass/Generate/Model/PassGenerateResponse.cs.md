# PassGenerateResponse.cs

**Project:** Budget.Pass
**Layer:** Model (API Contract)

## What it does

The payload returned from `POST /api/pass/generate`. Organizes payments into three groups that mirror eMAR's medication pass structure.

```csharp
public class PassGenerateResponse
{
    public List<FPayment> Standard  { get; set; }  // recurring scheduled bills
    public List<FPayment> Extra     { get; set; }  // ad-hoc / one-time payments
    public List<FPayment> Optional  { get; set; }  // auto-pay bills (handled automatically)
}
```

## Why three buckets?

In eMAR, a medication pass separates medications into:
- **Standard** — scheduled medications for the time window
- **PRN** (as-needed) — medications given on demand
- **Insulin** — special handling required

This budget app mirrors that grouping to teach the same pattern:
- **Standard** — regular bills the user manually marks paid
- **Extra** — ad-hoc payments created inline (same as PRN in spirit)
- **Optional** — auto-pay bills where no user action is required

The Angular `PassComponent` renders each bucket in its own section.

## NGXS state

`PassState` in Angular stores this response after a successful generate call. Each payment in the arrays can be individually dispatched with `PayAction` or `SkipAction`, updating local state optimistically before the submit call confirms with the server.
