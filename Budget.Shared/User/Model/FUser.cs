namespace Budget.Shared.User.Model;

/// <summary>Application user. PasswordHash stores a BCrypt hash — never store plaintext.</summary>
public class FUser
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}
