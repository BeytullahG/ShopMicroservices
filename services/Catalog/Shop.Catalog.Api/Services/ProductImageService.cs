using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Shop.Catalog.Api.Common;
using Shop.Catalog.Api.Contracts;
using Shop.Catalog.Api.Data;
using Shop.Catalog.Api.Entities;
using Shop.Catalog.Api.Options;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

using StackExchange.Redis;

namespace Shop.Catalog.Api.Services;
public interface IProductImageService
{
    Task<Result<ProductImageDto>> AddAsync(Guid productId, IFormFile file, CancellationToken ct);
    Task<Result> DeleteAsync(Guid productId, IFormFile file, CancellationToken ct);
}

public class ProductImageService
(
    CatalogDbContext db,
    ICatalogCache cache,
    IOptions<ImageStorageOptions> options,
    IWebHostEnvironment env,
    ILogger<ProductImageService> logger)
{
    private const int MaxDimension = 10_000;
    
    private static readonly (string Suffix, int MaxSide)[] Sizes = [
        (ImagePaths.Small, 300),
        (ImagePaths.Medium, 800),
        (ImagePaths.Large, 1600)
    ];

    private static ReadOnlySpan<byte> JpegMagic =>
        [0xFF, 0xD8, 0xFF];
    private static ReadOnlySpan<byte> PngMagic =>
        [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    private string RootPath =>
        Path.GetFullPath(options.Value.RootPath, env.ContentRootPath);

    public async Task<Result<ProductImageDto>> AddAsync(Guid productId, IFormFile file, CancellationToken ct)
    {
        var product = await db.Products
            .Where(p => p.Id == productId)
            .Select(p => new{p.Slug, ImageCount = p.Images.Count, MaxOrder = p.Images.Max(i => (int?)i.SortOrder)})
                .FirstOrDefaultAsync(ct);

        if(product is null)
        {
            return Error.NotFound("product not found");
        }
        if(product.ImageCount >= options.Value.MaxImagesPerProduct)
        {
            return Error.Conflict(
                "too_many_images",
                $"Product already has {product.ImageCount} images");
        }

        if(file.Length == 0 || file.Length > options.Value.MaxFileSizeBytes)
        {
            return Error.Validation(
                "invalid_file_size",
                "file is empty or too large");
        }


        await using var stream = file.OpenReadStream();

        var header = new byte[12];
        var read = await stream.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, ct);
        if(!IsSupportedFormat(header.AsSpan(0, read)))
        {
            return Error.Validation(
                "unsupported_format",
                "Only JPEG, PNG and WebP are allowed");
        }
        stream.Position = 0;
        
        var fileName = Guid.NewGuid().ToString("N");

        try
        {
            var info = await Image.IdentifyAsync(stream, ct);
            if (info.Width >= MaxDimension || info.Height >= MaxDimension)
            {
                return Error.Validation(
                    "image_too_large",
                    $"Image side must be at most {MaxDimension}px");
            }

            stream.Position = 0;

            using var image = await Image.LoadAsync(stream, ct);
            image.Mutate(x => x.AutoOrient());
            image.Metadata.ExifProfile = null;
            image.Metadata.XmpProfile = null;
            Directory.CreateDirectory(RootPath);
            var encoder = new WebpEncoder{Quality = 80};

            foreach (var (suffix, maxSide) in Sizes)
            {
                using var copy = image.Clone(x =>
                {
                    if(image.Height > maxSide || image.Width > maxSide)
                    {
                        x.Resize(new ResizeOptions
                        {
                            Mode = ResizeMode.Max,
                            Size = new Size(maxSide, maxSide)
                        });
                    }
                });
                await copy.SaveAsWebpAsync(Path.Combine(RootPath, fileName + suffix));
            }
        }
        catch(ImageFormatException)
        {
            DeleteFiles(fileName);
            return Error.Validation("invalid_image", "File is not a valid image");
        }

        var entity = new ProductImage
        {
            ProductId = productId,
            FileName = fileName,
            SortOrder = (product.MaxOrder ?? - 1) + 1
        };
        db.ProductImages.Add(entity);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch
        {
            DeleteFiles(fileName);
            throw;
        }

        await cache.RemoveAsync(CacheKeys.Product(product.Slug), ct);
        await cache.BumpProductListVersionAsync(ct);

        return ToDto(entity);


    }

    public async Task<Result> DeleteAsync(Guid productId, Guid imageId, CancellationToken ct)
    {
        var image = await db.ProductImages
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.Id == imageId && i.ProductId == productId, ct);
        
        if(image is null)
        {
            return Error.NotFound("Image not found");
        }
        db.ProductImages.Remove(image);
        await db.SaveChangesAsync(ct);
        DeleteFiles(image.FileName);
        await cache.RemoveAsync(CacheKeys.Product(image.Product.Slug), ct);
        await cache.BumpProductListVersionAsync(ct);
        return Result.Success();
    }

    private static bool IsSupportedFormat(ReadOnlySpan<byte> header) =>
        header.StartsWith(JpegMagic)
        || header.StartsWith(PngMagic)
        || (header.Length >= 12
            && header[..4].SequenceEqual("Riff"u8)
            && header[8..12].SequenceEqual("WEBP"u8));
    
    private void DeleteFiles(string fileName)
    {
        foreach(var (suffix, _) in Sizes)
        {
            try
            {
                File.Delete(Path.Combine(RootPath, fileName + suffix));
            }
            catch (IOException ex)
            {
                logger.LogWarning(ex, $"Failed to delete image file {fileName}{suffix}");
            }
        }
    }

    private static ProductImageDto ToDto(ProductImage i) => new()
    {
        Id = i.Id,
        SmallUrl = $"{ImagePaths.RequestPath}/{i.FileName}/{ImagePaths.Small}",
        MediumUrl = $"{ImagePaths.RequestPath}/{i.FileName}/{ImagePaths.Medium}",
        LargeUrl = $"{ImagePaths.RequestPath}/{i.FileName}/{ImagePaths.Large}",
    };

}