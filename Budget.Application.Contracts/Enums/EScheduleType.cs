namespace Budget.Application.Contracts.Enums;

/// <summary>How often a bill recurs — stored as a tinyint in dbo.Bill.ScheduleType.</summary>
public enum EScheduleType : byte
{
    Monthly   = 1,
    Biweekly  = 2,
    Weekly    = 3
}
