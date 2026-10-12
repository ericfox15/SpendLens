using System.ComponentModel.DataAnnotations;

namespace Budget.Application.Contracts.Options;

/// <summary>Holds JWT signing key and token lifetime, bound from appsettings "Jwt" section.</summary>
public class JwtOptions
{
    [Required]
    public string Secret { get; set; } = string.Empty;
    [Required]
    public string Issuer { get; set; } = string.Empty;
    [Required]
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 480;
}
