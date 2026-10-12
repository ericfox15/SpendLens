using Budget.Application.Contracts.Enums;

namespace Budget.Shared.User.Model;

/// <summary>
/// S* prefix = "Summary/Shared" read model. Joins User + Permission for auth decisions.
/// </summary>
public class SUserPermission
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public List<EPermission> Permissions { get; set; } = new();
}
