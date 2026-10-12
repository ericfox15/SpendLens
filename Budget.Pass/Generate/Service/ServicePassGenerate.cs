using Budget.Application.Contracts.Enums;
using Budget.Pass.Generate.Model;
using Budget.Pass.Generate.Repository;
using Budget.Shared.Payment.Model;

namespace Budget.Pass.Generate.Service;

/// <summary>
/// Core pass generation logic. Expands bill schedules into FPayment due-instances within the
/// requested window, left-joining existing payments so already-paid ones show their status.
/// This is the eMAR medpass generate analogue — the "what's due this period" computation.
/// </summary>
public class ServicePassGenerate(IRepositoryPassGenerate repositoryPassGenerate) : IServicePassGenerate
{
    public async Task<PassGenerateResponse> GeneratePassAsync(PassGenerateRequest request)
    {
        var bills = (await repositoryPassGenerate.SelectActiveBillsAsync()).ToList();
        var accounts = await repositoryPassGenerate.SelectActiveAccountsAsync();
        var existingPayments = (await repositoryPassGenerate.SelectExistingPaymentsAsync(request.StartUTC, request.EndUTC)).ToList();
        var adherenceFlags = await repositoryPassGenerate.SelectAdherenceFlagsAsync();
        var overdueCount = await repositoryPassGenerate.SelectOverdueCountAsync();

        // Expand schedules into due dates
        var dueInstances = ExpandSchedules(bills, request.StartUTC, request.EndUTC);

        // Left-join: use existing payment if one exists (UNIQUE constraint on BillId+DueAtUTC prevents doubles)
        var payments = new List<FPayment>();
        foreach (var (bill, dueAt) in dueInstances)
        {
            var existing = existingPayments.FirstOrDefault(p =>
                p.BillId == bill.BillId && p.DueAtUTC == dueAt);

            if (existing != null)
            {
                payments.Add(existing);
            }
            else
            {
                // Generate a new pending payment row — only insert if it doesn't exist yet
                var newPayment = new FPayment
                {
                    BillId = bill.BillId,
                    AccountId = bill.AccountId,
                    CategoryId = bill.CategoryId,
                    DueAtUTC = dueAt,
                    Amount = bill.ExpectedAmount,
                    Status = EPaymentStatus.Pending,
                    BillName = bill.Name,
                    CategoryName = bill.CategoryName,
                    ExpectedAmount = bill.ExpectedAmount,
                    IsAutoPay = bill.IsAutoPay,
                    ModifiedBy = 0,
                    ModifiedUTC = DateTimeOffset.UtcNow
                };
                newPayment.PaymentId = await repositoryPassGenerate.InsertPaymentAsync(newPayment);
                payments.Add(newPayment);
            }
        }

        // Include any ad-hoc payments in the window (BillId IS NULL)
        payments.AddRange(existingPayments.Where(p => p.BillId == null));

        return new PassGenerateResponse
        {
            Standard = new PassStandard
            {
                Accounts = accounts,
                Bills = bills,
                Payments = payments
            },
            Extra = new PassExtra
            {
                AdherenceFlags = adherenceFlags,
                OverdueCount = overdueCount
            }
        };
    }

    public async Task GenerateNextPeriodAsync()
    {
        var now = DateTimeOffset.UtcNow;
        var startOfNextMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero).AddMonths(1);
        var endOfNextMonth = startOfNextMonth.AddMonths(1);

        await GeneratePassAsync(new PassGenerateRequest
        {
            StartUTC = startOfNextMonth,
            EndUTC = endOfNextMonth
        });
    }

    /// <summary>
    /// Expands a bill's schedule into individual due-date instances within the window.
    /// Handles Monthly (DayOfMonth), Biweekly (anchor + 2-week steps), Weekly (anchor + 1-week steps).
    /// Edge cases: month-end (day > days-in-month → last day), Feb 29 (→ Feb 28 in non-leap years).
    /// </summary>
    private static List<(Shared.Bill.Model.FBill Bill, DateTimeOffset DueAt)> ExpandSchedules(
        IEnumerable<Shared.Bill.Model.FBill> bills, DateTimeOffset start, DateTimeOffset end)
    {
        var result = new List<(Shared.Bill.Model.FBill, DateTimeOffset)>();

        foreach (var bill in bills)
        {
            switch (bill.ScheduleType)
            {
                case EScheduleType.Monthly:
                    {
                        if (!bill.DayOfMonth.HasValue) break;
                        var current = new DateTimeOffset(start.Year, start.Month, 1, 0, 0, 0, TimeSpan.Zero);
                        while (current < end)
                        {
                            var daysInMonth = DateTime.DaysInMonth(current.Year, current.Month);
                            var day = Math.Min(bill.DayOfMonth.Value, daysInMonth);
                            var dueAt = new DateTimeOffset(current.Year, current.Month, day, 0, 0, 0, TimeSpan.Zero);
                            if (dueAt >= start && dueAt < end)
                                result.Add((bill, dueAt));
                            current = current.AddMonths(1);
                        }
                        break;
                    }
                case EScheduleType.Biweekly:
                    {
                        if (!bill.AnchorDateUTC.HasValue) break;
                        var anchor = bill.AnchorDateUTC.Value;
                        // Step back to first occurrence on or after start
                        var diff = (start - anchor).TotalDays;
                        var stepsBack = (int)Math.Floor(diff / 14.0);
                        var current = anchor.AddDays(stepsBack * 14);
                        while (current < start) current = current.AddDays(14);
                        while (current < end)
                        {
                            result.Add((bill, current));
                            current = current.AddDays(14);
                        }
                        break;
                    }
                case EScheduleType.Weekly:
                    {
                        if (!bill.AnchorDateUTC.HasValue) break;
                        var anchor = bill.AnchorDateUTC.Value;
                        var diff = (start - anchor).TotalDays;
                        var stepsBack = (int)Math.Floor(diff / 7.0);
                        var current = anchor.AddDays(stepsBack * 7);
                        while (current < start) current = current.AddDays(7);
                        while (current < end)
                        {
                            result.Add((bill, current));
                            current = current.AddDays(7);
                        }
                        break;
                    }
            }
        }

        return result;
    }
}
