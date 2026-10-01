namespace Shop.Identity.Api.Contracts;
public record TokenResponce(string AccessToken, DateTimeOffset ExpiresAt );