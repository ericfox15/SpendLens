using Budget.Shared.Setting.Service;
using Microsoft.AspNetCore.Mvc;

namespace Budget.Api.Budget.Core;

/// <summary>
/// Returns all settings + action routes to the Angular app at startup.
/// The frontend stores this in PassConfigState and uses it to drive the action pipeline.
/// Mirrors eMAR's GET api/core/initialization/settings.
/// </summary>
[Route("api/core/initialization")]
public class ControllerInitialization(RepositoryConfiguration repositoryConfiguration) : FControllerBase
{
    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        await repositoryConfiguration.LoadAsync();
        return Ok(new
        {
            Settings = repositoryConfiguration.GetAll(),
            ActionRoutes = repositoryConfiguration.GetActionRoutes(),
            ShowAutoPay = repositoryConfiguration.IsAutoPayShown(),
            OverBudgetWarnPercent = repositoryConfiguration.GetOverBudgetWarnPercent()
        });
    }
}
