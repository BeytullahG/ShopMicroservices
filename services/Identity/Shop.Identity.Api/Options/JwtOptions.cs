using System.ComponentModel.DataAnnotations;

namespace Shop.Identity.Api.Options;
public class JwtOptions{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer{get;set;} = string.Empty;
    [Required]
    public string Audience{get;set;} = string.Empty;
    [Range(1,60)]
    public int AccessTokenMinutes{get;set;}
    [Required]
    public string PrivateKey{get;} = string.Empty;
    

}
