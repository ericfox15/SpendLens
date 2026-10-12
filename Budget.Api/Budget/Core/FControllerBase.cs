using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Budget.Api.Budget.Core;

/// <summary>
/// All controllers inherit from this. [Authorize] here means every controller requires a valid
/// JWT by default — individual actions can opt out with [AllowAnonymous] + [PermissionSkip].
/// Mirrors eMAR's FControllerBase.
/// </summary>
[Authorize]
public abstract class FControllerBase : Controller { }
