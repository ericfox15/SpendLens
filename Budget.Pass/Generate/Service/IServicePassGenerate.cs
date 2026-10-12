using Budget.Pass.Generate.Model;

namespace Budget.Pass.Generate.Service;

public interface IServicePassGenerate
{
    Task<PassGenerateResponse> GeneratePassAsync(PassGenerateRequest request);
    /// <summary>Called by the background job to generate next period's payments.</summary>
    Task GenerateNextPeriodAsync();
}
