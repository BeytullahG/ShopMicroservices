using System.ComponentModel.DataAnnotations;

namespace Shop.Catalog.Api.Options;

public class ImageStorageOptions
{
    public const string SectionName = "ImageStorage";
    [Required]
    public string RootPath{get;set;} = "storage/images";
    [Range(1, 20 * 1024 * 1024)]
    public long MaxFileSizeBytes{get; set;} = 5 * 1024 * 1024;
    [Range(1, 50)]
    public int MaxImagesPerProduct {get; set;} = 10;
}