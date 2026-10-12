# FControllerBase.cs

**Project:** Budget.Api
**Layer:** Controller Base

## What it does

Abstract base class for all controllers in the app. Applies `[Authorize]` and `[ApiController]` to every controller that inherits from it.

```csharp
[Authorize]
[ApiController]
public abstract class FControllerBase : ControllerBase
{
}
```

## Why a custom base class?

Rather than adding `[Authorize]` to every controller individually (and risk forgetting one), the base class enforces it globally for all controllers that extend it. Any new controller you write is automatically protected.

`[ApiController]` enables:
- Automatic model validation (returns 400 with `ModelState` errors without manual checks)
- Automatic `[FromBody]` inference for complex types
- Problem details response format for errors

## FControllerBase vs ControllerBase vs Controller

- `ControllerBase` — base for API controllers (no view support)
- `Controller` — base for MVC controllers with views (Razor, etc.)
- `FControllerBase` — your custom base on top of `ControllerBase`

Since this is a REST API with no server-side views, `ControllerBase` is correct. Never inherit from `Controller` for pure API controllers — it adds unnecessary overhead.

## The F* naming

Consistent with eMAR's convention: `F` prefix on base infrastructure classes signals "this is a framework-level class in the eMAR architectural style."
