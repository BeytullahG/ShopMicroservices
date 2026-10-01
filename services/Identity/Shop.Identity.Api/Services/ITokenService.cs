using Shop.Identity.Api.Entities;

namespace Shop.Identity.Api.Services;

public interface ITokenService{
    AccessToken CreateAccessToken(AppUser user, IList<string> roles);
}

