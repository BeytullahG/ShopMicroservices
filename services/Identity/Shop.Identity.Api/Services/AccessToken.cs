namespace Shop.Identity.Api.Services;

public record AccessToken(string Token, DateTimeOffset ExpiresAt)
{
    
}
