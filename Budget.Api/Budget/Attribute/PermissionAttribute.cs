using Budget.Application.Contracts.Enums;

namespace Budget.Api.Budget.Attribute;

/// <summary>
/// Declares the minimum permission required for an action.
/// If not present, the global filter defaults to EPermission.Read.
/// Works with PermissionGlobalAttribute which reads this value and checks the JWT claims.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class PermissionAttribute(EPermission required) : System.Attribute
{
    public EPermission Required { get; } = required;
}
