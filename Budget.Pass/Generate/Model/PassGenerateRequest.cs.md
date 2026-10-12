# PassGenerateRequest.cs

**Project:** Budget.Pass
**Layer:** Model (API Contract)

## What it does

Simple request model for `POST /api/pass/generate`. Defines the time window to generate payments for.

```csharp
public class PassGenerateRequest
{
    public string StartUTC { get; set; }
    public string EndUTC { get; set; }
}
```

## Why string instead of DateTimeOffset?

The Angular frontend sends ISO 8601 strings (e.g., `"2024-02-01T00:00:00Z"`). ASP.NET Core's JSON binder can deserialize these to `DateTimeOffset` directly, but using `string` here gives explicit control over parsing in the service — the service calls `DateTimeOffset.Parse(request.StartUTC)` with an explicit culture, avoiding any ambiguity.

In production you'd add `[Required]` validation attributes. The current version keeps it minimal.

## eMAR parallel

In eMAR, `GenerateMedPassRequest` has `FacilityId`, `StartDTM`, `EndDTM` to scope a medication pass to one facility. Here we scope to a time window only since this is a single-user app.
