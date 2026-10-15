namespace Shop.Identity.Api.Services;
public record RefreshTokenData(string Token, string TokenHash, DateTimeOffset ExpiresAt);

