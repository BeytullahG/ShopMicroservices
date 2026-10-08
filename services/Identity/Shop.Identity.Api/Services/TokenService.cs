using System.Security.Claims;
using Microsoft.Extensions.Options;
using Shop.Identity.Api.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Shop.Identity.Api.Entities;
using Shop.Shared.Constants;

namespace Shop.Identity.Api.Services;


public class TokenService(IOptions<JwtOptions> options, RsaSecurityKey signingKey) : ITokenService
{
    public readonly JsonWebTokenHandler _tokenHandler = new();
    public AccessToken CreateAccessToken(AppUser user, IList<string> roles)
    {
        var jwt = options.Value;
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(jwt.AccessTokenMinutes);

        var email = user.Email
            ?? throw new InvalidOperationException($"User {user.Id} has no email");
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ShopClaimTypes.Role, role));
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer, 
            Audience = jwt.Audience,
            Subject = new ClaimsIdentity(claims), 
            IssuedAt = now.UtcDateTime, 
            NotBefore = now.UtcDateTime, 
            Expires = expiresAt.UtcDateTime, 
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256)
        };

        var token = _tokenHandler.CreateToken(descriptor);
        return new AccessToken(token, expiresAt);
    }
}