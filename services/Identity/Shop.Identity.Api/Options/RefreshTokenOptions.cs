using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using Shop.Identity.Api.Options;

public class RefreshTokenOptions
{
    public const string SectionName = "RefreshToken";
    [Range(1,30)]
    public int Days{get;set;}
    [Required]
    public string CookieName{get;set;} = string.Empty;
    [Required]
    public string CookiePath{get;set;} = string.Empty;
    
}