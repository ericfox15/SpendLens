using Budget.Api.Budget.Attribute;
using Budget.Application.Contracts.Options;
using Budget.Shared.User.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Budget.Api.Budget.Core;

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// JWT login endpoint. Validates username+BCrypt hash, returns a signed JWT.
/// [PermissionSkip] bypasses the PermissionGlobalAttribute.
/// [AllowAnonymous] bypasses the [Authorize] on FControllerBase.
/// </summary>
[Route("api/authentication")]
public class ControllerAuthentication(IRepositoryUser repositoryUser, IOptions<JwtOptions> jwtOptions) : FControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [PermissionSkip]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await repositoryUser.SelectUserByUsernameAsync(request.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { Message = "Invalid credentials" });

        var permissions = await repositoryUser.SelectPermissionsAsync(user.UserId);

        var claims = new List<Claim>
        {
            new("sub", user.UserId.ToString()),
            new("username", user.Username)
        };
        claims.AddRange(permissions.Select(p => new Claim("permission", ((long)p).ToString())));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: jwtOptions.Value.Issuer,
            audience: jwtOptions.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpiryMinutes),
            signingCredentials: creds);

        return Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
}
