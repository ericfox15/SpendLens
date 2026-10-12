namespace Budget.Application.Contracts.Options;

/// <summary>Toggles and intervals for background jobs, matching eMAR's BackgroundJobs config section.</summary>
public class BackgroundJobOptions
{
    public BackgroundJobEntry PaymentCleanup { get; set; } = new();
}

public class BackgroundJobEntry
{
    public bool Enabled { get; set; }
    public int IntervalSeconds { get; set; } = 3600;
}
