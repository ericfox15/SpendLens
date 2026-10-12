using Budget.Application.Contracts.Enums;
using Budget.Shared.User.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Budget.Api.Budget.Attribute;

/// <summary>
/// Global filter that enforces permissions on every action.
/// Flow: skip if [PermissionSkip] → read [Permission] attribute (default: Read) →
/// pull userId from JWT → load permissions from DB → forbid if insufficient.
/// Mirrors eMAR's PermissionGlobalAttribute exactly.
/// </summary>
public class PermissionGlobalAttribute : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Opt-out: [PermissionSkip] bypasses this check
        var skip = context.ActionDescriptor.EndpointMetadata.OfType<PermissionSkipAttribute>().Any();
        if (skip)
        {
            await next();
            return;
        }

        var requiredPermission = context.ActionDescriptor.EndpointMetadata
            .OfType<PermissionAttribute>().FirstOrDefault()?.Required ?? EPermission.Read;

        var userIdClaim = context.HttpContext.User.FindFirst("sub")?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            context.Result = new ForbidResult();
            return;
        }

        var repo = context.HttpContext.RequestServices.GetRequiredService<IRepositoryUser>();
        var permissions = (await repo.SelectPermissionsAsync(userId)).ToList();

        if (!permissions.Contains(requiredPermission) && !permissions.Contains(EPermission.Admin))
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}
