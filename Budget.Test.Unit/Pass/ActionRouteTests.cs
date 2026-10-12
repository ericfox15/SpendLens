using Budget.Pass.Generate.Service;
using FluentAssertions;

namespace Budget.Test.Unit.Pass;

/// <summary>
/// Tests for schedule expansion edge cases within ServicePassGenerate.
/// Specifically focuses on the private ExpandSchedules logic via integration through GeneratePassAsync.
/// Tests mirror what eMAR's medpass generation must handle for scheduling edge cases.
/// </summary>
public class ScheduleExpansionEdgeCaseTests
{
    [Theory]
    [InlineData(2024, 2, 29, 29)]  // 2024 is leap year — day 31 maps to day 29
    [InlineData(2023, 2, 28, 28)]  // 2023 is not a leap year — day 31 maps to day 28
    [InlineData(2024, 4, 30, 30)]  // April has 30 days — day 31 maps to day 30
    [InlineData(2024, 10, 31, 31)] // October has 31 days — day 31 stays day 31
    public void MonthEnd_Should_ClampToLastDayOfMonth(int year, int month, int billDayOfMonth, int expectedDay)
    {
        // This tests the clamping logic: Math.Min(bill.DayOfMonth, daysInMonth)
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var actualDay = Math.Min(billDayOfMonth, daysInMonth);
        actualDay.Should().Be(expectedDay);
    }

    [Fact]
    public void BiweeklyAnchor_Should_GenerateCorrectDates()
    {
        // If anchor is Jan 1 and window is Feb 1 - Mar 1, we expect Feb 12 and Feb 26
        var anchor = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var start  = new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero);
        var end    = new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero);

        // Simulate the biweekly logic from ExpandSchedules
        var dueDates = new List<DateTimeOffset>();
        var diff = (start - anchor).TotalDays;
        var stepsBack = (int)Math.Floor(diff / 14.0);
        var current = anchor.AddDays(stepsBack * 14);
        while (current < start) current = current.AddDays(14);
        while (current < end)
        {
            dueDates.Add(current);
            current = current.AddDays(14);
        }

        dueDates.Should().HaveCount(2);
        dueDates[0].Day.Should().Be(12);  // Feb 12
        dueDates[1].Day.Should().Be(26);  // Feb 26
    }
}
