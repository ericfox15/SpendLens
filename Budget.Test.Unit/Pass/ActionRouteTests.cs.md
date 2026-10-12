# ActionRouteTests.cs

**Project:** Budget.Test.Unit
**Layer:** Unit Tests — Schedule Edge Cases

## What it tests

Edge cases in the schedule expansion logic, specifically:
1. Month-end clamping across multiple calendar scenarios
2. Biweekly anchor alignment into a specific window

These tests are **data-focused** — they test the math directly without needing to mock services.

## MonthEnd_Should_ClampToLastDayOfMonth (Theory)

A parameterized test (`[Theory]` + `[InlineData]`) that verifies month-end clamping across four calendar scenarios:

| Year | Month | BillDayOfMonth | Expected |
|------|-------|---------------|----------|
| 2024 | Feb | 29 | 29 (leap year) |
| 2023 | Feb | 28 | 28 (non-leap year) |
| 2024 | Apr | 30 | 30 (April has 30 days) |
| 2024 | Oct | 31 | 31 (October has 31 days) |

```csharp
var daysInMonth = DateTime.DaysInMonth(year, month);
var actualDay = Math.Min(billDayOfMonth, daysInMonth);
actualDay.Should().Be(expectedDay);
```

Note: this test directly exercises the math (not via the service) — it's testing the logic unit independently before trusting it in the full service test.

## BiweeklyAnchor_Should_GenerateCorrectDates

Tests the biweekly step-walk algorithm in isolation:

- Anchor: Jan 1, 2024
- Window: Feb 1 – Mar 1
- Expected: Feb 12, Feb 26 (2 payments)

```csharp
dueDates.Should().HaveCount(2);
dueDates[0].Day.Should().Be(12);
dueDates[1].Day.Should().Be(26);
```

This window is different from `ServicePassGenerateTests` (which uses Oct 1 – Nov 1 with 3 results). Two different windows teaching that the count depends on the window, not just the schedule.

## [Theory] vs [Fact]

- `[Fact]` — single test, no parameters
- `[Theory]` + `[InlineData(...)]` — same test logic run once per `[InlineData]` row

Theories reduce code duplication when testing multiple input/output combinations of the same logic.
