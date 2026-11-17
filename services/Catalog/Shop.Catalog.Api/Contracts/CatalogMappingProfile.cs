namespace Shop.Catalog.Api.Contracts;

using AutoMapper;

using Shop.Catalog.Api.Common;
using Shop.Catalog.Api.Entities;

public class CatalogMappingProfile : Profile
{
    public CatalogMappingProfile()
    {
        CreateMap<Product, ProductListItemDto>()
            .ForMember(d => d.InStock, o => o.MapFrom(s => s.Stock > 0))
            .ForMember(d => d.MainImageUrl, o => o.MapFrom(s => s.Images
                .OrderBy(i => i.SortOrder)
                .Select(i => ImagePaths.RequestPath + "/" + i.FileName + ImagePaths.Medium)
                .FirstOrDefault()));

        CreateMap<Product, ProductDetailsDto>()
            .ForMember(d => d.InStock, o => o.MapFrom(s => s.Stock > 0))
            .ForMember(d => d.Images, o => o.MapFrom(s => s.Images.OrderBy(i => i.SortOrder)));

        CreateMap<ProductImage, ProductImageDto>()
            .ForMember(d => d.SmallUrl, o => o.MapFrom(s => ImagePaths.RequestPath + "/" + s.FileName + ImagePaths.Small))
            .ForMember(d => d.MediumUrl, o => o.MapFrom(s => ImagePaths.RequestPath + "/" + s.FileName + ImagePaths.Medium))
            .ForMember(d => d.LargeUrl, o => o.MapFrom(s => ImagePaths.RequestPath + "/" + s.FileName + ImagePaths.Large));
    }
}