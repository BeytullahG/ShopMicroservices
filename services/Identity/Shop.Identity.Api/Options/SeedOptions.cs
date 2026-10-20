using System.ComponentModel.DataAnnotations;

namespace Shop.Identity.Api.Options;
public class SeedOptions
{
    public const string SectionName = "Seed";
    [Required]
    [EmailAddress]
    public string AdminEmail{get;set;} = string.Empty;
    [Required]
    public string AdminPassword{get;set;} = string.Empty;

}