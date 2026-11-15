namespace Shop.Catalog.Api.Contracts;

public sealed record CategoryRow(
        Guid Id,
        Guid? ParentId,
        string Name,
        string Slug);