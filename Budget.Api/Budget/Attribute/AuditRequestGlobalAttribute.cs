using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Budget.Application.Contracts.Options;
using Microsoft.Extensions.Options;
using Dapper;

namespace Budget.Api.Budget.Attribute;

/// <summary>
/// Global action filter that logs every request to dbo.AuditLog.
/// Runs before any action — mirrors eMAR's AuditRequestGlobalAttribute.
/// UserId is pulled from the JWT "sub" claim (null for anonymous/login requests).
/// </summary>
public class AuditRequestGlobalAttribute(IOptions<RepositoryOptions> options) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Run the action first, audit after (don't block the request)
        await next();

        try
        {
            var userIdClaim = context.HttpContext.User.FindFirst("sub")?.Value;
            int? userId = userIdClaim != null && int.TryParse(userIdClaim, out var uid) ? uid : null;

            const string sql = """
                INSERT INTO dbo.AuditLog (UserId, Route, Method, CreatedUTC)
                VALUES (@UserId, @Route, @Method, SYSDATETIMEOFFSET())
                """;

            using var connection = new SqlConnection(options.Value.BudgetDatabaseConnectionString);
            await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                Route = context.HttpContext.Request.Path.Value ?? "",
                Method = context.HttpContext.Request.Method
            });
        }
        catch
        {
            // Never let audit failure break the response
        }
    }
}
