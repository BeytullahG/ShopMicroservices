using System.ComponentModel.DataAnnotations;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
namespace Shop.Catalog.Api.Options;

public class JwtValidationOptions
{
    public const string SectionName = "Jwt";
    [Required]
    public string Issuer{get; init;} = string.Empty;
    [Required]
    public string Audience {get; init;} = string.Empty;
    [Required]
    public string PublicKeyPath {get; init;} = string.Empty;
    

}