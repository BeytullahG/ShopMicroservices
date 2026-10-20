namespace Shop.Identity.Api.Contracts;

public record TokenResponse(string AccessToken, DateTimeOffset ExpiresAt);