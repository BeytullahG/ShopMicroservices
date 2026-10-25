using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shop.Catalog.Api.Entities;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.FileName).HasMaxLength(100).IsRequired();
        builder.HasOne(i => i.Product)
            .WithMany(i => i.Images)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(i => new {i.ProductId, i.SortOrder});
    }
}