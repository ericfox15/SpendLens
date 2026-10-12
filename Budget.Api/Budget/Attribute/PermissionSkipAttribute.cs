namespace Budget.Api.Budget.Attribute;

/// <summary>
/// Opt-out marker for the PermissionGlobalAttribute. Apply to actions that should be
/// accessible without a permission check (login endpoint, health check).
/// The global filter checks for this attribute in EndpointMetadata before running.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class PermissionSkipAttribute : System.Attribute { }
